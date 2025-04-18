﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CJG.Application.Business.Models;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Core.Entities.Interfaces;
using CJG.Core.Interfaces.Service;
using CJG.Core.Interfaces.Service.Settings;
using CJG.Infrastructure.Identity;
using CJG.Web.External.Areas.Int.Models;
using CJG.Web.External.Areas.Int.Models.Notifications;
using CJG.Web.External.Areas.Int.Models.Settings;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Controllers
{
    [RouteArea("Int")]
	public class NotificationController : BaseController
	{
		private readonly IStaticDataService _staticDataService;
		private readonly INotificationService _notificationService;
		private readonly INotificationSettings _notificationSettings;
		private readonly INotificationTypeService _notificationTypeService;
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IGrantProgramService _grantProgramService;
		private readonly IFiscalYearService _fiscalYearService;
		private readonly ISettingService _settingService;
		private readonly IUserService _userService;

		/// <summary>
		/// Creates a new instance of a NotificationController object, and initializes it with the specified services.
		/// </summary>
		/// <param name="controllerService"></param>
		/// <param name="notificationService"></param>
		/// <param name="notificationTypeService"></param>
		/// <param name="grantApplicationService"></param>
		/// <param name="notificationSettings"></param>
		/// <param name="grantProgramService"></param>
		/// <param name="fiscalYearService"></param>
		/// <param name="settingService"></param>
		/// <param name="userService"></param>
		/// <param name="staticDataService"></param>
		public NotificationController(
			IControllerService controllerService,
			INotificationService notificationService,
			INotificationTypeService notificationTypeService,
			IGrantApplicationService grantApplicationService,
			INotificationSettings notificationSettings,
			IGrantProgramService grantProgramService,
			IFiscalYearService fiscalYearService,
			ISettingService settingService,
			IUserService userService,
			IStaticDataService staticDataService) : base(controllerService.Logger)
		{
			_notificationService = notificationService;
			_notificationSettings = notificationSettings;
			_notificationTypeService = notificationTypeService;
			_grantApplicationService = grantApplicationService;
			_grantProgramService = grantProgramService;
			_fiscalYearService = fiscalYearService;
			_settingService = settingService;
			_userService = userService;
			_staticDataService = staticDataService;
		}

		/// <summary>
		/// Return a view to manage the notification types.
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[AuthorizeAction(Privilege.IM1, Privilege.SM)]
		[Route("Admin/Notification/Types/View")]
		public ActionResult NotificationTypesView()
		{
			return View();
		}

		/// <summary>
		/// Return a partial view to manage the notification type.
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[AuthorizeAction(Privilege.IM1, Privilege.SM)]
		[Route("Admin/Notification/Types/View/{id}")]
		public PartialViewResult NotificationTypeView(int id)
		{
			ViewBag.NotificationTypeId = id;
			return PartialView("_NotificationType");
		}

		/// <summary>
		/// Returns a ViewModel containing an array of notification types for the specified filter.
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="page"></param>
		/// <param name="quantity"></param>
		/// <returns></returns>
		[HttpPost]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Admin/Notification/Types")]
		public JsonResult GetNotificationTypes(NotificationTypeFilterViewModel filter)
		{
			var model = new NotificationTypeDashboardViewModel();
			try
			{
				int.TryParse(Request.QueryString["page"], out int pageNumber);
				int.TryParse(Request.QueryString["quantity"], out int quantityNumber);

				var types = _notificationTypeService.Get(pageNumber, quantityNumber, filter.GetFilter());

				model = new NotificationTypeDashboardViewModel(types, _settingService);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model);
		}

		/// <summary>
		/// Returns an arary of notification trigger types.
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[Route("Admin/Notification/Trigger/Types")]
		public JsonResult GetNotificationTriggers()
		{
			var model = new BaseViewModel();
			try
			{
				var triggers = _notificationTypeService.GetTriggerTypes(true);
				var result = triggers.Select(t => new NotificationTriggerViewModel(t)).ToArray();
				return Json(result, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Returns an array of internal application states.
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[Route("Admin/Notification/Application/States")]
		public JsonResult GetApplicationStates()
		{
			var internalStates = new InternalStateListViewModel();
			try
			{
				internalStates = new InternalStateListViewModel(_grantApplicationService.GetInternalStates(true));
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, internalStates);
			}

			return Json(internalStates, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Returns an array from the MilestoneDates enum.
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[Route("Admin/Notification/Milestone/Dates")]
		public JsonResult GetMilestoneDates()
		{
			var model = new BaseViewModel();
			try
			{
				var milestoneDates = new
				{
					Data =
						typeof(NotificationMilestoneDateName).GetEnumValues().OfType<NotificationMilestoneDateName>().Select(value =>
							new KeyValuePair<NotificationMilestoneDateName, string>(value, value.GetDescription())).ToArray()
				};
				return Json(milestoneDates, JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Returns the notification type data for the specified 'id'.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Admin/Notification/Type/{id}")]
		public JsonResult GetNotificationType(int id)
		{
			var notificationType = new NotificationTypeViewModel();
			try
			{
				notificationType = new NotificationTypeViewModel(_notificationTypeService.Get(id));
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}

			return Json(notificationType, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Returns the list of grant programs.
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[Route("Admin/Notification/Type/Grant/Programs")]
		public JsonResult GetGrantPrograms()
		{
			var model = new BaseViewModel();
			try
			{
				var grantPrograms = _grantProgramService.GetAll();
				var result = grantPrograms.Select(o => new KeyValueListItem<int, string>(o.Id, o.Name)).ToArray();
				return Json(result, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Route("Admin/Notification/RecipientRules")]
		public JsonResult GetRecipientRules()
		{
			var model = new BaseViewModel();
			try
			{
				// Fudge the names of the rules, but use the proper enum values
				var recipientRules = new List<KeyValuePair<int, string>>
				{
					new KeyValuePair<int, string>((int)NotificationRecipientRules.AllApplicantsInOrganization, "Applicant"),
					new KeyValuePair<int, string>((int)NotificationRecipientRules.AllExitingParticipants, "Participant")
				};
				
				return Json(recipientRules.ToArray(), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Returns the current user email.
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[Route("Admin/Notification/Type/User/Email")]
		public JsonResult GetUserEmail()
		{
			var model = new BaseViewModel();
			try
			{
				var user = _userService.GetInternalUser(User.GetUserId().Value);
				var result = user.Email;
				return Json(result, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Adds the new notification type to the datasource.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Admin/Notification/Type")]
		public JsonResult AddNotificationType(NotificationTypeViewModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var notificationType = new NotificationType
					{
						NotificationTemplate = new NotificationTemplate(model.NotificationTemplate.EmailSubject, model.NotificationTemplate.EmailSubject, model.NotificationTemplate.EmailBody),
					};

					Utilities.MapProperties(model, notificationType);

					_notificationTypeService.Add(notificationType);

					model = new NotificationTypeViewModel(notificationType);
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

			return Json(model);
		}

		/// <summary>
		/// Updates the specified notification type in the datasource.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPut]
		[ValidateRequestHeader]
		[PreventSpam]
		[AuthorizeAction(Privilege.IM1, Privilege.SM)]
		[Route("Admin/Notification/Type")]
		public JsonResult UpdateNotificationType(NotificationTypeViewModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var invalidSubjectModel = _notificationService.ValidateModelKeywords(model.NotificationTemplate.EmailSubject, new[] { "GrantApplication", "Applicant", "GrantApplicationId" });
					if (!string.IsNullOrWhiteSpace(invalidSubjectModel))
						AddAngularError(model, "NotificationTemplate.EmailSubject", "Invalid template variables: " + invalidSubjectModel);

					var invalidBodyModel = _notificationService.ValidateModelKeywords(model.NotificationTemplate.EmailBody, new[] { "GrantApplication", "Applicant", "GrantApplicationId" });
					if (!string.IsNullOrWhiteSpace(invalidBodyModel))
						AddAngularError(model, "NotificationTemplate.EmailBody", "Invalid template variables: " + invalidBodyModel);

					if (!string.IsNullOrWhiteSpace(invalidSubjectModel + invalidBodyModel))
						throw new InvalidOperationException("The following template variables are invalid: " + invalidSubjectModel + (!string.IsNullOrWhiteSpace(invalidSubjectModel) ? ", " : "") + invalidBodyModel);

					var notificationType = _notificationTypeService.Get(model.Id);

					notificationType.NotificationTemplate.Caption = model.Caption;
					notificationType.NotificationTemplate.EmailBody = model.NotificationTemplate.EmailBody;
					notificationType.NotificationTemplate.EmailSubject = model.NotificationTemplate.EmailSubject;

					Utilities.MapProperties(model, notificationType);

					_notificationTypeService.Update(notificationType);

					model = new NotificationTypeViewModel(notificationType);
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
			return Json(model);
		}

		/// <summary>
		/// Deletes the specified notification type.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPut]
		[ValidateRequestHeader]
		[PreventSpam]
		[Route("Admin/Notification/Type/Delete")]
		public JsonResult DeleteNotificationType(NotificationTypeViewModel model)
		{
			try
			{
				var notificationType = _notificationTypeService.Get(model.Id);
				_notificationTypeService.Delete(notificationType);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model);
		}

		/// <summary>
		/// Enables or disables the email notifications by updating the settings.
		/// </summary>
		/// <param name="enable"></param>
		/// <returns></returns>
		[HttpPut]
		[ValidateRequestHeader]
		[PreventSpam]
		[Route("Admin/Notification/Enable")]
		public JsonResult EnableNotifications(bool enable)
		{
			var settingModel = new UserSettingViewModel();
			try
			{
				var setting = _settingService.Get("EnableEmails") ?? new Setting("EnableEmails", enable);
				setting.SetValue(enable);
				_settingService.AddOrUpdate(setting);
				return Json(enable);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, settingModel);
			}

			return Json(settingModel);
		}

		/// <summary>
		/// Returns an array of all the notifications associated to the specified grant application and the filter.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="page"></param>
		/// <param name="quantity"></param>
		/// <param name="search"></param>
		/// <param name="filter"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("Application/Notifications/{grantApplicationId}/{page}/{quantity}")]
		public JsonResult GetApplicationNotifications(int grantApplicationId, int page, int quantity, string search, NotificationQueueFilterViewModel filter)
		{
			var model = new BaseViewModel();
			try
			{
				var notifications = _notificationService.GetGrantApplicationNotifications(grantApplicationId, page, quantity, search, filter.GetFilter());
				var result = new
				{
					RecordsFiltered = notifications.Items.Count(),
					RecordsTotal = notifications.Total,
					Data = notifications.Items.Select(n => new NotificationQueueViewModel(n)).ToArray()
				};
				return Json(result, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model);
		}

		/// <summary>
		/// Returns the data for the specified notification that was queued for the application.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/Notification/{id}")]
		public JsonResult GetApplicationNotification(int id)
		{
			var model = new NotificationQueueViewModel();
			try
			{
				model = new NotificationQueueViewModel(_notificationService.GetApplicationNotification(id));
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Sends or resends the specified grant application notification.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/Notification/Send/{id}")]
		public JsonResult SendApplicationNotification(int id)
		{
			var model = new NotificationQueueViewModel();
			try
			{
				var notification = _notificationService.GetApplicationNotification(id);

				_notificationService.SendNotification(notification);
				_notificationService.Commit();

				model = new NotificationQueueViewModel(notification);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model);
		}

		/// <summary>
		/// Sends a test email of the notification to the current user.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[HttpPost]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Admin/Notification/Type/Test")]
		public JsonResult TestNotificationType(NotificationQueuePreviewModel data)
		{
			var model = new BaseViewModel();
			try
			{
				_notificationService.SendNotification(GenerateNotification(data), true);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model);
		}

		/// <summary>
		/// Return a view for the notification preview.
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Admin/Notification/Type/Preview/View")]
		public ActionResult NotificationTypePreview(NotificationQueuePreviewModel model)
		{
			TempData["_RemoveHeader"] = true;
			return View(model);
		}

		/// <summary>
		/// Returns a view to display the specified notification parsed template.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Admin/Notification/Type/Preview")]
		public ActionResult GetNotificationTypePreview(NotificationQueuePreviewModel model)
		{
			try
			{
				var notification = GenerateNotification(model);

				model.Subject = notification.EmailSubject;
				model.Body = notification.EmailBody;
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model);
		}

		private NotificationQueue GenerateNotification(NotificationQueuePreviewModel model)
		{
			var data = model.GenerateTestEntities(User, _userService, _grantProgramService, _fiscalYearService, _staticDataService);

			var recipientRule = (NotificationRecipientRules) model.RecipientRule;

			var recipient = (recipientRule == NotificationRecipientRules.AllExitingParticipants
				                ? (INotificationRecipient)data.Participant 
				                : (INotificationRecipient)data.Applicant) ?? data.Applicant;

			return _notificationService.GenerateNotificationMessage(data.GrantApplication, recipient,
				new NotificationType(model.NotificationTriggerId, model.Name, model.Description,
				new NotificationTemplate(model.Name ?? "N/A", model.Subject ?? "N/A", model.Body ?? "N/A")));
		}
	}
}
