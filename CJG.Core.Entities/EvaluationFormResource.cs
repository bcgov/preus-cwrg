using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class EvaluationFormResource : EntityBase
	{
		/// <summary>
		/// get/set - Primary key uses IDENTITY.
		/// </summary>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public virtual Attachment Attachment { get; set; }

		public int RowSequence { get; set; }
	}
}