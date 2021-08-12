using System.Collections.Generic;
using CJG.Application.Business.Models;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models
{
	public class AccountsReceivableModel : BaseViewModel
	{
		public int SelectedFiscalYearId { get; set; }
		public List<AccountsReceivableBreakdown> FiscalYearBreakdowns { get; set; }
	}
}