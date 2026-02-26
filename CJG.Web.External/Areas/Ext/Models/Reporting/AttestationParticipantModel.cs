using System.Collections.Generic;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Ext.Models.Reporting
{
	public class AttestationParticipantModel : BaseViewModel
	{
		public int ParticipantFormId { get; set; }
		public string ParticipantName { get; set; }
		public decimal TotalApprovedCost { get; set; }

		// These are only used by the ng-side models to calculate totals
		public decimal TotalAmountSpent { get; set; }
		public decimal UnusedFunds { get; set; }

		public int? ProgramInitiativeId { get; set; }

		public List<AttestationParticipantCostModel> Costs { get; set; }

		public AttestationParticipantModel()
		{
			UnusedFunds = TotalApprovedCost - TotalAmountSpent;
		}
	}
}