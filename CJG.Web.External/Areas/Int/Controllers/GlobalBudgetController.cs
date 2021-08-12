using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CJG.Application.Business.Models.GlobalBudget;
using CJG.Core.Interfaces;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Int.Models.GlobalBudget;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;

namespace CJG.Web.External.Areas.Int.Controllers
{
    [RouteArea("Int")]
	public class GlobalBudgetController : BaseController
	{
		private readonly IStaticDataService _staticDataService;
		private readonly IFiscalYearService _fiscalYearService;
		private readonly IGlobalBudgetService _globalBudgetService;

		/// <summary>
		/// Creates a new instance of a IntakeController object.
		/// </summary>
		/// <param name="controllerService"></param>
		/// <param name="fiscalYearService"></param>
		/// <param name="globalBudgetService"></param>
		public GlobalBudgetController(
			IControllerService controllerService,
			IFiscalYearService fiscalYearService,
			IGlobalBudgetService globalBudgetService)
			: base(controllerService.Logger)
		{
			_staticDataService = controllerService.StaticDataService;
			_fiscalYearService = fiscalYearService;
			_globalBudgetService = globalBudgetService;
		}

		[HttpGet]
		[Route("Admin/GlobalBudget/View")]
		public ActionResult Index()
		{
			return View();
		}

		[HttpGet]
		[Route("Admin/GlobalBudget/FiscalYears")]
		public JsonResult GetFiscalYears()
		{
			IEnumerable<KeyValuePair<int, string>> results = new KeyValuePair<int, string>[0];
			try
			{
				var fiscalYears = _staticDataService.GetFiscalYears();
				results = fiscalYears.Select(fy => new KeyValuePair<int, string>(fy.Id, fy.Caption)).ToArray();
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}
			return Json(results, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Route("Admin/GlobalBudget/Dashboard")]
		public JsonResult GetGlobalBudgetDashboard(int? fiscalYearId)
		{
			var model = new GlobalBudgetDashboardModel();

			var fiscalYear = !fiscalYearId.HasValue
				? _fiscalYearService.GetCurrentFiscalYear()
				: _fiscalYearService.Get(fiscalYearId.Value);

			try
			{
				var streamsByIntakePeriod = _globalBudgetService.GetIntakePeriodStreams(fiscalYear.Id);
				model.SelectedFiscalYearId = fiscalYear.Id;
				model.IntakePeriodSlots = streamsByIntakePeriod;
				model.BudgetSummary = GetBudgetSummary(streamsByIntakePeriod);
				model.CanEditBudget = User.IsInRole("Director") || User.IsInRole("Financial Clerk");
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[Authorize(Roles = "Director, Financial Clerk")]
		[Route("Admin/GlobalBudget/Dashboard/Save")]
		public JsonResult UpdateBudgetDashboardAmounts(GlobalBudgetDashboardModel model)
		{
			var fiscalYear = !model.SelectedFiscalYearId.HasValue
				? _fiscalYearService.GetCurrentFiscalYear()
				: _fiscalYearService.Get(model.SelectedFiscalYearId.Value);

			try
			{
				if (ModelState.IsValid)
				{
					_globalBudgetService.UpdateBudget(model.SelectedFiscalYearId, model.IntakePeriodSlots);

					var streamsByIntakePeriod = _globalBudgetService.GetIntakePeriodStreams(fiscalYear.Id);
					model.SelectedFiscalYearId = fiscalYear.Id;
					model.IntakePeriodSlots = streamsByIntakePeriod;
					model.BudgetSummary = GetBudgetSummary(streamsByIntakePeriod);
					model.CanEditBudget = User.IsInRole("Director") || User.IsInRole("Financial Clerk");
				}
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		private GlobalBudgetSummaryModel GetBudgetSummary(List<IntakePeriodSlotModel> streamsByIntakePeriod)
		{
			var model = new GlobalBudgetSummaryModel
			{
				Committed = new List<KeyValuePair<string, decimal>>(),
				CommittedUnclaimed = new List<KeyValuePair<string, decimal>>(),
				CommittedClaimed = new List<KeyValuePair<string, decimal>>(),
				Forecast = new List<KeyValuePair<string, decimal>>(),
				TotalSpent = new List<KeyValuePair<string, decimal>>(),
				Slippage = new List<KeyValuePair<string, decimal>>()
			};

			var flatStreams = streamsByIntakePeriod
				.SelectMany(s => s.Streams)
				.ToList();

			var wdaStreams = flatStreams
				.Where(fs => fs.SortOrder == 0)
				.ToList();
			var crsStreams = flatStreams
				.Where(fs => fs.SortOrder == 1)
				.ToList();  // Community Response has a "SortOrder" of 1 to slot it at the end

			model.Committed.Add(new KeyValuePair<string, decimal>("WDA", wdaStreams.Sum(s => s.CommittedAmount)));
			model.Committed.Add(new KeyValuePair<string, decimal>("CRS", crsStreams.Sum(s => s.CommittedAmount)));

			model.CommittedUnclaimed.Add(new KeyValuePair<string, decimal>("WDA", wdaStreams.Sum(s => s.CommittedUnclaimedAmount)));
			model.CommittedUnclaimed.Add(new KeyValuePair<string, decimal>("CRS", crsStreams.Sum(s => s.CommittedUnclaimedAmount)));

			model.CommittedClaimed.Add(new KeyValuePair<string, decimal>("WDA", wdaStreams.Sum(s => s.CommittedClaimedAmount)));
			model.CommittedClaimed.Add(new KeyValuePair<string, decimal>("CRS", crsStreams.Sum(s => s.CommittedClaimedAmount)));

			model.Forecast.Add(new KeyValuePair<string, decimal>("WDA", wdaStreams.Sum(s => s.ForecastedCommitment)));
			model.Forecast.Add(new KeyValuePair<string, decimal>("CRS", crsStreams.Sum(s => s.ForecastedCommitment)));

			model.TotalSpent.Add(new KeyValuePair<string, decimal>("WDA", wdaStreams.Sum(s => s.TotalSpent)));
			model.TotalSpent.Add(new KeyValuePair<string, decimal>("CRS", crsStreams.Sum(s => s.TotalSpent)));

			model.Slippage.Add(new KeyValuePair<string, decimal>("WDA", wdaStreams.Sum(s => s.SlippageAmount)));
			model.Slippage.Add(new KeyValuePair<string, decimal>("CRS", crsStreams.Sum(s => s.SlippageAmount)));

			var committedTotal = model.Committed.Sum(m => m.Value);
			model.Committed.Add(new KeyValuePair<string, decimal>("Total", committedTotal));
			var committedUnclaimedTotal = model.CommittedUnclaimed.Sum(m => m.Value);
			model.CommittedUnclaimed.Add(new KeyValuePair<string, decimal>("Total", committedUnclaimedTotal));
			var committedClaimedTotal = model.CommittedClaimed.Sum(m => m.Value);
			model.CommittedClaimed.Add(new KeyValuePair<string, decimal>("Total", committedClaimedTotal));
			var forecastTotal = model.Forecast.Sum(m => m.Value);
			model.Forecast.Add(new KeyValuePair<string, decimal>("Total", forecastTotal));
			var spentTotal = model.TotalSpent.Sum(m => m.Value);
			model.TotalSpent.Add(new KeyValuePair<string, decimal>("Total", spentTotal));
			var slippageTotal = model.Slippage.Sum(m => m.Value);
			model.Slippage.Add(new KeyValuePair<string, decimal>("Total", slippageTotal));

			model.GrandTotal = committedTotal + forecastTotal + spentTotal + slippageTotal;
			return model;
		}
	}
}