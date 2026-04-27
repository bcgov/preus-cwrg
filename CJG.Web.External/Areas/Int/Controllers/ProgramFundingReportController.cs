using System;
using System.Linq;
using System.Web.Mvc;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Int.Models;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;

namespace CJG.Web.External.Areas.Int.Controllers
{
	[RouteArea("Int")]
	[RoutePrefix("Admin/Reports/LMDAWDAReport")]
	public class ProgramFundingReportController : BaseController
	{
		private readonly IFiscalYearService _fiscalYearService;
		private readonly IProgramFundingReportsService _programFundingReportsService;
		private readonly IStaticDataService _staticDataService;

		public ProgramFundingReportController(
			IControllerService controllerService,
			IFiscalYearService fiscalYearService,
			IProgramFundingReportsService programFundingReportsService,
			IStaticDataService staticDataService) : base(controllerService.Logger)
		{
			_fiscalYearService = fiscalYearService;
			_programFundingReportsService = programFundingReportsService;
			_staticDataService = staticDataService;
		}

		[Authorize]
		public ActionResult Index()
		{
			ViewBag.FiscalYearId = _staticDataService.GetFiscalYear(0)?.Id;
			if (TempData["Message"] != null)
			{
				ViewBag.Message = TempData["Message"].ToString();
				TempData["Message"] = "";
			}

			return View();
		}

		[Route("FiscalYears")]
		public JsonResult GetFiscalYears()
		{
			var fiscalYears = _staticDataService.GetFiscalYears(2)
				.Select(t => new
				{
					t.Id,
					t.Caption
				})
				.ToArray();

			return Json(fiscalYears, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Route("Dashboard/{fiscalYearId:int?}")]
		public JsonResult GetProgramFundingBudgetDashboard(int? fiscalYearId)
		{
			var model = new ProgramFundingBudgetSummaryModel();
			try
			{
				var fiscalYear = _fiscalYearService.GetFiscalYear(fiscalYearId);
				model = new ProgramFundingBudgetSummaryModel(_programFundingReportsService, fiscalYear, User);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[Authorize(Roles = "Director, Financial Clerk")]
		[Route("Dashboard/Save")]
		public JsonResult UpdateBudgetDashboardAmounts(ProgramFundingBudgetSummaryModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					_programFundingReportsService.UpdateBudget(model.DirectorsReport, model.OpeningBudgetRows, model.ClosingBudgetRows);

					var fiscalYear = _fiscalYearService.GetFiscalYear(model.FiscalYearId);
					model = new ProgramFundingBudgetSummaryModel(_programFundingReportsService, fiscalYear, User);
				}
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[Authorize(Roles = "Director, Financial Clerk")]
		[Route("Dashboard/Export")]
		public ActionResult ExportProgramFundingDashboardToExcel(ProgramFundingBudgetSummaryModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var excelOutput = model.GetExcelContent();
					var fileDownloadName = $"lmda_wda_report_{AppDateTime.Now:yyyy-MM-dd_HH-mm}.xlsx";
					var base64 = Convert.ToBase64String(excelOutput, 0, excelOutput.Length);

					return Json(new
					{
						FileData = base64,
						FileName = fileDownloadName,
						FileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
					}, JsonRequestBehavior.AllowGet);
				}
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}
	}
}