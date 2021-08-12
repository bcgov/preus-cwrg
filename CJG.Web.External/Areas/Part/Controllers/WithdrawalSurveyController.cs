using System;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using CJG.Application.Services.Extensions;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Part.Models.ExitSurvey;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;

namespace CJG.Web.External.Areas.Part.Controllers
{
    [ParticipantFilter]
	[RouteArea("Part")]
	[RoutePrefix("WithdrawalForm")]
	public class WithdrawalSurveyController : BaseController
	{
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IReCaptchaService _reCaptchaService;
		private readonly ISurveyService _surveyService;

		/// <param name="controllerService"></param>
		/// <param name="grantApplicationService"></param>
		/// <param name="recaptchaService"></param>
		/// <param name="surveyService"></param>
		public WithdrawalSurveyController(
			IControllerService controllerService,
			IGrantApplicationService grantApplicationService,
			IReCaptchaService recaptchaService,
			ISurveyService surveyService) : base(controllerService.Logger)
		{
			_grantApplicationService = grantApplicationService;
			_reCaptchaService = recaptchaService;
			_surveyService = surveyService;
		}

		/// <summary>
		/// Return participant information form view.
		/// </summary>
		/// <param name="invitationKey"></param>
		/// <param name="withdrawalKey"></param>
		/// <returns></returns>
		[Route("{invitationKey}/{withdrawalKey}")]
		public ActionResult Index(string invitationKey, string withdrawalKey)
		{
			try
			{
				var expired = IsInvitationExpired(invitationKey, out var invitationKeyGuid);
				if (expired != null)
					return expired;

				Guid.TryParse(withdrawalKey, out var withdrawalKeyGuid);
				var withdrawalPossible = IsWithdrawalAvailable(invitationKeyGuid, withdrawalKeyGuid);
				if (!withdrawalPossible)
					return RedirectToAction("InvitationExpired");

				var model = new WithdrawalSurveyModel
				{
					InvitationKey = invitationKeyGuid,
					WithdrawalKey = withdrawalKeyGuid,
					RecaptchaEnabled = _reCaptchaService.IsEnabled(),
					RecaptchaSiteKey = _reCaptchaService.GetSiteKey(),
					TimeoutPeriod = ConfigurationManager.AppSettings["ParticipantSessionDuration"]
				};

				ViewBag.InvitationKey = invitationKeyGuid;
				ViewBag.WithdrawalKey = withdrawalKeyGuid;

				return View(model);
			}
			catch (DbEntityValidationException e)
			{
				this.SetAlert(e);
			}
			catch (Exception e)
			{
				_logger.Error(e);
				this.SetAlert(e);
			}

			return RedirectToAction("SessionTimeout");
		}

		[HttpGet]
		[ValidateRequestHeader]
		[Route("Data/{invitationKey}/{withdrawalKey}")]
		public JsonResult GetWithdrawalSurvey(Guid invitationKey, Guid withdrawalKey)
		{
			var model = new WithdrawalSurveyModel();

			try
			{
				var dateLimits = GetWithdrawalDateLimits();

				var grantApplication = _grantApplicationService.Get(invitationKey);
				model.InvitationKey = invitationKey;
				model.WithdrawalKey = withdrawalKey;
				model.AgreementNumber = grantApplication.FileNumber;
				model.TrainingProgramTitle = grantApplication.GetSkillTrainingCourseTitle();
				model.EarliestWithdrawal = dateLimits.Item1;
				model.LatestWithdrawal = dateLimits.Item2;
				model.Questions = _surveyService
					.GetWithdrawalSurveyQuestions()
					.ToViewModel(grantApplication)
					.ToList();
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);

				return Json(model, JsonRequestBehavior.AllowGet);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[Route("Submit")]
		public ActionResult Submit(WithdrawalSurveyModel model)
		{
			// Validate the reCAPTCHA
			var encodedResponse = model.RecaptchaEncodedResponse;
			var errorCodes = "";

			if (!ModelState.IsValid)
			{
				model.ValidationErrors = GetClientErrors();
				AddGenericError(model, ModelState.GetErrorMessages("<br />"));

				return View("Index", model);
			}

			// Not doing server captcha validation as it messed with errors on reload.
			//if (_reCaptchaService.Validate(encodedResponse, ref errorCodes))
			//{
				if (model.WithdrawalDate == null)
					AddAngularError(model, "WithdrawalDate", "Please enter a date of withdrawal.");

				foreach (var question in model.Questions)
				{
					var questionAnswered = question.QuestionType != SurveyQuestionType.FreeText && question.Options.Any(a => a.AnswerGiven) || question.QuestionType == SurveyQuestionType.FreeText;

					if (!questionAnswered)
						AddAngularError(model, $"Question{question.QuestionId}", "Please choose an answer.");

					if (question.Options.Any(a => a.AnswerGiven && a.AllowOther && string.IsNullOrWhiteSpace(a.TextAnswer)))
						AddAngularError(model, $"Question{question.QuestionId}", "Please enter your reason for your choice.");

					//if (question.QuestionType == SurveyQuestionType.FreeText && question.Options.Any(a => string.IsNullOrWhiteSpace(a.TextAnswer)))
					//	AddAngularError(model, $"Question{question.QuestionId}", "Please enter an answer.");
				}

				if (model.HasError)
					return Json(model);

				var submissionModel = model.GetSubmissionModel();
				_surveyService.SubmitWithdrawalSurvey(submissionModel);

				model.RedirectURL = Url.Action("Confirmation");
			//}
			//else
			//{
			//	AddGenericError(model, "reCAPTCHA validation has failed.");
			//}

			return Json(model);
		}

		[Route("Confirmed")]
		public ActionResult Confirmation()
		{
			return View();
		}

		[Route("Timeout")]
		public ActionResult SessionTimeout()
		{
			return View();
		}

		[Route("Invitation/Expired")]
		public ActionResult InvitationExpired()
		{
			return View();
		}

		private ActionResult IsInvitationExpired(string invitationKey, out Guid guidResult)
		{
			bool invitationIsValid = Guid.TryParse(invitationKey, out guidResult);
			if (!invitationIsValid)
			{
				_logger.Error($"The invitationKey is not a valid GUID: {invitationKey}");
				this.SetAlert("Access to the page is denied, request did not have a valid invitation key.", AlertType.Error, true);

				return RedirectToAction("InvitationExpired");
			}

			return IsInvitationExpired(guidResult, false);
		}

		private ActionResult IsInvitationExpired(Guid guidResult, bool ajax)
		{
			var grantApplication = _grantApplicationService.Get(guidResult);
			if (grantApplication == null)
			{
				_logger.Error($"The invitation key is either invalid or expired: { guidResult.ToString() }");
				if (ajax)
					return RedirectToActionAjax("InvitationExpired");

				return RedirectToAction("InvitationExpired");
			}

			return null;
		}

		private bool IsWithdrawalAvailable(Guid invitationKey, Guid withdrawalKey)
		{
			var pifResult = _surveyService.FindParticipantFormForWithdrawalSurvey(invitationKey, withdrawalKey);
			return pifResult > 0;
		}

		private ActionResult RedirectToActionAjax(string action)
		{
			Response.StatusCode = 200;
			Response.TrySkipIisCustomErrors = true;
			var data = new
			{
				RedirectUrl = Url.Action(action)
			};

			return Json(data, JsonRequestBehavior.AllowGet);
		}

		private static Tuple<DateTime, DateTime> GetWithdrawalDateLimits()
		{
			var earliest = 2018;
			var latest = AppDateTime.UtcNow.AddYears(1).Year;

			var earliestDate = new DateTime(earliest, 1, 1);
			var oldestDate = new DateTime(latest, 12, 31);

			return new Tuple<DateTime, DateTime>(earliestDate, oldestDate);
		}
	}
}
