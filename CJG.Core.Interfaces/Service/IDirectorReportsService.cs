using System.Collections.Generic;
using CJG.Application.Business.Models;
using CJG.Core.Entities;

namespace CJG.Core.Interfaces.Service
{
	public interface IDirectorReportsService : IService
	{
		IEnumerable<DirectorBudget> GetDirectorBudgets(FiscalYear fiscalYear);
		IEnumerable<BudgetSummaryModel> GetBudgetSummaryModels(FiscalYear fiscalYear);
		void UpdateBudget(List<BudgetSummaryModel> budgetSummaries);
	}
}