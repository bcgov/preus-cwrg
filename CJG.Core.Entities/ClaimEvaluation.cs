using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class ClaimEvaluation : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[ForeignKey("Claim"), Column(Order = 0)]
		public int ClaimId { get; set; }
		public virtual Claim Claim { get; set; }

		[ForeignKey("Claim"), Column(Order = 1)]
		public int ClaimVersion { get; set; }

		public virtual ICollection<ClaimEvaluationAnswer> EvaluationAnswers { get; set; }
	}
}