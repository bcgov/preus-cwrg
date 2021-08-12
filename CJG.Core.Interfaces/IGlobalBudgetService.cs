using System.Collections.Generic;
using CJG.Application.Business.Models.GlobalBudget;

namespace CJG.Core.Interfaces
{
	public interface IGlobalBudgetService
	{
		List<IntakePeriodSlotModel> GetIntakePeriodStreams(int? fiscalYearId);
		void UpdateBudget(int? fiscalYearId, List<IntakePeriodSlotModel> intakePeriodsSlots);
	}
}