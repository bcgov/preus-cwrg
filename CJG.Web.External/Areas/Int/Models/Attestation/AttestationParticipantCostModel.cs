using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.Attestation
{
	public class AttestationParticipantCostModel : BaseViewModel
	{
		public int AttestationParticipantCostId { get; set; }
		public int ParticipantFormId { get; set; } // Probably don't need this
		public int CostCategoryId { get; set; }
		public string CostCategoryName { get; set; }
		public string CostCategoryOther { get; set; }
		public decimal TotalSpent { get; set; }
		public bool RequireOther { get; set; }
	}
}