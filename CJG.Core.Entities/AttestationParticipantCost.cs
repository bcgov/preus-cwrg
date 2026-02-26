using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class AttestationParticipantCost : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public int AttestationParticipantId { get; set; }

		[ForeignKey(nameof(AttestationParticipantId))]
		public virtual AttestationParticipant AttestationParticipant { get; set; }

		public int? EligibleExpenseBreakdownId { get; set; }

		[ForeignKey(nameof(EligibleExpenseBreakdownId))]
		public virtual EligibleExpenseBreakdown EligibleExpenseBreakdown { get; set; }

		public string RequestedOtherDefinition { get; set; }
		public decimal RequestedTotalSpent { get; set; }

		public string ApprovedOtherDefinition { get; set; }
		public decimal ApprovedTotalSpent { get; set; }
	}
}