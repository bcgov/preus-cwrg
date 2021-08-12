using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class EvaluationFormQuestion : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public string Text { get; set; }

		[Required]
		public EvaluationFormQuestionType EvaluationFormQuestionType { get; set; }

		[Required]
		public int RowSequence { get; set; }

		[Required]
		public bool IsActive { get; set; }
	}
}