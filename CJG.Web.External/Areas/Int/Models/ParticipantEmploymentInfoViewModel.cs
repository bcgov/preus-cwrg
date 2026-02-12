using System.Linq;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Helpers;

namespace CJG.Web.External.Areas.Int.Models
{
	public class ParticipantEmploymentInfoViewModel
	{
		private const string DATEFORMAT = "yyyy-MM-dd";

		public string EmploymentStatus { get; set; }
		public string MultipleEmploymentPositions { get; set; }
		public string HaveYouEverBeenEmployed { get; set; }

		public string CityofWork { get; set; }
		public string ReceivingEIValue { get; set; }

		public string CurrentNocLevel4 { get; set; }
		public string FutureNocLevel4 { get; set; }

		public string ReceivingIA { get; set; }
		public string Apprentice { get; set; }
		public string EmployedByEmployer { get; set; }
		public string ITARegistered { get; set; }
		public string DurationOfEmployment { get; set; }
		public string ParticipatingInOtherFundingProg { get; set; }
		public string OwnerOfBusiness { get; set; }
		public string ProgramDescription { get; set; }
		public string OtherProgramDesc { get; set; }
		public int? HoursPerWeek { get; set; }
		public int? HoursPerWeekDuringTraining { get; set; }
		public int? PreviousHoursPerWeek { get; set; }
		public string MostImportantResult { get; set; }
		public string TypeOfEmployment { get; set; }
		public string AverageHourlyWage { get; set; }
		public string PreviousHourlyWage { get; set; }
		public string PreviousEmploymentLastDayOfWork { get; set; }
		public string PreviousEmployerFullName { get; set; }
		public string MaternalParentalBenefits { get; set; }
		public bool ShowEmploymentFields { get; set; }

		public ParticipantEmploymentInfoViewModel()
		{
		}

		public ParticipantEmploymentInfoViewModel(ParticipantForm participantForm, INationalOccupationalClassificationService nationalOccupationalClassificationService)
		{
			EmploymentStatus = participantForm.EmploymentStatus?.Caption;
			HaveYouEverBeenEmployed = participantForm.HaveYouEverBeenEmployed.AsYesOrNo();

			CityofWork = participantForm.PrimaryCity;
			MultipleEmploymentPositions = participantForm.MultipleEmploymentPositions.AsYesOrNo();
			ReceivingEIValue = participantForm.EIBenefit?.Caption;
			if (participantForm?.GrantApplication?.GrantOpening?.GrantStream?.GrantProgram?.ProgramTypeId == ProgramTypes.WDAService)
				ReceivingEIValue = participantForm.ReceivingEIBenefit.ToStringValue();

			if (participantForm.CurrentNoc != null && participantForm.CurrentNoc.Id != 0 && !string.IsNullOrWhiteSpace(participantForm.CurrentNoc.Code))
				CurrentNocLevel4 = participantForm.CurrentNoc.ToString();

			if (participantForm.FutureNoc != null && participantForm.FutureNoc.Id != 0 && !string.IsNullOrWhiteSpace(participantForm.FutureNoc.Code))
				FutureNocLevel4 = participantForm.FutureNoc.ToString();

			ReceivingIA = participantForm.BceaClient ? "Yes" : "No";
			Apprentice = participantForm.Apprentice ? "Yes" : "No";
			EmployedByEmployer = participantForm.EmployedBySupportEmployer ? "Yes" : "No";
			ITARegistered = participantForm.ItaRegistered ? "Yes" : "No";
			DurationOfEmployment = participantForm.HowLongYears.HasValue || participantForm.HowLongMonths.HasValue
				? $"{participantForm.HowLongYears ?? 0} Years {participantForm.HowLongMonths ?? 0} Months"
				: null;
			ParticipatingInOtherFundingProg = participantForm.OtherPrograms ? "Yes" : "No";
			OwnerOfBusiness = participantForm.BusinessOwner ? "Yes" : "No";
			ProgramDescription = participantForm.ProgramDescription;
			OtherProgramDesc = participantForm.OtherProgramDesc;
			HoursPerWeek = participantForm.AvgHoursPerWeek;
			HoursPerWeekDuringTraining = participantForm.AvgHoursPerWeekDuringTraining;
			PreviousHoursPerWeek = participantForm.PreviousAvgHoursPerWeek;
			PreviousEmploymentLastDayOfWork = participantForm.PreviousEmploymentLastDayOfWork.HasValue
				? participantForm.PreviousEmploymentLastDayOfWork.Value.ToString(DATEFORMAT)
				: null;
			PreviousEmployerFullName = participantForm.PreviousEmployerFullName;
			MostImportantResult = participantForm.TrainingResult?.Caption;
			TypeOfEmployment = participantForm.EmploymentType?.Caption;
			AverageHourlyWage = $"{participantForm.HourlyWage:c}";
			PreviousHourlyWage = participantForm.PreviousHourlyWage.HasValue ? $"{participantForm.PreviousHourlyWage:c}" : string.Empty;
			MaternalParentalBenefits = participantForm.MaternalPaternal ? "Yes" : "No";
			ShowEmploymentFields = new[] { "Employed", "Self-employed" }.Contains(EmploymentStatus);
		}
	}
}