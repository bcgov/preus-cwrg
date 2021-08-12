using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.AccountsReceivables
{
	public class ApplicationAccountsReceivableModel : BaseViewModel
	{
		public string RowVersion { get; set; }

		[Required(ErrorMessage = "Paid Date is required")]
		public DateTime? AccountsReceivablePaidDate { get; set; }
		public string AccountsReceivablePaidDateFormatted { get; set; }
		public List<AccountsReceivableRowModel> Records { get; set; } = new List<AccountsReceivableRowModel>();

		public ApplicationAccountsReceivableModel()
		{
		}

		public ApplicationAccountsReceivableModel(GrantApplication grantApplication, AccountsReceivable accountsReceivable)
		{
			Id = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);
			Records = grantApplication.GrantOpening.GrantStream.GrantProgram.ProgramConfiguration.EligibleExpenseTypes
				.Where(ee => ee.IsActive)
				.Where(ee => ee.ServiceCategory.IsActive)
				.OrderBy(ee => ee.ServiceCategory.RowSequence)
				.Select(ee => new AccountsReceivableRowModel
				{
					ServiceCategoryId = ee.ServiceCategory.Id,
					ServiceCategoryName = ee.ServiceCategory.Caption,
					Overpayment = 0
				})
				.ToList();

			if (accountsReceivable == null)
				return;

			AccountsReceivablePaidDate = accountsReceivable.PaidDate;
			AccountsReceivablePaidDateFormatted = accountsReceivable.PaidDate.ToDateFormat();

			foreach (var ar in accountsReceivable.AccountsReceivableEntries)
			{
				var entry = Records.FirstOrDefault(r => r.ServiceCategoryId == ar.ServiceCategoryId);
				if (entry == null)
					continue;

				entry.Overpayment = ar.Overpayment;
			}
		}
	}
}