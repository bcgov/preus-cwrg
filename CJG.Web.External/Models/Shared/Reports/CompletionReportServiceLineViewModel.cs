using System;
using CJG.Application.Services;
using CJG.Core.Entities;

namespace CJG.Web.External.Models.Shared.Reports
{
    public class CompletionReportServiceLineViewModel : BaseViewModel
	{
		public int EligibleCostBreakdownId { get; set; }
		public string Caption { get; set; }

		public CompletionReportServiceLineViewModel()
		{
		}

		public CompletionReportServiceLineViewModel(EligibleCostBreakdown eligibleCostBreakdown)
		{
			if (eligibleCostBreakdown == null)
				throw new ArgumentNullException(nameof(eligibleCostBreakdown));

			Utilities.MapProperties(eligibleCostBreakdown.EligibleExpenseBreakdown.ServiceLine, this);
			EligibleCostBreakdownId = eligibleCostBreakdown.Id;
		}
	}
}
