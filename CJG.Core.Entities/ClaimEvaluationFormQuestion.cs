using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class ClaimEvaluationFormQuestion : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public string Text { get; set; }

		[Required]
		public ClaimEvaluationFormQuestionType ClaimEvaluationFormQuestionType { get; set; }

		[Required]
		public int RowSequence { get; set; }

		[Required]
		public bool IsActive { get; set; }
	}
}