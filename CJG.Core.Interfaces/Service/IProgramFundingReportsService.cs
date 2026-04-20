using System.Collections.Generic;
using CJG.Application.Business.Models.DirectorsReport;
using CJG.Core.Entities;

namespace CJG.Core.Interfaces.Service
{
	public interface IProgramFundingReportsService : IService
	{
		IEnumerable<ProgramFundingBudget> GetProgramFundingBudgets(FiscalYear fiscalYear);
		IEnumerable<BudgetSummaryModel> GetBudgetSummaryModels(FiscalYear fiscalYear, List<ProgramFundingBudget> programFundingBudgets);
		List<BudgetRowModel> GetBudgetRows(FiscalYear fiscalYear, List<ProgramFundingBudget> directorBudgets, ProgramFundingBudgetEntryType budgetEntryType);
		void UpdateBudget(List<BudgetSummaryModel> budgetSummaries, List<BudgetRowModel> openingRows, List<BudgetRowModel> closingRows);
	}
}