using System.Collections.Generic;

namespace CJG.Application.Business.Models.GlobalBudget
{
	public class IntakePeriodSlotModel
	{
		public int SlotSequence { get; set; }
		public List<GrantStreamBudgetModel> Streams { get; set; }
	}
}