using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class GlobalProgramBudget: EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public decimal? IntakeBudget { get; set; }

		public int TrainingPeriodId { get; set; }
		public int GrantStreamId { get; set; }
	}
}