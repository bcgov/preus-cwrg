using System;
using System.Collections.Generic;

namespace CJG.Web.External.Areas.Int.Models.GlobalBudget
{
	public class GlobalBudgetSummaryModel
	{
		[Obsolete("Might not need this anymore")]
		public List<KeyValuePair<string, decimal>> Committed { get; set; }
		public List<KeyValuePair<string, decimal>> CommittedUnclaimed { get; set; }
		public List<KeyValuePair<string, decimal>> CommittedClaimed { get; set; }

		public List<KeyValuePair<string, decimal>> Forecast { get; set; }
		public List<KeyValuePair<string, decimal>> TotalSpent { get; set; }
		public List<KeyValuePair<string, decimal>> Slippage { get; set; }

		public decimal GrandTotal { get; set; }
	}
}