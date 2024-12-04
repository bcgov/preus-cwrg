using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;

namespace CJG.Web.External.Areas.Int.Models.SteamAgreementDetails
{
    public class SteamAgreementApplicationModel
	{
		public int GrantApplicationId { get; set; }
		public string FiscalYear { get; set; }
		public string GrantStreamName { get; set; }
		public string FileNumber { get; set; }
		public ApplicationStateInternal ApplicationStateInternal { get; set; }
		public string ApplicationStateInternalCaption { get; }
		public DateTime? DateStatusChangedToClosed { get; set; }
		public string Applicant { get; set; }
		public string TrainingCourseTitle { get; set; }
		public string ProjectDescription { get; set; }
		public string PublicProgramDescription { get; set; }
		public string TrainingProviderName { get; set; }
		public string ESSTrainingProviderName { get; set; }
		public string NAIC { get; set; }
		public string NOC { get; set; }
		public string NOCVersion { get; set; }

		public DateTime DeliveryStartDate { get; set; }
		public DateTime DeliveryEndDate { get; set; }
		public DateTime TrainingStartDate { get; set; }
		public DateTime TrainingEndDate { get; set; }

		public int RequestedNumberOfParticipants { get; set; }
		public string TrainingLocation { get; set; }
		public string ModeOfInstruction { get; set; }
		public string CommunityNames { get; set; }
		public string Region { get; set; }
		public decimal? TrainingCostRequested { get; set; }
		public decimal? ScheduleAAmount { get; set; }
		public decimal? TotalClaimAssessment { get; set; }
		public decimal? AverageCostPerParticipant { get; set; }
		public int? NumberOfPIFsInClaim { get; set; }

		public SteamAgreementApplicationModel() { }

		public SteamAgreementApplicationModel(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			GrantApplicationId = grantApplication.Id;
			FiscalYear = grantApplication.GrantOpening.TrainingPeriod.FiscalYear.Caption;
			GrantStreamName = grantApplication.GrantOpening.GrantStream.Name;
			FileNumber = grantApplication.FileNumber;
			ApplicationStateInternalCaption = grantApplication.GetInternalStateCaption();
			var applicationClosedDate = grantApplication.GetStateChange(ApplicationStateInternal.Closed);
			if (applicationClosedDate != null)
				DateStatusChangedToClosed = applicationClosedDate.ChangedDate;

			Applicant = grantApplication.OrganizationLegalName;

			var courseTitles = new List<string>();
			var trainingProviderNames = new List<string>();
			var trainingLocations = new List<string>();
			var deliveryMethods = new List<string>();

			foreach (var trainingProgram in grantApplication.TrainingPrograms)
			{
				courseTitles.Add(trainingProgram.CourseTitle);
				trainingProviderNames.Add(trainingProgram.TrainingProvider.Name);

				if (trainingProgram.TrainingProvider.TrainingAddress != null)
					trainingLocations.Add(AsString(trainingProgram.TrainingProvider.TrainingAddress));
				deliveryMethods.AddRange(trainingProgram.DeliveryMethods.Select(dm => dm.Caption));
			}

			var essTrainingProviderNames = new List<string>();
			foreach (var essTrainingProvider in grantApplication.TrainingProviders)
				essTrainingProviderNames.Add(essTrainingProvider.Name);

			TrainingCourseTitle = JoinLines(courseTitles, sort: false);
			TrainingProviderName = JoinLines(trainingProviderNames, sort: false);
			ESSTrainingProviderName = JoinLines(essTrainingProviderNames, sort: false);
			TrainingLocation = JoinLines(trainingLocations, doubleNewLine: true);
			ModeOfInstruction = JoinLines(deliveryMethods, sort: true);

			if (grantApplication.ProgramDescription != null)
			{
				ProjectDescription = grantApplication.ProgramDescription.Description;
				//PublicProgramDescription = grantApplication.ProgramDescription.PubliclyAvailableDescription;
				NAIC = grantApplication.ProgramDescription.TargetNAICS.Code;
				NOC = grantApplication.ProgramDescription.NationalOccupationalClassification.Code;
				NOCVersion = grantApplication.ProgramDescription.NationalOccupationalClassification.NOCVersion;

				var communities = grantApplication
					.ProgramDescription
					.Communities
					.OrderBy(c => c.Caption)
					.ToList();

                CommunityNames = JoinLines(communities.Select(c => c.GetCommunityName()));
                Region = JoinLines(communities.Select(c => c.GetRegionName()).OrderBy(r => r).Distinct());
            }

			DeliveryStartDate = grantApplication.StartDate;
            DeliveryEndDate = grantApplication.EndDate;
            TrainingStartDate = grantApplication.GetTrainingStartDate().ToLocalMorning();
            TrainingEndDate = grantApplication.GetTrainingEndDate().ToLocalMorning();

            RequestedNumberOfParticipants = grantApplication.TrainingCost.EstimatedParticipants;

            //TrainingCostRequested = grantApplication.TrainingCost?.TotalEstimatedCost;
            ScheduleAAmount = grantApplication.TrainingCost?.TotalAgreedMaxCost;

            var claim = grantApplication.GetCurrentClaim();

            if (claim != null)
            {
	            TotalClaimAssessment = claim.TotalAssessedReimbursement;
	            var averageCost = claim.EligibleCosts
		            .Where(c => c.EligibleExpenseType.ServiceCategory.ServiceTypeId.In(ServiceTypes.SkillsTraining, ServiceTypes.EmploymentServicesAndSupports))
		            .Sum(c => c.ClaimMaxParticipantCost);
	            AverageCostPerParticipant = averageCost;
            }

            NumberOfPIFsInClaim = grantApplication.ParticipantForms.Count;
		}

		public string AsString(ApplicationAddress address)
		{
			return
				address.AddressLine1 + Environment.NewLine +
				(string.IsNullOrWhiteSpace(address.AddressLine2) ? string.Empty : address.AddressLine2 + Environment.NewLine) +
				(string.IsNullOrWhiteSpace(address.City) ? string.Empty : address.City + Environment.NewLine) +
				(address.Region == null ? string.Empty : address.Region.Name + Environment.NewLine) +
				(string.IsNullOrWhiteSpace(address.PostalCode) ? string.Empty : address.PostalCode);
		}

		private static string JoinLines(IEnumerable<string> items, bool doubleNewLine = false, bool sort = true)
		{
			if (sort)
				items = items.OrderBy(s => s);

			var newLine = doubleNewLine ? $"{Environment.NewLine}{Environment.NewLine}" : Environment.NewLine;
			return string.Join(newLine, items.Distinct());
		}
    }
}