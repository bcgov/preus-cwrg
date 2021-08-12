using System;
using System.Web;
using System.Web.Mvc;
using CJG.Application.Services;
using CJG.Application.Services.Exceptions;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Ext.Models;
using CJG.Web.External.Areas.Ext.Models.ParticipantReporting;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;

namespace CJG.Web.External.Areas.Ext.Controllers
{
    [RouteArea("Ext")]
	[ExternalFilter]
	public class ParticipantReportingController : BaseController
	{
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IParticipantService _participantService;
		private readonly ISurveyService _surveyService;

		/// <summary>
		/// Creates a new instance of a ParticipantReportingController object.
		/// </summary>
		/// <param name="controllerService"></param>
		/// <param name="grantApplicationService"></param>
		/// <param name="participantService"></param>
		/// <param name="surveyService"></param>
		public ParticipantReportingController(
			IControllerService controllerService,
			IGrantApplicationService grantApplicationService,
			IParticipantService participantService,
			ISurveyService surveyService) : base(controllerService.Logger)
		{
			_grantApplicationService = grantApplicationService;
			_participantService = participantService;
			_surveyService = surveyService;
		}

		/// <summary>
		/// Launched when the 'Report Participants' link on the 'GrantFiles' page is clicked
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Reporting/Participant/View/{grantApplicationId}")]
		public ActionResult ParticipantReportingView(int grantApplicationId)
		{
			var grantApplication = _grantApplicationService.Get(grantApplicationId);

			if (!User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.ViewParticipants))
			{
				this.SetAlert("You are not authorized to manage participants.", AlertType.Error, true);
				return RedirectToAction("ApplicationDetailsView", "ApplicationView", new { grantApplicationId });
			}

			ViewBag.GrantApplicationId = grantApplicationId;
			return View();
		}

		/// <summary>
		/// Get the data for the participant reporting view.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Reporting/Participants/{grantApplicationId}")]
		public JsonResult GetParticipantReporting(int grantApplicationId)
		{
			var model = new ReportingViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);

				if (!User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.ViewParticipants))
				{
					throw new NotAuthorizedException("You are not authorized to manage participants.");
				}

				model = new ReportingViewModel(grantApplication, HttpContext);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Get the data for the participant reporting view.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Reporting/InvitationInfo/{grantApplicationId}")]
		public JsonResult GetParticipantInvitationInfo(int grantApplicationId)
		{
			var model = new ProgramTitleLabelViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);

				if (!User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.ViewParticipants))
				{
					throw new NotAuthorizedException("You are not authorized to manage participants.");
				}

				model = new ProgramTitleLabelViewModel(grantApplication, false);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Route("Reporting/Participant/Exit/{invitationKey}")]
		public ActionResult ParticipantExitView(Guid invitationKey)
		{
			var grantApplication = _grantApplicationService.Get(invitationKey);

			if (!User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.ViewParticipants))
			{
				this.SetAlert("You are not authorized to manage participants.", AlertType.Error, true);
				return RedirectToAction(nameof(ApplicationViewController.ApplicationDetailsView), nameof(ApplicationViewController).Replace("Controller", ""), new { grantApplication.Id });
			}

			ViewBag.GrantApplicationId = grantApplication.Id;
			ViewBag.ExitInvitationUrlLink = $"{HttpContext.Request.Url.GetLeftPart(UriPartial.Authority)}/Part/ExitForm/{HttpUtility.UrlEncode(grantApplication.InvitationKey.ToString())}";

			return View();
		}

		[HttpGet]
		[Route("Reporting/Participant/Withdrawal/{invitationKey}/{withdrawalKey}")]
		public ActionResult ParticipantWithdrawalView(Guid invitationKey, Guid withdrawalKey)
		{
			var grantApplication = _grantApplicationService.Get(invitationKey);

			if (!User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.ViewParticipants))
			{
				this.SetAlert("You are not authorized to manage participants.", AlertType.Error, true);
				return RedirectToAction(nameof(ApplicationViewController.ApplicationDetailsView), nameof(ApplicationViewController).Replace("Controller", ""), new { grantApplication.Id });
			}

			var participantForm = _surveyService.FindParticipantFormForWithdrawalSurvey(grantApplication, withdrawalKey);

			ViewBag.GrantApplicationId = grantApplication.Id;
			ViewBag.WithdrawalInvitationUrlLink = $"{HttpContext.Request.Url.GetLeftPart(UriPartial.Authority)}/Part/WithdrawalForm/{HttpUtility.UrlEncode(grantApplication.InvitationKey.ToString())}/{HttpUtility.UrlEncode(participantForm.WithdrawalKey.ToString())}";
			ViewBag.ParticipantName = $"{participantForm.FirstName} {participantForm.LastName}";

			return View();
		}

		/// <summary>
		/// Launched when the 'Remove' link on the 'ParticipantsReport' page is clicked
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Reporting/Participant/Delete")]
		public ActionResult RemoveParticipant(ParticipantViewModel model)
		{
			var viewModel = new ReportingViewModel();
			try
			{
				var participant = _participantService.Get(model.Id);
				participant.RowVersion = Convert.FromBase64String(model.RowVersion);
				var grantApplication = participant.GrantApplication;
				_participantService.RemoveParticipant(participant);

				viewModel = new ReportingViewModel(grantApplication, this.HttpContext);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, viewModel);
			}

			return Json(viewModel, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Launched when the 'Withdrawn' link on the 'ParticipantsReport' page is clicked
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Reporting/Participant/Withdraw")]
		public ActionResult WithdrawParticipant(ParticipantViewModel model)
		{
			var viewModel = new ReportingViewModel();
			try
			{
				var participant = _participantService.Get(model.Id);
				participant.RowVersion = Convert.FromBase64String(model.RowVersion);
				var grantApplication = participant.GrantApplication;
				_participantService.WithdrawParticipant(participant);

				viewModel = new ReportingViewModel(grantApplication, HttpContext);
				viewModel.RedirectURL = Url.Action("ParticipantWithdrawalView", new { participant.GrantApplication.InvitationKey, participant.WithdrawalKey });
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, viewModel);
			}

			return Json(viewModel, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Toggles whether a participant is included or excluded from the claim currently being reported.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Reporting/Participant/Toggle")]
		public JsonResult ToggleParticipant(IncludeParticipantsViewModel model)
		{
			var viewModel = new ReportingViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(model.GrantApplicationId);
				var claim = grantApplication.GetCurrentClaim();
				claim.RowVersion = Convert.FromBase64String(model.ClaimRowVersion);
				
				foreach (var participantFormId in model.ParticipantFormIds ?? new int[0])
				{
					if (participantFormId <= 0) continue;
					var participantForm = _participantService.Get(participantFormId);

					if (model.Include) _participantService.IncludeParticipant(participantForm);
					else _participantService.ExcludeParticipant(participantForm);
				}

				viewModel = new ReportingViewModel(grantApplication, this.HttpContext);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, viewModel);
			}
			return Json(viewModel);
		}
	}
}
