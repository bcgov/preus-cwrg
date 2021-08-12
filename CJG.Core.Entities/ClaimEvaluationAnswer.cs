using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class ClaimEvaluationAnswer : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[ForeignKey("ClaimEvaluation")]
		public int ClaimEvaluationId { get; set; }
		public virtual ClaimEvaluation ClaimEvaluation { get; set; }

		// This is the question that was asked, but we're not linking to the entity itself
		public int ClaimEvaluationFormQuestionReferenceId { get; set; }

		public ClaimEvaluationFormQuestionType QuestionType { get; set; }
		public string QuestionAsked { get; set; }
		public int AnswerGiven { get; set; }
		public int RowSequence { get; set; }
	}
}