using System;

namespace CJG.Application.Business.Models
{
	public class AccountsReceivableApplicationBreakdownModel
	{
		public string FiscalYear { get; set; }
		public string StreamName { get; set; }
		public int GrantApplicationId { get; set; }
		public string ApplicationNumber { get; set; }
		public decimal Overpayment { get; set; }
		public DateTime DateCreated { get; set; }
		public string OverpaymentCategory { get; set; }
	}
}