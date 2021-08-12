using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Core.Interfaces;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Helpers;
using CJG.Web.External.Models.Shared;
using CJG.Web.External.Models.Shared.SkillsTrainings;

namespace CJG.Web.External.Areas.Int.Models.SkillsTraining
{
    public class SkillsTrainingProgramViewModel : BaseViewModel
	{
		//STOC => Short Term Occupational Cert
		public enum OccupationalSkillsTraining
		{
			[Description("Occupational training with STOC and on-the-job training")]
			STOCandOntheJob,
			[Description("Occupational training with STOC only")]
			STOC,
			[Description("Occupational training with on-the-job training")]
			OntheJob,
			[Description("Occupational training without STOC or on-the-job training")]
			NoSTOCandNoOntheJob
		}

		public bool CanEdit { get; set; }
		public bool CanRemove { get; set; }
		public string RowVersion { get; set; }
		public int GrantApplicationId { get; set; }
		public int ServiceLineId { get; set; }
		public int? ServiceLineBreakdownId { get; set; }
		public int EligibleCostId { get; set; }
		public int EligibleCostBreakdownId { get; set; }
		public string EligibleCostBreakdownRowVersion { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public string CourseTitle { get; set; }
		public int? TotalTrainingHours { get; set; }
		public string TitleOfQualification { get; set; }
		public int[] SelectedDeliveryMethodIds { get; set; }
		public int? ExpectedQualificationId { get; set; }
		public int? SkillLevelId { get; set; }
		public decimal EstimatedCost { get; set; }
		public decimal AgreedCost { get; set; }

		public DateTime? TrainingPeriodMaxStartDate { get; set; }
		public DateTime? TrainingPeriodMaxEndDate { get; set; }

		public bool ShowSkillsTrainingFocusDropDown { get; set; }
		public OccupationalSkillsTraining OccupationalTraining { get; set; }
		public bool? SkillsTrainingFocusTypeIsOccupational { get; set; }

		[CustomValidation(typeof(ProgramDescriptionViewModelValidation), "ValidateCIPS")]
		public int? CipsCode1Id { get; set; }
		[CustomValidation(typeof(ProgramDescriptionViewModelValidation), "ValidateCIPS")]
		public int? CipsCode2Id { get; set; }
		[CustomValidation(typeof(ProgramDescriptionViewModelValidation), "ValidateCIPS")]
		public int? CipsCode3Id { get; set; }

		public SkillsTrainingProgramViewModel() { }

		public SkillsTrainingProgramViewModel(TrainingProgram trainingProgram, IPrincipal user, ICipsCodesService cipsCodesService, IFiscalYearService fiscalYearService)
		{
			if (trainingProgram == null)
				throw new ArgumentNullException(nameof(trainingProgram));

			if (user == null)
				throw new ArgumentNullException(nameof(user));

			Id = trainingProgram.Id;
			RowVersion = trainingProgram.RowVersion != null ? Convert.ToBase64String(trainingProgram.RowVersion) : null;
			GrantApplicationId = trainingProgram.GrantApplicationId;

			CanEdit = user.CanPerformAction(trainingProgram.GrantApplication, ApplicationWorkflowTrigger.EditTrainingProgram);
			CanRemove = (trainingProgram.EligibleCostBreakdown?.AddedByAssessor ?? true) && user.CanPerformAction(trainingProgram.GrantApplication, ApplicationWorkflowTrigger.AddOrRemoveTrainingProgram);

			ServiceLineId = trainingProgram.ServiceLineId ?? throw new ArgumentException("The training program is not a valid skills training component.", nameof(trainingProgram));
			ServiceLineBreakdownId = trainingProgram.ServiceLineBreakdownId;
			EligibleCostId = trainingProgram.EligibleCostBreakdown?.EligibleCostId ?? throw new ArgumentException("The training program is not a valid skills training component.", nameof(trainingProgram));
			EligibleCostBreakdownId = trainingProgram.EligibleCostBreakdownId ?? throw new ArgumentException("The training program is not a valid skills training component.", nameof(trainingProgram));
			EligibleCostBreakdownRowVersion = Convert.ToBase64String(trainingProgram.EligibleCostBreakdown.RowVersion);
			StartDate = trainingProgram.StartDate.ToLocalTime();
			EndDate = trainingProgram.EndDate.ToLocalTime();

			var maxDates = trainingProgram.GetMaxDates(fiscalYearService.GetFiscalYears().ToList());

			TrainingPeriodMaxStartDate = maxDates.Item1.ToLocalTime();
			TrainingPeriodMaxEndDate = maxDates.Item2.ToLocalTime();

			CourseTitle = trainingProgram.CourseTitle;
			TotalTrainingHours = trainingProgram.TotalTrainingHours;
			TitleOfQualification = trainingProgram.TitleOfQualification;
			SelectedDeliveryMethodIds = trainingProgram.DeliveryMethods.Select(dm => dm.Id).ToArray();
			ExpectedQualificationId = trainingProgram.ExpectedQualificationId;
			SkillLevelId = trainingProgram.SkillLevelId;
			EstimatedCost = trainingProgram.EligibleCostBreakdown.EstimatedCost;
			AgreedCost = trainingProgram.EligibleCostBreakdown.AssessedCost;

			ShowSkillsTrainingFocusDropDown = trainingProgram.GrantApplication.ShowSkillsTrainingFocusDropDown();
            if (!ShowSkillsTrainingFocusDropDown)
            {
	            SetOccupationalTrainingType(trainingProgram);

	            if (trainingProgram.SkillsTrainingFocusType.HasValue)
                {
					SkillsTrainingFocusTypeIsOccupational = trainingProgram.SkillsTrainingFocusType.Value == 0;
				}
            }

            var cipsCodes = cipsCodesService.GetListOfCipsCodes(trainingProgram.CipsCode?.Id ?? 0);
			cipsCodes.ForEach(item =>
			{
				var property = GetType().GetProperty($"CipsCode{item.Level}Id");
				property?.SetValue(this, item.Id);
			});
		}

		private void SetOccupationalTrainingType(TrainingProgram trainingProgram)
		{
			var shortTermOccupationalCert = trainingProgram.ShortTermOccupationalCert ?? false;
			var onTheJobTraining = trainingProgram.OnTheJobTraining ?? false;

			if (shortTermOccupationalCert && onTheJobTraining)
				OccupationalTraining = OccupationalSkillsTraining.STOCandOntheJob;
			else if (shortTermOccupationalCert)
				OccupationalTraining = OccupationalSkillsTraining.STOC;
			else if (onTheJobTraining)
				OccupationalTraining = OccupationalSkillsTraining.OntheJob;
			else
				OccupationalTraining = OccupationalSkillsTraining.NoSTOCandNoOntheJob;
		}

		public static explicit operator SkillTrainingViewModel(SkillsTrainingProgramViewModel model)
		{
			var result = new SkillTrainingViewModel();
			Utilities.MapProperties(model, result);
			return result;
		}
	}
}
