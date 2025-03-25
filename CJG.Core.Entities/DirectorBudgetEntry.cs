using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	/// <summary>
	///  This holds the value of the budget associated with each opening and closing budget row per Directors Report
	/// </summary>
	public class DirectorBudgetEntry : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public int DirectorBudgetId { get; set; }
		[ForeignKey(nameof(DirectorBudgetId))]
		public virtual DirectorBudget DirectorBudget { get; set; }

		public int DirectorBudgetRowId { get; set; }
		[ForeignKey(nameof(DirectorBudgetRowId))]
		public virtual DirectorBudgetRow DirectorBudgetRow { get; set; }

		public decimal? Budget { get; set; }
	}
}