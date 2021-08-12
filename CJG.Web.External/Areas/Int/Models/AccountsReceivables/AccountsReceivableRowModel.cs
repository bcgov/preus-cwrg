namespace CJG.Web.External.Areas.Int.Models.AccountsReceivables
{
	public class AccountsReceivableRowModel
	{
		public int ServiceCategoryId { get; set; }
		public string ServiceCategoryName { get; set; }
		public decimal Overpayment { get; set; }
	}
}