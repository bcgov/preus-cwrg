namespace CJG.Application.Business.Models
{
	public class ClaimPaymentModel
	{
		public int ClaimId { get; set; }
		public int ClaimVersion { get; set; }

		public decimal? PaidLMDA { get; set; }
		public decimal? PaidWDA { get; set; }

		public bool LMDATariffTRSW { get; set; }
		public bool LMDATariffTRST { get; set; }
		public bool LMDATariffTRCO { get; set; }
		public bool WDATariffCWRG { get; set; }
	}
}