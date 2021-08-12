using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Ext.Models.ParticipantReporting
{
	public class ReportingViewModel : BaseViewModel
	{
		#region Properties
		public int GrantApplicationId { get; set; }

		public string ClaimRowVersion { get; set; }

		public string GrantProgramName { get; set; }

		public string GrantProgramCode { get; set; }

		public bool CanApplicantReportParticipants { get; set; }

		public Guid InvitationKey { get; set; }

		public string InvitationEmailText { get; set; }

		public string InvitationBrowserLink { get; set; }

		public bool ParticipantsEditable { get; set; }
		public bool ParticipantsCanWithdraw { get; set; }

		public ApplicationStateInternal ApplicationStateInternal { get; set; }

		public ApplicationStateExternal ApplicationStateExternal { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public DateTime ParticipantReportingDueDate { get; set; }

		public int MaxParticipantsAllowed { get; set; }

		public bool AllowIncludeAll { get; set; }

		public IEnumerable<ParticipantViewModel> Participants { get; set; } = new List<ParticipantViewModel>();

		public ProgramTitleLabelViewModel ProgramTitleLabel { get; set; }

		public ProgramTypes ProgramType { get; set; }
		public bool ShowEligibility { get; set; }
		#endregion

		public ReportingViewModel()
		{

		}

		public ReportingViewModel(GrantApplication grantApplication, HttpContextBase context)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			GrantApplicationId = grantApplication?.Id ?? throw new ArgumentNullException(nameof(grantApplication));

			var currentClaim = grantApplication.GetCurrentClaim();

			ClaimRowVersion = currentClaim != null ? Convert.ToBase64String(currentClaim.RowVersion) : null;
			GrantProgramName = grantApplication?.GrantOpening?.GrantStream?.GrantProgram?.Name ?? throw new ArgumentNullException(nameof(grantApplication), "The argument 'grantApplication' must provide the grant program name.");
			GrantProgramCode = grantApplication?.GrantOpening?.GrantStream?.GrantProgram?.ProgramCode ?? throw new ArgumentNullException(nameof(grantApplication), "The argument 'grantApplication' must provide the grant program code.");
			CanApplicantReportParticipants = grantApplication.CanApplicantReportParticipants && grantApplication.CanReportParticipants;
			InvitationKey = grantApplication.InvitationKey;
			ApplicationStateInternal = grantApplication.ApplicationStateInternal;
			ApplicationStateExternal = grantApplication.ApplicationStateExternal;
			StartDate = grantApplication.StartDate.ToLocalTime();
			EndDate = grantApplication.EndDate.ToLocalTime();

			var trainingStartDate = grantApplication.GetTrainingStartDate();
			if (grantApplication.TrainingPrograms?.Count > 0)
				ParticipantReportingDueDate = trainingStartDate.AddDays(-5);

			MaxParticipantsAllowed = grantApplication.GetMaxParticipants();
			ShowEligibility = grantApplication.CanViewParticipantEligibilty();

			string baseUrlFragment = context.Request.Url?.GetLeftPart(UriPartial.Authority);
			Participants = grantApplication.ParticipantForms
												.OrderBy(pe => pe.LastName)
												.ThenBy(pe => pe.FirstName)
												.Select(pf => new ParticipantViewModel(pf, ShowEligibility, currentClaim, baseUrlFragment))
												.ToArray();
			var withdrawAbleStatuses = new List<ApplicationStateInternal>
			{
				ApplicationStateInternal.ClaimApproved,
				ApplicationStateInternal.CompletionReporting
			};
			var canBeWithdrawn = withdrawAbleStatuses.Contains(grantApplication.ApplicationStateInternal);

			ParticipantsEditable = context.User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditParticipants);
			ParticipantsCanWithdraw = canBeWithdrawn;
			AllowIncludeAll = Participants.Any(pf => pf.ClaimReported) && ApplicationStateExternal.In(ApplicationStateExternal.ClaimReturned, ApplicationStateExternal.Approved, ApplicationStateExternal.AmendClaim, ApplicationStateExternal.ClaimApproved, ApplicationStateExternal.ClaimDenied);

			ProgramTitleLabel = new ProgramTitleLabelViewModel(grantApplication, false);

			ProgramType = grantApplication.GrantOpening.GrantStream.GrantProgram.ProgramTypeId;
			InvitationBrowserLink = $"{baseUrlFragment}/Part/Information/{HttpUtility.UrlEncode(grantApplication.InvitationKey.ToString())}";

			var invitation = $"As this training is being funded through the {GrantProgramName}, you must complete a participant information form using the following link:\r\n\r\n{InvitationBrowserLink}\r\n\r\n";
			InvitationEmailText =
				"Dear {{participant}},\r\n\r\n" +
				"You have been identified as a participant for the following training program:\r\n\r\n" +
				$"{grantApplication.GetProgramDescription()}\r\n" +
				$"Start Date: {trainingStartDate.ToLocalMorning():yyyy-MM-dd}\r\n" +
				$"Location: {grantApplication.GetProviderLocation()}\r\n\r\n" +
				$"{invitation}" +
				"Please use a current version of Chrome or Firefox to enter participant information.\r\n\r\n" +
				$"Please complete your participant information form prior to midnight on {ParticipantReportingDueDate:yyyy-MM-dd}. " +
				"If you do not complete this form, you may not be able to participate in the training.";
		}
	}
}