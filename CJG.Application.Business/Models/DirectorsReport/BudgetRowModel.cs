using System.Collections.Generic;

namespace CJG.Application.Business.Models.DirectorsReport
{
	public class BudgetRowModel
	{
		public int BudgetRowId { get; set; }
		public string Name { get; set; }
		public decimal? Budget { get; set; }
		public int Sequence { get; set; }

		public List<BudgetEntryModel> DirectorBudgetEntries { get; set; }
	}
}