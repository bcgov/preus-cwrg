using System.Collections.Generic;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.Attestation
{
	public class AttestationParticipantModel : BaseViewModel
	{
		public int AttestationParticipantId { get; set; }
		public int ParticipantFormId { get; set; } // Probably don't need this
		public string ParticipantName { get; set; }
		public decimal TotalApprovedCost { get; set; }

		public bool TariffAffected { get; set; }
		public string TariffCategory { get; set; }

		// These are only used by the ng-side models to calculate totals
		public decimal TotalAmountSpent { get; set; }
		public decimal UnusedFunds { get; set; }

		public int? ParticipantFundingStreamId { get; set; }
		public string ParticipantFundingStream { get; set; }

		public List<AttestationParticipantCostModel> Costs { get; set; }

		public AttestationParticipantModel()
		{
			UnusedFunds = TotalApprovedCost - TotalAmountSpent;
		}
	}
}