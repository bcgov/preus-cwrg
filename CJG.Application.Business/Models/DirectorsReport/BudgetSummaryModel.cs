using System;

namespace CJG.Application.Business.Models.DirectorsReport
{
	public class BudgetSummaryModel
	{
		public int DirectorBudgetId { get; set; }

		public string GroupingName { get; set; }
		public string GroupingStreams { get; set; }

		// Directors Report
		public decimal Budget { get; set; }
		public bool IncludeInSlippageCalculation { get; set; }

		// New Directors Reports (Program Initiative Version)
		public decimal DirectorsReportCommittedScheduleA { get; set; }
		public decimal DirectorsReportClaimsProcessed { get; set; }
		public decimal DirectorsReportUnclaimed { get; set; }
		public decimal DirectorsReportReceivables { get; set; }
		public decimal DirectorsReportSlippage { get; set; }
		public decimal DirectorsReportYtdActual { get; set; }
		public decimal DirectorsReportPartialAvailableBudget { get; set; }

		// These three values are used for front-end output only - Not used internally
		public decimal DirectorsReportAdjustedBudget { get; set; }
		public decimal DirectorsReportAvailableBudget { get; set; }
		public decimal DirectorsReportRemainingBudget { get; set; }

		// Summary Group Report Values
		public int SummaryReportApplicationsReceived { get; set; }
		public int SummaryReportApplicationsApproved { get; set; }
		public decimal SummaryReportApprovedCommitmentAmount { get; set; }
		public decimal SummaryReportApplicationsPendingDecision { get; set; }
		public decimal SummaryReportCancelledApplicationsTotal { get; set; }
		public decimal SummaryReportApplicationsReceivedTotal { get; set; }

		// Weekly Claims Reports
		//public int NumberOfClaimsSubmitted { get; set; }
		//public int NumberOfClaimsLeftToSubmit { get; set; }
		//public decimal ClaimsUnclaimedTotal { get; set; }
		//public int ReceivablesSetupNumber { get; set; }
		//public decimal ReceivablesSetupTotal { get; set; }
		//public decimal SlippageTotal { get; set; }

		// Forecast Report
		//public decimal ForecastBudget { get; set; }

		public DateTime? LastUpdated { get; set; }
	}
}