using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CJG.Application.Business.Models;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Int.Models;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;

namespace CJG.Web.External.Areas.Int.Controllers
{
    /// <summary>
    /// AccountsReceivableController class, provides endpoints to view Accounts Receivable numbers
    /// </summary>
    [RouteArea("Int")]
	public class AccountsReceivableController : BaseController
	{
		private readonly IStaticDataService _staticDataService;
		private readonly IAccountsReceivableService _accountsReceivableService;
		private readonly IFiscalYearService _fiscalYearService;

		/// <summary>
		/// Creates a new instance of a AccountsReceivableController class.
		/// </summary>
		/// <param name="controllerService"></param>
		/// <param name="accountsReceivableService"></param>
		/// <param name="fiscalYearService"></param>
		public AccountsReceivableController(
			IControllerService controllerService,
			IAccountsReceivableService accountsReceivableService,
			IFiscalYearService fiscalYearService)
			: base(controllerService.Logger)
		{
			_staticDataService = controllerService.StaticDataService;
			_accountsReceivableService = accountsReceivableService;
			_fiscalYearService = fiscalYearService;
		}

		[HttpGet]
		[Route("Admin/AccountsReceivable/View")]
		public ActionResult Index()
		{
			return View();
		}


		[HttpGet]
		[Route("Admin/AccountsReceivable/FiscalYears")]
		public JsonResult GetFiscalYears()
		{
			IEnumerable<KeyValuePair<int, string>> results = new KeyValuePair<int, string>[0];
			try
			{
				var fiscalYears = _staticDataService.GetFiscalYears(2);
				results = fiscalYears
					.Select(fy => new KeyValuePair<int, string>(fy.Id, fy.Caption))
					.ToArray();
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}

			return Json(results, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Route("Admin/AccountsReceivable/Data")]
		public JsonResult GetAccountsReceivables(int? fiscalYearId)
		{
			var model = new AccountsReceivableModel();

			var fiscalYear = !fiscalYearId.HasValue
				? _fiscalYearService.GetCurrentFiscalYear()
				: _fiscalYearService.Get(fiscalYearId.Value);

			try
			{
				model.SelectedFiscalYearId = fiscalYear.Id;
				model.FiscalYearBreakdowns = _accountsReceivableService.GetAccountsReceivableReportData();
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Route("Admin/AccountsReceivable/Breakdowns")]
		public JsonResult GetAccountsReceivableBreakdowns(int fiscalYearId)
		{
			var model = new List<AccountsReceivableApplicationBreakdownModel>();
			var fiscalYear = _fiscalYearService.Get(fiscalYearId);

			try
			{
				var breakdowns = _accountsReceivableService.GetAccountsReceivableBreakdownData(fiscalYearId);
				model.AddRange(breakdowns);
			}
			catch (Exception ex)
			{
				//HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

	}
}