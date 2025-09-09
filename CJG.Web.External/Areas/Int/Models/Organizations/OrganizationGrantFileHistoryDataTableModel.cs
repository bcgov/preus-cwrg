using System;
using System.Linq;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;

namespace CJG.Web.External.Areas.Int.Models.Organizations
{
    public class OrganizationGrantFileHistoryDataTableModel
	{
		public int Id { get; set; }
		public string FileNumber { get; set; }
		public string CurrentStatus { get; set; }
		public string ApplicationStream { get; set; }
		public string ApplicantName { get; set; }
		public string ApplicantEmail { get; set; }
		public string TrainingProgramTitle { get; set; }
		public string StartDate { get; set; }
		public string EndDate { get; set; }
		public int NumberOfParticipants { get; set; }
		public int NumberOfParticipantsAgreed { get; set; }
		public decimal RequestedAmount { get; set; } = decimal.Zero;
		public decimal ApprovedAmount { get; set; } = decimal.Zero;
		public decimal PaidAmount { get; set; } = decimal.Zero;
		public decimal AverageCostPerParticipant { get; set; }
		public string RowVersionString { get; set; }

		public OrganizationGrantFileHistoryDataTableModel(GrantApplication grantApplication, IUserService userService)
		{
			Id = grantApplication.Id;
			FileNumber = grantApplication.FileNumber;
			CurrentStatus = grantApplication.ApplicationStateInternal.GetDescription();
			ApplicationStream = grantApplication.GrantOpening.GrantStream.Name;
			ApplicantName = grantApplication.ApplicantFirstName + " " + grantApplication.ApplicantLastName;
			ApplicantEmail = userService.GetUser(grantApplication.ApplicantBCeID).EmailAddress;
			TrainingProgramTitle = grantApplication.TrainingPrograms.FirstOrDefault()?.CourseTitle;
			StartDate = grantApplication.GetTrainingStartDate().ToLocalMorning().ToString("yyyy-MM-dd");
			EndDate = grantApplication.GetTrainingEndDate().ToLocalMorning().ToString("yyyy-MM-dd");
			NumberOfParticipants = grantApplication.TrainingCost.EstimatedParticipants;
			NumberOfParticipantsAgreed = grantApplication.TrainingCost.AgreedParticipants;
			RequestedAmount = grantApplication.TrainingCost.TotalEstimatedReimbursement;
			ApprovedAmount = grantApplication.TrainingCost.CalculateApprovedAmount();
			PaidAmount = grantApplication.Claims.Where(c => c.ClaimState.In(ClaimState.ClaimApproved, ClaimState.AmountReceived, ClaimState.ClaimPaid, ClaimState.PaymentRequested)).Sum(c => c.TotalAssessedReimbursement);
			AverageCostPerParticipant = ApprovedAmount / grantApplication.TrainingCost.GetEstimatedParticipants();
			RowVersionString = Convert.ToBase64String(grantApplication.RowVersion);
		}
	}
}