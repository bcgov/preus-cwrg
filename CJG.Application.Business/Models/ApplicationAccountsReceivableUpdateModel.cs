using System;
using System.Collections.Generic;

namespace CJG.Application.Business.Models
{
	public class ApplicationAccountsReceivableUpdateModel
	{
		public int GrantApplicationId { get; set; }
		public DateTime PaidDate { get; set; }

		public List<KeyValuePair<int, OverpaymentGroup>> ReceivablesByServiceCategory = new List<KeyValuePair<int, OverpaymentGroup>>();
	}

	public class OverpaymentGroup
	{
		public decimal Overpayment { get; set; }
		public decimal OverpaymentLMDA { get; set; }
		public decimal OverpaymentWDA { get; set; }

		public OverpaymentGroup(decimal overpayment, decimal overpaymentLMDA, decimal overpaymentWDA)
		{
			Overpayment = overpayment;
			OverpaymentLMDA = overpaymentLMDA;
			OverpaymentWDA = overpaymentWDA;
		}
	}
}