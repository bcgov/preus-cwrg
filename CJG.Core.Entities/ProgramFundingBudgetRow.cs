using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	/// <summary>
	///  This holds the names of the opening and closing budget rows on the Program Funding Report (LMDA/WDA Report)
	/// </summary>
	public class ProgramFundingBudgetRow : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public ProgramFundingBudgetEntryType EntryType { get; set; }

		public int FiscalYearId { get; set; }

		[ForeignKey(nameof(FiscalYearId))]
		public virtual FiscalYear FiscalYear { get; set; }

		public string Name { get; set; }

		public int Sequence { get; set; }
	}
}