using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	/// <summary>
	///  This holds the value of the budget associated with each opening and closing budget row per Program Funding Report (LMDA/WDA Report)
	/// </summary>
	public class ProgramFundingBudgetEntry : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public int ProgramFundingBudgetId { get; set; }
		[ForeignKey(nameof(ProgramFundingBudgetId))]
		public virtual ProgramFundingBudget ProgramFundingBudget { get; set; }

		public int ProgramFundingBudgetRowId { get; set; }
		[ForeignKey(nameof(ProgramFundingBudgetRowId))]
		public virtual ProgramFundingBudgetRow ProgramFundingBudgetRow { get; set; }

		public decimal? Budget { get; set; }
	}
}