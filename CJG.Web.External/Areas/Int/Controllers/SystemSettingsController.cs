using System;
using System.Web.Mvc;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Identity;
using CJG.Web.External.Areas.Int.Models.SystemSettings;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;

namespace CJG.Web.External.Areas.Int.Controllers
{
    [AuthorizeAction(Privilege.AM4, Privilege.SM)]
	[RouteArea("Int")]
	[RoutePrefix("Admin/SystemSettings")]
	public class SystemSettingsController : BaseController
	{
		private readonly ISettingService _settingService;

		public SystemSettingsController(
			IControllerService controllerService,
			ISettingService settingService
		   ) : base(controllerService.Logger)
		{
			_settingService = settingService;
		}

		[HttpGet]
		[Route("View")]
		public ActionResult Index()
		{
			return View();
		}

		[HttpGet, Route("Settings")]
		[ValidateRequestHeader]
		public JsonResult GetSettings()
		{
			var model = new SystemSettingsModel();

			try
			{
				var jobSetting = _settingService.Get(SettingServiceKeys.EiEligibilityCheckServiceSettingKey);
				var currentState = jobSetting?.GetValue<bool>() ?? false;

				model = new SystemSettingsModel
				{
					EiEligibilityCheckServiceState = currentState
				};

				return Json(model, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Settings")]
		public JsonResult UpdateSettings(SystemSettingsModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var setting = _settingService.Get(SettingServiceKeys.EiEligibilityCheckServiceSettingKey)
								  ?? new Setting(SettingServiceKeys.EiEligibilityCheckServiceSettingKey, model.EiEligibilityCheckServiceState);
					setting.SetValue(model.EiEligibilityCheckServiceState);

					_settingService.AddOrUpdate(setting);
				}
				else
				{
					HandleModelStateValidation(model);
				}
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			model.RedirectURL = "/Int/Admin/SystemSettings/View";
			return Json(model);
		}
	}
}
