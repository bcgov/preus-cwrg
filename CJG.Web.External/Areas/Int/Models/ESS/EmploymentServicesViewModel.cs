using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.ESS
{
    public class EmploymentServicesViewModel : BaseViewModel
	{
		public bool AllowEstimatedCostEntry { get; set; }
		public string RowVersion { get; set; }
		public IEnumerable<int> SelectedServiceLineIds { get; set; }
		public IEnumerable<EmploymentServicesLinesViewModel> EmploymentServiceLines { get; set; }

		public EmploymentServicesViewModel() { }

		public EmploymentServicesViewModel(EligibleCost eligibleCost)
		{
			if (eligibleCost == null)
				throw new ArgumentNullException(nameof(eligibleCost));

			Id = eligibleCost.Id;
			RowVersion = Convert.ToBase64String(eligibleCost.RowVersion);
			SelectedServiceLineIds = eligibleCost.Breakdowns.Select(b => b.EligibleExpenseBreakdownId).ToArray();

			AllowEstimatedCostEntry = eligibleCost.EligibleExpenseType.MaxProviders == 0;

			var allExpenseTypes = eligibleCost.EligibleExpenseType.Breakdowns.OrderBy(eet => eet.RowSequence).ThenBy(eet => eet.Caption);

			EmploymentServiceLines = allExpenseTypes.Select(b =>
			{
				var costBreakdown = eligibleCost.Breakdowns.FirstOrDefault(eeb => eeb.EligibleExpenseBreakdownId == b.Id);
				var enableCustomTitle  = b.EnableCustomTitle;

				return new EmploymentServicesLinesViewModel
				{
					Key = b.Id,
					Value = b.Caption,
					EstimatedAmount = costBreakdown?.EstimatedCost ?? 0m,
					AssessedCost = costBreakdown?.AssessedCost ?? 0m,
					IsSelected = costBreakdown != null,
					EnableCustomTitle = enableCustomTitle,
					CustomCostTitle = costBreakdown?.CustomCostTitle
				};
			}).ToList();
		}
	}

    public class EmploymentServicesLinesViewModel
	{
		public int Key { get; set; }  // This is the EligibleExpenseBreakdown id - rename later
		public string Value { get; set; }  // This is the EligibleExpenseBreakdown Caption

		public decimal EstimatedAmount { get; set; }  // This should be the amount being posted to save to the EligibleCostBreakdown record
		public decimal AssessedCost { get; set; }
		public bool IsSelected { get; set; }
		public bool EnableCustomTitle { get; set; }
		public string CustomCostTitle { get; set; }

		public EmploymentServicesLinesViewModel() { }
	}
}