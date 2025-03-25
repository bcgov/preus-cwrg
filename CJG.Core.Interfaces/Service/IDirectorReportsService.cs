using System.Collections.Generic;
using CJG.Application.Business.Models;
using CJG.Application.Business.Models.DirectorsReport;
using CJG.Core.Entities;

namespace CJG.Core.Interfaces.Service
{
	public interface IDirectorReportsService : IService
	{
		IEnumerable<DirectorBudget> GetDirectorBudgets(FiscalYear fiscalYear);
		IEnumerable<BudgetSummaryModel> GetBudgetSummaryModels(FiscalYear fiscalYear, List<DirectorBudget> directorBudgets);
		List<BudgetRowModel> GetBudgetRows(FiscalYear fiscalYear, List<DirectorBudget> directorBudgets, DirectorBudgetEntryType budgetEntryType);
		void UpdateBudget(List<BudgetSummaryModel> budgetSummaries, List<BudgetRowModel> openingRows, List<BudgetRowModel> closingRows);
	}
}