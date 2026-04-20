using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using CJG.Application.Business.Models.DirectorsReport;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models
{
	public class ProgramFundingBudgetSummaryModel : BaseViewModel
	{
		public bool ShowDirectorDashboard { get; set; }
		public bool CanUpdateBudget { get; set; }
		public int FiscalYearId { get; set; }

		public DateTime ReportDay { get; set; }
		public string FiscalYear { get; set; }
		public string AgreementYearRange { get; set; }

		public DateTime? BudgetLastUpdated { get; set; }

		public List<BudgetRowModel> OpeningBudgetRows { get; set; }
		public List<BudgetRowModel> ClosingBudgetRows { get; set; }
		public List<BudgetSummaryModel> DirectorsReport { get; set; }

		public ProgramFundingBudgetSummaryModel()
		{
		}

		public ProgramFundingBudgetSummaryModel(IProgramFundingReportsService programFundingReportsService, FiscalYear fiscalYear, IPrincipal user)
		{
			var directorBudgets = programFundingReportsService.GetProgramFundingBudgets(fiscalYear).ToList();

			directorBudgets.Add(new ProgramFundingBudget
			{
				ProgramInitiative = null
			});

			var directorsReport = programFundingReportsService.GetBudgetSummaryModels(fiscalYear, directorBudgets).ToList();
			var openingBudgetRows = programFundingReportsService.GetBudgetRows(fiscalYear, directorBudgets, ProgramFundingBudgetEntryType.Opening).ToList();
			var closingBudgetRows = programFundingReportsService.GetBudgetRows(fiscalYear, directorBudgets, ProgramFundingBudgetEntryType.Closing).ToList();

			var allowBudgetUpdates = user.IsInRole("Director") || user.IsInRole("Financial Clerk");
			var showDirectorDashboard = allowBudgetUpdates || user.IsInRole("Assessor");

			ShowDirectorDashboard = showDirectorDashboard;
			CanUpdateBudget = allowBudgetUpdates;

			ReportDay = AppDateTime.UtcNow;
			FiscalYear = fiscalYear.Caption;
			AgreementYearRange = $"{fiscalYear.StartDate.Year}-{fiscalYear.EndDate:yy}";
			BudgetLastUpdated = directorsReport.Max(d => d.LastUpdated);

			DirectorsReport = directorsReport;
			OpeningBudgetRows = openingBudgetRows;
			ClosingBudgetRows = closingBudgetRows;
		}
	}
}