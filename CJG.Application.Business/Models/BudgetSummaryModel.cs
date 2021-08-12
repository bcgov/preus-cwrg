using System;

namespace CJG.Application.Business.Models
{
	public class BudgetSummaryModel
	{
		public int DirectorBudgetId { get; set; }

		public string GroupingName { get; set; }
		public string GroupingStreams { get; set; }

		// Directors Report
		public decimal Budget { get; set; }
		public int ApplicationsReceived { get; set; }
		public decimal ForecastCommitmentAmount { get; set; }
		public decimal CancelledApplicationsTotal { get; set; }
		public decimal ApplicationsReceivedTotal { get; set; }
		public decimal NewApplicationsTotal { get; set; }
		public int ApplicationsApproved { get; set; }
		public decimal ApprovedCommitmentAmount { get; set; }
		public decimal ClaimsProcessedTotal { get; set; }
		public bool IncludeInSlippageCalculation { get; set; }

		// Weekly Claims Reports
		public int NumberOfClaimsSubmitted { get; set; }
		public int NumberOfClaimsLeftToSubmit { get; set; }
		public decimal ClaimsUnclaimedTotal { get; set; }
		public int ReceivablesSetupNumber { get; set; }
		public decimal ReceivablesSetupTotal { get; set; }
		public decimal SlippageTotal { get; set; }

		// Forecast Report
		public decimal ForecastBudget { get; set; }

		public DateTime? LastUpdated { get; set; }
	}
}