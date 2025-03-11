using CJG.Application.Services;
using CJG.Core.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace CJG.Web.External.Areas.Ext.Models.TrainingCosts
{
	public class EligibleExpenseBreakdownViewModel
	{
		public int Id { get; set; }
		public string RowVersion { get; set; }
		public string Caption { get; set; }
		public string Description { get; set; }
		public bool IsActive { get; set; } = true;
		public int RowSequence { get; set; }
		public bool EnableCost { get; set; }

		public bool AllowEstimatedCostEntry { get; set; }

		// Do we allow the applicant to supply a custom expense title. Used on on ESS expenses.
		public bool EnableCustomTitle { get; set; }

		// The applicant-supplied estimated costs. Used only on ESS expenses.
		//[CustomValidation(typeof(EligibleExpenseBreakdownViewModelValidation), "ValidateEstimatedCost")]
		public decimal EstimatedCost { get; set; }

		[CustomValidation(typeof(EligibleExpenseBreakdownViewModelValidation), "ValidateCustomTitle")]
		public string CustomCostTitle { get; set; }

		public int? ServiceLineId { get; set; }

		public bool Selected { get; set; }
		public bool Deleted { get; set; }

		public EligibleExpenseBreakdownViewModel()
		{
		}

		public EligibleExpenseBreakdownViewModel(EligibleExpenseBreakdown eligibleExpenseBreakdown)
		{
			if (eligibleExpenseBreakdown == null)
				throw new ArgumentNullException(nameof(eligibleExpenseBreakdown));

			Utilities.MapProperties(eligibleExpenseBreakdown, this);
		}
	}
}