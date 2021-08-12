using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class GrantApplicationEvaluation : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[ForeignKey("GrantApplication")]
		public int GrantApplicationId { get; set; }
		public virtual GrantApplication GrantApplication { get; set; }

		public string HighLevelRationale { get; set; }
		public string ApplicationNotes { get; set; }

		public EvaluationStatus EvaluationStatus { get; set; }

		public virtual ICollection<GrantApplicationEvaluationAnswer> EvaluationAnswers { get; set; }
	}

	public enum EvaluationStatus
	{
		NotStarted = 0,
		Started = 1,
		Complete = 2
	}
}