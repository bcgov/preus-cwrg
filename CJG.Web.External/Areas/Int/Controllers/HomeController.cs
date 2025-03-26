using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http.Validation;
using System.Web.Mvc;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Identity;
using CJG.Web.External.Areas.Int.Models;
using CJG.Web.External.Areas.Int.Models.SteamAgreementDetails;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;
using ClosedXML.Excel;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Newtonsoft.Json;

namespace CJG.Web.External.Areas.Int.Controllers
{
    /// <summary>
    /// HomeController class, provides default home page endpoints.
    /// </summary>
    public class HomeController : BaseController
	{
		private readonly ISiteMinderService _siteMinderService;
		private readonly ApplicationUserManager _userManager;
		private readonly IFiscalYearService _fiscalYearService;
		private readonly IAuthenticationManager _authenticationManager;
		private readonly IDirectorReportsService _directorReportsService;
		private readonly IStaticDataService _staticDataService;

		public HomeController(
			IControllerService controllerService,
			ApplicationUserManager userManager, 
			IFiscalYearService fiscalYearService,
			IAuthenticationManager authenticationManager,
			IDirectorReportsService directorReportsService,
			IStaticDataService staticDataService) : base(controllerService.Logger)
		{
			_siteMinderService = controllerService.SiteMinderService;
			_userManager = userManager;
			_fiscalYearService = fiscalYearService;
			_authenticationManager = authenticationManager;
			_directorReportsService = directorReportsService;
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

		[HttpGet]
		public ActionResult Logout()
		{
			// CJG-612: Check that referrer matches. Note, this is handled by STG for forms, but LogOut does not have a form
			//			so we need to check manually here.
			if (!Request.Url.Host.Contains(Request.UrlReferrer.Host))
				return Redirect("~/Error");

			_authenticationManager.SignOut();
			_siteMinderService.LogOut();

			return RedirectToAction("Index", "Home", new { area = ""});
		}

		[ChildActionOnly]
		public ActionResult InternalHeaderPartial()
		{
			var user = _userManager.FindById(User.Identity.GetUserId());
			return PartialView("_InternalHeader", user?.InternalUser);
		}

		[HttpGet]
		[AuthorizeAction(Privilege.IA1)]
		public ActionResult MyFiles()
		{
			return RedirectToRoute(nameof(WorkQueueController.WorkQueueView));
		}

		[Route("Int/Home/Director/FiscalYears")]
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
		[Route("Int/Home/Director/Dashboard/{fiscalYearId:int?}")]
		public JsonResult GetDirectorBudgetDashboard(int? fiscalYearId)
		{
			var model = new DirectorBudgetSummaryModel();
			var showDirectorDashboard = User.IsInRole("Director") || User.IsInRole("Financial Clerk") || User.IsInRole("Assessor");

			if (!showDirectorDashboard)
				return Json(model, JsonRequestBehavior.AllowGet);

            try
            {
	            var fiscalYear = _fiscalYearService.GetFiscalYear(fiscalYearId);
	            model = new DirectorBudgetSummaryModel(_directorReportsService, fiscalYear, User);
            }
			catch (Exception ex)
            {
                HandleAngularException(ex, model);
            }

            return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[Authorize(Roles = "Director, Financial Clerk")]
		[Route("Int/Home/Director/Dashboard/Save")]
		public JsonResult UpdateBudgetDashboardAmounts(DirectorBudgetSummaryModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					_directorReportsService.UpdateBudget(model.DirectorsReport, model.OpeningBudgetRows, model.ClosingBudgetRows);

					var fiscalYear = _fiscalYearService.GetFiscalYear(model.FiscalYearId);
					model = new DirectorBudgetSummaryModel(_directorReportsService, fiscalYear, User);
				}
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[Authorize(Roles = "Director, Financial Clerk")]
		[Route("Int/Home/Director/Dashboard/Export")]
		public ActionResult ExportDirectorDashboardToExcel(DirectorBudgetSummaryModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var excelOutput = GetExcelContent(model);
					var fileDownloadName = $"director_dashboard_{AppDateTime.Now:yyyy-MM-dd_HH-mm}.xlsx";
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

		private byte[] GetExcelContent(DirectorBudgetSummaryModel model)
		{
			using (var stream = new MemoryStream())
			{
				var wb = new XLWorkbook();
				var ws = wb.AddWorksheet();

				ws.ShowRowColHeaders = true;
				ws.Name = "CWRG Directors Report";

				var columnOffset = 1;
				var rowOffset = 1;

				ws.Cell(rowOffset, columnOffset).SetValue($"CWRG Directors Report - {model.FiscalYear}");
				ws.Cell(rowOffset, columnOffset).Style.Font.Bold = true;
				ws.Cell(rowOffset, columnOffset).Style.Font.FontSize = 12;
				ws.Cell(rowOffset, columnOffset).Style.Fill.BackgroundColor = XLColor.AshGrey;

				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					var budgetTitle = budget.GroupingStreams;
					ws.Cell(rowOffset, columnOffset).SetValue(budgetTitle);
					ws.Cell(rowOffset, columnOffset).Style.Font.Bold = true;
					ws.Cell(rowOffset, columnOffset).Style.Font.FontSize = 12;
					ws.Cell(rowOffset, columnOffset).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
					ws.Cell(rowOffset, columnOffset).Style.Fill.BackgroundColor = XLColor.AshGrey;
				}

				rowOffset = 2;
				columnOffset = 1;

				ws.Cell(rowOffset, columnOffset).SetValue("Budget");
				ws.Cell(rowOffset, columnOffset).Style.Font.Bold = true;
				ws.Cell(rowOffset, columnOffset).Style.Fill.BackgroundColor = XLColor.MintGreen;
				ws.Cell(rowOffset, columnOffset).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;

					var xlCell = ws.Cell(rowOffset, columnOffset);
					xlCell.Style.Font.Bold = true;
					xlCell.Style.Fill.BackgroundColor = XLColor.MintGreen;
					xlCell.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
					FormatMoneyCell(xlCell, budget.Budget);
				}

				rowOffset = 3;
				foreach (var budgetRow in model.OpeningBudgetRows)
				{
					var rowTitle = !string.IsNullOrWhiteSpace(budgetRow.Name) ? budgetRow.Name : "Adjustment";
					ws.Cell(rowOffset, 1).SetValue(rowTitle);
					ws.Cell(rowOffset, 1).Style.Fill.BackgroundColor = XLColor.MistyRose;

					var budgetEntryColumnOffset = 2;
					foreach (var budgetEntry in budgetRow.DirectorBudgetEntries)
					{
						var xlCell = ws.Cell(rowOffset, budgetEntryColumnOffset);
						xlCell.Style.Fill.BackgroundColor = XLColor.MistyRose;
						FormatMoneyCell(xlCell, budgetEntry.Budget);
						budgetEntryColumnOffset++;
					}

					rowOffset++;
				}

				columnOffset = 1;
				ws.Cell(rowOffset, columnOffset).SetValue("Adjusted Budget");
				ws.Cell(rowOffset, columnOffset).Style.Font.Bold = true;
				ws.Cell(rowOffset, columnOffset).Style.Fill.BackgroundColor = XLColor.MintGreen;
				ws.Cell(rowOffset, columnOffset).Style.Border.BottomBorder = XLBorderStyleValues.Thick;

				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					var xlCell = ws.Cell(rowOffset, columnOffset);
					xlCell.Style.Font.Bold = true;
					xlCell.Style.Fill.BackgroundColor = XLColor.MintGreen;
					xlCell.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
					FormatMoneyCell(xlCell, budget.DirectorsReportAdjustedBudget);
				}

				rowOffset++;
				columnOffset = 1;
				ws.Cell(rowOffset, columnOffset).SetValue("Committed (Schedule A)");
				ws.Cell(rowOffset, columnOffset).Style.Font.Bold = true;
				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					var xlCell = ws.Cell(rowOffset, columnOffset);
					xlCell.Style.Font.Bold = true;
					FormatMoneyCell(xlCell, budget.DirectorsReportCommittedScheduleA);
				}

				rowOffset++;
				columnOffset = 1;
				ws.Cell(rowOffset, columnOffset).SetValue("Claims Processed");
				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					FormatMoneyCell(ws.Cell(rowOffset, columnOffset), budget.DirectorsReportClaimsProcessed);
				}

				rowOffset++;
				columnOffset = 1;
				ws.Cell(rowOffset, columnOffset).SetValue("Unclaimed");
				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					FormatMoneyCell(ws.Cell(rowOffset, columnOffset), budget.DirectorsReportUnclaimed);
				}

				rowOffset++;
				columnOffset = 1;
				ws.Cell(rowOffset, columnOffset).SetValue("Receivables (total set up in current FY)");
				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					FormatMoneyCell(ws.Cell(rowOffset, columnOffset), budget.DirectorsReportReceivables);
				}

				rowOffset++;
				columnOffset = 1;
				ws.Cell(rowOffset, columnOffset).SetValue("Slippage (Sched A - Claims Processed - Unclaimed)");
				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					FormatMoneyCell(ws.Cell(rowOffset, columnOffset), budget.DirectorsReportSlippage);
				}

				rowOffset++;
				columnOffset = 1;
				ws.Cell(rowOffset, columnOffset).SetValue("YTD Actuals (Claims Processed - Receivables)");
				ws.Cell(rowOffset, columnOffset).Style.Border.BottomBorder = XLBorderStyleValues.Thick;

				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					var xlCell = ws.Cell(rowOffset, columnOffset);
					xlCell.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
					FormatMoneyCell(xlCell, budget.DirectorsReportYtdActual);

				}

				rowOffset++;
				columnOffset = 1;
				ws.Cell(rowOffset, columnOffset).SetValue("Available Budget");
				ws.Cell(rowOffset, columnOffset).Style.Font.Bold = true;
				ws.Cell(rowOffset, columnOffset).Style.Fill.BackgroundColor = XLColor.MintGreen;

				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					var xlCell = ws.Cell(rowOffset, columnOffset);
					xlCell.Style.Font.Bold = true;
					xlCell.Style.Fill.BackgroundColor = XLColor.MintGreen;
					FormatMoneyCell(xlCell, budget.DirectorsReportAvailableBudget);
				}

				rowOffset++;
				foreach (var budgetRow in model.ClosingBudgetRows)
				{
					var rowTitle = !string.IsNullOrWhiteSpace(budgetRow.Name) ? budgetRow.Name : "Adjustment";
					ws.Cell(rowOffset, 1).SetValue(rowTitle);
					ws.Cell(rowOffset, 1).Style.Fill.BackgroundColor = XLColor.MistyRose;

					var budgetEntryColumnOffset = 2;
					foreach (var budgetEntry in budgetRow.DirectorBudgetEntries)
					{
						var xlCell = ws.Cell(rowOffset, budgetEntryColumnOffset);
						xlCell.Style.Fill.BackgroundColor = XLColor.MistyRose;
						FormatMoneyCell(xlCell, budgetEntry.Budget);
						budgetEntryColumnOffset++;
					}

					rowOffset++;
				}

				columnOffset = 1;
				ws.Cell(rowOffset, columnOffset).SetValue("Total Remaining Budget");
				ws.Cell(rowOffset, columnOffset).Style.Font.Bold = true;
				ws.Cell(rowOffset, columnOffset).Style.Fill.BackgroundColor = XLColor.LightGreen;

				foreach (var budget in model.DirectorsReport)
				{
					columnOffset++;
					var xlCell = ws.Cell(rowOffset, columnOffset);
					xlCell.Style.Font.Bold = true;
					xlCell.Style.Fill.BackgroundColor = XLColor.LightGreen;
					FormatMoneyCell(xlCell, budget.DirectorsReportRemainingBudget);
				}

				foreach (var cell in ws.CellsUsed())
				{
					var useThickBorder = cell.Style.Border.BottomBorder == XLBorderStyleValues.Thick;
					cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

					if (useThickBorder)
						cell.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
				}

				ws.ColumnsUsed().AdjustToContents(20d, 60d);
				ws.RowsUsed().CellsUsed().Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
				ws.RowsUsed().AdjustToContents();

				wb.SaveAs(stream);

				return stream.ToArray();
			}
		}

		private static void FormatMoneyCell(IXLCell cell, decimal? budgetAmount)
		{
			if (budgetAmount.HasValue)
				cell.SetValue(budgetAmount.Value);
			cell.Style.NumberFormat.Format = "#,##0.00";
			cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
			cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
		}

		private static void FormatMoneyCell(IXLCell cell, decimal budgetAmount)
		{
			cell.SetValue(budgetAmount);
			cell.Style.NumberFormat.Format = "#,##0.00";
			cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
			cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
		}
	}
}