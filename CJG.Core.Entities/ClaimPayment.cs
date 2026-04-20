using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class ClaimPayment : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		/// <summary>
		/// get/set - Foreign key to the Claim.
		/// </summary>
		[ForeignKey(nameof(Claim)), Column(Order = 1)]
		public int ClaimId { get; set; }

		/// <summary>
		/// get/set - Foreign key to the Claim version.
		/// </summary>
		[ForeignKey(nameof(Claim)), Column(Order = 2)]
		public int ClaimVersion { get; set; }

		public virtual Claim Claim { get; set; }

		[Description("LMDA")]
		public decimal? PaidLMDA { get; set; }

		[Description("WDA")]
		public decimal? PaidWDA { get; set; }

		// Tariff Codes
		[Description("111TRSW")]
		public bool LMDATariffTRSW { get; set; }
		[Description("111TRST")]
		public bool LMDATariffTRST { get; set; }
		[Description("111TRCO")]
		public bool LMDATariffTRCO { get; set; }

		[Description("111CWRG")]
		public bool WDATariffCWRG { get; set; }
	}
}