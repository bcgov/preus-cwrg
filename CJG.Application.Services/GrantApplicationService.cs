using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using CJG.Application.Business.Models;
using CJG.Application.Services.Exceptions;
using CJG.Core.Entities;
using CJG.Core.Entities.Helpers;
using CJG.Core.Interfaces;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Entities;
using CJG.Infrastructure.Identity;
using NLog;

namespace CJG.Application.Services
{
	/// <summary>
	/// <typeparamref name="GrantApplicationService"/> class, provides a way to manage grant applications.
	/// </summary>
	public class GrantApplicationService : Service, IGrantApplicationService
	{
		private readonly INotificationService _notificationService;
		private readonly ISiteMinderService _siteMinderService;
		private readonly IUserService _userService;
		private readonly IGrantOpeningService _grantOpeningService;
		private readonly IGrantAgreementService _grantAgreementService;
		private readonly INoteService _noteService;
		private readonly IGrantStreamService _grantStreamService;
		private readonly IUserManagerAdapter _userManager;
		private readonly IEligibleCostService _eligibleCostService;
		private readonly IEligibleCostBreakdownService _eligibleCostBreakdownService;
		private readonly IAttachmentService _attachmentService;

		private ITrainingProgramService _trainingProgramService;
		private ITrainingProviderService _trainingProviderService;

		/// <summary>
		/// Creates a new instance of a <typeparamref name="GrantApplicationService"/> object.
		/// </summary>
		/// <param name="notificationService"></param>
		/// <param name="siteMinderService"></param>
		/// <param name="userService"></param>
		/// <param name="grantStreamService"></param>
		/// <param name="grantAgreementService"></param>
		/// <param name="grantOpeningService"></param>
		/// <param name="noteService"></param>
		/// <param name="userManager"></param>
		/// <param name="eligibleCostService"></param>
		/// <param name="eligibleCostBreakdownService"></param>
		/// <param name="attachmentService"></param>
		/// <param name="dbContext"></param>
		/// <param name="httpContext"></param>
		/// <param name="logger"></param>
		public GrantApplicationService(
			INotificationService notificationService,
			ISiteMinderService siteMinderService,
			IUserService userService,
			IGrantStreamService grantStreamService,
			IGrantAgreementService grantAgreementService,
			IGrantOpeningService grantOpeningService,
			INoteService noteService,
			IUserManagerAdapter userManager,
			IEligibleCostService eligibleCostService,
			IEligibleCostBreakdownService eligibleCostBreakdownService,
			IAttachmentService attachmentService,
			IDataContext dbContext,
			HttpContextBase httpContext,
			ILogger logger) : base(dbContext, httpContext, logger)
		{
			_notificationService = notificationService;
			_siteMinderService = siteMinderService;
			_userService = userService;
			_grantStreamService = grantStreamService;
			_grantAgreementService = grantAgreementService;
			_grantOpeningService = grantOpeningService;
			_noteService = noteService;
			_userManager = userManager;
			_eligibleCostService = eligibleCostService;
			_eligibleCostBreakdownService = eligibleCostBreakdownService;
			_attachmentService = attachmentService;
		}

		/// <summary>
		/// Get the GrantApplication for the specified Id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public GrantApplication Get(int id)
		{
			var grantApplication = Get<GrantApplication>(id);

			if (!_httpContext.User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.ViewApplication))
				throw new NotAuthorizedException($"User does not have permission to access application '{grantApplication?.Id}'.");

			// This shouldn't be required to force load all these properties.
			var grantOpening = grantApplication.GrantOpening;
			var grantStream = grantApplication.GrantOpening.GrantStream;
			var grantProgram = grantApplication.GrantOpening.GrantStream.GrantProgram;
			var programConfiguration = grantApplication.GrantOpening.GrantStream.ProgramConfiguration;
			var programDescription = grantApplication.ProgramDescription;
			var trainingCost = grantApplication.TrainingCost;
			var eligibleCosts = trainingCost.EligibleCosts;
			var trainingProgram = grantApplication.TrainingPrograms;
			var trainingProvider = grantApplication.TrainingProviders;
			var x = trainingCost.EligibleCosts.Select(t => t.EligibleExpenseType).Select(t => t.ServiceCategory);
			var grantStreamEligibilityAnswers = grantApplication.GrantStreamEligibilityAnswers;

			return grantApplication;
		}

		/// <summary>
		/// Get the GrantApplication for the specified invitation key if it hasn't expired.
		/// </summary>
		/// <param name="invitationKey"></param>
		/// <returns></returns>
		public GrantApplication Get(Guid invitationKey)
		{
			if (invitationKey == Guid.Empty)
				return null;

			return _dbContext.GrantApplications
				.FirstOrDefault(o => o.InvitationKey.ToString().Equals(invitationKey.ToString(), StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Get the default application type.
		/// </summary>
		/// <returns></returns>
		public ApplicationType GetDefaultApplicationType()
		{
			return _dbContext.ApplicationTypes.FirstOrDefault(at => at.IsActive);
		}

		/// <summary>
		/// Add the specified GrantApplication to the datasource.
		/// </summary>
		/// <param name="grantApplication"></param>
		/// <returns></returns>
		public GrantApplication Add(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			if (string.IsNullOrWhiteSpace(grantApplication.FileNumber))
			{
				if (grantApplication.GrantOpening == null)
					grantApplication.GrantOpening = _dbContext.GrantOpenings.SingleOrDefault(x => x.Id == grantApplication.GrantOpeningId);
			}

			grantApplication.ApplicationStateExternal = ApplicationStateExternal.Incomplete;
			grantApplication.ApplicationStateInternal = ApplicationStateInternal.Draft;

			_dbContext.GrantApplications.Add(grantApplication);
			CommitTransaction();

			return grantApplication;
		}

		/// <summary>
		/// Update the specified GrantApplication in the datasource.
		/// </summary>
		/// <param name="grantApplication"></param>
		/// <param name="trigger"></param>
		/// <param name="canPerformAction">A custom authorization check.</param>
		/// <returns></returns>
		public GrantApplication Update(GrantApplication grantApplication, ApplicationWorkflowTrigger trigger = ApplicationWorkflowTrigger.EditApplication, Func<ApplicationWorkflowTrigger, bool> canPerformAction = null)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			if (canPerformAction == null ? !_httpContext.User.CanPerformAction(grantApplication, trigger) : !canPerformAction(trigger))
				throw new NotAuthorizedException($"User does not have permission to edit this application '{grantApplication?.Id}'.");

			// Under the condition that the user invalidates any form, then the state must return to Incomplete.
			if (grantApplication.ApplicationStateExternal.In(ApplicationStateExternal.Complete, ApplicationStateExternal.Incomplete))
				grantApplication.ApplicationStateExternal = grantApplication.IsSubmittable() ? ApplicationStateExternal.Complete : ApplicationStateExternal.Incomplete;

			// If the estimated number of participants is changed then recalculate costs.
			var originalEstimatedParticipants = (int?)_dbContext.OriginalValue(grantApplication.TrainingCost, nameof(TrainingCost.EstimatedParticipants)) ?? 0;
			if (grantApplication.TrainingCost?.EstimatedParticipants != originalEstimatedParticipants)
				grantApplication.TrainingCost.RecalculateEstimatedCostsFor(grantApplication.TrainingCost.EstimatedParticipants);

			var accountType = _httpContext.User.GetAccountType();
			if (accountType == AccountTypes.Internal)
			{
				if (_grantAgreementService.AgreementUpdateRequired(grantApplication))
					_grantAgreementService.UpdateAgreement(grantApplication);

				_noteService.GenerateUpdateNote(grantApplication);
			}

			_dbContext.Update(grantApplication);
			CommitTransaction();

			return grantApplication;
		}

		/// <summary>
		/// Delete the specified grant application from the datasource.
		/// </summary>
		/// <param name="grantApplication"></param>
		public void Delete(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			var trainingProviders = _dbContext.TrainingProviders.Where(tp => tp.GrantApplicationId == grantApplication.Id);
			foreach (var trainingProvider in trainingProviders)
			{
				if (trainingProvider.BusinessCaseDocument != null)
					_dbContext.Attachments.Remove(trainingProvider.BusinessCaseDocument);

				if (trainingProvider.CourseOutlineDocument != null)
					_dbContext.Attachments.Remove(trainingProvider.CourseOutlineDocument);

				if (trainingProvider.ProofOfQualificationsDocument != null)
					_dbContext.Attachments.Remove(trainingProvider.ProofOfQualificationsDocument);

				_dbContext.TrainingProviders.Remove(trainingProvider);
			}

			var trainingPrograms = _dbContext.TrainingPrograms.Where(tp => tp.GrantApplicationId == grantApplication.Id).Include(tp => tp.TrainingProviders);
			foreach (var trainingProgram in trainingPrograms)
			{
				var trainingProviderIds = trainingProgram.TrainingProviders.Select(tp => tp.Id).ToArray();
				var providers = _dbContext.TrainingProviders.Where(tp => trainingProviderIds.Contains(tp.Id));
				foreach (var trainingProvider in providers)
				{
					if (trainingProvider.BusinessCaseDocument != null)
						_dbContext.Attachments.Remove(trainingProvider.BusinessCaseDocument);

					if (trainingProvider.CourseOutlineDocument != null)
						_dbContext.Attachments.Remove(trainingProvider.CourseOutlineDocument);

					if (trainingProvider.ProofOfQualificationsDocument != null)
						_dbContext.Attachments.Remove(trainingProvider.ProofOfQualificationsDocument);

					_dbContext.TrainingProviders.Remove(trainingProvider);
				}
				trainingProgram.UnderRepresentedGroups.Clear();
				trainingProgram.DeliveryMethods.Clear();
				_dbContext.TrainingPrograms.Remove(trainingProgram);
			}

			if (grantApplication.ProgramDescription != null)
			{
				grantApplication.ProgramDescription.Communities.Clear();
				grantApplication.ProgramDescription.UnderRepresentedPopulations.Clear();
				grantApplication.ProgramDescription.VulnerableGroups.Clear();
				grantApplication.ProgramDescription.ParticipantEmploymentStatuses.Clear();
				_dbContext.ProgramDescriptions.Remove(grantApplication.ProgramDescription);
			}

			var eligibleCosts = _dbContext.EligibleCosts.Where(ec => ec.GrantApplicationId == grantApplication.Id).Include(ec => ec.Breakdowns);
			foreach (var eligibleCost in eligibleCosts)
			{
				var eligibleCostBreakdownIds = eligibleCost.Breakdowns.Select(tp => tp.Id).ToArray();
				var breakdowns = _dbContext.EligibleCostBreakdowns.Where(ecb => eligibleCostBreakdownIds.Contains(ecb.Id));
				foreach (var eligibleCostBreakdown in breakdowns)
				{
					_dbContext.EligibleCostBreakdowns.Remove(eligibleCostBreakdown);
				}
				_dbContext.EligibleCosts.Remove(eligibleCost);
			}

			if (grantApplication.ApplicantMailingAddress != null)
				_dbContext.ApplicationAddresses.Remove(grantApplication.ApplicantMailingAddress);
			if (grantApplication.ApplicantPhysicalAddress != null)
				_dbContext.ApplicationAddresses.Remove(grantApplication.ApplicantPhysicalAddress);
			if (grantApplication.OrganizationAddress != null)
				_dbContext.ApplicationAddresses.Remove(grantApplication.OrganizationAddress);

			var notes = _dbContext.Notes.Where(n => n.GrantApplicationId == grantApplication.Id).Include(n => n.Attachment);
			foreach (var note in notes)
			{
				if (note.Attachment != null)
					_dbContext.Attachments.Remove(note.Attachment);
				_dbContext.Notes.Remove(note);
			}

			var evaluationNotes = _dbContext.GrantApplicationEvaluationNotes.Where(n => n.GrantApplicationId == grantApplication.Id).Include(n => n.Attachment);
			foreach (var evaluationNote in evaluationNotes)
			{
				if (evaluationNote.Attachment != null)
					_dbContext.Attachments.Remove(evaluationNote.Attachment);
				_dbContext.GrantApplicationEvaluationNotes.Remove(evaluationNote);
			}

			var businessContactRoles = _dbContext.BusinessContactRoles.Where(bcr => bcr.GrantApplicationId == grantApplication.Id);
			foreach (var businessContactRole in businessContactRoles)
			{
				_dbContext.BusinessContactRoles.Remove(businessContactRole);
			}

			var stateChanges = _dbContext.GrantApplicationStateChanges.Where(sc => sc.GrantApplicationId == grantApplication.Id);
			foreach (var stateChange in stateChanges)
			{
				_dbContext.GrantApplicationStateChanges.Remove(stateChange);
			}

			var notifications = _dbContext.NotificationQueue.Where(o => o.GrantApplicationId == grantApplication.Id);
			foreach (var notification in notifications)
			{
				_dbContext.NotificationQueue.Remove(notification);
			}

			var attachmentIds = grantApplication.Attachments.Select(a => a.Id).ToArray();
			var attachments = _dbContext.Attachments.Where(a => attachmentIds.Contains(a.Id)).Include(a => a.Versions);
			foreach (var attachment in attachments)
			{
				var versions = _dbContext.VersionedAttachments.Where(va => va.AttachmentId == attachment.Id);
				foreach (var version in versions)
				{
					_dbContext.VersionedAttachments.Remove(version);
				}
				_dbContext.Attachments.Remove(attachment);
			}

			var answers = _dbContext.GrantStreamEligibilityAnswers.Where(n => n.GrantApplicationId == grantApplication.Id);
			foreach (var answer in answers)
			{
				_dbContext.GrantStreamEligibilityAnswers.Remove(answer);
			}

			var successStories = _dbContext.SuccessStories
				.Where(s => s.GrantApplicationId == grantApplication.Id)
				.Include(n => n.Documents);
			foreach (var successStory in successStories)
			{
				if (successStory.Documents != null)
					_dbContext.Attachments.RemoveRange(successStory.Documents);
				_dbContext.SuccessStories.Remove(successStory);
			}

			grantApplication.NotificationQueue.Clear();
			grantApplication.DeliveryPartnerServices.Clear();

			_dbContext.TrainingCosts.Remove(grantApplication.TrainingCost);
			_dbContext.GrantApplications.Remove(grantApplication);

			CommitTransaction();
		}

		/// <summary>
		/// Update the grant application delivery dates.
		/// </summary>
		/// <param name="grantApplication"></param>
		/// <returns></returns>
		public void UpdateDeliveryDates(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			if (!_httpContext.User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.SubmitChangeRequest))
				throw new NotAuthorizedException("The delivery dates cannot be changed at this time.");

			_grantAgreementService.UpdateAgreement(grantApplication);

			var originalStartDate = OriginalValue(grantApplication, ga => ga.StartDate);
			var originalEndDate = OriginalValue(grantApplication, ga => ga.EndDate);

			if (grantApplication.StartDate != originalStartDate)
				_noteService.AddDateChangedNote(grantApplication, "Delivery Start Date", originalStartDate,
					grantApplication.StartDate);

			if (grantApplication.EndDate != originalEndDate)
				_noteService.AddDateChangedNote(grantApplication, "Delivery End Date", originalEndDate,
					grantApplication.EndDate);

			_dbContext.Update(grantApplication);

			_dbContext.CommitTransaction();
		}

		public void SynchroniseDeliveryDatesToTraining(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			var changeRequired = false;

			var earliestTrainingStartDate = grantApplication.TrainingPrograms.Min(tp => tp.StartDate);
			var furthestTrainingEndDate = grantApplication.TrainingPrograms.Max(tp => tp.EndDate);

			if (grantApplication.StartDate != earliestTrainingStartDate)
			{
				grantApplication.StartDate = earliestTrainingStartDate;
				changeRequired = true;
			}

			if (grantApplication.EndDate != furthestTrainingEndDate.AddDays(45))
			{
				grantApplication.EndDate = furthestTrainingEndDate.AddDays(45);
				changeRequired = true;
			}

			if (!changeRequired)
				return;

			_dbContext.Update(grantApplication);
			_dbContext.CommitTransaction();
		}

		/// <summary>
		/// When changing the selected grant opening for an existing grant application there are a number of associated records that need to be updated.
		/// If the grant opening belongs to a grant program that is a WDA Service, all the expense types need to be reassigned or deleted.
		/// </summary>
		/// <param name="grantApplication"></param>
		public void ChangeGrantOpening(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			// Only allow changing the opening during application development.
			if (!grantApplication.ApplicationStateInternal.In(ApplicationStateInternal.Draft, ApplicationStateInternal.ApplicationWithdrawn, ApplicationStateInternal.Unfunded))
				throw new InvalidOperationException("Cannot change the grant opening in the current state.");

			// Training costs must be reassigned to eligible expense types for the selected grant opening.
			var eligibleExpenseTypes = grantApplication.GrantOpening.GrantStream.ProgramConfiguration.EligibleExpenseTypes.Where(eet => eet.IsActive);

			var eligibleCosts = grantApplication.TrainingCost.EligibleCosts.ToArray();
			foreach (var eligibleCost in eligibleCosts)
			{
				var currentEligibleExpenseType = _dbContext.EligibleExpenseTypes.FirstOrDefault(eet => eet.Id == eligibleCost.EligibleExpenseTypeId);
				var eligibleExpenseType = eligibleExpenseTypes.FirstOrDefault(eet => eet.ServiceCategoryId == currentEligibleExpenseType.ServiceCategoryId);
				if (eligibleExpenseType != null)
				{
					// There is a matching eligible expense type in this grant opening, it can be reassigned.
					eligibleCost.EligibleExpenseTypeId = eligibleExpenseType.Id;

					var breakdowns = eligibleCost.Breakdowns.ToArray();
					foreach (var eligibleCostBreakdown in breakdowns)
					{
						var currentEligibleExpenseBreakdown = _dbContext.EligibleExpenseBreakdowns.FirstOrDefault(eeb => eeb.Id == eligibleCostBreakdown.EligibleExpenseBreakdownId);
						var eligibleExpenseBreakdown = eligibleExpenseType.Breakdowns.FirstOrDefault(b => b.ServiceLineId == currentEligibleExpenseBreakdown.ServiceLineId && b.IsActive);

						// There is a matching eligible expense breakdown in this grant opening, it can be reassigned.
						if (eligibleExpenseBreakdown != null)
						{
							eligibleCostBreakdown.EligibleExpenseBreakdownId = eligibleExpenseBreakdown.Id;
						}
						else
						{
							// There is a no matching eligible expense breakdown in this grant opening, delete all related records.
							var trainingPrograms = grantApplication.TrainingPrograms.Where(tp => tp.EligibleCostBreakdownId == eligibleCostBreakdown.Id).ToArray();

							foreach (var trainingProgram in trainingPrograms)
							{
								var trainingProviders = trainingProgram.TrainingProviders.ToArray();
								foreach (var trainingProvider in trainingProviders)
								{
									if (trainingProvider.BusinessCaseDocumentId.HasValue && trainingProvider.BusinessCaseDocument != null)
										_dbContext.Attachments.Remove(trainingProvider.BusinessCaseDocument);
									if (trainingProvider.ProofOfQualificationsDocumentId.HasValue && trainingProvider.ProofOfQualificationsDocument != null)
										_dbContext.Attachments.Remove(trainingProvider.ProofOfQualificationsDocument);
									if (trainingProvider.CourseOutlineDocumentId.HasValue && trainingProvider.CourseOutlineDocument != null)
										_dbContext.Attachments.Remove(trainingProvider.CourseOutlineDocument);

									if (trainingProvider.TrainingAddress != null)
										_dbContext.ApplicationAddresses.Remove(trainingProvider.TrainingAddress);

									_dbContext.TrainingProviders.Remove(trainingProvider);
								}

								trainingProgram.DeliveryMethods.Clear();
								trainingProgram.UnderRepresentedGroups.Clear();
								_dbContext.TrainingPrograms.Remove(trainingProgram);
							}
							_dbContext.EligibleCostBreakdowns.Remove(eligibleCostBreakdown);
						}
					}

					// Skills training eligible costs are the sum of the breakdown.
					if (eligibleCost.EligibleExpenseType.ServiceCategory?.ServiceTypeId == ServiceTypes.SkillsTraining)
						eligibleCost.RecalculateEstimatedCost();
				}
				else
				{
					var trainingProviders = grantApplication.TrainingProviders.Where(tp => tp.EligibleCostId == eligibleCost.Id).ToArray();

					// There is no matching eligible expense type in this grant opening, delete all related records.
					var breakdowns = eligibleCost.Breakdowns.ToArray();
					foreach (var breakdown in breakdowns)
					{
						// There is a no matching eligible expense breakdown in this grant opening, delete all related records.
						var trainingPrograms = grantApplication.TrainingPrograms.Where(tp => tp.EligibleCostBreakdownId == breakdown.Id).ToArray();
						foreach (var trainingProgram in trainingPrograms)
						{
							var programTrainingProviders = trainingProgram.TrainingProviders.ToArray();
							foreach (var trainingProvider in programTrainingProviders)
							{
								if (trainingProvider.BusinessCaseDocumentId.HasValue) _dbContext.Attachments.Remove(trainingProvider.BusinessCaseDocument);
								if (trainingProvider.ProofOfQualificationsDocumentId.HasValue) _dbContext.Attachments.Remove(trainingProvider.ProofOfQualificationsDocument);
								if (trainingProvider.CourseOutlineDocumentId.HasValue) _dbContext.Attachments.Remove(trainingProvider.CourseOutlineDocument);
								_dbContext.ApplicationAddresses.Remove(trainingProvider.TrainingAddress);
								_dbContext.TrainingProviders.Remove(trainingProvider);
							}
							trainingProgram.DeliveryMethods.Clear();
							trainingProgram.UnderRepresentedGroups.Clear();
							_dbContext.TrainingPrograms.Remove(trainingProgram);
						}
						_dbContext.EligibleCostBreakdowns.Remove(breakdown);
					}
					eligibleCost.Breakdowns.Clear();
					grantApplication.TrainingCost.EligibleCosts.Remove(eligibleCost);
					_dbContext.EligibleCosts.Remove(eligibleCost);

					foreach (var trainingProvider in trainingProviders)
					{
						if (trainingProvider.BusinessCaseDocumentId.HasValue) _dbContext.Attachments.Remove(trainingProvider.BusinessCaseDocument);
						if (trainingProvider.ProofOfQualificationsDocumentId.HasValue) _dbContext.Attachments.Remove(trainingProvider.ProofOfQualificationsDocument);
						if (trainingProvider.CourseOutlineDocumentId.HasValue) _dbContext.Attachments.Remove(trainingProvider.CourseOutlineDocument);
						_dbContext.ApplicationAddresses.Remove(trainingProvider.TrainingAddress);
						_dbContext.TrainingProviders.Remove(trainingProvider);
					}
				}
			}

			grantApplication.RecalculateEstimatedCosts();

			grantApplication.TrainingCost.TrainingCostState = TrainingCostStates.Incomplete;
			grantApplication.TrainingPrograms.ForEach(tp =>
			{
				tp.TrainingProgramState = TrainingProgramStates.Incomplete;
				tp.TrainingProviders.ForEach(p => p.TrainingProviderState = TrainingProviderStates.Incomplete);
			});
			grantApplication.TrainingProviders.ForEach(p => p.TrainingProviderState = TrainingProviderStates.Incomplete);
		}

		/// <summary>
		/// Changes the current Application Administrator to the specified one.
		/// </summary>
		/// <param name="grantApplication"></param>
		/// <param name="applicantId"></param>
		public void ChangeApplicationAdministrator(GrantApplication grantApplication, int? applicantId)
		{
			var applicant = _dbContext.Users.FirstOrDefault(x => x.Id == applicantId);

			var newName = $"{applicant.FirstName} {applicant.LastName}";
			var oldName = $"{grantApplication.ApplicantFirstName} {grantApplication.ApplicantLastName}";

			// We should change this in the future to support switching a specific AA, as we can support multiple AAs.
			// I think if we are only changing the AA, this for loop should be used, foreach (var b in grantApplication.BusinessContactRoles.Where(bcr => bcr.EmployerEnrollmentId == null).ToList())
			foreach (var b in grantApplication.BusinessContactRoles)
			{
				b.UserId = applicant.Id;
				b.User = applicant;
			}

			var applicationMailAddress = grantApplication.ApplicantMailingAddress;
			var applicationPhysicalAddress = grantApplication.ApplicantPhysicalAddress;

			if (applicationMailAddress != null)
			{
				grantApplication.ApplicantMailingAddress = null;
			}
			if (applicationPhysicalAddress != null)
			{
				grantApplication.ApplicantPhysicalAddress = null;
			}
			grantApplication.CopyApplicant(applicant);

			_dbContext.Update(grantApplication);

			if (applicationMailAddress != null &&
				applicationMailAddress.Id != grantApplication.OrganizationAddressId &&
				!IsApplicantAddressInUse(grantApplication.Id, applicationMailAddress.Id))
			{
				_dbContext.ApplicationAddresses.Remove(applicationMailAddress);
			}

			if (applicationPhysicalAddress != null &&
				applicationPhysicalAddress.Id != grantApplication.OrganizationAddressId &&
				!IsApplicantAddressInUse(grantApplication.Id, applicationPhysicalAddress.Id))
			{
				_dbContext.ApplicationAddresses.Remove(applicationPhysicalAddress);
			}

			var internalUser = _userManager.FindById(_siteMinderService.CurrentUserGuid.ToString()).InternalUser;
			_noteService.AddValueChangedNote(grantApplication, "Applicant contact", oldName, newName);

			_dbContext.CommitTransaction();
		}

		public void ChangeAlternateContact(GrantApplication grantApplication, AlternateContactModel model)
		{
			if (grantApplication.AlternateFirstName != model.AlternateFirstName)
				grantApplication.AlternateFirstName = model.AlternateFirstName;

			if (grantApplication.AlternateLastName != model.AlternateLastName)
				grantApplication.AlternateLastName = model.AlternateLastName;

			if (grantApplication.AlternateEmail != model.AlternateEmail)
				grantApplication.AlternateEmail = model.AlternateEmail;

			if (grantApplication.AlternateJobTitle != model.AlternateJobTitle)
				grantApplication.AlternateJobTitle = model.AlternateJobTitle;

			if (grantApplication.AlternatePhoneNumber != model.AlternatePhoneNumber)
				grantApplication.AlternatePhoneNumber = model.AlternatePhoneNumber;

			if (grantApplication.AlternatePhoneExtension != model.AlternatePhoneExtension)
				grantApplication.AlternatePhoneExtension = model.AlternatePhoneExtension;

			grantApplication.IsAlternateContact = true;

			_noteService.GenerateUpdateNote(grantApplication, true);

			_dbContext.Update(grantApplication);
			_dbContext.CommitTransaction();
		}

		/// <summary>
		/// Assigns the specified assessor to the application as the primary assessor.
		/// </summary>
		/// <param name="grantApplication"></param>
		/// <param name="primaryAssessorId"></param>
		public void AssignPrimaryAssessor(GrantApplication grantApplication, int? primaryAssessorId)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			if (!_httpContext.User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.ReassignPrimaryAssessor))
				throw new NotAuthorizedException($"User does not have permission to assign an assessor to application '{grantApplication.Id}'.");

			if (primaryAssessorId == null)
				return;

			if (primaryAssessorId == grantApplication.PrimaryAssessorId)
				return;

			grantApplication.PrimaryAssessor = _dbContext.InternalUsers.Find(primaryAssessorId);

			var noteContent = $"{grantApplication.PrimaryAssessor.FirstName} {grantApplication.PrimaryAssessor.LastName} assigned as primary assessor";
			_noteService.AddNote(grantApplication, NoteTypes.AA, noteContent);
			_dbContext.CommitTransaction();
		}

		/// <summary>
		/// Assigns the specified assessor to the application.
		/// </summary>
		/// <param name="grantApplication"></param>
		/// <param name="assessorId"></param>
		public void AssignAssessor(GrantApplication grantApplication, int? assessorId)
		{
			if (grantApplication == null) throw new ArgumentNullException(nameof(grantApplication));

			if (!_httpContext.User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.ReassignAssessor))
			{
				throw new NotAuthorizedException($"User does not have permission to assign an assessor to application '{grantApplication.Id}'.");
			}

			if (assessorId == null)
			{
				grantApplication.RemoveAssignedAssessor();
			}
			else
			{
				grantApplication.AssessorId = assessorId;
				grantApplication.Assessor = _dbContext.InternalUsers.Find(assessorId);
			}

			//var internalUser = _userManager.FindById(_siteMinderService.CurrentUserGuid.ToString()).InternalUser;
			var noteContent = (grantApplication.Assessor == null) ? "Assessor unassigned" : $"{grantApplication.Assessor.FirstName} {grantApplication.Assessor.LastName} assigned as assessor";
			_noteService.AddNote(grantApplication, NoteTypes.AA, noteContent);
			_dbContext.CommitTransaction();
		}

		/// <summary>
		/// Change grant application assess as null when the assessor is inactive
		/// </summary>
		/// <param name="assessorId"></param>
		/// <returns></returns>
		public void UnassignAssessor(int assessorId)
		{
			var grantApplications = _dbContext.GrantApplications.Where(x => x.AssessorId == assessorId).ToArray();
			for (var i = 0; i < grantApplications.Count(); i++)
			{
				grantApplications[i].RemoveAssignedAssessor();
				_dbContext.Update(grantApplications[i]);
			}
			_dbContext.CommitTransaction();
		}

		/// <summary>
		/// Return List of Users excluding current one.
		/// </summary>
		/// <param name="grantApplication"></param>
		/// <returns></returns>
		public List<User> GetAvailableApplicationContacts(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			var appCurrentUserId = grantApplication.BusinessContactRoles.FirstOrDefault()?.UserId;
			var orgId = grantApplication.OrganizationId;

			return _dbContext.Users.Where(x => x.OrganizationId == orgId &&
												x.Id != appCurrentUserId).ToList();
		}

		/// <summary>
		/// Get the total number of GrantApplication objects in the datasource for the specified <typeparamref name="int"/>.
		/// </summary>
		/// <param name="trainingProviderInventoryId"></param>
		/// <returns></returns>
		public int GetTotalGrantApplications(int trainingProviderInventoryId)
		{
			var defaultGrantProgramId = GetDefaultGrantProgramId();

			var grantApplicationWithTrainingProviders = _dbContext.GrantApplications
				.Where(ga => ga.GrantOpening.GrantStream.GrantProgram.Id == defaultGrantProgramId)
				.Where(ga => ga.TrainingProviders
					.Any(tp => tp.TrainingProviderInventoryId == trainingProviderInventoryId)
				);

			var grantApplicationWithTrainingProgramProviders = _dbContext.GrantApplications
				.Where(ga => ga.GrantOpening.GrantStream.GrantProgram.Id == defaultGrantProgramId)
				.Where(ga => ga.TrainingPrograms
					.Any(tp => tp.TrainingProviders
						.Any(tpp => tpp.TrainingProviderInventoryId == trainingProviderInventoryId)
					)
				);

			return grantApplicationWithTrainingProviders.Union(grantApplicationWithTrainingProgramProviders).Distinct().Count();
		}

		public int GetApplicationsCountByFiscal(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			return GetGrantApplicationsForSummary(grantApplication).Count();
		}

		public decimal GetApplicationsCostByFiscal(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			return GetGrantApplicationsForSummary(grantApplication)
				.ToList()
				.Sum(y => y.TrainingCost.TotalAgreedMaxCost);
		}

		private IQueryable<GrantApplication> GetGrantApplicationsForSummary(GrantApplication grantApplication)
		{
			var states = StateExtensions.GetInternalStatesForSummary();
			return _dbContext.GrantApplications
				.Where(x => x.GrantOpening.GrantStream.GrantProgramId == grantApplication.GrantOpening.GrantStream.GrantProgramId
				            && x.OrganizationId == grantApplication.OrganizationId
				            && x.GrantOpening.TrainingPeriod.FiscalYearId == grantApplication.GrantOpening.TrainingPeriod.FiscalYearId
				            && states.Contains(x.ApplicationStateInternal));
		}

		public GrantApplicationStateChange GetStateChange(int grantApplicationId, ApplicationStateInternal state)
		{
			return Get(grantApplicationId).GetStateChange(state);
		}

		/// <summary>
		/// Returns an array of application internal states, filtered by the isActive argument.
		/// </summary>
		/// <param name="isActive"></param>
		/// <returns></returns>
		public IEnumerable<GrantApplicationInternalState> GetInternalStates(bool? isActive)
		{
			return _dbContext.GrantApplicationInternalStates.Where(s => s.IsActive == (isActive ?? true)).ToArray();
		}

		/// <summary>
		/// Enables or disables the specified grant applications scheduled notifications.
		/// </summary>
		/// <param name="grantApplication"></param>
		/// <param name="enable"></param>
		public void EnableScheduledNotifications(GrantApplication grantApplication, bool enable = true)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			var dbGrantApplication = _dbContext.GrantApplications.FirstOrDefault(ga => ga.Id == grantApplication.Id);
			if (dbGrantApplication == null)
				throw new ArgumentNullException(nameof(dbGrantApplication));

			dbGrantApplication.ScheduledNotificationsEnabled = enable;
		}

		#region Filter Grant Applications
		/// <summary>
		/// Find all the grant applications in the specified states and filter.
		/// </summary>
		/// <param name="page"></param>
		/// <param name="quantity"></param>
		/// <param name="filter"></param>
		/// <returns></returns>
		public PageList<GrantApplication> GetGrantApplications(int page, int quantity, ApplicationFilter filter)
		{
			if (page <= 0)
				page = 1;

			if (quantity <= 0 || quantity > 100)
				quantity = 10;

			var query = _dbContext.GrantApplications.Where(ga => true);

			if (filter.States?.Length > 0)
			{
				var include = filter.States
					.Where(s => s.Include)
					.Select(s => s.State)
					.ToArray();

				if (include.Length > 0)
				{
					var showReturnedToDraft = include.Contains(ApplicationStateInternal.ReturnedToDraft);

					if (!showReturnedToDraft)
						query = query.Where(ga => include.Contains(ga.ApplicationStateInternal));
					else
						query = query.Where(ga => include.Contains(ga.ApplicationStateInternal)
						                          || ga.ApplicationStateInternal == ApplicationStateInternal.Draft && ga.ReturnedToDraft != null);
				}

				var exclude = filter.States.Where(s => !s.Include)
					.Select(s => s.State)
					.ToArray();

				if (exclude.Length > 0)
					query = query.Where(ga => !exclude.Contains(ga.ApplicationStateInternal)
					                          || ga.ApplicationStateInternal == ApplicationStateInternal.Draft && ga.ReturnedToDraft != null);
			}

			if (!string.IsNullOrWhiteSpace(filter.FileNumber))
				query = query.Where(ga => ga.FileNumber.Contains(filter.FileNumber));

			if (filter.AssessorId.HasValue)
				query = query.Where(ga => ga.AssessorId == filter.AssessorId || ga.PrimaryAssessorId == filter.AssessorId);

			if (filter.FiscalYearId.HasValue)
				query = query.Where(ga => ga.GrantOpening.TrainingPeriod.FiscalYearId == filter.FiscalYearId);

			if (!string.IsNullOrWhiteSpace(filter.TrainingPeriodCaption))
				query = query.Where(ga => ga.GrantOpening.TrainingPeriod.Caption == filter.TrainingPeriodCaption.Trim());

			if (filter.GrantStreamId.HasValue)
				query = query.Where(ga => ga.GrantOpening.GrantStreamId == filter.GrantStreamId);

			if (!filter.GrantProgramId.HasValue || filter.GrantProgramId.Value == 0)
				filter.GrantProgramId = GetDefaultGrantProgramId();

			query = query.Where(ga => ga.GrantOpening.GrantStream.GrantProgramId == filter.GrantProgramId);

			if (!string.IsNullOrWhiteSpace(filter.Applicant))
				query = query.Where(ga => ga.OrganizationLegalName.Contains(filter.Applicant));

			if (filter.IsAssigned.HasValue)
			{
				if (filter.IsAssigned.Value)
					query = query.Where(ga => ga.AssessorId != null);

				if (!filter.IsAssigned.Value)
					query = query.Where(ga => ga.AssessorId == null);
			}

			var total = query.Count();
			if (filter.OrderBy?.Any(x => x.Contains(nameof(GrantApplication.StateChanges))) ?? false)
			{
				if (filter.OrderBy.Any(x => x.Contains("desc")))
					query = query.Distinct()
						.OrderByDescending(g => g.StateChanges.OrderByDescending(sc => sc.ChangedDate).FirstOrDefault().ChangedDate)
						.Skip((page - 1) * quantity)
						.Take(quantity);
				else
					query = query.Distinct()
						.OrderBy(g =>g.StateChanges.OrderByDescending(sc => sc.ChangedDate).FirstOrDefault().ChangedDate)
						.Skip((page - 1) * quantity)
						.Take(quantity);
			}
			else
			{
				var orderBy = filter.OrderBy != null && filter.OrderBy.Length > 0 ? filter.OrderBy : new[] { $"{nameof(GrantApplication.DateSubmitted)} desc" };

				if (filter.OrderBy != null && filter.OrderBy[0].StartsWith("TrainingStartDate"))
				{
					query = query.ToList()
						.AsQueryable();

					var sortDesc = filter.OrderBy[0].Contains("desc");
					query = query.OrderByDynamic(f => f.TrainingStartDate, !sortDesc);
				}
				else
				{
					query = query.OrderByProperty(orderBy);
				}

				query = query
					.Skip((page - 1) * quantity)
					.Take(quantity);
			}

			return new PageList<GrantApplication>(page, quantity, total, query.ToArray());
		}

		public PageList<GrantApplication> GetGrantApplications(int trainingProviderInventoryId, int page, int quantity, string search)
		{
			var defaultGrantProgramId = GetDefaultGrantProgramId();
			var grantApplicationWithTrainingProviders = _dbContext.GrantApplications
				.Where(ga => ga.GrantOpening.GrantStream.GrantProgram.Id == defaultGrantProgramId)
				.Where(ga => ga.TrainingProviders.Any(tp => tp.TrainingProviderInventoryId == trainingProviderInventoryId)
				);

			var grantApplicationWithTrainingProgramProviders = _dbContext.GrantApplications
					 .Where(ga => ga.GrantOpening.GrantStream.GrantProgram.Id == defaultGrantProgramId)
					 .Where(ga => ga.TrainingPrograms
						 .Any(tp => tp.TrainingProviders
						 .Any(tpp => tpp.TrainingProviderInventoryId == trainingProviderInventoryId)
					 )
				 );

			var filtered = grantApplicationWithTrainingProviders.Union(grantApplicationWithTrainingProgramProviders).Distinct()
				.Where(x => string.IsNullOrEmpty(search) || x.FileNumber.Contains(search) || x.OrganizationLegalName != null && x.OrganizationLegalName.Contains(search))
				.OrderBy(o => o.FileNumber);


			var total = filtered.Count();
			var result = filtered
				.Skip((page - 1) * quantity)
				.Take(quantity);
			
			return new PageList<GrantApplication>(page, quantity, total, result.ToArray());
		}

		public PageList<GrantApplication> GetGrantApplicationsForOrg(int orgId, int page, int quantity, int grantProgramId, string search)
		{
			var grantApplications =
				_dbContext.GrantApplications
					.Where(ga => ga.OrganizationId == orgId && ga.ApplicationStateInternal != ApplicationStateInternal.Draft)
					.OrderBy(o => o.FileNumber);

			if (grantProgramId == 0)
				grantProgramId = GetDefaultGrantProgramId();

			var filtered = grantApplications
				.Where(x => (grantProgramId == 0 || x.GrantOpening.GrantStream.GrantProgramId == grantProgramId)
				            && (string.IsNullOrEmpty(search) ||
				                x.FileNumber != null && x.FileNumber.Contains(search) ||
				                x.ApplicantFirstName.Contains(search) || x.ApplicantLastName.Contains(search))
				).OrderBy(x => x.FileNumber);

			var total = filtered.Count();
			var result = filtered.Skip((page - 1) * quantity).Take(quantity);

			return new PageList<GrantApplication>(page, quantity, total, result.ToArray());
		}

		public IEnumerable<GrantApplication> GetGrantApplications(StreamAgreementDetailsFilterModel filter)
		{
			var grantProgramId = GetDefaultGrantProgramId();

			var query = _dbContext.GrantApplications
				.Where(ga => ga.GrantOpening.GrantStream.GrantProgramId == grantProgramId)
				.Where(ga => ga.ApplicationStateInternal != ApplicationStateInternal.Draft);

			if (!string.IsNullOrWhiteSpace(filter.Keywords))
			{
				var keywords = filter.Keywords.ToLower().Trim();

				query = query.Where(ga => ga.FileNumber.Contains(keywords)
				                          || ga.Organization.LegalName.Contains(keywords)
				                          || ga.ProgramDescription.Description.Contains(keywords)
				                          || ga.ProgramDescription.PubliclyAvailableDescription.Contains(keywords)
				                          || ga.TrainingProviders.Any(p => p.Name.Contains(keywords))
				                          || ga.TrainingPrograms.Any(p => p.CourseTitle.Contains(keywords))
				                          || ga.TrainingPrograms.Any(t => t.TrainingProviders.Any(tp => tp.Name.Contains(keywords)))
				);
			}

			if (filter.ApplicationStatuses.Any())
			{
				var statuses = filter.ApplicationStatuses.Select(s => (ApplicationStateInternal)s);
				query = query.Where(ga => statuses.Contains(ga.ApplicationStateInternal));
			}

			if (filter.CourseTitles.Any())
			{
				var courseTitles = filter.CourseTitles.Select(c => c.Trim());
				query = query.Where(ga => ga.TrainingPrograms.Any(t => courseTitles.Contains(t.CourseTitle)));
			}

			if (!string.IsNullOrWhiteSpace(filter.AgreementHolder))
				query = query.Where(ga => ga.OrganizationLegalName.Contains(filter.AgreementHolder));

			if (filter.FiscalYearId.HasValue)
				query = query.Where(ga => ga.GrantOpening.TrainingPeriod.FiscalYearId == filter.FiscalYearId);

			if (filter.GrantStreamIds.Any())
			{
				query = query.Where(ga => filter.GrantStreamIds.Contains(ga.GrantOpening.GrantStreamId));
			}

			if (filter.Intake.Any())
			{
				var intakePeriods = filter.Intake.Select(c => c.Trim());
				query = query.Where(ga => intakePeriods.Contains(ga.GrantOpening.TrainingPeriod.Caption));
			}

			if (filter.RegionNames.Any())
			{
				var regionNames = filter.RegionNames.Select(r => r.Trim());
				var communityIds = _dbContext.Communities.Where(c => regionNames.Any(r => c.Caption.Contains(r)))
					.Select(c => c.Id)
					.ToList();
				query = query.Where(ga => ga.ProgramDescription.Communities.Any(c => communityIds.Contains(c.Id)));
			}

			if (filter.CommunityId.HasValue)
				query = query.Where(ga => ga.ProgramDescription.Communities.Any(c => c.Id == filter.CommunityId.Value));

			if (filter.NocId.HasValue)
				query = query.Where(ga => ga.ProgramDescription.TargetNOCId == filter.NocId.Value);

			if (!string.IsNullOrWhiteSpace(filter.TrainingLocationName))
			{
				query = query
					.Where(ga => ga.TrainingPrograms
						.Any(tp => tp.TrainingProviders
							.Any(p => p.TrainingAddress != null && p.TrainingAddress.City.Contains(filter.TrainingLocationName.Trim()))));
			}

			if (!string.IsNullOrWhiteSpace(filter.TrainingProvider))
				query = query.Where(ga => ga.TrainingPrograms.Any(tp => tp.TrainingProviders.Any(p => p.Name.Contains(filter.TrainingProvider))));

			query = query.OrderByDescending(g => g.DateSubmitted);

			//var total = query.Count();
			//var orderBy = filter.OrderBy != null && filter.OrderBy.Length > 0 ? filter.OrderBy : new[] { $"{nameof(GrantApplication.DateSubmitted)} desc" };
			//var specialOrders = new List<string> { "TrainingStartDate", "TrainingProviderName", "TrainingProgramName" };
			//var filterField = filter.OrderBy?[0].Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

			//if (!string.IsNullOrWhiteSpace(filterField) && specialOrders.Contains(filterField))
			//{
			//	query = query.ToList()
			//		.AsQueryable();

			//	var sortDesc = filter.OrderBy[0].Contains("desc");

			//	switch (filterField)
			//	{
			//		case "TrainingStartDate":
			//			query = query.OrderByDynamic(f => f.TrainingStartDate, !sortDesc);
			//			break;

			//		case "TrainingProviderName":
			//			query = query.OrderByDynamic(f => f.TrainingPrograms.OrderBy(tp => tp.StartDate).FirstOrDefault().TrainingProvider.Name, !sortDesc);
			//			break;

			//		case "TrainingProgramName":
			//			query = query.OrderByDynamic(f => f.TrainingPrograms.OrderBy(tp => tp.StartDate).FirstOrDefault().CourseTitle, !sortDesc);
			//			break;

			//		case "TrainingCostRequested":
			//			query = query.OrderByDynamic(f => f.TrainingPrograms.OrderBy(tp => tp.StartDate).FirstOrDefault().CourseTitle, !sortDesc);
			//			break;
			//	}
			//}
			//else
			//{
			//	query = query.OrderByProperty(orderBy);
			//}

			//query = query
			//	.Skip((page - 1) * quantity)
			//	.Take(quantity);

			return query.AsEnumerable();
		}

		public int GetTotalGrantApplications(List<ApplicationStateInternal> applicationStates, int assessorId, int grantOpeningId,
			int fiscalYearId, int intakePeriodId, int grantProgramId, int grantStreamId, string fileNumber, string applicant)
		{
			if (applicationStates.Count == 0)
			{
				applicationStates =
					Enum.GetValues(typeof(ApplicationStateInternal))
						.Cast<ApplicationStateInternal>()
						.Where(w => w != ApplicationStateInternal.Draft)
						.ToList();
			}

			var filteredApplications = _dbContext.GrantApplications
				.Where(x => applicationStates.Count == 0 || applicationStates.Contains(x.ApplicationStateInternal));

			if (assessorId > 0)
			{
				filteredApplications = filteredApplications.Where(x => x.AssessorId == assessorId);
			}

			if (grantOpeningId > 0)
			{
				filteredApplications = filteredApplications.Where(x => x.GrantOpeningId == grantOpeningId);
			}

			if (fiscalYearId > 0)
			{
				if (intakePeriodId > 0)
				{
					filteredApplications = filteredApplications.Where(x => x.GrantOpening.TrainingPeriodId == intakePeriodId);
				}
				else
				{
					filteredApplications = filteredApplications.Where(x => x.GrantOpening.TrainingPeriod.FiscalYearId == fiscalYearId);
				}
			}

			if (grantProgramId > 0)
			{
				if (grantStreamId > 0)
				{
					filteredApplications = filteredApplications.Where(x => x.GrantOpening.GrantStreamId == grantStreamId);
				}
				else
				{
					filteredApplications = filteredApplications.Where(x => x.GrantOpening.GrantStream.GrantProgramId == grantProgramId);
				}
			}

			if (!string.IsNullOrEmpty(fileNumber))
			{
				filteredApplications = filteredApplications.Where(x => x.FileNumber == fileNumber);
			}

			if (!string.IsNullOrEmpty(applicant))
			{
				filteredApplications = filteredApplications.Where(x => x.Organization.LegalName.Contains(applicant));
			}

			return filteredApplications.Count();
		}

		/// <summary>
		/// Get grant applications.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="page"></param>
		/// <param name="quantity"></param>
		/// <param name="orderByExpression"></param>
		/// <returns></returns>
		public PageList<GrantApplication> GetGrantApplications<TKey>(User user, int page, int quantity, Expression<Func<GrantApplication, TKey>> orderByExpression)
		{
			if (user == null)
				throw new ArgumentNullException(nameof(user));

			var defaultGrantProgramId = GetDefaultGrantProgramId();
			var filtered =
				  _dbContext.BusinessContactRoles
					.Include(bcr => bcr.GrantApplication)
					.Where(bcr => bcr.UserId == user.Id)
					.Where(bcr => bcr.GrantApplication.GrantOpening.GrantStream.GrantProgram.Id == defaultGrantProgramId)
					.Select(bcr => bcr.GrantApplication)
					.Distinct()
					.OrderByDynamic(orderByExpression, false);

			var total = filtered.Count();
			var result = filtered.Skip((page - 1) * quantity).Take(quantity);
			return new PageList<GrantApplication>(page, quantity, total, result.ToArray());
		}
		#endregion

		#region Workflow
		/// <summary>
		/// Submit the GrantApplication.
		/// </summary>
		/// <param name="grantApplication"></param>
		public void Submit(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			if (!_httpContext.User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.SubmitApplication))
				throw new NotAuthorizedException($"User does not have permission to access Grant Application '{grantApplication?.Id}'.");

			User currentUser = _userService.GetUser(_siteMinderService.CurrentUserGuid);

			if (_userService.SyncOrganizationFromBCeIDAccount(currentUser))
			{
				grantApplication.OrganizationLegalName = currentUser.Organization.LegalName;
				grantApplication.OrganizationDoingBusinessAs = currentUser.Organization.DoingBusinessAs;
			}

			if (string.IsNullOrWhiteSpace(grantApplication.FileNumber))
				grantApplication.FileNumber = GetNextFileNumber(grantApplication.GrantOpening.TrainingPeriod.FiscalYearId);

			if (!grantApplication.IsFileNumberWithinFiscalYear())
				grantApplication.FileNumber = GetNextFileNumber(grantApplication.GrantOpening.TrainingPeriod.FiscalYearId);

			CreateWorkflowStateMachine(grantApplication).SubmitApplication();
		}

		/// <summary>
		/// Withdraw the GrantApplication for the specified Id and include the reason.
		/// </summary>
		/// <param name="grantApplication"></param>
		/// <param name="reason"></param>
		public void Withdraw(GrantApplication grantApplication, string reason)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).WithdrawApplication(reason);
		}

		/// <summary>
		/// Restart the GrantApplication, create a new GrantApplication based on the data in the withdrawn app
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public int RestartApplicationFromWithdrawn(int id)
		{
			var withdrawnApp = Get(id);
			var newApplication = withdrawnApp.Clone();

			newApplication.FileNumber = null;
			newApplication.ApplicationStateExternal = ApplicationStateExternal.Incomplete;
			newApplication.ApplicationStateInternal = ApplicationStateInternal.Draft;
			newApplication.DateSubmitted = null;
			newApplication.DateCancelled = null;
			newApplication.DateUpdated = null;
			newApplication.AssessorId = null;

			newApplication.ApplicantMailingAddress = new ApplicationAddress(withdrawnApp.ApplicantMailingAddress);
			newApplication.ApplicantPhysicalAddress = new ApplicationAddress(withdrawnApp.ApplicantPhysicalAddress);
			newApplication.OrganizationAddress = new ApplicationAddress(withdrawnApp.OrganizationAddress);

			foreach (var businessContactRole in withdrawnApp.BusinessContactRoles)
				newApplication.BusinessContactRoles.Add(new BusinessContactRole { UserId = businessContactRole.UserId });

			var startDateOverride = withdrawnApp.StartDate;
			var endDateOverride = withdrawnApp.EndDate;

			// Will the GrantApplication method block this successfully saving due to an invalid GrantOpening or Start and End dates?
			var forceNewGrantOpening = newApplication.GrantOpening.State.In(GrantOpeningStates.Closed, GrantOpeningStates.Scheduled, GrantOpeningStates.Unscheduled)
			                            || !newApplication.HasValidDates();

			var foundFutureGrantOpening = false;
			if (forceNewGrantOpening)
			{
				var nextGrantOpening = _grantOpeningService.GetCurrentGrantOpening(withdrawnApp.GrantOpening.Id);

				// If we found no future opening, don't bother setting the dates, and let the validation fail downstream
				if (nextGrantOpening != null)
				{
					foundFutureGrantOpening = true;
					newApplication.GrantOpening = nextGrantOpening;

					var now = AppDateTime.UtcNow;

					startDateOverride = nextGrantOpening.TrainingPeriod.StartDate;

					if (now > startDateOverride)
						startDateOverride = now;

					endDateOverride = startDateOverride.AddDays(1);

					if (endDateOverride > nextGrantOpening.TrainingPeriod.EndDate)
						endDateOverride = nextGrantOpening.TrainingPeriod.EndDate;
				}
			}

			newApplication.StartDate = startDateOverride;
			newApplication.EndDate = endDateOverride;

			//add GrantApplication to the database
			newApplication = Add(newApplication);

			//clone ProgramDescription
			var programDescription = new ProgramDescription(newApplication);
			programDescription.Clone(withdrawnApp.ProgramDescription);
			programDescription.Description = "created from " + programDescription.Description;

			//add ProgramDescription to the database
			var programDescriptionService = new ProgramDescriptionService(_dbContext, _httpContext, _logger);
			programDescriptionService.Add(programDescription);

			newApplication.ProgramDescription = programDescription;
			newApplication.ProgramDescription.DescriptionState = ProgramDescriptionStates.Incomplete;

			newApplication.ProgramInitiativeId = withdrawnApp.ProgramInitiativeId;

			// Training
			newApplication.TrainingCost = new TrainingCost(newApplication, withdrawnApp.TrainingCost.EstimatedParticipants);
			newApplication.TrainingCost.Clone(withdrawnApp.TrainingCost);

			// TrainingPrograms - should be injected/resolved, but we get a circular dependency issue (possibly break this RestartApplicationFromWithdrawn method into it's own service?)
			_trainingProgramService = new TrainingProgramService(this, _grantAgreementService, _noteService, _dbContext, _httpContext, _logger);
			
			// TrainingProviders - should be injected/resolved, but we get a circular dependency issue
			_trainingProviderService = new TrainingProviderService(this, null, null, _noteService, _dbContext, _httpContext, _logger);

			// Get a list of the costs from the withdrawn app
			var eligibleCosts = _eligibleCostService.GetForGrantApplication(withdrawnApp.Id).ToList();

			decimal totalEstimateCost = 0;
			decimal totalEstimatedReimbursement = 0;

			foreach (var eligibleCost in eligibleCosts)
			{
				var clonedEligibleCost = new EligibleCost();
				clonedEligibleCost.Clone(eligibleCost);

				totalEstimateCost += clonedEligibleCost.EstimatedCost;
				totalEstimatedReimbursement += clonedEligibleCost.EstimatedReimbursement;

				clonedEligibleCost.GrantApplicationId = newApplication.Id;
				clonedEligibleCost.TrainingCost = newApplication.TrainingCost;
				clonedEligibleCost.TrainingCost.TotalEstimatedCost = totalEstimateCost;
				clonedEligibleCost.TrainingCost.TotalEstimatedReimbursement = totalEstimatedReimbursement;

				_eligibleCostService.Add(clonedEligibleCost);

				// Check for Service Providers, and clone any that are found
				if (eligibleCost.TrainingProviders.Any())
				{
					foreach (var serviceProvider in eligibleCost.TrainingProviders)
					{
						var newServiceProvider = CloneFullTrainingProviderRecord(newApplication, serviceProvider, clonedEligibleCost, _attachmentService);
						newServiceProvider.EligibleCost = clonedEligibleCost;
						newServiceProvider.EligibleCostId = clonedEligibleCost.Id;

						//clonedEligibleCost.TrainingProviders.Add(newServiceProvider);
						_trainingProviderService.Add(newServiceProvider);
					}
				}

				// Cost Breakdowns
				foreach (var eligibleCostBreakdown in eligibleCost.Breakdowns)
				{
					var clonedEligibleCostBreakdown = new EligibleCostBreakdown();
					clonedEligibleCostBreakdown.Clone(eligibleCostBreakdown);
					clonedEligibleCostBreakdown.EligibleCost = clonedEligibleCost;
					clonedEligibleCostBreakdown.EligibleCostId = clonedEligibleCost.Id;
					
					_eligibleCostBreakdownService.Add(clonedEligibleCostBreakdown);

					// Is there a TrainingProgram associated to the cost breakdown
					var trainingProgram = withdrawnApp.TrainingPrograms.FirstOrDefault(f => f.EligibleCostBreakdownId == eligibleCostBreakdown.Id);
					if (trainingProgram == null)
						continue;

					// Clone Training program
					var newTrainingProgram = new TrainingProgram(newApplication);
					newTrainingProgram.Clone(trainingProgram);

					if (foundFutureGrantOpening)
					{
						// If we found a future GrantOpening, make sure to use the overridden start/end dates if needed, otherwise we get a Validation issue
						newTrainingProgram.StartDate = startDateOverride;
						newTrainingProgram.EndDate = endDateOverride;
					}

					newTrainingProgram.EligibleCostBreakdownId = clonedEligibleCostBreakdown.Id;

					_trainingProgramService.Add(newTrainingProgram);

					// Clone Training providers
					foreach (var trainingProvider in trainingProgram.TrainingProviders)
					{
						var newTrainingProvider = CloneFullTrainingProviderRecord(newApplication, trainingProvider, clonedEligibleCost, _attachmentService);

						//link the training provider to the training program
						newTrainingProvider.TrainingPrograms.Add(newTrainingProgram);

						//add the training provider
						_trainingProviderService.Add(newTrainingProvider);
					}
				}
			}

			// Clone Grant Application Attachments
			foreach (var existingAttachment in withdrawnApp.Attachments)
			{
				var newAttachment = existingAttachment.Clone();
				newApplication.Attachments.Add(newAttachment);
				
				_attachmentService.Add(newAttachment, commit: true);
			}

			return newApplication.Id;
		}

		private static TrainingProvider CloneFullTrainingProviderRecord(GrantApplication grantApp, TrainingProvider trainingProviderBeingCloned, EligibleCost ec, IAttachmentService attachmentService)
		{
			var newProvider = new TrainingProvider();
			newProvider.Clone(trainingProviderBeingCloned);

			// Clone the documents
			if (trainingProviderBeingCloned.BusinessCaseDocument != null)
			{
				var doc = new Attachment(trainingProviderBeingCloned.BusinessCaseDocument);
				attachmentService.Add(doc);

				newProvider.BusinessCaseDocument = doc;
				newProvider.BusinessCaseDocumentId = doc.Id;
			}

			if (trainingProviderBeingCloned.ProofOfQualificationsDocument != null)
			{
				var doc = new Attachment(trainingProviderBeingCloned.ProofOfQualificationsDocument);
				attachmentService.Add(doc);

				newProvider.ProofOfQualificationsDocument = doc;
				newProvider.ProofOfQualificationsDocumentId = doc.Id;
			}

			if (trainingProviderBeingCloned.CourseOutlineDocument != null)
			{
				var doc = new Attachment(trainingProviderBeingCloned.CourseOutlineDocument);
				attachmentService.Add(doc);

				newProvider.CourseOutlineDocument = doc;
				newProvider.CourseOutlineDocumentId = doc.Id;
			}

			if (trainingProviderBeingCloned.TrainingAddress != null)
				newProvider.TrainingAddress = new ApplicationAddress(trainingProviderBeingCloned.TrainingAddress);

			if (trainingProviderBeingCloned.TrainingProviderAddress != null)
				newProvider.TrainingProviderAddress = new ApplicationAddress(trainingProviderBeingCloned.TrainingProviderAddress);

			newProvider.TrainingProviderInventoryId = null;
			newProvider.GrantApplication = grantApp;
			newProvider.GrantApplicationId = grantApp.Id;
			newProvider.EligibleCost = ec;
			newProvider.EligibleCostId = ec.Id;

			return newProvider;
		}

		public void SelectForAssessment(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).SelectForAssessment();
		}

		public void BeginAssessment(GrantApplication grantApplication, int internalUserId)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			var internalUser = _userService.GetInternalUser(internalUserId);

			CreateWorkflowStateMachine(grantApplication).BeginAssessment(internalUser);
		}

		public void RemoveFromAssessment(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).RemoveFromAssessment();
		}

		public void RecommendForApproval(GrantApplication grantApplication)
		{
			if (grantApplication == null) throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).RecommendForApproval();
		}

		public void RecommendForDenial(GrantApplication grantApplication, string reason = null)
		{
			if (grantApplication == null) throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).RecommendForDenial(reason);
		}

		public void EditRecommendForDenialReason(GrantApplication grantApplication, string reason = null)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).EditRecommendForDenialReason(reason);
		}

		public void IssueOffer(GrantApplication grantApplication)
		{
			if (grantApplication == null) throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).IssueOffer();
		}

		public void ReturnOfferToAssessment(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).ReturnOfferToAssessment();
		}

		public void ReturnToAssessment(GrantApplication grantApplication, string reason = null)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).ReturnToAssessment(reason);
		}

		public void ReturnToDraft(GrantApplication grantApplication, string reason = null)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).ReturnUnderAssessmentToDraft(reason);
		}

		public void RecommendChangeForApproval(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));
			var claimType = grantApplication.GrantOpening.GrantStream.ProgramConfiguration.ClaimTypeId;
			if (claimType == ClaimTypes.SingleAmendableClaim)
			{
				foreach (var trainingProgram in grantApplication.TrainingPrograms)
				{
					var requestProvider = trainingProgram.RequestedTrainingProvider;
					if (requestProvider == null)
						throw new InvalidOperationException("Cannot approve change request when there are no requested training providers.");
				}
			}
			else if (claimType == ClaimTypes.MultipleClaimsWithoutAmendments)
			{
				if (!grantApplication.CanApproveChangeRequest())
				{
					throw new InvalidOperationException($"Click 'Recommend Change For Approval' after one of the request skill training provider or service provider is 'For Approval'.");
				}
			}
			CreateWorkflowStateMachine(grantApplication).RecommendChangeForApproval();
		}

		public void RecommendChangeForDenial(GrantApplication grantApplication, string reason = null)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			var claimType = grantApplication.GrantOpening.GrantStream.ProgramConfiguration.ClaimTypeId;
			if (claimType == ClaimTypes.SingleAmendableClaim)
			{
				foreach (var trainingProgram in grantApplication.TrainingPrograms)
				{
					var requestProvider = trainingProgram.RequestedTrainingProvider;
					if (requestProvider == null)
						throw new NoContentException(nameof(requestProvider));
				}
			}
			else if (claimType == ClaimTypes.MultipleClaimsWithoutAmendments)
			{
				if (!grantApplication.CanDenyChangeRequest())
				{
					throw new InvalidOperationException($"Click 'Recommend Change For Denial' after all request skill training providers and service providers are 'For Denial'.");
				}

			}

			CreateWorkflowStateMachine(grantApplication).RecommendChangeForDenial(reason);
		}

		public void DenyApplication(GrantApplication grantApplication, string reason)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).DenyApplication(reason);
		}

		public void WithdrawOffer(GrantApplication grantApplication, string reason)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).WithdrawOffer(reason);
		}

		public void ReturnWithdrawnOfferToAssessment(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).ReturnWithdrawnOfferToAssessment();
		}

		public void SubmitChangeRequest(GrantApplication grantApplication, string reason)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).SubmitChangeRequest(reason);
		}

		public void ReturnUnfunded(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).ReturnUnfundedApplications();
		}

		public void ReturnUnassessed(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).ReturnApplicationUnassessed();
		}

		public void ReturnUnassessedToNew(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).ReturnApplicationUnassessedToNew();
		}

		public void ApproveChangeRequest(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).ApproveChangeRequest();
		}

		public void DenyChangeRequest(GrantApplication grantApplication, string reason)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).DenyChangeRequest(reason);
		}

		public void CancelApplicationAgreement(GrantApplication grantApplication, string reason)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).CancelAgreementMinistry(reason);
		}

		public void ReverseCancelledApplicationAgreement(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).ReverseCancelledAgreementMinistry();
		}

		public void CloseGrantFile(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).CloseGrantFile();
		}

		public void ReturnChangeToAssessment(GrantApplication grantApplication, string reason = null)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).ReturnChangeToAssessment(reason);
		}

		public void ReturnUnfundedApplications(int grantOpeningId)
		{
			foreach (var grantApplication in _dbContext.GrantApplications.Where(x => x.GrantOpeningId == grantOpeningId
				&& x.ApplicationStateInternal == ApplicationStateInternal.New).ToList())
			{
				ReturnUnfunded(grantApplication);
			}
		}

		public void SubmitCompletionReportToCloseGrantFile(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).SubmitCompletionReportToCloseGrantFile();
		}

		/// <summary>
		/// Enable completion reporting for the specified grant application.
		/// </summary>
		/// <param name="claim"></param>
		public void EnableCompletionReporting(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			CreateWorkflowStateMachine(grantApplication).EnableCompletionReporting();
		}

		public void UpdateProofOfPayment(GrantApplication grantApplication, bool sendCompletionNotification = false)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			if (_httpContext.User.GetAccountType() == AccountTypes.Internal)
				_noteService.GenerateUpdateNote(grantApplication);

			if (sendCompletionNotification && grantApplication.Assessor != null)
			{
				var subject = $"Grant Application #{grantApplication.FileNumber} - Proof of Payment Completed";
				var body = $@"Dear {grantApplication.Assessor.FirstName},
<br /><br />
The applicant for Application #{grantApplication.FileNumber} has completed their Proof of Payment.
<br /><br />
Thank you.";

				var notification = _notificationService.GenerateNotificationMessage(grantApplication, grantApplication.Assessor, subject, body);
				_notificationService.SendNotification(notification);
			}

			_dbContext.Update(grantApplication);
			CommitTransaction();
		}

		public void UpdateAttestation(GrantApplication grantApplication, bool sendCompletionNotification = false)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			if (_httpContext.User.GetAccountType() == AccountTypes.Internal)
				_noteService.GenerateUpdateNote(grantApplication);

			if (sendCompletionNotification && grantApplication.Assessor != null)
 			{
				var subject = $"Grant Application #{grantApplication.FileNumber} - Attestation Completed";
				var body = $@"Dear {grantApplication.Assessor.FirstName},
<br /><br />
The applicant for Application #{grantApplication.FileNumber} has completed their Attestation.
<br /><br />
Thank you.";

				var notification = _notificationService.GenerateNotificationMessage(grantApplication, grantApplication.Assessor, subject, body);
				_notificationService.SendNotification(notification);
			}

			_dbContext.Update(grantApplication);
			CommitTransaction();
		}

		#endregion

		#region Attachments
		public IEnumerable<AttachmentModel> GetAttachments(int id)
		{
			var grantApplication = _dbContext.GrantApplications.Find(id);

			if (grantApplication == null)
			{
				throw new NoContentException(nameof(grantApplication));
			}

			if (!_httpContext.User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.ViewApplication))
			{
				throw new NotAuthorizedException($"User does not have permission to access application '{grantApplication?.Id}'.");
			}

			int Sequence = 1;
			return grantApplication.Attachments.Select(x => new AttachmentModel()
			{
				Id = x.Id,
				Name = x.FileName,
				Description = x.Description,
				RowVersion = System.Convert.ToBase64String(x.RowVersion),
				Sequence = Sequence++
			});
		}
		#endregion

		#region Training Costs
		public void UpdateTrainingCosts(GrantApplication grantApplication)
		{
			if (grantApplication == null) throw new ArgumentNullException(nameof(grantApplication));

			if (!_httpContext.User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditTrainingCosts))
				throw new NotAuthorizedException($"User does not have permission to edit training costs.");

			var isInternal = _httpContext.User.GetAccountType() == AccountTypes.Internal;
			var trainingCost = grantApplication.TrainingCost;

			// Remove any eligible cost that exists in the datasource but not in the updated training cost.
			var existingCostIds = _dbContext.EligibleCosts
				.AsNoTracking()
				.Where(ec => ec.GrantApplicationId == grantApplication.Id)
				.Select(x => x.Id)
				.ToArray();

			var currentCostIds = trainingCost.EligibleCosts
				.Where(ec => ec.Id > 0)
				.Select(ec => ec.Id)
				.Distinct()
				.ToArray();

			var removeCostIds = existingCostIds
				.Except(currentCostIds)
				.ToArray();

			var removeEligibleCosts = removeCostIds.Any()
				? _dbContext.EligibleCosts
					.Where(ec => removeCostIds.Contains(ec.Id))
					.ToArray()
				: new EligibleCost[0];

			var existingBreakdownIds = _dbContext.EligibleCosts
				.AsNoTracking()
				.Where(ec => ec.GrantApplicationId == grantApplication.Id)
				.SelectMany(t => t.Breakdowns)
				.Select(t => t.Id)
				.Distinct()
				.ToArray();

			var currentBreakdownIds = trainingCost.EligibleCosts
				.SelectMany(t => t.Breakdowns)
				.Select(t => t.Id)
				.Distinct()
				.ToArray();

			var removeBreakdownIds = existingBreakdownIds
				.Except(currentBreakdownIds)
				.ToArray();

			var removeEligibleCostBreakdowns = currentBreakdownIds.Any()
				? _dbContext.EligibleCostBreakdowns
					.Where(ecb => removeBreakdownIds.Contains(ecb.Id))
					.ToArray()
				: new EligibleCostBreakdown[0];

			// Remove eligible cost breakdowns.
			foreach (var remove in removeEligibleCostBreakdowns)
			{
				foreach (var program in remove.TrainingPrograms.ToArray())
				{
					foreach (var method in program.DeliveryMethods.ToArray())
						program.DeliveryMethods.Remove(method);

					foreach (var trainingProvider in program.TrainingProviders.ToArray())
					{
						if (trainingProvider.ProofOfQualificationsDocumentId.HasValue)
							_dbContext.Attachments.Remove(trainingProvider.ProofOfQualificationsDocument);

						if (trainingProvider.BusinessCaseDocumentId.HasValue)
							_dbContext.Attachments.Remove(trainingProvider.BusinessCaseDocument);

						if (trainingProvider.CourseOutlineDocumentId.HasValue)
							_dbContext.Attachments.Remove(trainingProvider.CourseOutlineDocument);

						if (trainingProvider.TrainingAddress != null)
							_dbContext.ApplicationAddresses.Remove(trainingProvider.TrainingAddress);

						_dbContext.TrainingProviders.Remove(trainingProvider);
					}

					_dbContext.TrainingPrograms.Remove(program);
				}

				_dbContext.EligibleCostBreakdowns.Remove(remove);
			}

			// Remove eligible costs.
			foreach (var remove in removeEligibleCosts)
			{
				foreach (var breakdown in remove.Breakdowns)
				{
					if (remove.EligibleExpenseType.ServiceCategory?.ServiceTypeId == ServiceTypes.SkillsTraining)
					{
						var programs = breakdown.TrainingPrograms.ToArray();
						foreach (var program in programs)
						{
							_dbContext.TrainingPrograms.Remove(program);
						}
					}

					_dbContext.EligibleCostBreakdowns.Remove(breakdown);
				}

				if (remove.EligibleExpenseType.ServiceCategory?.ServiceTypeId == ServiceTypes.EmploymentServicesAndSupports)
				{
					var removeTrainingProviders = remove.TrainingProviders.ToArray();
					foreach (var trainingProvider in removeTrainingProviders)
					{
						// Remove attachments if they exists.
						if (trainingProvider.ProofOfQualificationsDocumentId.HasValue) _dbContext.Attachments.Remove(trainingProvider.ProofOfQualificationsDocument);
						if (trainingProvider.BusinessCaseDocumentId.HasValue) _dbContext.Attachments.Remove(trainingProvider.BusinessCaseDocument);
						if (trainingProvider.CourseOutlineDocumentId.HasValue) _dbContext.Attachments.Remove(trainingProvider.CourseOutlineDocument);
						_dbContext.ApplicationAddresses.Remove(trainingProvider.TrainingAddress);
						_dbContext.TrainingProviders.Remove(trainingProvider);
					}
				}

				trainingCost.EligibleCosts.Remove(remove);
				_dbContext.EligibleCosts.Remove(remove);
			}

			foreach (var eligibleCost in trainingCost.EligibleCosts)
			{
				var expenseType = Get<EligibleExpenseType>(eligibleCost.EligibleExpenseType.Id);

				foreach (var breakdown in eligibleCost.Breakdowns)
				{
					if (breakdown.EstimatedCost <= 0)
					{
						breakdown.TrainingPrograms.ForEach(tp => tp.TrainingProgramState = TrainingProgramStates.Incomplete);
					}
					_dbContext.Update(breakdown);
				}

				eligibleCost.RecalculateEstimatedCost();
				eligibleCost.RecalculateAgreedCosts();

				if (eligibleCost.Id == 0)
				{
					if (isInternal)
						eligibleCost.AddedByAssessor = true;

					_dbContext.EligibleCosts.Add(eligibleCost);
				}
				else
				{
					_dbContext.Update(eligibleCost);
				}
			}

			trainingCost.RecalculateEstimatedCosts();
			trainingCost.RecalculateAgreedCosts();

			if (!isInternal)
			{
				trainingCost.TrainingCostState = trainingCost.EstimatedParticipants > 0 && trainingCost.TotalEstimatedReimbursement > 0
					? TrainingCostStates.Complete
					: TrainingCostStates.Incomplete;
			}

			// Agreed commitment cannot exceed 10% unless user is a Director
			if (trainingCost.DoesAgreedCommitmentExceedEstimatedContribution() && !_httpContext.User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditTrainingCostOverride))
				throw new InvalidOperationException("You may not increase the assessed total government contribution more than 10% over the estimated total government contribution.");

			// If the applicant is working on a current claim, update it with the latest changes made to the agreed costs.
			var claim = grantApplication.GetCurrentClaim();
			if (claim?.ClaimState.In(ClaimState.Incomplete, ClaimState.Complete) ?? false)
			{
				UpdateClaim(claim);
			}

			if (isInternal)
			{
				if (_grantAgreementService.AgreementUpdateRequired(grantApplication))
					_grantAgreementService.UpdateAgreement(grantApplication);

				_noteService.GenerateUpdateNote(grantApplication);
			}

			_dbContext.Update(trainingCost);
			_dbContext.CommitTransaction();
		}

		/// <summary>
		/// Returns the training costs for the specified grant application.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		public TrainingCostModel GetTrainingCostModel(int grantApplicationId)
		{
			var grantApplication = Get<GrantApplication>(grantApplicationId);

			if (!_httpContext.User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.ViewApplication))
			{
				throw new NotAuthorizedException($"User does not have permission to access application '{grantApplicationId}'.");
			}
			var eligibleExpenseTypes = _grantStreamService.GetAllEligibleExpenseTypes(grantApplication.GrantOpening.GrantStreamId).Select(eet => new EligibleExpenseTypeModel(eet));
			var autoIncludeEligibleExpenseTypes = _grantStreamService.GetAutoIncludeActiveEligibleExpenseTypes(grantApplication.GrantOpening.GrantStreamId).Select(eet => new EligibleExpenseTypeModel(eet));
			return new TrainingCostModel(grantApplication, eligibleExpenseTypes, autoIncludeEligibleExpenseTypes);
		}

		/// <summary>
		/// Updates the specified training program and its eligible costs.
		/// </summary>
		/// <param name="model"></param>
		public TrainingCost Update(TrainingCostModel model)
		{
			bool isInternal = _httpContext.User.GetAccountType() == AccountTypes.Internal;
			if (model == null)
				throw new ArgumentNullException(nameof(model));

			var grantApplication = Get(model.GrantApplicationId);

			if (!_httpContext.User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditTrainingCosts))
				throw new NotAuthorizedException($"User does not have permission to access application {model.GrantApplicationId}.");

			var trainingCost = grantApplication.TrainingCost;

			if (isInternal)
			{
				trainingCost.AgreedParticipants = model.AgreedParticipants.Value;
			}
			else
			{
				trainingCost.EstimatedParticipants = model.EstimatedParticipants.Value;
			}

			// Remove any eligible cost that exists in the datasource but not in the updated training cost.
			var currentCostIds = model.EligibleCosts.Select(x => x.Id).ToArray();
			var removeEligibleCosts = trainingCost.EligibleCosts.Where(ec => !currentCostIds.Contains(ec.Id)).ToArray();
			var currentBreakdownIds = model.EligibleCosts.SelectMany(t => t.Breakdowns).Select(t => t.Id);
			var removeEligibleCostBreakdowns = trainingCost.EligibleCosts.SelectMany(t => t.Breakdowns).Where(t => !currentBreakdownIds.Contains(t.Id)).ToArray();

			foreach (var remove in removeEligibleCostBreakdowns)
			{
				if (remove.EligibleCost.EligibleExpenseType.ServiceCategory?.ServiceTypeId == ServiceTypes.SkillsTraining)
				{
					var programs = remove.TrainingPrograms.ToArray();
					foreach (var program in programs)
					{
						foreach (var method in program.DeliveryMethods.ToArray())
						{
							program.DeliveryMethods.Remove(method);
						}

						foreach (var trainingProvider in program.TrainingProviders.ToArray())
						{
							_dbContext.TrainingProviders.Remove(trainingProvider);
						}

						_dbContext.TrainingPrograms.Remove(program);
					}
				}

				_dbContext.EligibleCostBreakdowns.Remove(remove);
			}

			foreach (var remove in removeEligibleCosts)
			{
				foreach (var breakdown in remove.Breakdowns)
				{
					if (remove.EligibleExpenseType.ServiceCategory?.ServiceTypeId == ServiceTypes.SkillsTraining)
					{
						var programs = breakdown.TrainingPrograms.ToArray();
						foreach (var program in programs)
						{
							_dbContext.TrainingPrograms.Remove(program);
						}
					}

					_dbContext.EligibleCostBreakdowns.Remove(breakdown);
				}


				if (remove.EligibleExpenseType.ServiceCategory?.ServiceTypeId == ServiceTypes.EmploymentServicesAndSupports)
				{
					var removeTrainingProviders = remove.TrainingProviders.ToArray();
					foreach (var item in removeTrainingProviders)
					{
						_dbContext.TrainingProviders.Remove(item);
					}
				}

				trainingCost.EligibleCosts.Remove(remove);
				_dbContext.EligibleCosts.Remove(remove);
			}

			foreach (var cost in model.EligibleCosts)
			{
				var expenseType = Get<EligibleExpenseType>(cost.EligibleExpenseType.Id);
				var entity = cost.Id == 0 ? new EligibleCost(grantApplication, expenseType, cost.EstimatedCost, cost.EstimatedParticipants) : Get<EligibleCost>(cost.Id);

				switch (cost.EligibleExpenseType.ExpenseTypeId)
				{
					case (ExpenseTypes.ParticipantLimited):
					case (ExpenseTypes.NotParticipantLimited):
					case (ExpenseTypes.AutoLimitEstimatedCosts):
						if (isInternal)
							entity.AgreedMaxParticipants = model.AgreedParticipants.Value;
						else
							entity.EstimatedParticipants = model.EstimatedParticipants.Value;
						break;
					default:
						if (isInternal)
							entity.AgreedMaxParticipants = cost.AgreedMaxParticipants;
						else
							entity.EstimatedParticipants = cost.EstimatedParticipants;
						break;
				}
				if (isInternal)
				{
					entity.AgreedMaxParticipantCost = entity.AgreedMaxParticipants > 0 ? cost.AgreedMaxParticipantCost : 0;
					entity.AgreedMaxCost = entity.AgreedMaxParticipants > 0 ? cost.AgreedCost : 0;
				}
				else
				{
					entity.EstimatedParticipantCost = entity.EstimatedParticipants > 0 ? cost.EstimatedParticipantCost : 0;
					entity.EstimatedCost = entity.EstimatedParticipants > 0 ? cost.EstimatedCost : 0;
				}

				foreach (var breakdown in cost.Breakdowns)
				{
					var breakdownEntity = Get<EligibleCostBreakdown>(breakdown.Id);


					if (isInternal)
					{
						breakdownEntity.AssessedCost = breakdown.AssessedCost;
					}
					else
					{
						breakdownEntity.EstimatedCost = breakdown.EstimatedCost;
						if (breakdown.EstimatedCost <= 0)
						{
							breakdownEntity.TrainingPrograms.ForEach(tp => tp.TrainingProgramState = TrainingProgramStates.Incomplete);
						}
					}
					_dbContext.Update(breakdownEntity);
				}

				entity.EligibleExpenseTypeId = cost.EligibleExpenseType.Id;
				entity.RecalculateEstimatedCost();
				entity.RecalculateAgreedCosts();

				if (cost.Id == 0)
				{
					if (isInternal)
					{
						entity.EstimatedParticipants = entity.AgreedMaxParticipants;
						entity.AddedByAssessor = true;
					}
					_dbContext.EligibleCosts.Add(entity);
				}
				else
				{
					_dbContext.Update(entity);
				}
			}

			trainingCost.RecalculateEstimatedCosts();
			trainingCost.RecalculateAgreedCosts();
			if (!isInternal)
			{
				if (trainingCost.EstimatedParticipants > 0 && trainingCost.TotalEstimatedReimbursement > 0)
				{
					trainingCost.TrainingCostState = TrainingCostStates.Complete;
				}
				else
				{
					trainingCost.TrainingCostState = TrainingCostStates.Incomplete;
				}
			}

			// Agreed commitment cannot exceed 10% unless user is a Director
			if (trainingCost.DoesAgreedCommitmentExceedEstimatedContribution() && !_httpContext.User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditTrainingCostOverride))
				throw new InvalidOperationException("You may not increase the assessed total government contribution more than 10% over the estimated total government contribution.");

			// If the applicant is working on a current claim, update it with the latest changes made to the agreed costs.
			var claim = grantApplication.GetCurrentClaim();
			if (claim?.ClaimState.In(ClaimState.Incomplete, ClaimState.Complete) ?? false)
			{
				UpdateClaim(claim);
			}

			var accountType = _httpContext.User.GetAccountType();
			if (accountType == AccountTypes.Internal)
			{
				if (_grantAgreementService.AgreementUpdateRequired(grantApplication)) _grantAgreementService.UpdateAgreement(grantApplication);
				_noteService.GenerateUpdateNote(grantApplication);
			}

			_dbContext.Update(trainingCost);
			_dbContext.CommitTransaction();

			return trainingCost;
		}

		/// <summary>
		/// Update the delivery partner.
		/// </summary>
		/// <param name="grantApplication"></param>
		/// <param name="deliveryPartnerId"></param>
		/// <param name="selectedServices"></param>
		public void UpdateDeliveryPartner(GrantApplication grantApplication, int? deliveryPartnerId, IEnumerable<int> selectedServices)
		{
			if (!grantApplication.GrantOpening.GrantStream.IncludeDeliveryPartner)
				throw new InvalidOperationException("Unable to include delivery partner information for this grant stream.");

			grantApplication.UsedDeliveryPartner = deliveryPartnerId.HasValue;
			grantApplication.DeliveryPartnerId = deliveryPartnerId;
			if (selectedServices.Any())
			{
				var modelIds = selectedServices.ToArray();
				var currentIds = grantApplication.DeliveryPartnerServices.Select(ds => ds.Id).ToArray();
				var removeIds = currentIds.Except(modelIds);
				var addIds = modelIds.Except(currentIds).Except(removeIds);

				foreach (var remove in _dbContext.DeliveryPartnerServices.Where(ds => removeIds.Contains(ds.Id)))
				{
					grantApplication.DeliveryPartnerServices.Remove(remove);
				}

				foreach (var add in _dbContext.DeliveryPartnerServices.Where(ds => addIds.Contains(ds.Id)))
				{
					grantApplication.DeliveryPartnerServices.Add(add);
				}
			}
			else
			{
				// Remove all the delivery methods.
				grantApplication.DeliveryPartnerServices.Clear();
			}
		}

		public void UpdateChecklist(GrantApplication grantApplication, List<int> modelChecklistItemIds)
		{
			var checklistItems = _grantStreamService.GetGrantStreamChecklist(grantApplication.GrantOpening.GrantStreamId).ToList();

			if (!checklistItems.Any())
				return;

			var currentTasks = grantApplication.GrantApplicationTasks.ToList();

			var flattenedItems = checklistItems
				.SelectMany(c => c.Items)
				.ToList();

			// Update existing values by setting them to false
			grantApplication.GrantApplicationTasks.ForEach(a =>
			{
				a.IsChecked = false;
				a.DateUpdated = AppDateTime.UtcNow;
			});

			foreach (var checklistItemId in modelChecklistItemIds)
			{
				var item = flattenedItems.FirstOrDefault(i => i.Id == checklistItemId);
				var task = currentTasks.FirstOrDefault(t => t.ChecklistItemId == checklistItemId);

				if (task == null)
				{
					grantApplication.GrantApplicationTasks.Add(new GrantApplicationTask
					{
						GrantApplication = grantApplication,
						ChecklistItem = item,
						IsChecked = true,
						DateAdded = AppDateTime.UtcNow
					});
				}
				else
				{
					task.IsChecked = true;
					task.DateUpdated = DateTime.UtcNow;
				}
			}
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Get the next file number for the specified fiscal year Id.
		/// </summary>
		/// <param name="fiscalYearId"></param>
		/// <returns></returns>
		private string GetNextFileNumber(int fiscalYearId)
		{
			string fileNumber = null;

			_dbContext.CommitTransaction(() =>
			{
				// need to get the next sequential number for the fiscal year in order to create the agreement number
				var fiscalYear = _dbContext.FiscalYears.First(x => x.Id == fiscalYearId);
				var lastNumber = fiscalYear.NextAgreementNumber++;
				fileNumber = $"{fiscalYear.EndDate:yy}{lastNumber:d5}";
				return lastNumber;
			});

			return fileNumber;
		}

		private bool IsApplicantAddressInUse(int grantApplicationId, int addressId)
		{
			return _dbContext.GrantApplications
				.Where(x => x.Id != grantApplicationId && (x.ApplicantMailingAddressId == addressId ||
				                                           x.ApplicantPhysicalAddressId == addressId ||
				                                           x.OrganizationAddressId == addressId))
				.Select(x => x.Id)
				.ToList()
				.Any();
		}

		/// <summary>
		/// Creates a new instance of a <typeparamref name="ApplicationWorkflowStateMachine"/> object.
		/// </summary>
		/// <param name="grantApplication"></param>
		/// <returns></returns>
		private ApplicationWorkflowStateMachine CreateWorkflowStateMachine(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			return new ApplicationWorkflowStateMachine(grantApplication, _dbContext, _notificationService, _grantAgreementService, _grantOpeningService, _noteService, _userService, _httpContext, _logger);
		}

		/// <summary>
		/// When the training costs are updated the claim may need to be updated to match.
		/// This will add/remove/update claim costs based on the agreed costs.
		/// </summary>
		/// <param name="claim"></param>
		private void UpdateClaim(Claim claim)
		{
			var grantApplication = claim.GrantApplication;
			var trainingCost = grantApplication.TrainingCost;
			claim.ClaimState = ClaimState.Incomplete; // By default we want to force the applicant to view the claim again.

			foreach (var eligibleCost in trainingCost.EligibleCosts)
			{
				var claimEligibleCost = claim.EligibleCosts.FirstOrDefault(cec => cec.EligibleCostId == eligibleCost.Id);

				if (claimEligibleCost == null && eligibleCost.AgreedMaxCost > 0)
				{
					// Add a new claim eligible cost.
					claimEligibleCost = new ClaimEligibleCost(claim, eligibleCost);
					_dbContext.ClaimEligibleCosts.Add(claimEligibleCost);
					claim.EligibleCosts.Add(claimEligibleCost);
				}
				else if (claimEligibleCost != null)
				{
					if (eligibleCost.AgreedMaxCost == 0)
					{
						// Remove the claim eligible cost.
						var claimEligibleCostBreakdowns = claimEligibleCost.Breakdowns.ToArray();
						claimEligibleCost.Breakdowns.Clear();
						claim.EligibleCosts.Remove(claimEligibleCost);
						foreach (var breakdown in claimEligibleCostBreakdowns)
							_dbContext.ClaimBreakdownCosts.Remove(breakdown);
						_dbContext.ClaimEligibleCosts.Remove(claimEligibleCost);
					}
					else
					{
						// Update the breakdown costs.
						foreach (var breakdown in eligibleCost.Breakdowns)
						{
							var claimBreakdownCost = claimEligibleCost.Breakdowns.FirstOrDefault(b => b.EligibleCostBreakdownId == breakdown.Id);
							if (!breakdown.IsEligible && claimBreakdownCost != null)
							{
								// Remove in-eligible costs.
								claimEligibleCost.Breakdowns.Remove(claimBreakdownCost);
								_dbContext.ClaimBreakdownCosts.Remove(claimBreakdownCost);
							}
							else if (breakdown.IsEligible)
							{
								if (claimBreakdownCost != null && claimBreakdownCost.ClaimCost > breakdown.AssessedCost)
								{
									// Reset the breakdown.
									claimBreakdownCost.ClaimCost = 0;
								}
								else if (claimBreakdownCost == null)
								{
									// Add the new breakdown.
									claimBreakdownCost = new ClaimBreakdownCost(breakdown, claimEligibleCost);
									claimEligibleCost.Breakdowns.Add(claimBreakdownCost);
								}
							}
						}

						// Remove any breakdowns that no longer have matching costs.
						var breakdownIds = eligibleCost.Breakdowns.Select(b => b.Id).ToArray();
						var deleteClaimBreakdowns = claimEligibleCost.Breakdowns.Where(b => !breakdownIds.Contains(b.EligibleCostBreakdownId)).ToArray();
						deleteClaimBreakdowns.ForEach(breakdown =>
						{
							claimEligibleCost.Breakdowns.Remove(breakdown);
							_dbContext.ClaimBreakdownCosts.Remove(breakdown);
						});

						if (eligibleCost.Breakdowns.Any() && eligibleCost.EligibleExpenseType?.ServiceCategory.ServiceTypeId != ServiceTypes.EmploymentServicesAndSupports)
						{
							// Recalculate agreed costs and claimed costs.
							eligibleCost.AgreedMaxCost = eligibleCost.Breakdowns.Sum(b => b.AssessedCost);

							if (claimEligibleCost.Breakdowns.Any())
							{
								claimEligibleCost.ClaimCost = claimEligibleCost.Breakdowns.Sum(b => b.ClaimCost);
							}
						}

						if (eligibleCost.AgreedMaxCost < (claimEligibleCost?.ClaimCost ?? 0) || eligibleCost.AgreedMaxParticipants < (claimEligibleCost?.ClaimParticipants ?? 0))
						{
							// Reset the claim eligible cost.
							claimEligibleCost.ClaimCost = 0;
							switch (claimEligibleCost.EligibleExpenseType.ExpenseTypeId)
							{
								case (ExpenseTypes.NotParticipantLimited):
									claimEligibleCost.ClaimParticipants = grantApplication.ParticipantForms.Count(pf => !pf.IsExcludedFromClaim);
									break;
								default:
									claimEligibleCost.ClaimParticipants = 0;
									break;
							}
							claimEligibleCost.Breakdowns.ForEach(b => b.ClaimCost = 0);
						}
					}
				}
			}

			// Remove any claim eligible costs that no longer have matching eligible costs.
			var eligibleCostIds = trainingCost.EligibleCosts.Select(ec => ec.Id).ToArray();
			var deleteClaimEligibleCosts = claim.EligibleCosts.Where(ec => !eligibleCostIds.Contains(ec.EligibleCostId.Value)).ToArray();
			deleteClaimEligibleCosts.ForEach(cost =>
			{
				claim.EligibleCosts.Remove(cost);
				_dbContext.ClaimEligibleCosts.Remove(cost);
			});

			claim.RecalculateClaimedCosts();
		}

		public void RevertApplicationStatus(Guid userBCeID, int orgId)
		{
			//var applications = _dbContext.GrantApplications.Where(x => x.ApplicantBCeID == userBCeID && x.ApplicationStateExternal == ApplicationStateExternal.Complete).ToArray();

			var applications = (from p in _dbContext.ProgramDescriptions
								join g in _dbContext.GrantApplications
								on p.GrantApplicationId equals g.Id
								join n in _dbContext.NaIndustryClassificationSystems
								on p.TargetNAICSId equals n.Id
								where n.NAICSVersion == 2012 && g.ApplicantBCeID == userBCeID && g.OrganizationId == orgId &&
								(g.ApplicationStateExternal == ApplicationStateExternal.Complete)
								select g).ToArray();

			for (var i = 0; i < applications.Length; i++)
			{
				applications[i].RevertStatus();
				_dbContext.Update(applications[i]);
			}
			_dbContext.CommitTransaction();
		}

		#endregion
	}
}