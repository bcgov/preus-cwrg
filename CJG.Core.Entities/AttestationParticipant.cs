using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class AttestationParticipant : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public int AttestationId { get; set; }

		[ForeignKey(nameof(AttestationId))]
		public virtual Attestation Attestation { get; set; }

		public int ParticipantFormId { get; set; }

		[ForeignKey(nameof(ParticipantFormId))]
		public virtual ParticipantForm ParticipantForm { get; set; }

		public int? ProgramInitiativeId { get; set; }

		[ForeignKey(nameof(ProgramInitiativeId))]
		public virtual ProgramInitiative ProgramInitiative { get; set; }

		// Maybe not used? Saved for history.
		public decimal TotalApprovedCost { get; set; }

		// Maybe not used? Saved for history.
		public decimal TotalAmountSpent { get; set; }

		// Maybe not used? Saved for history.
		public decimal UnusedFunds { get; set; }

		public virtual ICollection<AttestationParticipantCost> Costs { get; set; } = new List<AttestationParticipantCost>();
	}
}