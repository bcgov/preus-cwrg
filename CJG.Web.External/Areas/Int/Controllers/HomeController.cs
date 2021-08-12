using System;
using System.Linq;
using System.Web.Mvc;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Identity;
using CJG.Web.External.Areas.Int.Models;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

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
					_directorReportsService.UpdateBudget(model.DirectorsReport);

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
	}
}