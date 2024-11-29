using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;

namespace CJG.Web.External.Models.Shared.Reports
{
    public class CompletionReportDynamicCheckboxViewModel : BaseViewModel
	{
		public IEnumerable<CompletionReportServiceCategoryViewModel> ServiceCategories { get; set; }

		public CompletionReportDynamicCheckboxViewModel()
		{
		}

		public CompletionReportDynamicCheckboxViewModel(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			if (grantApplication.GrantOpening.GrantStream.GrantProgram.ProgramTypeId != ProgramTypes.WDAService)
				return;

			var eligibleCosts = grantApplication.TrainingCost.EligibleCosts.Where(x =>
				x.EligibleExpenseType.ServiceCategory.ServiceTypeId == ServiceTypes.EmploymentServicesAndSupports
				&& x.EligibleExpenseType.IsActive).ToArray();

			ServiceCategories = eligibleCosts.Where(o => o.Breakdowns.Any()).Select(o => new CompletionReportServiceCategoryViewModel(o)).ToArray();
		}
	}
}
