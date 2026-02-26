using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Ext.Models.Reporting
{
	public class AttestationParticipantCostModel : BaseViewModel
	{
		public int ParticipantFormId { get; set; }
		public int CostCategoryId { get; set; }
		public string CostCategoryName { get; set; }
		public string CostCategoryOther { get; set; }
		public decimal TotalSpent { get; set; }
		public bool RequireOther { get; set; }
	}
}