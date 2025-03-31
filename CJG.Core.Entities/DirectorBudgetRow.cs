using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	/// <summary>
	///  This holds the names of the opening and closing budget rows on the Directors Report
	/// </summary>
	public class DirectorBudgetRow : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public DirectorBudgetEntryType EntryType { get; set; }

		public int FiscalYearId { get; set; }

		[ForeignKey(nameof(FiscalYearId))]
		public virtual FiscalYear FiscalYear { get; set; }

		public string Name { get; set; }

		public int Sequence { get; set; }
	}
}