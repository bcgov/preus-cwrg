using System.Collections.Generic;
using CJG.Application.Business.Models.GlobalBudget;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.GlobalBudget
{
	public class GlobalBudgetDashboardModel : BaseViewModel
	{
		public int? SelectedFiscalYearId { get; set; }
		public List<IntakePeriodSlotModel> IntakePeriodSlots { get; set; }
		public GlobalBudgetSummaryModel BudgetSummary { get; set; }

		public bool CanEditBudget { get; set; }

		public GlobalBudgetDashboardModel()
		{
		}
	}
}