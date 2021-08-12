using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.GrantOpenings
{
    public class GrantOpeningDashboardViewModel : BaseViewModel
	{
		public int? SelectedFiscalYearId { get; set; }
		public int? SelectedGrantProgramId { get; set; }
	}
}
