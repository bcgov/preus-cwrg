using System;
using System.ComponentModel.DataAnnotations;
using CJG.Core.Entities;

namespace CJG.Web.External.Areas.Part.Models
{
    public class ParticipantInfoStep3VmValidation
    {
        public static ValidationResult ValidateYearToCanada(int yearToCanada, ValidationContext context)
        {
            ParticipantInfoStep3ViewModel model = context.ObjectInstance as ParticipantInfoStep3ViewModel;
            if (model == null)
	            throw new ArgumentNullException();

            ValidationResult result = ValidationResult.Success;

            if (model.CanadaImmigrant.GetValueOrDefault())
            {
                int upperYear = AppDateTime.UtcNow.Year;
                int lowerYear = upperYear - 75;

                if (yearToCanada < lowerYear || yearToCanada > upperYear)
                {
                    result = new ValidationResult($"Year field must be between {lowerYear} and {upperYear}.");
                }
                else
                {
                    if (yearToCanada < model.BirthYear)
                    {
                        result = new ValidationResult($"Immigration Date cannot be earlier than your birth year of {model.BirthYear}.");
                    }
                }
            }
            return result;
        }

        public static ValidationResult ValidateYouthInCare(bool? youthInCare, ValidationContext context)
        {
            ParticipantInfoStep3ViewModel model = context.ObjectInstance as ParticipantInfoStep3ViewModel;
            if (model == null) throw new ArgumentNullException();
            ValidationResult result = ValidationResult.Success;

            if (model.Age > 14 && model.Age < 25)
            {
                if (!youthInCare.HasValue)
                {
                    result = new ValidationResult("The Youth In Care field is required");
                }
            }
            return result;
        }

        public static ValidationResult ValidateLiveOnReserve(bool? liveOnReserve, ValidationContext context)
        {
            ParticipantInfoStep3ViewModel model = context.ObjectInstance as ParticipantInfoStep3ViewModel;
            if (model == null)
	            throw new ArgumentNullException();

            ValidationResult result = ValidationResult.Success;

            if (model.PersonAboriginal == 1)
            {
                if (model.AboriginalBand != null && model.AboriginalBand.Value == 1)
                {
	                if (!liveOnReserve.HasValue)
	                {
		                result = new ValidationResult("On/Off Reserve field is required");
	                }
                }
            }
            return result;
        }

        public static ValidationResult ValidatePersonAboriginal(int? personAboriginal, ValidationContext context)
        {
	        ParticipantInfoStep3ViewModel model = context.ObjectInstance as ParticipantInfoStep3ViewModel;
	        if (model == null)
		        throw new ArgumentNullException();

	        ValidationResult result = ValidationResult.Success;

	        if (model.CanadianStatus == 1)
	        {
		        if (!personAboriginal.HasValue)
		        {
			        result = new ValidationResult("Indigenous field is required.");
		        }
	        }

	        return result;
        }

        public static ValidationResult ValidateImmigrant(bool? immigrant, ValidationContext context)
		{
			ParticipantInfoStep3ViewModel model = context.ObjectInstance as ParticipantInfoStep3ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			var result = ValidationResult.Success;

			if (model.CanadianStatus != 5 && (model.PersonAboriginal == 2 || model.PersonAboriginal == 3))
			{
				if (!immigrant.HasValue)
				{
					result = new ValidationResult("Canada Immigrant answer is required.");
				}
			}
			return result;
		}

		public static ValidationResult ValidateRefugee(bool? refugee, ValidationContext context)
		{
			ParticipantInfoStep3ViewModel model = context.ObjectInstance as ParticipantInfoStep3ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			ValidationResult result = ValidationResult.Success;

			if (model.CanadianStatus != 5 && (model.PersonAboriginal == 2 || model.PersonAboriginal == 3))
			{
				if (!refugee.HasValue)
				{
					return new ValidationResult("Canada Refugee answer is required.");

				}
			}
			return result;
		}

		public static ValidationResult ValidateVisibleMinority(int? visibleMinority, ValidationContext context)
		{
			ParticipantInfoStep3ViewModel model = context.ObjectInstance as ParticipantInfoStep3ViewModel;
			if (model == null)
				throw new ArgumentNullException();

			ValidationResult result = ValidationResult.Success;

			if (model.CanadianStatus == 5 || model.PersonAboriginal == 2 || model.PersonAboriginal == 3)
			{
				if (!visibleMinority.HasValue)
				{
					result = new ValidationResult("Visible Minority answer is required.");
				}
			}
			return result;
		}

		public static ValidationResult ValidateAboriginalBand(int? aboriginalBand, ValidationContext context)
        {
            ParticipantInfoStep3ViewModel model = context.ObjectInstance as ParticipantInfoStep3ViewModel;
            if (model == null)
	            throw new ArgumentNullException();

            ValidationResult result = ValidationResult.Success;

            if (model.PersonAboriginal == 1)
            {
                if (!aboriginalBand.HasValue)
                {
                    result = new ValidationResult("A selection is required");
                }
            }
            return result;
        }

        public static ValidationResult ValidateFromCountry(string fromCountry, ValidationContext context)
        {
            ParticipantInfoStep3ViewModel model = context.ObjectInstance as ParticipantInfoStep3ViewModel;
            if (model == null)
	            throw new ArgumentNullException();

            ValidationResult result = ValidationResult.Success;

            if (model.CanadaRefugee.GetValueOrDefault() && string.IsNullOrWhiteSpace(fromCountry))
            {
                result = new ValidationResult("The Country field is required.");
            }
            return result;
        }
	}
}