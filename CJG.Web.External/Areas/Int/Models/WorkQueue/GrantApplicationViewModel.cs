using System;
using System.Linq;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.WorkQueue
{
    public class GrantApplicationViewModel : BaseViewModel
	{
		public string RowVersion { get; set; }
		public bool ShowPIFWarning { get; set; }
		public string FileNumber { get; set; }
		public int? AssessorId { get; set; }
		public string Assessor { get; set; }
		public string Applicant { get; set; }
		public DateTime? DateSubmitted { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime TrainingStartDate { get; set; }
		public ApplicationStateInternal ApplicationStateInternal { get; set; }
		public string ApplicationStateInternalCaption { get; }
		public DateTime StatusChanged { get; set; }
		public string GrantStreamName { get; set; }
		public string ProgramInitiativeName { get; set; }
		public bool RiskFlag { get; set; }
		public int OrgId { get; set; }

		public GrantApplicationViewModel() { }

		public GrantApplicationViewModel(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			Id = grantApplication.Id;
			ShowPIFWarning = ShowPifWarning(grantApplication);
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);
			FileNumber = grantApplication.FileNumber;
			AssessorId = grantApplication.AssessorId;
			Assessor = $"{grantApplication.Assessor?.FirstName} {grantApplication.Assessor?.LastName}";
			Applicant = grantApplication.OrganizationLegalName;
			DateSubmitted = grantApplication.DateSubmitted;
			StartDate = grantApplication.StartDate;
			TrainingStartDate = grantApplication.GetTrainingStartDate().ToLocalMorning();
			ApplicationStateInternal = grantApplication.ApplicationStateInternal;

			var lastStateChange = grantApplication.StateChanges?
				.OrderByDescending(s => s.DateAdded)
				.FirstOrDefault();

			if (lastStateChange != null)
				StatusChanged = lastStateChange.ChangedDate.ToLocalTime();

			GrantStreamName = grantApplication.GrantOpening.GrantStream.Name;
			if (grantApplication.ProgramInitiative != null)
				ProgramInitiativeName = grantApplication.ProgramInitiative.Name;
			RiskFlag = grantApplication.Organization.RiskFlag;
			OrgId = grantApplication.Organization.Id;

			ApplicationStateInternalCaption = grantApplication.GetInternalStateCaption();
		}

		public bool ShowPifWarning(GrantApplication grantApplication)
		{
			if (grantApplication.ApplicationStateInternal != ApplicationStateInternal.AgreementAccepted)
				return false;

			var requiredPifs = grantApplication.TrainingCost?.EstimatedParticipants;
			var currentPifs = grantApplication.ParticipantForms?.Count ?? 0;

			var currentDate = AppDateTime.Now;
			var trainingStartDate = grantApplication.GetTrainingStartDate();

			if (currentDate < trainingStartDate)
				return false;

			if (currentDate > trainingStartDate.AddDays(8))
				return false;

			if (currentPifs >= requiredPifs)
				return false;

			return true;
		}
	}
}
