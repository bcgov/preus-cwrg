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
	[RoutePrefix("ExitForm")]
	public class ExitSurveyController : BaseController
	{
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IReCaptchaService _reCaptchaService;
		private readonly ISurveyService _surveyService;

		/// <param name="controllerService"></param>
		/// <param name="grantApplicationService"></param>
		/// <param name="recaptchaService"></param>
		/// <param name="surveyService"></param>
		public ExitSurveyController(
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
		/// <returns></returns>
		[Route("{invitationKey}")]
		public ActionResult Index(string invitationKey)
		{
			try
			{
				var expired = IsInvitationExpired(invitationKey, out var invitationKeyGuid);
				if (expired != null)
					return expired;

				var allSurveysComplete = AreAllSurveysComplete(invitationKeyGuid);
				if (allSurveysComplete != null)
					return allSurveysComplete;

				var model = new ExitSurveyModel
				{
					InvitationKey = invitationKeyGuid,
					RecaptchaEnabled = _reCaptchaService.IsEnabled(),
					RecaptchaSiteKey = _reCaptchaService.GetSiteKey(),
					TimeoutPeriod = ConfigurationManager.AppSettings["ParticipantSessionDuration"]
				};

				ViewBag.InvitationKey = invitationKeyGuid;

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
		[Route("Data/{invitationKey}")]
		public JsonResult GetExitSurvey(Guid invitationKey)
		{
			var model = new ExitSurveyModel
			{
				RecaptchaEnabled = _reCaptchaService.IsEnabled(),
				RecaptchaSiteKey = _reCaptchaService.GetSiteKey(),
				TimeoutPeriod = ConfigurationManager.AppSettings["ParticipantSessionDuration"]
			};

			try
			{
				var grantApplication = _grantApplicationService.Get(invitationKey);
				var ageLimits = GetDateOfBirthLimits();
				var dateLimits = GetExitDateLimits();

				model.InvitationKey = invitationKey;
				model.AgreementNumber = grantApplication.FileNumber;
				model.ParticipantYoungestAge = ageLimits.Item1;
				model.ParticipantOldestAge = ageLimits.Item2;
				model.ExitDate = grantApplication.GetTrainingEndDate();
				//model.EarliestExit = dateLimits.Item1;
				//model.LatestExit = dateLimits.Item2;
				model.Questions = _surveyService
					.GetExitSurveyQuestions()
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
		[ValidateRequestHeader]
		[Route("FindPIF")]
		public JsonResult FindParticipantForm(Guid invitationKey, string firstName, string lastName, DateTime? dateOfBirth)
		{
			if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || !dateOfBirth.HasValue)
				return Json(null);

			int? participantFormId = null;
			try
			{
				participantFormId = _surveyService.FindParticipantFormForExitSurvey(invitationKey, firstName, lastName, dateOfBirth.Value);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
				return Json(participantFormId);
			}

			return Json(participantFormId);
		}

		[Route("Submit")]
		[HttpPost]
		public ActionResult Submit(ExitSurveyModel model)
		{
			model.RecaptchaEnabled = _reCaptchaService.IsEnabled();
			model.RecaptchaSiteKey = _reCaptchaService.GetSiteKey();
			model.TimeoutPeriod = ConfigurationManager.AppSettings["ParticipantSessionDuration"];

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
			if (model.ExitDate == null)
				AddAngularError(model, "ExitDate", "Please enter a date of exit.");

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
			_surveyService.SubmitExitSurvey(submissionModel);

			model.RedirectURL = Url.Action("Confirmation");
			//}
            //else
            //{
            //    AddGenericError(model, "reCAPTCHA validation has failed.");
            //}

            return Json(model);
		}

		/// <summary>
		/// Return the confirmation page view.
		/// </summary>
		/// <returns></returns>
		[Route("Confirmed")]
		public ActionResult Confirmation()
		{
			return View();
		}

		[Route("AllSurveysComplete")]
		public ActionResult AllSurveysComplete()
		{
			return View();
		}

		/// <summary>
		/// Return the session timed-out view.
		/// </summary>
		/// <returns></returns>
		[Route("Timeout")]
		public ActionResult SessionTimeout()
		{
			return View();
		}

		/// <summary>
		/// Return the invitation expired view.
		/// </summary>
		/// <returns></returns>
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

				return RedirectToAction(nameof(InvitationExpired));
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
					return RedirectToActionAjax(nameof(InvitationExpired));

				return RedirectToAction(nameof(InvitationExpired));
			}

			return null;
		}

		private ActionResult AreAllSurveysComplete(Guid invitationKey)
		{
			var grantApplication = _grantApplicationService.Get(invitationKey);

			var totalPifs = grantApplication.ParticipantForms.Count;
			var totalComplete = grantApplication.ParticipantForms.Count(p => p.ParticipantExitSurveyAnswers.Any());

			if (totalPifs <= totalComplete)
			{
				_logger.Info("All Exit Forms have been completed.");
				return RedirectToAction(nameof(AllSurveysComplete));
			}

			return null;
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


		private static Tuple<DateTime, DateTime> GetDateOfBirthLimits()
		{
			int participantOldestAge = 0;
			int participantYoungestAge = 0;

			try
			{
				participantOldestAge = int.Parse(ConfigurationManager.AppSettings["ParticipantOldestAge"]);
			}
			catch
			{
				participantOldestAge = 90;
			}

			try
			{
				participantYoungestAge = int.Parse(ConfigurationManager.AppSettings["ParticipantYoungestAge"]);
			}
			catch
			{
				participantYoungestAge = 15;
			}

			var youngestYear = AppDateTime.UtcNow.AddYears(-1 * participantYoungestAge).Year;
			var oldestYear = AppDateTime.UtcNow.AddYears(-1 * participantOldestAge).Year;

			var youngestDate = new DateTime(youngestYear, 1, 1);
			var oldestDate = new DateTime(oldestYear, 12, 31);

			return new Tuple<DateTime, DateTime>(youngestDate, oldestDate);
		}

		private static Tuple<DateTime, DateTime> GetExitDateLimits()
		{
			var earliest = 2018;
			var latest = AppDateTime.UtcNow.AddYears(1).Year;

			var earliestDate = new DateTime(earliest, 1, 1);
			var oldestDate = new DateTime(latest, 12, 31);

			return new Tuple<DateTime, DateTime>(earliestDate, oldestDate);
		}
	}
}
