using System;
using System.ComponentModel;
using System.Data;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.EvaluationForm
{
	public class ClaimPaymentsAndTariffCodesModel : BaseViewModel
	{
		public int ClaimId { get; set; }
		public int ClaimVersion { get; set; }

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

		public ClaimPaymentsAndTariffCodesModel()
		{
		}

		public ClaimPaymentsAndTariffCodesModel(Claim claim)
		{
			if (claim == null)
				throw new NullReferenceException("Claim");

			var claimPayment = claim.ClaimPayment;

			if (claimPayment == null)
				claimPayment = new ClaimPayment
				{
					ClaimId = claim.Id,
					ClaimVersion = claim.ClaimVersion,
					RowVersion = claim.RowVersion
				};

			Id = claimPayment.Id;
			ClaimId = claimPayment.ClaimId;
			ClaimVersion = claimPayment.ClaimVersion;

			PaidLMDA = claimPayment.PaidLMDA;
			PaidWDA = claimPayment.PaidWDA;

			LMDATariffTRSW = claimPayment.LMDATariffTRSW;
			LMDATariffTRST = claimPayment.LMDATariffTRST;
			LMDATariffTRCO = claimPayment.LMDATariffTRCO;

			WDATariffCWRG = claimPayment.WDATariffCWRG;
		}
	}

}