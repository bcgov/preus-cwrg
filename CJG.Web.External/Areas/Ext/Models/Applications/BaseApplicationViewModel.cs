using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;
using CJG.Web.External.Areas.Ext.Models.ParticipantReporting;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Ext.Models
{
	public class BaseApplicationViewModel : BaseViewModel
	{
		public string RowVersion { get; set; }
		public string FileNumber { get; set; }
		public string FileName { get; set; }
		public string FullName { get; set; }
		public string GrantProgramName { get; set; }
		public string GrantStreamName { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public ApplicationStateExternal ApplicationStateExternal { get; set; }
		public GrantOpeningStates GrantOpeningState { get; set; }
		public DateTime? GrantOpeningOpeningDate { get; set; }
		public ProgramTypes ProgramType { get; set; }
		public DateTime GrantOpeningTrainingPeriodStartDate { get; set; }
		public DateTime GrantOpeningTrainingPeriodEndDate { get; set; }
		public bool EligibilityConfirmed { get; set; }
		public bool HasValidDate { get; set; }
		public OverviewTrainingProvider TrainingProvider { get; set; }
		public OverviewTrainingProgram TrainingProgram { get; set; }
		public OverviewProgramDescription ProgramDescription { get; set; }
		public OverviewTrainingCost TrainingCost { get; set; }
		public IEnumerable<DynamicProcess> DynamicProcesses { get; set; }
		public int EstimatedParticipants { get; set; }
		public bool EnableAttachments { get; set; }
		public string AttachmentsHeader { get; set; }
		public int AttachmentsState { get; set; }
		public int AttachmentCount { get; set; }
		public bool AttachmentsRequired { get; set; }
		public bool CanReportParticipants { get; set; }
		public bool EnableBusinessCase { get; set; }
		public bool BusinessCaseRequired { get; set; }
		public string BusinessCaseHeader { get; set; }
		public int BusinessCaseState { get; set; }
		public bool HasBusinessCase { get; }
		public List<ParticipantViewModel> Participants { get; set; }
		public bool PIFCompletionConfirmed { get; }
		public bool ShowEligibility { get; set; }

		public BaseApplicationViewModel()
		{
		}

		public BaseApplicationViewModel(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			Id = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);
			FileNumber = grantApplication.FileNumber;
			FileName = string.IsNullOrWhiteSpace(grantApplication.TrainingPrograms.FirstOrDefault()?.CourseTitle)
				? "Training Project Title"
				: grantApplication.TrainingPrograms.FirstOrDefault().CourseTitle;
			GrantProgramName = grantApplication.GrantOpening.GrantStream.GrantProgram.Name;
			ProgramType = grantApplication.GrantOpening.GrantStream.GrantProgram.ProgramTypeId;
			GrantStreamName = grantApplication.GrantOpening.GrantStream.Name;
			GrantOpeningOpeningDate = grantApplication.GrantOpening.OpeningDate == DateTime.MinValue ? null : (DateTime?)grantApplication.GrantOpening.OpeningDate.ToLocalMorning();
			StartDate = grantApplication.StartDate.ToLocalTime();
			EndDate = grantApplication.EndDate.ToLocalTime();
			ApplicationStateExternal = grantApplication.ApplicationStateExternal;
			GrantOpeningState = grantApplication.GrantOpening.State;

			GrantOpeningTrainingPeriodStartDate = grantApplication.GrantOpening.TrainingPeriod.StartDate;
			GrantOpeningTrainingPeriodEndDate = grantApplication.GrantOpening.TrainingPeriod.EndDate;

			FullName = grantApplication.GrantOpening.GrantStream.FullName;
			EligibilityConfirmed = grantApplication.EligibilityConfirmed();
			HasValidDate = grantApplication.HasValidDates(grantApplication.GrantOpening.TrainingPeriod.StartDate, grantApplication.GrantOpening.TrainingPeriod.EndDate);
			EnableAttachments = grantApplication.GrantOpening.GrantStream.AttachmentsIsEnabled;
			AttachmentsRequired = grantApplication.GrantOpening.GrantStream.AttachmentsRequired;
			AttachmentsHeader = grantApplication.GrantOpening.GrantStream.AttachmentsHeader;
			AttachmentCount = grantApplication.Attachments.Count();

			AttachmentsState =
				AttachmentsRequired
				&& grantApplication.ApplicationStateExternal == ApplicationStateExternal.ApplicationWithdrawn
					? 1
					: grantApplication.HasMetAttachmentRequirements()
						? 2
						: 0;

			CanReportParticipants = grantApplication.CanReportParticipants;
			EnableBusinessCase = grantApplication.GrantOpening.GrantStream.BusinessCaseIsEnabled;
			BusinessCaseRequired = grantApplication.GrantOpening.GrantStream.BusinessCaseRequired;
			BusinessCaseHeader = grantApplication.GrantOpening.GrantStream.BusinessCaseExternalHeader;
			BusinessCaseState = BusinessCaseRequired && grantApplication.ApplicationStateExternal == ApplicationStateExternal.ApplicationWithdrawn ? 1 : (grantApplication.BusinessCaseDocument != null) ? 2 : 0;
			HasBusinessCase = grantApplication.BusinessCaseDocument != null;

			ShowEligibility = grantApplication.CanViewParticipantEligibilty();
			Participants = new List<ParticipantViewModel>();
			foreach (var participantForm in grantApplication.ParticipantForms)
				Participants.Add(new ParticipantViewModel(participantForm, ShowEligibility));

			PIFCompletionConfirmed = grantApplication.PIFCompletionConfirmed();			

			if (ProgramType == ProgramTypes.EmployerGrant)
			{
				var trainingProgram = grantApplication.TrainingPrograms.FirstOrDefault();
				if (trainingProgram != null)
				{
					TrainingProgram = new OverviewTrainingProgram(trainingProgram);
				}

				var trainingProvider = grantApplication.TrainingProviders.FirstOrDefault() ?? trainingProgram?.TrainingProviders.FirstOrDefault();
				if (trainingProvider != null)
				{
					TrainingProvider = new OverviewTrainingProvider(trainingProvider);
				}
			}
			else
			{
				ProgramDescription = new OverviewProgramDescription(grantApplication);
				DynamicProcesses = grantApplication.GrantOpening.GrantStream.ProgramConfiguration.EligibleExpenseTypes
					.Where(x => x.ServiceCategory.ServiceTypeId != ServiceTypes.Administration && x.IsActive)
					.Select(x => new DynamicProcess(grantApplication, x))
					.OrderBy(x => x.RowSequence)
					.ToList();

				if (!string.IsNullOrEmpty(ProgramDescription.Description))
					FileName = ProgramDescription.Description;
			}

			if (grantApplication.TrainingCost != null)
			{
				TrainingCost = new OverviewTrainingCost(grantApplication.TrainingCost);
				if (grantApplication.TrainingCost.EligibleCosts != null)
				{
					var hasOfferBeenIssued = grantApplication.HasOfferBeenIssued();
					if (!hasOfferBeenIssued)
					{
						TrainingCost.EstimatedCosts = grantApplication.TrainingCost.EligibleCosts.Where(t => !t.AddedByAssessor).Select(x => new EstimatedCostViewModel(x, grantApplication)).ToArray();
						TrainingCost.TotalCost = grantApplication.TrainingCost.TotalEstimatedCost;
						TrainingCost.TotalRequest = grantApplication.TrainingCost.TotalEstimatedReimbursement;
					}
					else
					{
						TrainingCost.EstimatedCosts = grantApplication.TrainingCost.EligibleCosts.Where(t => t.AgreedMaxReimbursement > 0).Select(x => new EstimatedCostViewModel(x, grantApplication)).ToArray();
						TrainingCost.TotalCost = grantApplication.TrainingCost.TotalAgreedMaxCost;
						TrainingCost.TotalRequest = grantApplication.TrainingCost.AgreedCommitment;
					}

					TrainingCost.TotalEmployer = TrainingCost.TotalCost - TrainingCost.TotalRequest;
					TrainingCost.ESSAveragePerParticipant = TrainingCost.EstimatedCosts.Where(x => x.ServiceType == (int?)ServiceTypes.EmploymentServicesAndSupports).Sum(x => !hasOfferBeenIssued ? x.EstimatedParticipantCost : x.AgreedMaxParticipantCost);
				}
			}
		}
	}
}