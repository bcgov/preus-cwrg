namespace CJG.Application.Business.Models.GlobalBudget
{
	public class GrantStreamBudgetModel
	{
		public string StreamName { get; set; }
		public decimal StreamBudget { get; set; }
		public decimal CommitmentTarget { get; set; }
		public decimal ForecastedCommitment { get; set; }
		public decimal CommittedAmount { get; set; }
		public decimal CommittedUnclaimedAmount { get; set; }
		public decimal CommittedClaimedAmount { get; set; }
		public int ApplicationsApproved { get; set; }
		public int ApplicationsUnderAssessment { get; set; }
		public decimal TotalSpent { get; set; }
		public decimal SlippageAmount { get; set; }
		public decimal SlippagePercent { get; set; }
		public decimal OverUnderAllocation { get; set; }
		public decimal UnallocatedFundingMovingForward { get; set; }  // Calculated in JS
		public int TrainingPeriodId { get; set; }
		public int GrantStreamId { get; set; }

		public int SortOrder { get; set; }  // Used to sort CRS "Community Response" to the bottom of the heap.
	}
}