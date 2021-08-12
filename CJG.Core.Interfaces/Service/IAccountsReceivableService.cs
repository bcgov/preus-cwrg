using System.Collections.Generic;
using CJG.Application.Business.Models;
using CJG.Core.Entities;

namespace CJG.Core.Interfaces.Service
{
	public interface IAccountsReceivableService : IService
	{
		AccountsReceivable GetByApplication(GrantApplication grantApplication);
		void UpdateAccountsReceivables(ApplicationAccountsReceivableUpdateModel model);

		List<AccountsReceivableBreakdown> GetAccountsReceivableReportData(int? fiscalYearId = null);
		List<AccountsReceivableApplicationBreakdownModel> GetAccountsReceivableBreakdownData(int fiscalYearId);
	}
}