using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class ProofOfPayment : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required, DefaultValue(0)]
		public ProofOfPaymentState State { get; set; }

		public bool? ProofNotApplicable { get; set; }

		public virtual ICollection<Attachment> Documents { get; set; } = new List<Attachment>();
	}
}