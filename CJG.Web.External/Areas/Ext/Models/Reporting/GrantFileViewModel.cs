using System;
using System.Linq;
using System.Security.Principal;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Ext.Models.Claims;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Ext.Models.Reporting
{
    public class GrantFileViewModel : BaseViewModel
	{
		public int ClaimId { get; set; }
		public int ClaimVersion { get; set; }
		public ProgramTitleLabelViewModel ProgramTitleLabel { get; set; }
		public ClaimAssessmentOutcomeViewModel ClaimAssessmentOutcome { get; set; }
		public SidebarViewModel SidebarViewModel { get; set; }
		public ClaimTypes ClaimType { get; set; }
		public bool AllowParticipantReport { get; set; }
		public DateTime ParticipantDueDate { get; set; }
		public int MaxParticipants { get; set; }
		public int ParticipantCount { get; set; }
		public int ParticipantsWithCostCount { get; set; }
		public bool HasClaim { get; set; }
		public ClaimState? CurrentClaimState { get; set; }
		public bool AllowClaimReport { get; set; }
		public DateTime ClaimDueDate { get; set; }
		public bool AllowReviewAndSubmit { get; set; }

		public DateTime SuccessStoryDueDate { get; set; }
		public SuccessStoryStatusViewModel SuccessStory { get; set; } = new SuccessStoryStatusViewModel();
		public bool AllowSuccessStoryReporting { get; set; }

		public DateTime CompletionDueDate { get; set; }
		public CompletionReportStatusViewModel CompletionReport { get; set; } = new CompletionReportStatusViewModel();
		public bool AllowReportCompletion { get; set; }

		public bool EnableSubmit { get; set; }
		public bool IsFinalClaim { get; set; }
		public ProgramTypes ProgramType { get; set; }
		public bool RequireAllParticipantsBeforeSubmission { get; set; }

		public bool AllowProofOfPayment { get; set; }
		public DateTime ProofOfPaymentDueDate { get; set; }
		public ProofOfPaymentStatusViewModel ProofOfPayment { get; set; } = new ProofOfPaymentStatusViewModel();

		public bool AllowAttestations { get; set; }
		public DateTime AttestationDueDate { get; set; }
		public Models.AttestationViewModel Attestation { get; set; } = new Models.AttestationViewModel();

		public GrantFileViewModel()
		{
		}

		public GrantFileViewModel(GrantApplication grantApplication, IParticipantService participantService, ICompletionReportService completionReportService, ISuccessStoryService successStoryService, IPrincipal user)
		{
			if (participantService == null)
				throw new ArgumentNullException(nameof(participantService));

			if (completionReportService == null)
				throw new ArgumentNullException(nameof(completionReportService));

			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			Id = grantApplication.Id;
			ClaimType = grantApplication.GrantOpening.GrantStream.ProgramConfiguration.ClaimTypeId;
			ProgramType = grantApplication.GrantOpening.GrantStream.GrantProgram.ProgramTypeId;
			ProgramTitleLabel = new ProgramTitleLabelViewModel(grantApplication);
			ParticipantDueDate = grantApplication.GetParticipantReportingDueDate();
			MaxParticipants = grantApplication.TrainingCost.GetMaxParticipants();

			ParticipantCount = grantApplication.RequireAllParticipantsBeforeSubmission
				? grantApplication.ParticipantForms.Count(pf => !pf.IsExcludedFromClaim && pf.Approved.HasValue && pf.Approved.Value)
				: grantApplication.ParticipantForms.Count(pf => !pf.IsExcludedFromClaim);

			RequireAllParticipantsBeforeSubmission = grantApplication.RequireAllParticipantsBeforeSubmission;

			var claim = grantApplication.GetCurrentClaim();
			var successStory = successStoryService.GetSuccessStory(grantApplication.Id);

			if (ClaimType == ClaimTypes.SingleAmendableClaim)
				ParticipantsWithCostCount = claim == null ? 0 : participantService.GetParticipantsWithClaimEligibleCostCount(claim.Id, claim.ClaimVersion);

			if (claim != null)
			{
				HasClaim = true;
				ClaimId = claim.Id;
				ClaimVersion = claim.ClaimVersion;
				CurrentClaimState = claim.ClaimState;
				IsFinalClaim = claim.IsFinalClaim;
			}

			ClaimDueDate = grantApplication.StartDate.AddDays(30);
			CompletionDueDate = grantApplication.EndDate.AddDays(30);

			var completionReportStatus = completionReportService.GetCompletionReportStatus(Id);
			CompletionReport.CompletionReportStatus = (CompletionReportStatus) Enum.Parse(typeof(CompletionReportStatus), completionReportStatus.Replace(" ", string.Empty));

			AllowReportCompletion = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.SubmitCompletionReport)
			                        && AppDateTime.UtcNow.ToUtcMidnight() >= grantApplication.EndDate
			                        && grantApplication.ApplicationStateInternal == ApplicationStateInternal.CompletionReporting;

			ClaimAssessmentOutcome = new ClaimAssessmentOutcomeViewModel(grantApplication);

			DateTime? claimStateDate = null;
			var claimApproved = grantApplication.HasPriorApprovedClaim();
			var lastApprovedClaim = grantApplication.GetPriorApprovedClaim();

			if (lastApprovedClaim != null)
				claimStateDate = lastApprovedClaim.GrantApplication.GetStateChange(ApplicationStateInternal.ClaimApproved).ChangedDate;

			var proofOfPayment = grantApplication.ProofOfPayment;

			AllowProofOfPayment = claimApproved;
			AllowAttestations = claimApproved;

			ProofOfPaymentDueDate = (claimStateDate ?? grantApplication.StartDate).AddDays(30);
			AttestationDueDate = grantApplication.EndDate.AddDays(30);

			ProofOfPayment = new ProofOfPaymentStatusViewModel { ProofOfPaymentStatus = ProofOfPaymentReportStatus.NotStarted };
			if (proofOfPayment != null)
			{
				ProofOfPayment.ProofOfPaymentStatus = proofOfPayment.State == ProofOfPaymentState.Complete
					? ProofOfPaymentReportStatus.Complete
					: ProofOfPaymentReportStatus.Incomplete;
			}

			Attestation = new Models.AttestationViewModel { AttestationReportStatus = AttestationReportStatus.NotStarted };

			var attestation = grantApplication.Attestation;
			if (attestation != null)
			{
				Attestation.AttestationReportStatus = attestation.State == AttestationState.Complete
					? AttestationReportStatus.Complete
					: AttestationReportStatus.Incomplete;
			}

			// Override Report Completion to include the Proof of Payment and Attestation states
			var proofOfPaymentComplete = proofOfPayment != null && proofOfPayment.State == ProofOfPaymentState.Complete;
			var attestationComplete = attestation != null && attestation.State == AttestationState.Complete;
			var successStoryComplete = successStory != null && successStory.State == SuccessStoryState.Complete;

			SuccessStory = new SuccessStoryStatusViewModel(successStory);
			SuccessStoryDueDate = grantApplication.EndDate.AddDays(30);

			var canAccessSuccessStories = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.SubmitCompletionReport)
			                              && grantApplication.ApplicationStateInternal == ApplicationStateInternal.CompletionReporting;

			AllowSuccessStoryReporting = canAccessSuccessStories
										 && proofOfPaymentComplete
			                             && attestationComplete;

			AllowReportCompletion = AllowReportCompletion
			                        && proofOfPaymentComplete
			                        && attestationComplete
			                        && successStoryComplete;
		}
	}
}
