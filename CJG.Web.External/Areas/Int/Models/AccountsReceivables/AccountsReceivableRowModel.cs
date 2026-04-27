using System;

namespace CJG.Web.External.Areas.Int.Models.AccountsReceivables
{
	public class AccountsReceivableRowModel
	{
		public int ServiceCategoryId { get; set; }
		public string ServiceCategoryName { get; set; }

		[Obsolete("This may be removed in favour of the LMDA/WDA split. Keeping here for exporting/data-history.")]
		public decimal Overpayment { get; set; }

		public decimal OverpaymentLMDA { get; set; }
		public decimal OverpaymentWDA { get; set; }
	}
}