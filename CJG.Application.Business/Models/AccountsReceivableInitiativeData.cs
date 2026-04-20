using System;

namespace CJG.Application.Business.Models
{
	public class AccountsReceivableInitiativeData
	{
		public int Number;
		[Obsolete("This is a new field added in place of the existing Sum of 'Overpayments'")]
		public decimal HistoricalTotal;
		public decimal Total;
		public decimal TotalWDA;
		public decimal TotalLMDA;
	}
}