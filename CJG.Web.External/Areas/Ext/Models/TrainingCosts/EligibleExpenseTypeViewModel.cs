using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Application.Services;
using CJG.Core.Entities;

namespace CJG.Web.External.Areas.Ext.Models.TrainingCosts
{
    public class EligibleExpenseTypeViewModel
	{
		public int Id { get; set; }
		public string Caption { get; set; }
		public string Description { get; set; }
		public bool AllowMultiple { get; set; }
		public bool AutoInclude { get; set; }
		public bool IsActive { get; set; }
		public int RowSequence { get; set; }
		public ExpenseTypes ExpenseTypeId { get; set; }
		public int? ServiceCategoryId { get; set; }
		public IEnumerable<EligibleExpenseBreakdownViewModel> EligibleExpenseBreakdowns { get; set; }
		public ServiceTypes? ServiceType { get; set; }

		public bool AllowEstimatedCostEntry { get; set; }

		public EligibleExpenseTypeViewModel()
		{

		}

		public EligibleExpenseTypeViewModel(EligibleExpenseType eligibleExpenseType)
		{
			if (eligibleExpenseType == null)
				throw new ArgumentNullException(nameof(eligibleExpenseType));

			Utilities.MapProperties(eligibleExpenseType, this);

			ServiceType = eligibleExpenseType.ServiceCategory?.ServiceTypeId;
			AllowEstimatedCostEntry = eligibleExpenseType.MaxProviders == 0;

			EligibleExpenseBreakdowns = eligibleExpenseType.Breakdowns.Where(b => b.IsActive).Select(eeb => new EligibleExpenseBreakdownViewModel(eeb)).ToArray();
		}

		public EligibleExpenseTypeViewModel(EligibleExpenseType eligibleExpenseType, EligibleCost eligibleCost)
		{
			if (eligibleExpenseType == null)
				throw new ArgumentNullException(nameof(eligibleExpenseType));

			Utilities.MapProperties(eligibleExpenseType, this);

			ServiceType = eligibleExpenseType.ServiceCategory?.ServiceTypeId;
			AllowEstimatedCostEntry = eligibleExpenseType.MaxProviders == 0;

			EligibleExpenseBreakdowns = eligibleExpenseType.Breakdowns.Where(b => b.IsActive)
				.Select(eeb =>
				{
					var lineItem = new EligibleExpenseBreakdownViewModel(eeb);
					if (eligibleCost != null)
					{
						var breakdown = eligibleCost.Breakdowns.FirstOrDefault(t => t.EligibleExpenseBreakdown.ServiceLineId == eeb.ServiceLineId);

						if (breakdown == null)
							return lineItem;

						lineItem.Selected = true;
						lineItem.EstimatedCost = breakdown.EstimatedCost;
						lineItem.CustomCostTitle = breakdown.CustomCostTitle;
						lineItem.AllowEstimatedCostEntry = AllowEstimatedCostEntry;
					}

					return lineItem;
				});
		}
	}
}