namespace CJG.Application.Business.Models
{
	public class AccountsReceivableBreakdown
	{
		public int FiscalYearId { get; set; }
		public string FiscalYear { get; set; }

		public int CoreApplicationNumber { get; set; }
		public int CRSApplicationNumber { get; set; }

		public decimal CoreApplicationTotal { get; set; }
		public decimal CRSApplicationTotal { get; set; }
	}
}