using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Web.Mvc;
using CJG.Application.Business.Models;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Int.Models.EvaluationForm;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Controllers
{
    [RouteArea("Int")]
	[Authorize(Roles = "Assessor, System Administrator, Director, Financial Clerk")]
	public class ApplicationEvaluationController : BaseController
	{
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IAttachmentService _attachmentService;
		private readonly IEvaluationFormService _evaluationFormService;

		public ApplicationEvaluationController(
			IControllerService controllerService,
			IGrantApplicationService grantApplicationService,
			IAttachmentService attachmentService,
			IEvaluationFormService evaluationFormService
		   ) : base(controllerService.Logger)
		{
			_grantApplicationService = grantApplicationService;
			_attachmentService = attachmentService;
			_evaluationFormService = evaluationFormService;
		}

		/// <summary>
		/// Returns the application details view.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet, Route("Application/Evaluation/View/{grantApplicationId}", Name = "CompleteEvaluation")]
		public ActionResult EvaluationView(int grantApplicationId)
		{
			ViewBag.GrantApplicationId = grantApplicationId;
			var grantApplication = _grantApplicationService.Get(grantApplicationId);
			ViewBag.GrantFileNumber = grantApplication.FileNumber;

			return View();
		}

		/// <summary>
		/// Get the application details view data.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/Evaluation/Summary/{grantApplicationId}")]
		public JsonResult GetApplicationDetails(int grantApplicationId)
		{
			var model = new EvaluationSummaryViewModel();

			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				model = new EvaluationSummaryViewModel(grantApplication, _evaluationFormService, User);
				
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Route("Application/Evaluation/QuestionTypes")]
		public JsonResult GetQuestionTypes()
		{
			var results = new
			{
				Header = new List<KeyValuePair<int, string>>(),
				YesNoAnswers = new List<KeyValuePair<int, string>>
				{
					new KeyValuePair<int, string>(0, "---"),
					new KeyValuePair<int, string>(1, "Yes"),
					new KeyValuePair<int, string>(2, "No"),
					new KeyValuePair<int, string>(3, "N/A")
				},
				Employment = new List<KeyValuePair<int, string>>
				{
					new KeyValuePair<int, string>(0, "---"),
					new KeyValuePair<int, string>(10, "Employment"),
					new KeyValuePair<int, string>(11, "Self-Employment")
				}
			};

			return Json(results, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Route("Application/Evaluation/Questions/{grantApplicationId}")]
		public JsonResult GetQuestions(int grantApplicationId)
		{
			var model = new EvaluationFormListViewModel();
			try
			{
				SetModelQuestions(grantApplicationId, model);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		private void SetModelQuestions(int grantApplicationId, EvaluationFormListViewModel model, bool showCurrentQuestions = true)
		{
			var grantApplication = _grantApplicationService.Get(grantApplicationId);

			model.Id = grantApplication.Id;

			model.QuestionsWithAnswers = _evaluationFormService
				.GetQuestions()
				.Select(t => new EvaluationQuestionAndAnswerViewModel(t))
				.ToList();

			var currentAnswers = grantApplication.GrantApplicationEvaluation?.EvaluationAnswers?.ToList() ?? new List<GrantApplicationEvaluationAnswer>();

			var questionCounter = 1;

			foreach (var question in model.QuestionsWithAnswers)
			{
				if (question.EvaluationFormQuestionType == EvaluationFormQuestionType.Header)
					continue;

				question.Number = questionCounter;
				questionCounter++;

				var currentAnswer = currentAnswers.FirstOrDefault(a => a.EvaluationFormQuestionReferenceId == question.Id);

				if (currentAnswer != null)
				{
					if (!showCurrentQuestions)
						question.Text = question.FullAnswer;

					question.Answer = currentAnswer.AnswerGiven;
					question.FullAnswer = GetFullAnswer(currentAnswer.AnswerGiven, question.EvaluationFormQuestionType);
				}
			}
		}

		[HttpGet]
		[Route("Application/Evaluation/QuestionsPrint/{grantApplicationId}")]
		public JsonResult GetQuestionsForPrint(int grantApplicationId)
		{
			var model = new EvaluationFormListViewModel
			{
				QuestionsWithAnswers = new List<EvaluationQuestionAndAnswerViewModel>()
			};

			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);

				model.Id = grantApplication.Id;
				model.QuestionsWithAnswers = grantApplication.GrantApplicationEvaluation?
					                             .EvaluationAnswers?
												 .ToList()
					                             .OrderBy(q => q.RowSequence)
					                             .Select(t => new EvaluationQuestionAndAnswerViewModel(t))
					                             .ToList() ?? new List<EvaluationQuestionAndAnswerViewModel>();

				var questionCounter = 1;
				foreach (var question in model.QuestionsWithAnswers)
				{
					if (question.EvaluationFormQuestionType == EvaluationFormQuestionType.Header)
						continue;

					question.Number = questionCounter;
					questionCounter++;

					question.FullAnswer = GetFullAnswer(question.Answer, question.EvaluationFormQuestionType);
				}
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		private string GetFullAnswer(int answer, EvaluationFormQuestionType questionType)
		{
			switch (questionType)
			{
				case EvaluationFormQuestionType.Header:
					return string.Empty;

				case EvaluationFormQuestionType.YesNoOrNa:
					switch (answer)
					{
						case 1:
							return "Yes";
						case 2:
							return "No";
						case 3:
							return "N/A";
					}
					break;

				case EvaluationFormQuestionType.Employment:
					switch (answer)
					{
						case 10:
							return "Employment";
						case 11:
							return "Self-Employment";
					}
					break;

				default:
					return string.Empty;
			}
			return string.Empty;
		}


		[HttpGet]
		[Route("Application/Evaluation/Resources")]
		public JsonResult GetResourceAttachments()
		{
			var model = new EvaluationFormAttachmentsViewModel();
			try
			{
				var resources = _evaluationFormService.GetResources().ToList();
				model = new EvaluationFormAttachmentsViewModel(resources);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[PreventSpam]
		[Route("Application/Evaluation/Resource/{attachmentId}")]
		public ActionResult DownloadResource(int attachmentId)
		{
			var model = new BaseViewModel();
			try
			{
				var attachment = _attachmentService.Get(attachmentId);
				return File(attachment.AttachmentData, MediaTypeNames.Application.Octet, $"{attachment.FileName}{attachment.FileExtension}");
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Submit the evaluation to complete it
		/// </summary>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/Evaluation/Submit/{grantApplicationId}")]
		public JsonResult SubmitEvaluation(int grantApplicationId)
		{
			var model = new BaseViewModel();

			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);

				if (ModelState.IsValid)
				{
					_evaluationFormService.SubmitEvaluation(grantApplication);
					_grantApplicationService.Update(grantApplication, ApplicationWorkflowTrigger.EditSummary);
				}
				else
				{
					HandleModelStateValidation(model, ModelState.GetErrorMessages("<br />"));
				}
			}
			catch (Exception e)
			{
				HandleAngularException(e, model);
			}

			//this.SetAlert("Evaluation form successfully submitted.", AlertType.Success, true);
			model.RedirectURL = Url.Action("ApplicationDetailsView", "Application", new { grantApplicationId });

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Unsubmit the evaluation
		/// </summary>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/Evaluation/Withdraw/{grantApplicationId}")]
		public JsonResult WithdrawEvaluation(int grantApplicationId)
		{
			var model = new BaseViewModel();

			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);

				if (ModelState.IsValid)
				{
					_evaluationFormService.WithdrawEvaluation(grantApplication);
					_grantApplicationService.Update(grantApplication, ApplicationWorkflowTrigger.EditSummary);
				}
				else
				{
					HandleModelStateValidation(model, ModelState.GetErrorMessages("<br />"));
				}
			}
			catch (Exception e)
			{
				HandleAngularException(e, model);
			}

			//this.SetAlert("Evaluation form successfully submitted.", AlertType.Success, true);
			model.RedirectURL = Url.Action("EvaluationView", "ApplicationEvaluation", new { grantApplicationId });

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Update the evaluation summary information in the datasource.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/Evaluation/Summary")]
		public JsonResult UpdateSummary(EvaluationSummaryViewModel model)
		{
			try
			{
				GrantApplication grantApplication = _grantApplicationService.Get(model.Id);
				grantApplication.RowVersion = Convert.FromBase64String(model.RowVersion);

				if (ModelState.IsValid)
				{
					var evaluation = grantApplication.GrantApplicationEvaluation;
					if (evaluation == null)
					{
						evaluation = new GrantApplicationEvaluation
						{
							GrantApplication = grantApplication,
							EvaluationStatus = EvaluationStatus.Started
						};
						grantApplication.GrantApplicationEvaluation = evaluation;
					}

					evaluation.HighLevelRationale = model.HighLevelRationale;
					evaluation.ApplicationNotes = model.ApplicationNotes;

					_grantApplicationService.Update(grantApplication, ApplicationWorkflowTrigger.EditSummary);
					model = new EvaluationSummaryViewModel(grantApplication, _evaluationFormService, User);
				}
				else
				{
					HandleModelStateValidation(model, ModelState.GetErrorMessages("<br />"));
				}
			}
			catch (Exception e)
			{
				HandleAngularException(e, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Update the evaluation summary information in the datasource.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/Evaluation/Questions")]
		public JsonResult UpdateQuestions(EvaluationFormListViewModel model)
		{
			try
			{
				var grantApplication = _grantApplicationService.Get(model.Id);
				if (ModelState.IsValid)
				{
					var answersModel = model.QuestionsWithAnswers.Select(a => new EvaluationAnswerModel
					{
						EvaluationQuestionId = a.Id,
						Answer = a.Answer
					}).ToList();
					
					_evaluationFormService.UpdateAnswers(grantApplication, answersModel);
					_grantApplicationService.Update(grantApplication, ApplicationWorkflowTrigger.EditSummary);
				}
				else
				{
					HandleModelStateValidation(model, ModelState.GetErrorMessages("<br />"));
				}
			}
			catch (Exception e)
			{
				HandleAngularException(e, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Route("Application/Evaluation/Print/{grantApplicationId}")]
		public ActionResult EvaluationPrint(int grantApplicationId)
		{
			ViewBag.GrantApplicationId = grantApplicationId;
			var grantApplication = _grantApplicationService.Get(grantApplicationId);
			ViewBag.GrantFileNumber = grantApplication.FileNumber;

			return View();
		}

		//[HttpGet]
		//[Route("Application/Evaluation/Print/Data/{grantApplicationId}")]
		//public JsonResult GetPrintDetails(int grantApplicationId)
		//{
		//	var model = new EvaluationSummaryViewModel();

		//	try
		//	{
		//		var grantApplication = _grantApplicationService.Get(grantApplicationId);
		//		model = new EvaluationSummaryViewModel(grantApplication, _evaluationFormService, User);
		//	}
		//	catch (Exception ex)
		//	{
		//		HandleAngularException(ex, model);
		//	}

		//	return Json(model, JsonRequestBehavior.AllowGet);
		//}

	}
}
