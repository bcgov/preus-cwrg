using System;
using System.ComponentModel.DataAnnotations;
using CJG.Core.Entities;

namespace CJG.Web.External.Areas.Part.Models
{
    public class ParticipantInfoStep4VmValidation
	{
		private static bool IsEmployed(int employmentType)
		{
			return employmentType == 2 || employmentType == 3 || employmentType == 6;
		}

		private static bool IsUnemployed(int employmentType)
		{
			return employmentType == 1 || employmentType == 4;
		}

		private static bool WasEmployed(int employmentType, bool? haveYouEverBeenEmployed)
		{
			var previouslyEmployed = haveYouEverBeenEmployed ?? false;
			return previouslyEmployed && (employmentType == 1 || employmentType == 4);
		}

		public static ValidationResult ValidateHaveYouEverBeenEmployed(bool? haveYouEverBeenEmployed, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			var result = ValidationResult.Success;

			if (!IsUnemployed(model.EmploymentStatus))
				return result;

			if (!haveYouEverBeenEmployed.HasValue)
				result = new ValidationResult("The Have you ever been employed field is required.");

			return result;
		}

		public static ValidationResult ValidateMultipleEmploymentPositions(bool? multipleEmploymentPositions, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			var result = ValidationResult.Success;

			if (!IsEmployed(model.EmploymentStatus))
				return result;

			if (!multipleEmploymentPositions.HasValue)
				result = new ValidationResult("The more than one Employer field is required.");

			return result;
		}

		public static ValidationResult ValidatePreviousEmploymentLastDayOfWork(DateTime? lastDateOfWork, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			if (!WasEmployed(model.EmploymentStatus, model.HaveYouEverBeenEmployed))
				return ValidationResult.Success;

			if (!lastDateOfWork.HasValue)
				return new ValidationResult("The End date of most recent employment is required.");

			if (lastDateOfWork.Value.ToUniversalTime() > AppDateTime.UtcNow)
				return new ValidationResult("The End date of most recent employment cannot be greater than today.");

			return ValidationResult.Success;
		}

		//public static ValidationResult ValidateHowLongYears(int? HowLongYears, ValidationContext context)
		//{
		//	ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
		//	if (model == null) throw new ArgumentNullException();
		//	ValidationResult result = ValidationResult.Success;

		//	if (IsEmployed(model.EmploymentStatus))
		//	{
		//		if (!HowLongYears.HasValue)
		//		{
		//			result = new ValidationResult("The Year field is required.");
		//		}
		//		else if (HowLongYears.Value < 0)
		//		{
		//			result = new ValidationResult("The Year field must be greater than or equal to 0.");
		//		}
		//	}
		//	return result;
		//}

		//public static ValidationResult ValidateHowLongMonths(int? HowLongMonths, ValidationContext context)
		//{
		//	ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
		//	if (model == null) throw new ArgumentNullException();
		//	ValidationResult result = ValidationResult.Success;

		//	if (IsEmployed(model.EmploymentStatus))
		//	{
		//		if (!HowLongMonths.HasValue)
		//		{
		//			result = new ValidationResult("The Month field is required.");
		//		}
		//		else if (HowLongMonths.Value < 0)
		//		{
		//			result = new ValidationResult("The Month field must be greater than or equal to 0.");
		//		}
		//	}
		//	return result;
		//}

		//public static ValidationResult ValidateAvgHoursPerWeek(int? AvgHoursPerWeek, ValidationContext context)
		//{
		//	ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
		//	if (model == null) throw new ArgumentNullException();
		//	ValidationResult result = ValidationResult.Success;

		//	if (IsEmployed(model.EmploymentStatus))
		//	{
		//		if (!AvgHoursPerWeek.HasValue)
		//		{
		//			result = new ValidationResult("The Average Hour field is required.");
		//		}
		//		else if (AvgHoursPerWeek.Value < 0)
		//		{
		//			result = new ValidationResult("The Average Hour field must be greater than or equal to 0.");
		//		}
		//	}
		//	return result;
		//}

		public static ValidationResult ValidateAvgHoursPerWeekDuringTraining(int? avgHoursPerWeekDuringTraining, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			ValidationResult result = ValidationResult.Success;

			if (IsEmployed(model.EmploymentStatus))
			{
				if (!avgHoursPerWeekDuringTraining.HasValue)
				{
					result = new ValidationResult("The Average Hours during Training field is required.");
				}
				else if (avgHoursPerWeekDuringTraining.Value < 0)
				{
					result = new ValidationResult("The Average Hours during Training field must be greater than or equal to 0.");
				}
			}

			return result;
		}

		public static ValidationResult ValidatePreviousAvgHoursPerWeek(int? previousAverageHoursPerWeek, ValidationContext context)
		{
			var model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			if (!WasEmployed(model.EmploymentStatus, model.HaveYouEverBeenEmployed))
				return ValidationResult.Success;

			if (!previousAverageHoursPerWeek.HasValue)
				return new ValidationResult("The Previous Average Hour field is required.");

			var averageHoursPerWeek = previousAverageHoursPerWeek.Value;
			if (averageHoursPerWeek < 0 || averageHoursPerWeek > 168.0m)
				return new ValidationResult("The previous average hours per week must be within 0 to 168.");

			return ValidationResult.Success;
		}

		public static ValidationResult ValidatePreviousEmployerFullName(string previousEmployerFullName, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			if (!WasEmployed(model.EmploymentStatus, model.HaveYouEverBeenEmployed))
				return ValidationResult.Success;

			if (string.IsNullOrWhiteSpace(previousEmployerFullName))
				return new ValidationResult("The business name of your most recent previous employer is required.");

			return ValidationResult.Success;
		}

		public static ValidationResult ValidateApprentice(bool? apprentice, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			ValidationResult result = ValidationResult.Success;

			if (!apprentice.HasValue)
				result = new ValidationResult("The Apprentice field is required.");

			return result;
		}

		public static ValidationResult ValidateOtherPrograms(bool? otherPrograms, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			ValidationResult result = ValidationResult.Success;

			if (!otherPrograms.HasValue)
				result = new ValidationResult("The Other Funded field is required.");

			return result;
		}

		public static ValidationResult ValidateItaRegistered(bool? itaRegistered, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			ValidationResult result = ValidationResult.Success;

			if (model.Apprentice.HasValue && model.Apprentice.Value == true)
			{
				if (!itaRegistered.HasValue)
				{
					result = new ValidationResult("The ITA Registered field is required.");
				}
			}

			return result;
		}

		public static ValidationResult ValidateEIBenefit(int eIBenefit, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			ValidationResult result = ValidationResult.Success;
			bool currentReceiveEI = model.CurrentReceiveEI ?? false;

			if (currentReceiveEI)
			{
				if (!(eIBenefit >= 1 && eIBenefit <= 6))
				{
					result = new ValidationResult("EI Benefit field is required.");
				}
			}

			return result;
		}

		public static ValidationResult ValidateMaternalPaternal(bool? maternalPaternal, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			ValidationResult result = ValidationResult.Success;

			bool currentReceiveEI = model.CurrentReceiveEI ?? false;

			if (model.EIBenefit != 6)
			{
				if (!maternalPaternal.HasValue && currentReceiveEI)
				{
					result = new ValidationResult("The Maternal / Paternal field is required.");
				}
			}

			return result;
		}

		public static ValidationResult ValidatePastMaternalPaternal(bool? pastMaternalPaternal, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			ValidationResult result = ValidationResult.Success;

			bool currentReceiveEI = model.CurrentReceiveEI ?? false;


			if (!pastMaternalPaternal.HasValue && currentReceiveEI && (model.EIBenefit >= 2 && model.EIBenefit <= 5))
			{
				result = new ValidationResult("The past Maternal / Paternal field is required.");
			}

			return result;
		}

		public static ValidationResult ValidateCurrentReceiveEI(bool? currentReceiveEI, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			ValidationResult result = ValidationResult.Success;

			if (!currentReceiveEI.HasValue && model.ProgramType == ProgramTypes.WDAService)
			{
				result = new ValidationResult("Currently receiving EI field is required.");
			}

			return result;
		}

		public static ValidationResult ValidateBceaClient(bool? bceaClient, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			ValidationResult result = ValidationResult.Success;

			if (model.EmploymentStatus == 1)
			{
				if (!bceaClient.HasValue)
				{
					result = new ValidationResult("The Income Assistance field is required.");
				}
			}

			return result;
		}

		//public static ValidationResult ValidateEmployedBy(bool? employedBy, ValidationContext context)
		//{
		//	ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
		//	if (model == null) throw new ArgumentNullException();
		//	ValidationResult result = ValidationResult.Success;

		//	if (IsEmployed(model.EmploymentStatus))
		//	{
		//		if (!employedBy.HasValue)
		//		{
		//			result = new ValidationResult("The Employed By field is required.");
		//		}
		//	}
		//	return result;
		//}

		//public static ValidationResult ValidateBusinessOwner(bool? businessOwner, ValidationContext context)
		//{
		//	ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
		//	if (model == null) throw new ArgumentNullException();
		//	ValidationResult result = ValidationResult.Success;

		//	if (IsEmployed(model.EmploymentStatus))
		//	{
		//		if (!businessOwner.HasValue)
		//		{
		//			result = new ValidationResult("The Owner field is required.");
		//		}
		//	}
		//	return result;
		//}

		public static ValidationResult ValidateHourlyWage(decimal? HourlyWage, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			ValidationResult result = ValidationResult.Success;

			if (IsEmployed(model.EmploymentStatus))
			{
				if (!HourlyWage.HasValue)
				{
					result = new ValidationResult("The Wage field is required.");
				}
				else if (HourlyWage.Value < 0)
				{
					result = new ValidationResult("The Wage field must be greater than or equal to 0.");
				}
			}
			return result;
		}

		public static ValidationResult ValidatePreviousHourlyWage(decimal? previousHourlyWage, ValidationContext context)
		{
			var model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			if (!WasEmployed(model.EmploymentStatus, model.HaveYouEverBeenEmployed))
				return ValidationResult.Success;

			if (!previousHourlyWage.HasValue)
				return new ValidationResult("The Previous Hourly Wage field is required.");

			if (previousHourlyWage.Value < 0)
				return new ValidationResult("The Previous Hourly Wage field must be greater than or equal to 0.");

			return ValidationResult.Success;
		}

		public static ValidationResult ValidatePrimaryCity(string PrimaryCity, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			ValidationResult result = ValidationResult.Success;

			if (IsEmployed(model.EmploymentStatus))
			{
				if (String.IsNullOrWhiteSpace(PrimaryCity))
				{
					result = new ValidationResult("The City field is required.");
				}
			}
			return result;
		}

		public static ValidationResult ValidateEmploymentType(int? EmploymentType, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			ValidationResult result = ValidationResult.Success;

			if (IsEmployed(model.EmploymentStatus))
			{
				if (EmploymentType == null || EmploymentType <= 0)
				{
					result = new ValidationResult("The Employment Type field is required.");
				}
			}
			return result;
		}

		public static ValidationResult ValidateCurrentNoc(int? nocCode, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			ValidationResult result = ValidationResult.Success;

			if (model.EmploymentStatus == 2 || model.EmploymentStatus == 3)
			{
				if (!nocCode.HasValue || nocCode <= 0)
				{
					result = new ValidationResult("Your current National Occupation Classification (NOC) before training is required.");
				}
			}
			return result;
		}

		public static ValidationResult ValidateFutureNoc(int? nocCode, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			ValidationResult result = ValidationResult.Success;

			if (!nocCode.HasValue || nocCode <= 0)
			{
				result = new ValidationResult("Your National Occupation Classification (NOC) after training is required.");
			}
			return result;
		}


		public static ValidationResult ValidatePreviousEmploymentNoc(int? nocCode, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			if (!WasEmployed(model.EmploymentStatus, model.HaveYouEverBeenEmployed))
				return ValidationResult.Success;

			if (!nocCode.HasValue || nocCode <= 0)
				return new ValidationResult("Your National Occupation Classification (NOC) for previous employment is required.");

			return ValidationResult.Success;
		}

		public static ValidationResult ValidateOtherProgramDesc(string otherProgramDesc, ValidationContext context)
		{
			ParticipantInfoStep4ViewModel model = context.ObjectInstance as ParticipantInfoStep4ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			ValidationResult result = ValidationResult.Success;

			if (model.OtherPrograms.HasValue && model.OtherPrograms.Value == true)
			{
				if (string.IsNullOrWhiteSpace(otherProgramDesc))
				{
					result = new ValidationResult("The provincially or federally funded program field is required.");
				}
			}
			return result;
		}
	}
}