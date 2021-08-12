using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class GrantApplicationEvaluationAnswer : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[ForeignKey("GrantApplicationEvaluation")]
		public int GrantApplicationEvaluationId { get; set; }
		public virtual GrantApplicationEvaluation GrantApplicationEvaluation { get; set; }

		// This is the question that was asked, but we're not linking to the entity itself
		public int EvaluationFormQuestionReferenceId { get; set; }

		public EvaluationFormQuestionType QuestionType { get; set; }
		public string QuestionAsked { get; set; }
		public int AnswerGiven { get; set; }
		public int RowSequence { get; set; }
	}
}