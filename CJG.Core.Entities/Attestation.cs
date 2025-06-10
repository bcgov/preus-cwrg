using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class Attestation: EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required, DefaultValue(0)]
		public AttestationState State { get; set; }

		/// <summary>
		/// The Claimed Costs from Schedule A
		/// </summary>
		public decimal ClaimedCosts { get; set; }

		/// <summary>
		/// The user supplied costs that were allocated to training
		/// </summary>
		public decimal AllocatedCosts { get; set; }

		/// <summary>
		///  Difference between Claimed and Allocated
		/// </summary>
		public decimal UnusedFunds { get; set; }

		public bool? AttestationNotApplicable { get; set; }

		public virtual ICollection<Attachment> Documents { get; set; } = new List<Attachment>();
	}
}