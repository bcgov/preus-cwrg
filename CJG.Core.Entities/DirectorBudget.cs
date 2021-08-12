using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class DirectorBudget : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public int FiscalYearId { get; set; }

		[ForeignKey(nameof(FiscalYearId))]
		public virtual FiscalYear FiscalYear { get; set; }

		public BudgetEntryType BudgetEntryType { get; set; }
		public string StreamFilter { get; set; }

		public decimal? Budget { get; set; }
		public decimal? ForecastBudget { get; set; }
	}

	public enum BudgetEntryType
	{
		CoreStream = 1,
		NonCore = 2
	}
}