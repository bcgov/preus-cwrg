using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Web.External.Areas.Int.Models.Applications;
using CJG.Web.External.Areas.Int.Models.GrantPrograms;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models
{
	public class ApplicationDetailsViewModel : BaseViewModel
	{
		public string RowVersion { get; set; }

		public int GrantStreamId { get; set; }
		public int GrantProgramId { get; set; }
		public ProgramTypes ProgramType { get; set; }

		public DateTime TrainingPeriodStart { get; set; }
		public DateTime TrainingPeriodEnd { get; set; }
		public DateTime DeliveryStartDate { get; set; }

		public WorkflowViewModel WorkflowViewModel { get; set; }

		public bool ShowAttachments { get; }
		public bool ShowAssessors { get; }
		public bool ShowCompletionReport { get; }
		public bool ShowTrainingProviderChangeRequested { get; }
		public bool ShowTrainingProviders { get; }
		public bool ShowProgramDescription { get; }
		public bool ShowTrainingPrograms { get; }
		public bool ShowSkillsTraining { get; }
		public bool RequiresTrainingProviderValidation { get; set; }
		public bool RequiresProgramInitiative { get; set; }
		public bool RequiresCIPSValidation { get; set; }
		public bool CanManageParticipantEligibilty { get; set; }
		public bool RequiresNumParticipantsMatchNumApprovedParticipants { get; set; }
		public bool RequiresReviewOfAllParticipants { get; set; }
		public bool ShowESS { get; }
		public bool ShowParticipants { get; }
		public bool ShowClaims { get; }

		public bool IsAssessor { get; set; }
		public bool UnderAssessment { get; set; }
		public string AttachmentHeader { get; set; }
		public bool ShowApplicantReportingOfParticipantsButton { get; set; }
		public bool ShowParticipantReportingButton { get; set; }
		public bool ShowHoldPaymentRequestButton { get; set; }
		public bool HoldPaymentRequests { get; set; }
		public bool CanViewParticipants { get; set; }
		public bool CanReportParticipants { get; set; }
		public bool CanApplicantReportParticipants { get; set; }
		public int MaxParticipants { get; set; }
		public int TotalWithdrawals { get; set; }
		public int TotalParticipants { get; set; }
		public int TotalApprovedParticipants { get; set; }

		public bool EditSummary { get; set; }
		public bool EditApplicantContact { get; set; }
		public bool EditApplicant { get; set; }
		public bool EditAttachments { get; set; }
		public bool EditProgramDescription { get; set; }
		public bool EditTrainingProgram { get; set; }
		public bool AddRemoveTrainingPrograms { get; set; }
		public bool EditTrainingProvider { get; set; }
		public bool EditProviderServices { get; set; }
		public bool AddRemoveTrainingProviders { get; set; }
		public bool EditProgramCost { get; set; }
		public ClaimTypes ClaimType { get; set; }
		public bool HasAgreement { get; set; }
		public bool ScheduledNotificationsEnabled { get; set; }
		public bool? HasRequestedAdditionalFunding { get; set; }
		public string DescriptionOfFundingRequested { get; set; }
		public bool ShowSkillsTrainingFocusDropDown { get; set; }

		public bool ShowProofOfPayment { get; set; }
		public bool EditProofOfPayment { get; set; }
		public ProofOfPaymentState ProofOfPaymentState { get; set; }
		public string ProofOfPaymentStateDescription { get; set; }

		public bool ShowAttestation { get; set; }
		public bool EditAttestation { get; set; }
		public AttestationState AttestationState { get; set; }
		public string AttestationStateDescription { get; set; }

		public bool ShowAccountsReceivables { get; set; }
		public bool EditAccountsReceivables { get; set; }

		public bool ShowSuccessStories { get; set; }
		public bool EditSuccessStories { get; set; }
		public SuccessStoryState SuccessStoriesState { get; set; }
		public string SuccessStoriesStateDescription { get; set; }

		public bool EvaluationIsComplete { get; set; }
		public bool EvaluationIsEditableWhileComplete { get; set; }
		public bool AllEvaluationQuestionsAnswered { get; set; }
		public bool AllowEvaluationSubmission { get; set; }
		public bool AllowEvaluationWithdrawal { get; set; }
		public bool AllowEvaluationPrint { get; set; }

		public string DenialReason { get; set; }
		public List<int> DenialReasonsSelection { get; set; } = new List<int>();

		public IEnumerable<ApplicationComponentViewModel> Components { get; set; }
		public IEnumerable<GrantProgramDenialReasonViewModel> GrantProgramDenialReasons { get; set; }

		public ApplicationDetailsViewModel() { }

		public ApplicationDetailsViewModel(GrantApplication grantApplication, IPrincipal user, Func<string, string> getWorkflowUrl, ISuccessStoryService successStoryService)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			if (grantApplication.GrantOpening?.GrantStream == null)
				throw new ArgumentNullException(nameof(GrantStream));

			if (grantApplication.GrantOpening?.GrantStream?.GrantProgram == null)
				throw new ArgumentNullException(nameof(GrantProgram));

			Id = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);
			GrantStreamId = grantApplication.GrantOpening.GrantStreamId;
			GrantProgramId = grantApplication.GrantOpening.GrantStream.GrantProgramId;
			ProgramType = grantApplication.GetProgramType();

			TrainingPeriodStart = grantApplication.GrantOpening.TrainingPeriod.StartDate.ToLocalTime();
			TrainingPeriodEnd = grantApplication.GrantOpening.TrainingPeriod.EndDate.ToLocalTime();
			DeliveryStartDate = grantApplication.StartDate.ToLocalTime();

			WorkflowViewModel = new WorkflowViewModel(grantApplication, user, getWorkflowUrl);

			RequiresProgramInitiative = grantApplication.RequiresProgramInitiative();
			RequiresCIPSValidation = grantApplication.RequiresCIPSValidation();
			RequiresTrainingProviderValidation = grantApplication.RequiresTrainingProviderValidation();

			CanManageParticipantEligibilty = ProgramType == ProgramTypes.EmployerGrant && grantApplication.RequireAllParticipantsBeforeSubmission;

			RequiresNumParticipantsMatchNumApprovedParticipants = CanManageParticipantEligibilty && grantApplication.RequiresNumParticipantsMatchNumApprovedParticipants();
			RequiresReviewOfAllParticipants = CanManageParticipantEligibilty && grantApplication.ParticipantForms.Any(a => !a.Approved.HasValue);

			ShowAttachments = grantApplication.GrantOpening?.GrantStream?.AttachmentsIsEnabled == true || grantApplication.Attachments.Count > 0;
			ShowAssessors = grantApplication.ApplicationStateInternal == ApplicationStateInternal.PendingAssessment;
			ShowCompletionReport = grantApplication.HasBeenApproved()
			                       && (grantApplication.EndDate < AppDateTime.UtcNow
			                           || grantApplication.ApplicationStateInternal.In(ApplicationStateInternal.Closed, ApplicationStateInternal.CompletionReporting));

			ShowTrainingProviderChangeRequested = grantApplication.ApplicationStateExternal == ApplicationStateExternal.ChangeRequestSubmitted;
			ShowProgramDescription = ProgramType == ProgramTypes.WDAService;
			ShowTrainingProviders = ProgramType == ProgramTypes.EmployerGrant;
			ShowTrainingPrograms = ProgramType == ProgramTypes.EmployerGrant;
			AttachmentHeader = grantApplication.GrantOpening.GrantStream.AttachmentsHeader;

			// This isn't technically correct
			ShowSkillsTraining = ProgramType == ProgramTypes.WDAService;

			// This isn't technically correct
			ShowESS = ProgramType == ProgramTypes.WDAService;

			ShowParticipants = grantApplication.ParticipantForms.Count > 0
				|| user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EnableApplicantReportingOfParticipants)
				|| user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EnableParticipantReporting);

			var applicationHasSubmittedClaims = grantApplication.Claims.Any() && grantApplication.HasSubmittedAClaim();
			ShowClaims = applicationHasSubmittedClaims;

			IsAssessor = user.GetUserId() == grantApplication.AssessorId;
			UnderAssessment = grantApplication?.ApplicationStateInternal.In(ApplicationStateInternal.New,
																			ApplicationStateInternal.PendingAssessment,
																			ApplicationStateInternal.UnderAssessment,
																			ApplicationStateInternal.ReturnedToAssessment,
																			ApplicationStateInternal.RecommendedForApproval,
																			ApplicationStateInternal.RecommendedForDenial) == true;

			ShowApplicantReportingOfParticipantsButton = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EnableApplicantReportingOfParticipants);
			ShowParticipantReportingButton = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EnableParticipantReporting);
			ShowHoldPaymentRequestButton = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.HoldPaymentRequests);
			HoldPaymentRequests = grantApplication.HoldPaymentRequests;
			CanViewParticipants = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.ViewParticipants);
			CanReportParticipants = grantApplication.CanReportParticipants;
			CanApplicantReportParticipants = grantApplication.CanApplicantReportParticipants;
			TotalParticipants = grantApplication.ParticipantForms.Count;
			TotalApprovedParticipants = grantApplication.ParticipantForms.Count(w => w.Approved.HasValue && w.Approved.Value);
			MaxParticipants = grantApplication.TrainingCost.AgreedParticipants;
			TotalWithdrawals = grantApplication.ParticipantForms.Count(pf => pf.WithdrawalKey != null);

			Components = grantApplication.TrainingCost.EligibleCosts.Where(ec => ec.EligibleExpenseType.ServiceCategory?.ServiceTypeId != ServiceTypes.Administration).Select(ec => new ApplicationComponentViewModel(ec, user)).OrderBy(c => c.RowSequence).ToArray();

			EditSummary = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditSummary);
			EditApplicantContact = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditApplicantContact);
			EditApplicant = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditApplicant);
			EditAttachments = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditApplicationAttachments);
			EditProgramDescription = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditProgramDescription);
			EditTrainingProvider = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditTrainingProvider);
			EditTrainingProgram = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditTrainingProgram);
			AddRemoveTrainingPrograms = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.AddOrRemoveTrainingProgram);
			EditProgramCost = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditTrainingCosts);
			EditProviderServices = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditTrainingCosts);
			AddRemoveTrainingProviders = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.AddRemoveTrainingProvider);

			ClaimType = grantApplication.GrantOpening.GrantStream.ProgramConfiguration.ClaimTypeId;
			HasAgreement = grantApplication.GrantAgreement != null;
			ScheduledNotificationsEnabled = grantApplication.ScheduledNotificationsEnabled;
			HasRequestedAdditionalFunding = grantApplication.HasRequestedAdditionalFunding;
			DescriptionOfFundingRequested = grantApplication.DescriptionOfFundingRequested;

			GrantProgramDenialReasons = grantApplication.GrantOpening.GrantStream.GrantProgram.DenialReasons
				.Where(r => r.IsActive)
				.Select(nt => new GrantProgramDenialReasonViewModel(nt))
				.ToArray();

			ShowSkillsTrainingFocusDropDown = grantApplication.ShowSkillsTrainingFocusDropDown();

			ShowProofOfPayment = applicationHasSubmittedClaims;
			EditProofOfPayment = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditProofOfPayments);
			ProofOfPaymentState = grantApplication.ProofOfPayment?.State ?? ProofOfPaymentState.Incomplete;
			ProofOfPaymentStateDescription = ProofOfPaymentState.GetDescription();

			ShowAttestation = applicationHasSubmittedClaims;
			EditAttestation = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditAttestation);
			AttestationState = grantApplication.Attestation?.State ?? AttestationState.Incomplete;
			AttestationStateDescription = AttestationState.GetDescription();

			ShowAccountsReceivables = true;
			EditAccountsReceivables = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditClaimAR);

			var successStory = successStoryService.GetSuccessStory(grantApplication.Id);
			ShowSuccessStories = applicationHasSubmittedClaims;
			EditSuccessStories = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditProofOfPayments);
			SuccessStoriesState = successStory?.State ?? SuccessStoryState.Incomplete;
			SuccessStoriesStateDescription = (successStory?.State ?? SuccessStoryState.Incomplete).GetDescription();

			EvaluationIsComplete = !grantApplication.RequiresEvaluationCompletion();
			EvaluationIsEditableWhileComplete = grantApplication.ApplicationStateInternal == ApplicationStateInternal.RecommendedForApproval;
			AllowEvaluationSubmission = grantApplication.RequiresEvaluationCompletion();

			if (grantApplication.ApplicationStateInternal == ApplicationStateInternal.RecommendedForDenial)
			{ 
				DenialReason = grantApplication.GetDeniedReason();
	            DenialReasonsSelection = grantApplication.GrantApplicationDenialReasons
		            .Select(dr => dr.Id)
		            .ToList();
			}
		}
	}
}
