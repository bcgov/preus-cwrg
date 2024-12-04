using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Application.Services;
using CJG.Core.Entities;

namespace CJG.Web.External.Models.Shared.Reports
{
    public class CompletionReportServiceCategoryViewModel : BaseViewModel
	{
		public string Caption { get; set; }
		public IEnumerable<CompletionReportServiceLineViewModel> ServiceLines { get; set; }

		public CompletionReportServiceCategoryViewModel()
		{
		}

		public CompletionReportServiceCategoryViewModel(EligibleCost eligibleCost)
		{
			if (eligibleCost == null)
				throw new ArgumentNullException(nameof(eligibleCost));

			Utilities.MapProperties(eligibleCost.EligibleExpenseType.ServiceCategory, this);
			ServiceLines = eligibleCost.Breakdowns.Select(o => new CompletionReportServiceLineViewModel(o)).ToArray();
		}
	}
}
