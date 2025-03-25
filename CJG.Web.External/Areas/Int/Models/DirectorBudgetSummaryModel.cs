using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using CJG.Application.Business.Models;
using CJG.Application.Business.Models.DirectorsReport;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models
{
    public class DirectorBudgetSummaryModel : BaseViewModel
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

		public DirectorBudgetSummaryModel()
		{
		}

		public DirectorBudgetSummaryModel(IDirectorReportsService directorReportsService, FiscalYear fiscalYear, IPrincipal user)
		{
			var directorBudgets = directorReportsService.GetDirectorBudgets(fiscalYear).ToList();

			var directorsReport = directorReportsService.GetBudgetSummaryModels(fiscalYear, directorBudgets).ToList();
			var openingBudgetRows = directorReportsService.GetBudgetRows(fiscalYear, directorBudgets, DirectorBudgetEntryType.Opening).ToList();
			var closingBudgetRows = directorReportsService.GetBudgetRows(fiscalYear, directorBudgets, DirectorBudgetEntryType.Closing).ToList();

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