﻿using System;
using System.Web.Mvc;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Ext.Models.Reporting;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;

namespace CJG.Web.External.Areas.Ext.Controllers
{
    /// <summary>
    /// ReportingController class, provides a controller to manage grant file reporting.
    /// </summary>
    [RouteArea("Ext")]
	[ExternalFilter]
	public class ReportingController : BaseController
	{
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IParticipantService _participantService;
		private readonly ISuccessStoryService _successStoryService;
		private readonly ICompletionReportService _completionReportService;

		/// <summary>
		/// Creates a new instance of a ReportingController object.
		/// </summary>
		/// <param name="controllerService"></param>
		/// <param name="grantApplicationService"></param>
		/// <param name="participantService"></param>
		/// <param name="successStoryService"></param>
		/// <param name="completionReportService"></param>
		public ReportingController(
			IControllerService controllerService,
			IGrantApplicationService grantApplicationService,
			IParticipantService participantService,
			ISuccessStoryService successStoryService,
			ICompletionReportService completionReportService) : base(controllerService.Logger)
		{
			_grantApplicationService = grantApplicationService;
			_participantService = participantService;
			_successStoryService = successStoryService;
			_completionReportService = completionReportService;
		}

		#region Endpoints
		/// <summary>
		/// Load the grant file view
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Reporting/Grant/File/View/{grantApplicationId}")]
		public ActionResult GrantFileView(int grantApplicationId)
		{
			var grantApplication = _grantApplicationService.Get(grantApplicationId);

			if (!User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.ViewParticipants))
			{
				this.SetAlert("User does not have permission to view participant reporting.", AlertType.Warning, true);
				return RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", ""));
			}

			var claim = grantApplication.GetCurrentClaim();

			ViewBag.GrantApplicationId = grantApplicationId;
			ViewBag.ClaimId = claim?.Id ?? 0;
			ViewBag.ClaimVersion = claim?.ClaimVersion ?? 0;
			return View(SidebarViewModelFactory.Create(grantApplication, ControllerContext));
		}

		/// <summary>
		/// After the grant file view is loaded, the system calls this method to get the required data
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Reporting/Grant/File/{grantApplicationId}")]
		public JsonResult GetGrantFile(int grantApplicationId)
		{
			var model = new GrantFileViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				model = new GrantFileViewModel(grantApplication, _participantService, _completionReportService, _successStoryService, User);
				model.ProgramType = grantApplication.GrantOpening.GrantStream.GrantProgram.ProgramTypeId;

				if (!model.HasClaim || User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditClaim) || User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.CreateClaim))
				{
					var hasStarted = AppDateTime.UtcNow >= grantApplication.StartDate;
					model.AllowReviewAndSubmit = User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.SubmitClaim) && hasStarted;
					model.AllowClaimReport = (User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditClaim) || User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.CreateClaim)) && hasStarted;
				}
				model.AllowParticipantReport = User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditParticipants);
				model.EnableSubmit = User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.SubmitClaim);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}
		#endregion
	}
}