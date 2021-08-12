using System;
using System.ComponentModel.DataAnnotations;
using CJG.Web.External.Areas.Ext.Models.TrainingCosts;

namespace CJG.Web.External.Areas.Ext.Models
{
	public class EligibleExpenseBreakdownViewModelValidation
	{
		public static ValidationResult ValidateCustomTitle(string customTitle, ValidationContext context)
		{
			var model = context.ObjectInstance as EligibleExpenseBreakdownViewModel;
			if (model == null)
				throw new ArgumentNullException();

			var result = ValidationResult.Success;
			bool customTitleEnabled = model.EnableCustomTitle;
			bool isSelected = model.Selected;

			if (!isSelected)
				return result;

			if (!customTitleEnabled)
				return result;

			if (string.IsNullOrWhiteSpace(customTitle))
				return new ValidationResult("Custom service type is required.");

			return result;
		}

		public static ValidationResult ValidateEstimatedCost(decimal estimatedCost, ValidationContext context)
		{
			var model = context.ObjectInstance as EligibleExpenseBreakdownViewModel;
			if (model == null)
				throw new ArgumentNullException();

			var result = ValidationResult.Success;
			bool isSelected = model.Selected;

			if (!isSelected)
				return result;

			if (model.AllowEstimatedCostEntry && estimatedCost <= 0m)
				return new ValidationResult("Estimated cost is required.");

			return result;
		}
	}

}