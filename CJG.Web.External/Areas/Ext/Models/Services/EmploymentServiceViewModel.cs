using System;
using CJG.Application.Services;
using CJG.Core.Entities;

namespace CJG.Web.External.Areas.Ext.Models.Services
{
    public class EmploymentServiceViewModel : External.Models.Shared.LookupTableViewModel
	{
		public int GrantApplicationId { get; set; }
		public int EligibleCostId { get; set; }
		public decimal? EstimatedCost { get; set; }

		public bool IsPFS { get; set; }

		public TrainingCosts.EligibleExpenseTypeViewModel EligibleExpenseType { get; set; } = new TrainingCosts.EligibleExpenseTypeViewModel();

		public EmploymentServiceViewModel()
		{
		}

		public EmploymentServiceViewModel(EligibleExpenseType eligibleExpenseType, EligibleCost eligibleCost, GrantApplication grantApplication)
		{
			Utilities.MapProperties(eligibleExpenseType, this, t => new { t.RowVersion });

			if (eligibleExpenseType.ServiceCategory != null)
				IsPFS = eligibleExpenseType.ServiceCategory.Caption == "Participant Financial Supports";

			if (eligibleExpenseType.ServiceCategoryId.HasValue)
			{
				EligibleExpenseType = new TrainingCosts.EligibleExpenseTypeViewModel(eligibleExpenseType, eligibleCost);
			}

			if (eligibleCost != null)
			{
				EligibleCostId = eligibleCost.Id;
				EstimatedCost = eligibleCost.EstimatedCost;
				RowVersion = Convert.ToBase64String(eligibleCost.RowVersion);
			}

			GrantApplicationId = grantApplication.Id;
		}
	}
}
