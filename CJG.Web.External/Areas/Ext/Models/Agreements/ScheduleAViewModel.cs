using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Application.Business.Models;
using CJG.Core.Entities;
using CJG.Web.External.Areas.Ext.Models.ChangeRequest;

namespace CJG.Web.External.Areas.Ext.Models.Agreements
{
	public class ScheduleAViewModel : GrantAgreementDocumentViewModel
	{
		public string AgreementNumber { get; set; }
		public string ApplicantName { get; set; }
		public ProgramTypes ProgramType { get; set; }
		public DateTime ParticipantReportingDueDate { get; set; }
		public DateTime ReimbursementClaimDueDate { get; set; }
		public int NumberOfParticipant { get; set; }

		public DeliveryDateViewModel DeliveryDate { get; set; }

		public IEnumerable<AgreementTrainingProgramViewModel> TrainingPrograms { get; set; }
		public IEnumerable<AgreementTrainingProviderViewModel> TrainingProviders { get; set; }

		public string CourseTitle { get; set; }
		public string TrainingProviderName { get; set; }
		public int TrainingProviderId { get; set; }

		public string RequestTrainingProviderRowVersion { get; set; }
		public string RequestTrainingProviderName { get; set; }
		public int RequestTrainingProviderId { get; set; }

		public IEnumerable<ChangeRequestSkillTrainingViewModel> SkillTrainings { get; set; } = new List<ChangeRequestSkillTrainingViewModel>();
		public IEnumerable<ChangeRequestESSViewModel> ESSComponents { get; set; } = new List<ChangeRequestESSViewModel>();

		public IEnumerable<EligibleCostModel> EligibleCosts { get; set; }
		public decimal TotalAgreedEmployerContribution { get; set; }
		public decimal TotalAgreedCost { get; set; }
		public decimal TotalAgreedMaxReimbursement { get; set; }
		public decimal ESSAveragePerParticipant { get; set; }
		public bool ShowAgreedCosts { get; set; }
		public bool ShowContributionColumn { get; set; }
		public DateTime GrantOpeningTrainingPeriodStartDate { get; set; }
		public DateTime GrantOpeningTrainingPeriodEndDate { get; set; }
		public string FiscalYearDisplay { get; set; }
		public DateTime ClaimDeadline { get; set; } = DateTime.Now;

		public ScheduleAViewModel()
		{

		}

		public ScheduleAViewModel(GrantApplication grantApplication) : base(grantApplication, ga => ga.ScheduleA)
		{
			Confirmation = grantApplication.GrantAgreement.ScheduleAConfirmed;
			AgreementNumber = grantApplication.FileNumber;
			ApplicantName = grantApplication.Organization?.LegalName;

			ProgramType = grantApplication.GetProgramType();
			ShowAgreedCosts = grantApplication.ApplicationStateInternal.ShowAgreedCosts();
			NumberOfParticipant = ShowAgreedCosts ? grantApplication.TrainingCost.AgreedParticipants : grantApplication.TrainingCost.EstimatedParticipants;
			DeliveryDate = new DeliveryDateViewModel(grantApplication);

			SkillTrainings = grantApplication.TrainingPrograms
				.Where(x => x.EligibleCostBreakdown.IsEligible)
				.Select(x => new ChangeRequestSkillTrainingViewModel(x))
				.ToArray();
			ESSComponents = grantApplication.TrainingCost.EligibleCosts
				.Where(x => x.EligibleExpenseType.ServiceCategory.ServiceTypeId == ServiceTypes.EmploymentServicesAndSupports)
				.OrderBy(ec => ec.EligibleExpenseType.RowSequence)
				.Select(x => new ChangeRequestESSViewModel(x)).ToArray();
			ESSAveragePerParticipant = grantApplication.TrainingCost.EligibleCosts
				.Where(x => x.EligibleExpenseType.ServiceCategory.ServiceTypeId == ServiceTypes.EmploymentServicesAndSupports)
				.Sum(x => x.EstimatedParticipantCost);

			TrainingPrograms = grantApplication.TrainingPrograms
				.Where(tp => tp.EligibleCostBreakdown?.IsEligible ?? true)
				.Select(tp => new AgreementTrainingProgramViewModel(tp))
				.ToArray();
			TrainingProviders = grantApplication.TrainingProviders
				.Where(tp => tp.ApprovedTrainingProvider.IsValidated())
				.Select(tp => tp.ApprovedTrainingProvider)
				.Distinct()
				.Select(tp => new AgreementTrainingProviderViewModel(tp))
				.ToArray();

			TotalAgreedCost = grantApplication.TrainingCost.TotalAgreedMaxCost;
			TotalAgreedEmployerContribution = grantApplication.CalculateAgreedEmployerContribution();
			TotalAgreedMaxReimbursement = grantApplication.CalculateAgreedMaxReimbursement();
			EligibleCosts = grantApplication.TrainingCost.EligibleCosts
				.Select(x => new EligibleCostModel(x))
				.ToArray();
			ShowContributionColumn = grantApplication.ReimbursementRate != 1;
			GrantOpeningTrainingPeriodStartDate = grantApplication.GrantOpening.TrainingPeriod.StartDate.ToLocalTime();
			GrantOpeningTrainingPeriodEndDate = grantApplication.GrantOpening.TrainingPeriod.EndDate.ToLocalTime();
			FiscalYearDisplay = $"{grantApplication.GrantOpening.TrainingPeriod.FiscalYear.StartDate.ToLocalTime().ToString("yyyy-MM-dd")} to {grantApplication.GrantOpening.TrainingPeriod.FiscalYear.EndDate.ToLocalTime().ToString("yyyy-MM-dd")}";
			ClaimDeadline = grantApplication.GrantAgreement.GetClaimSubmissionDeadline();
		}
	}
}