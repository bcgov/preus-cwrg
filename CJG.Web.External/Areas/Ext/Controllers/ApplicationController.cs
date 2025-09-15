using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using CJG.Core.Entities;
using CJG.Core.Interfaces;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Ext.Models;
using CJG.Web.External.Areas.Ext.Models.Applications;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;

namespace CJG.Web.External.Areas.Ext.Controllers
{
    /// <summary>
    /// <typeparamref name="ApplicationController"/> class, provides a controller endpoints for managing external user grant applications.
    /// </summary>
    [RouteArea("Ext")]
	[ExternalFilter]
	public class ApplicationController : BaseController
	{
		private readonly IStaticDataService _staticDataService;
		private readonly ISiteMinderService _siteMinderService;
		private readonly IUserService _userService;
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IGrantOpeningService _grantOpeningService;
		private readonly IGrantStreamService _grantStreamService;
		private readonly IGrantOpeningManageScheduledService _grantOpeningManageScheduledService;
		private readonly IFiscalYearService _fiscalYearService;
		private readonly IGrantProgramService _grantProgramService;
		private readonly ISettingService _settingService;

		/// <summary>
		/// Creates a new instance of a <typeparamref name="ApplicationController"/> object.
		/// </summary>
		/// <param name="controllerService"></param>
		/// <param name="grantApplicationService"></param>
		/// <param name="grantOpeningService"></param>
		/// <param name="grantStreamService"></param>
		/// <param name="grantOpeningManageScheduledService"></param>
		/// <param name="grantProgramService"></param>
		/// <param name="fiscalYearService"></param>
		/// <param name="settingService"></param>
		public ApplicationController(
			IControllerService controllerService,
			IGrantApplicationService grantApplicationService,
			IGrantOpeningService grantOpeningService,
			IGrantStreamService grantStreamService,
			IGrantOpeningManageScheduledService grantOpeningManageScheduledService,
			IGrantProgramService grantProgramService,
			IFiscalYearService fiscalYearService,
			ISettingService settingService) : base(controllerService.Logger)
		{
			_userService = controllerService.UserService;
			_staticDataService = controllerService.StaticDataService;
			_siteMinderService = controllerService.SiteMinderService;
			_grantApplicationService = grantApplicationService;
			_grantOpeningService = grantOpeningService;
			_grantStreamService = grantStreamService;
			_grantOpeningManageScheduledService = grantOpeningManageScheduledService;
			_fiscalYearService = fiscalYearService;
			_grantProgramService = grantProgramService;
			_settingService = settingService;
		}

		/// <summary>
		/// Create a new Grant Application View / Edit an exist Grant Application View.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="grantProgramId"></param>
		/// <returns></returns>
		[Route("Application/Grant/Selection/View/{grantApplicationId}/{grantProgramId?}")]
		public ActionResult GrantSelectionView(int grantApplicationId, int grantProgramId = 0)
		{
			if (grantApplicationId == 0)
			{
				var currentUser = _userService.GetUser(_siteMinderService.CurrentUserGuid);
				if (currentUser.PhysicalAddress == null)
				{
					this.SetAlert("Business Address is required to start new application", AlertType.Warning, true);
					return RedirectToAction(nameof(UserProfileController.UpdateUserProfileView), nameof(UserProfileController).Replace("Controller", ""));
				}
			}
			else
			{
				_grantApplicationService.Get(grantApplicationId);
			}

			ViewBag.GrantApplicationId = grantApplicationId;
			ViewBag.GrantProgramId = grantProgramId;
			return View();
		}

		/// <summary>
		/// Get the data for the grant selection view page.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="grantProgramId"></param>
		/// <returns></returns>
		[Route("Application/Grant/Selection/{grantApplicationId}/{grantProgramId?}")]
		public JsonResult GetGrantSelection(int grantApplicationId, int grantProgramId = 0)
		{
			var model = new ApplicationStartViewModel();
			try
			{
				var fiscalYear = _fiscalYearService.GetFiscalYear(AppDateTime.UtcNow);
				_grantOpeningManageScheduledService.ManageStateTransitions(fiscalYear.Id);

				if (grantApplicationId == 0)
				{
					model = new ApplicationStartViewModel(grantProgramId, _grantOpeningService, _grantProgramService, _staticDataService, _grantStreamService);
				}
				else
				{
					var grantApplication = _grantApplicationService.Get(grantApplicationId);
					model = new ApplicationStartViewModel(grantApplication, _grantOpeningService, _grantProgramService, _staticDataService, _grantStreamService);
				}
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Get the stream eligibility requirements data.
		/// </summary>
		/// <param name="grantOpeningId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/Grant/Stream/Eligibility/Requirements/{grantOpeningId}")]
		public JsonResult GetStreamEligibilityRequirements(int grantOpeningId)
		{
			var model = new GrantStreamEligibilityViewModel();
			try
			{
				var grantOpening = _grantOpeningService.Get(grantOpeningId);
				model = new GrantStreamEligibilityViewModel(grantOpening.GrantStream, _grantStreamService);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Add the specified grant application.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application")]
		public JsonResult AddGrantApplication(ApplicationStartViewModel model)
		{
			try
			{
				var grantOpening = _grantOpeningService.Get(model.GrantOpeningId.GetValueOrDefault());
				var dbModel = new GrantStreamEligibilityViewModel(grantOpening.GrantStream, _grantStreamService);

				// Handle the eligibility questions. Use the database, valid versions of the questions to test if the client data is correct.
				// passedEligibilityQuestions replaces previous code which set GrantApplication.EligibilityConfirmed to false if
				// there were no eligibility questions, and set it to true if there were (since if there are questions that must be
				// true to add the grant application with the extant eligibility question).
				int numQuestions = 0;
				int numFoundQuestions = 0;
				bool passedEligibilityQuestions = true;
				if (dbModel.StreamEligibilityQuestions.Count() != 0)
				{
					foreach (var question in dbModel.StreamEligibilityQuestions)
					{
						numQuestions++;
						foreach (var clientQuestion in model.GrantStream.StreamEligibilityQuestions)
						{
							if (question.Id != clientQuestion.Id)
								continue;

							numFoundQuestions++;
							if (clientQuestion.EligibilityAnswer == null || question.EligibilityPositiveAnswerRequired && clientQuestion.EligibilityAnswer != true)
							{
								ModelState.AddModelError("EligibilityQuestion" + question.Id, "The stream eligibility requirements must be met for your application to be submitted and assessed.");
								passedEligibilityQuestions = false;
							}

							if (!question.CollectContactInformation)
								continue;
								
							var validateContactInformation = clientQuestion.EligibilityAnswer ?? false;
							if (!validateContactInformation)
								continue;

							var validateGrantWriter = question.RequiresGrantWriter;
							if (validateGrantWriter)
							{
								if (!clientQuestion.Designation.HasValue || clientQuestion.Designation.Value <= 0)
								{
									ModelState.AddModelError("Designation" + question.Id, "You must provide a Grant Writer Designation when selecting 'Yes' for this question.");
									passedEligibilityQuestions = false;
								}
							}

							if (string.IsNullOrWhiteSpace(clientQuestion.ContactName))
							{
								ModelState.AddModelError("ContactName" + question.Id, "You must provide a Contact name when selecting 'Yes' for this question.");
								passedEligibilityQuestions = false;
							}

							var isValidEmail = new EmailAddressAttribute().IsValid(clientQuestion.ContactEmailAddress);
							if (string.IsNullOrWhiteSpace(clientQuestion.ContactEmailAddress) || !isValidEmail)
							{
								ModelState.AddModelError("ContactEmailAddress" + question.Id, "You must provide a Contact email address when selecting 'Yes' for this question.");
								passedEligibilityQuestions = false;
							}

							if (string.IsNullOrWhiteSpace(clientQuestion.ContactPhoneNumber))
							{
								ModelState.AddModelError("ContactPhoneNumber" + question.Id, "You must provide a Contact phone number when selecting 'Yes' for this question.");
								passedEligibilityQuestions = false;
							}
						}
					}
				}

				if (numFoundQuestions != numQuestions)
				{
					ModelState.AddModelError("EligibilityQuestion", "The stream eligibility requirements must be met for your application to be submitted and assessed.");
					passedEligibilityQuestions = false;
				}

				if (model.ProgramType == ProgramTypes.WDAService && !model.HasRequestedAdditionalFunding.HasValue)
					ModelState.AddModelError("AdditionalFundingQuestion", "You must select whether you have previously received or are requesting additional funding.");

				if (!model.HasRequestedAdditionalFunding ?? true)
					ModelState.Remove("DescriptionOfFundingRequested");

				ModelState.Remove("AlternatePhoneViewModel.PhoneNumber");
				ModelState.Remove("AlternatePhoneViewModel.Phone");

				if (ModelState.IsValid)
				{
					var currentUser = _userService.GetUser(_siteMinderService.CurrentUserGuid);

					var grantApplication = new GrantApplication();
					grantApplication.CopyApplicant(currentUser);
					grantApplication.AddApplicationAdministrator(currentUser);
					grantApplication.ApplicationType = _grantApplicationService.GetDefaultApplicationType();

					// Base the completion report that will be processed, on whether the grant program has Account Code for CWRG.
					if (grantOpening.GrantStream.GrantProgram.AccountCodeId == Constants.GrantProgramNameCWRGIdKey)
						grantApplication.CompletionReportId = Constants.CompletionReportCWRG;
					else
						grantApplication.CompletionReportId = Constants.CompletionReportETG;
					grantApplication.GrantOpeningId = model.GrantOpeningId.GetValueOrDefault();
					grantApplication.OrganizationId = currentUser.OrganizationId;
					grantApplication.OrganizationBCeID = currentUser.Organization.BCeIDGuid;
					grantApplication.EligibilityConfirmed = passedEligibilityQuestions;
					grantApplication.InvitationKey = Guid.NewGuid();
					grantApplication.IsAlternateContact = model.IsAlternateContact;

					if (grantApplication.IsAlternateContact == true)
					{
						grantApplication.AlternateEmail = model.AlternateEmail;
						grantApplication.AlternateFirstName = model.AlternateFirstName;
						grantApplication.AlternateJobTitle = model.AlternateJobTitle;
						grantApplication.AlternateLastName = model.AlternateLastName;
						grantApplication.AlternatePhoneExtension = model.AlternatePhoneExtension;
						grantApplication.AlternatePhoneNumber = model.AlternatePhone;
						grantApplication.AlternateSalutation = model.AlternateSalutation;
					}

					grantApplication.InsuranceConfirmed = null;     // InsuranceConfirmed is no longer usable, values stay in GrantApp and are copied to Eligibility Answers
					grantApplication.HasRequestedAdditionalFunding = model.HasRequestedAdditionalFunding;
					grantApplication.DescriptionOfFundingRequested = model.DescriptionOfFundingRequested;

					grantApplication.MaxReimbursementAmt = grantOpening.GrantStream.MaxReimbursementAmt;
					grantApplication.ReimbursementRate = grantOpening.GrantStream.ReimbursementRate;
					grantApplication.TrainingCost = grantOpening.GrantStream.GrantProgram.ProgramTypeId == ProgramTypes.EmployerGrant ? new TrainingCost(grantApplication, 0) : new TrainingCost(grantApplication, 1);

					grantApplication.RequireAllParticipantsBeforeSubmission = grantOpening.GrantStream.RequireAllParticipantsBeforeSubmission;
					
					// Bypass need for applicant to supply Delivery Dates - and set the start/end to the GrantOpening Training Dates.
					// This will be updated later when the applicant defines their Training Program Start/End Dates.
					if (model.GrantApplicationId == 0)
					{
						// set start/end dates to user selected dates
						var earliest = grantApplication.DateSubmitted ?? grantApplication.DateAdded;
						var defaultStartDate = grantOpening.TrainingPeriod.StartDate;
						var defaultEndDate = grantOpening.TrainingPeriod.EndDate.AddDays(45);

						if (earliest > defaultStartDate)
							defaultStartDate = earliest;

						grantApplication.StartDate = defaultStartDate.ToUtcMorning();
						grantApplication.EndDate = defaultEndDate.ToUtcMidnight();
					}

					foreach (var question in dbModel.StreamEligibilityQuestions)
					{
						foreach (var clientQuestion in model.GrantStream.StreamEligibilityQuestions)
						{
							if (question.Id != clientQuestion.Id)
								continue;

							if (clientQuestion.EligibilityAnswer == null)
								continue;

							GrantWriterDesignation? grantWriterDesignation = null;
							if (clientQuestion.CollectContactInformation && clientQuestion.RequiresGrantWriter && clientQuestion.Designation.HasValue)
							{
								grantWriterDesignation = (GrantWriterDesignation) clientQuestion.Designation.Value;
							}

							grantApplication.GrantStreamEligibilityAnswers.Add(new GrantStreamEligibilityAnswer
							{
								GrantApplication = grantApplication,
								GrantApplicationId = grantApplication.Id,
								GrantStreamEligibilityQuestionId = question.Id,
								EligibilityAnswer = clientQuestion.EligibilityAnswer.GetValueOrDefault(false),
								GrantWriterDesignation = grantWriterDesignation,
								ContactName = clientQuestion.CollectContactInformation ? clientQuestion.ContactName : null,
								ContactEmailAddress = clientQuestion.CollectContactInformation ? clientQuestion.ContactEmailAddress : null,
								ContactPhoneNumber = clientQuestion.CollectContactInformation ? clientQuestion.ContactPhoneNumber : null
							});
						}
					}
					_grantApplicationService.Add(grantApplication);

					model = new ApplicationStartViewModel(grantApplication, _grantOpeningService, _grantProgramService, _staticDataService, _grantStreamService)
					{
						RedirectURL = Url.Action(nameof(ApplicationOverviewView), new { grantApplicationId = grantApplication.Id })
					};
				}
				else
				{
					HandleModelStateValidation(model);
				}
			}
			catch (Exception e)
			{
				HandleAngularException(e, model);
			}

			return Json(model);
		}

		/// <summary>
		/// Display the Grant Application Funding Selection View.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application")]
		public JsonResult UpdateGrantApplication(ApplicationStartViewModel model)
		{
			try
			{
				// retrieve the original entry
				var grantApplication = _grantApplicationService.Get(model.Id);
				var currentUser = _userService.GetUser(_siteMinderService.CurrentUserGuid);

				var grantOpening = _grantOpeningService.Get(model.GrantOpeningId.GetValueOrDefault());
				var dbModel = new GrantStreamEligibilityViewModel(grantOpening.GrantStream, _grantStreamService);

				// Handle the eligibility questions. Use the database, valid versions of the questions to test if the client data is correct.
				// passedEligibilityQuestions replaces previous code which set GrantApplication.EligibilityConfirmed to false if
				// there were no eligibility questions, and set it to true if there were (since if there are questions that must be
				// true to add the grant application with the extant eligibility question).
				int numQuestions = 0;
				int numFoundQuestions = 0;
				bool passedEligibilityQuestions = true;

				if (dbModel.StreamEligibilityQuestions.Count() != 0)
				{
					foreach (var question in dbModel.StreamEligibilityQuestions)
					{
						numQuestions++;
						foreach (var clientQuestion in model.GrantStream.StreamEligibilityQuestions)
						{
							if (question.Id != clientQuestion.Id)
								continue;

							numFoundQuestions++;
							if (clientQuestion.EligibilityAnswer == null || question.EligibilityPositiveAnswerRequired && clientQuestion.EligibilityAnswer != true)
							{
								ModelState.AddModelError("EligibilityQuestion" + question.Id, "The stream eligibility requirements must be met for your application to be submitted and assessed.");
								passedEligibilityQuestions = false;
							}

							if (!question.CollectContactInformation)
								continue;

							var validateContactInformation = clientQuestion.EligibilityAnswer ?? false;
							if (!validateContactInformation)
								continue;

							if (string.IsNullOrWhiteSpace(clientQuestion.ContactName))
							{
								ModelState.AddModelError("ContactName" + question.Id, "You must provide a Contact name when selecting 'Yes' for this question.");
								passedEligibilityQuestions = false;
							}

							var isValidEmail = new EmailAddressAttribute().IsValid(clientQuestion.ContactEmailAddress);
							if (string.IsNullOrWhiteSpace(clientQuestion.ContactEmailAddress) || !isValidEmail)
							{
								ModelState.AddModelError("ContactEmailAddress" + question.Id, "You must provide a Contact email address when selecting 'Yes' for this question.");
								passedEligibilityQuestions = false;
							}

							if (string.IsNullOrWhiteSpace(clientQuestion.ContactPhoneNumber))
							{
								ModelState.AddModelError("ContactPhoneNumber" + question.Id, "You must provide a Contact phone number when selecting 'Yes' for this question.");
								passedEligibilityQuestions = false;
							}
						}
					}
				}

				if (numFoundQuestions != numQuestions)
				{
					ModelState.AddModelError("EligibilityQuestion", "The stream eligibility requirements must be met for your application to be submitted and assessed.");
					passedEligibilityQuestions = false;
				}

				grantApplication.IsAlternateContact = model.IsAlternateContact;
				grantApplication.InsuranceConfirmed = null;     // InsuranceConfirmed is no longer usable, values stay in GrantApp and are copied to Eligibility Answers
				grantApplication.HasRequestedAdditionalFunding = model.HasRequestedAdditionalFunding;

				// Bypass need for applicant to supply Delivery Dates - and set the start/end to the GrantOpening Training Dates.
				// This will be updated later when the applicant defines their Training Program Start/End Dates.
				//if (grantApplication.ApplicationStateInternal == ApplicationStateInternal.Draft)
				//{
				//	var earliestValidStartDate = grantApplication.EarliestValidStartDate();
				//	// set start/end dates to user selected dates
				//	//var earliest = grantApplication.DateSubmitted ?? grantApplication.DateAdded;
				//	var defaultStartDate = grantOpening.TrainingPeriod.StartDate;
				//	var defaultEndDate = grantOpening.TrainingPeriod.EndDate.AddDays(45);

				//	if (earliestValidStartDate > defaultStartDate)
				//		defaultStartDate = earliestValidStartDate;

				//	grantApplication.StartDate = defaultStartDate;
				//	grantApplication.EndDate = defaultEndDate;
				//}

				// record UTC time only
				//grantApplication.StartDate = new DateTime(model.DeliveryStartYear, model.DeliveryStartMonth, model.DeliveryStartDay, 0, 0, 0, DateTimeKind.Local).ToUtcMorning();
				//grantApplication.EndDate = new DateTime(model.DeliveryEndYear, model.DeliveryEndMonth, model.DeliveryEndDay, 0, 0, 0, DateTimeKind.Local).ToUtcMidnight();

				// If the training program dates fall outside of the delivery dates, make the training program dates equal to the delivery dates.
				//if (grantApplication.ApplicationStateInternal == ApplicationStateInternal.Draft)
				//{
				//	grantApplication.TrainingPrograms.Where(tp => tp.StartDate < grantApplication.StartDate || tp.StartDate > grantApplication.EndDate).ForEach(x => x.StartDate = grantApplication.StartDate);
				//	grantApplication.TrainingPrograms.Where(tp => tp.EndDate > grantApplication.EndDate || tp.EndDate < grantApplication.StartDate).ForEach(x => x.EndDate = grantApplication.EndDate);
				//}

				// If Saving the application violates the StartDate, we need to move it forward
				if (grantApplication.ApplicationStateInternal == ApplicationStateInternal.Draft && !grantApplication.HasValidStartDate())
					grantApplication.StartDate = grantApplication.EarliestValidStartDate().ToUtcMorning();

				// update the original entry
				_grantApplicationService.ConvertAndValidate(model, grantApplication, ModelState);

				if (grantApplication.GrantOpening.State == GrantOpeningStates.Closed)
					ModelState.AddModelError("", "Your grant selection is no longer available.  You must make a new grant selection for this application.");

				ModelState.Remove("AlternatePhoneViewModel.PhoneNumber");
				ModelState.Remove("AlternatePhoneViewModel.Phone");

				if (!model.HasRequestedAdditionalFunding ?? true)
					ModelState.Remove("DescriptionOfFundingRequested");

				if (ModelState.IsValid)
				{
					var originalStartDate = _grantApplicationService.OriginalValue(grantApplication, ga => ga.StartDate);
					var originalEndDate = _grantApplicationService.OriginalValue(grantApplication, ga => ga.EndDate);
					var originalGrantOpeningId = _grantApplicationService.OriginalValue(grantApplication, ga => ga.GrantOpeningId);
					var originalEligibilityConfirmed = _grantApplicationService.OriginalValue(grantApplication, ga => ga.EligibilityConfirmed);
					var originalIsAlternateContact = _grantApplicationService.OriginalValue(grantApplication, ga => ga.IsAlternateContact);
					grantApplication.EligibilityConfirmed = passedEligibilityQuestions;

					// updates to alternate contact
					if (grantApplication.IsAlternateContact == true)
					{
						grantApplication.AlternateEmail = model.AlternateEmail;
						grantApplication.AlternateFirstName = model.AlternateFirstName;
						grantApplication.AlternateJobTitle = model.AlternateJobTitle;
						grantApplication.AlternateLastName = model.AlternateLastName;
						grantApplication.AlternatePhoneExtension = model.AlternatePhoneExtension;
						grantApplication.AlternatePhoneNumber = model.AlternatePhone;
						grantApplication.AlternateSalutation = model.AlternateSalutation;
					}
					else
					{
						grantApplication.AlternateEmail = null;
						grantApplication.AlternateFirstName = null;
						grantApplication.AlternateJobTitle = null;
						grantApplication.AlternateLastName = null;
						grantApplication.AlternatePhoneExtension = null;
						grantApplication.AlternatePhoneNumber = null;
						grantApplication.AlternateSalutation = null;
					}

					_grantStreamService.RemoveGrantStreamAnswers(grantApplication.Id);
					foreach (var question in dbModel.StreamEligibilityQuestions)
					{
						foreach (var clientQuestion in model.GrantStream.StreamEligibilityQuestions)
						{
							if (question.Id != clientQuestion.Id)
								continue;

							if (clientQuestion.EligibilityAnswer == null)
								continue;

							var answerWasAffirmative = clientQuestion.EligibilityAnswer.GetValueOrDefault(false);

							GrantWriterDesignation? grantWriterDesignation = null;
							if (answerWasAffirmative && clientQuestion.CollectContactInformation && clientQuestion.RequiresGrantWriter && clientQuestion.Designation.HasValue)
							{
								grantWriterDesignation = (GrantWriterDesignation)clientQuestion.Designation.Value;
							}

							grantApplication.GrantStreamEligibilityAnswers.Add(new GrantStreamEligibilityAnswer
							{
								GrantApplication = grantApplication,
								GrantApplicationId = grantApplication.Id,
								GrantStreamEligibilityQuestionId = question.Id,
								EligibilityAnswer = answerWasAffirmative,
								GrantWriterDesignation = grantWriterDesignation,
								ContactName = clientQuestion.CollectContactInformation && answerWasAffirmative ? clientQuestion.ContactName : null,
								ContactEmailAddress = clientQuestion.CollectContactInformation && answerWasAffirmative ? clientQuestion.ContactEmailAddress : null,
								ContactPhoneNumber = clientQuestion.CollectContactInformation && answerWasAffirmative ? clientQuestion.ContactPhoneNumber : null
							});
						}
					}

					if (originalStartDate != grantApplication.StartDate || originalEndDate != grantApplication.EndDate || originalGrantOpeningId != grantApplication.GrantOpeningId || originalEligibilityConfirmed != grantApplication.EligibilityConfirmed)
					{
						_grantApplicationService.ChangeGrantOpening(grantApplication);

						grantApplication.ParticipantForms
							.ForEach(x => x.ProgramStartDate = grantApplication.StartDate);

						// mark TrainingProgram state as Incomplete if dates are out of range
						grantApplication.TrainingPrograms
							.Where(tp => !tp.HasValidDates())
							.ForEach(x => x.TrainingProgramState = TrainingProgramStates.Incomplete);

						var originalReimbursement = grantApplication.ReimbursementRate;
						grantApplication.ReimbursementRate = grantApplication.GrantOpening.GrantStream.ReimbursementRate;
						grantApplication.MaxReimbursementAmt = grantApplication.GrantOpening.GrantStream.MaxReimbursementAmt;

						// if the reimbursement rate has changed, then mark the state of the training cost as incomplete and store the new rate
						if (Math.Abs(originalReimbursement - grantApplication.GrantOpening.GrantStream.ReimbursementRate) > TypeExtensions.FloatTolerance)
						{
							if (grantApplication.TrainingCost.TrainingCostState == TrainingCostStates.Complete || grantApplication.TrainingCost.EligibleCosts.Any() || grantApplication.TrainingCost.TotalEstimatedCost > 0)
								grantApplication.TrainingCost.TrainingCostState = TrainingCostStates.Incomplete;

							// also need to re-calculate the eligible costs
							grantApplication.RecalculateEstimatedCosts();
						}

						grantApplication.MarkWithdrawnAndReturnedApplicationAsIncomplete();

						_grantApplicationService.Update(grantApplication);
					}
					else if (grantApplication.ApplicationStateInternal == ApplicationStateInternal.ApplicationWithdrawn)
					{
						grantApplication.MarkWithdrawnAndReturnedApplicationAsIncomplete();
						_grantApplicationService.Update(grantApplication);
					}
					else if (originalIsAlternateContact != grantApplication.IsAlternateContact)
					{
						_grantApplicationService.Update(grantApplication);
					}
					else
					{
						_grantApplicationService.Update(grantApplication);
					}

					model = new ApplicationStartViewModel(grantApplication, _grantOpeningService, _grantProgramService, _staticDataService, _grantStreamService)
					{
						RedirectURL = Url.Action(nameof(ApplicationOverviewView), new { grantApplicationId = grantApplication.Id })
					};

					this.SetAlert("Grant Selection details are complete.", AlertType.Success, true);
				}
				else
				{
					HandleModelStateValidation(model);
				}
			}
			catch (Exception e)
			{
				HandleAngularException(e, model);
			}

			return Json(model);
		}

		/// <summary>
		/// Display the Grant Application overview View.
		/// This view is used when editing a grant application.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[Route("Application/Overview/View/{grantApplicationId}")]
		public ActionResult ApplicationOverviewView(int grantApplicationId)
		{
			ViewBag.GrantApplicationId = grantApplicationId;

			var grantApplication = _grantApplicationService.Get(grantApplicationId);
			if (grantApplication == null)
			{
				this.SetAlert("The application was not found.", AlertType.Warning, true);
				return RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", ""));
			}

			if (!grantApplication.ApplicationStateExternal.In(
					ApplicationStateExternal.NotStarted,
					ApplicationStateExternal.Incomplete,
					ApplicationStateExternal.Complete,
					ApplicationStateExternal.ApplicationWithdrawn,
					ApplicationStateExternal.NotAccepted))
			{
				this.SetAlert("The application overview page is not available when in current state.", AlertType.Warning, true);
				return RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", ""));
			}

			if (grantApplication.TrainingPrograms.Any(x => x.TrainingProgramState == TrainingProgramStates.Incomplete && !x.HasValidDates()))
				this.SetAlert("Skills training dates do not fall within your delivery period and will need to be rescheduled. Make sure all your skills training dates are accurate to your plan.", AlertType.Warning);

			if (grantApplication.IsSubmittable())
			{
				if (grantApplication.ApplicationStateExternal == ApplicationStateExternal.Incomplete)
				{
					grantApplication.ApplicationStateExternal = ApplicationStateExternal.Complete;
					_grantApplicationService.Update(grantApplication);
				}
			}
			else if (grantApplication.ApplicationStateExternal == ApplicationStateExternal.Complete)
			{
				grantApplication.ApplicationStateExternal = ApplicationStateExternal.Incomplete;
				_grantApplicationService.Update(grantApplication);
			}

			if (!grantApplication.CanReportParticipants && grantApplication.IsPIFSubmittable())
			{
				grantApplication.EnableParticipantReporting();
				_grantApplicationService.Update(grantApplication);
			}

			return View(SidebarViewModelFactory.Create(grantApplication, ControllerContext));
		}

		/// <summary>
		/// Get the data for the ApplicationOverviewView page.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/Overview/{grantApplicationId}")]
		public JsonResult GetApplicationOverview(int grantApplicationId)
		{
			var model = new ApplicationOverviewViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				model = new ApplicationOverviewViewModel(grantApplication, _settingService);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[Route("Application/Resume/{grantApplicationId}")]
		public ActionResult ApplicationResume(int grantApplicationId)
		{
			ViewBag.GrantApplicationId = grantApplicationId;

			var newId = _grantApplicationService.RestartApplicationFromWithdrawn(grantApplicationId);

			return RedirectToAction("ApplicationOverviewView", new { grantApplicationId = newId });
		}

		/// <summary>
		/// Delete the specified grant application.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="rowVersion"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/Delete/{grantApplicationId}")]
		public JsonResult DeleteApplication(int grantApplicationId, string rowVersion)
		{
			var model = new BaseApplicationViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);

				if (!grantApplication.ApplicationStateExternal.In(ApplicationStateExternal.Incomplete, ApplicationStateExternal.Complete)) throw new InvalidOperationException("Unable to delete application.");

				grantApplication.RowVersion = Convert.FromBase64String(rowVersion.Replace(" ", "+"));
				_grantApplicationService.Delete(grantApplication);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model);
		}
	}
}
