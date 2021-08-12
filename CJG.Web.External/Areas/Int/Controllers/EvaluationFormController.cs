using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Identity;
using CJG.Web.External.Areas.Int.Models.Attachments;
using CJG.Web.External.Areas.Int.Models.EvaluationForm;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Controllers
{
    /// <summary>
    /// <paramtyperef name="EvaluationFormController"/> class, provides endpoints to manage evaluation form questions
    /// </summary>
    [AuthorizeAction(Privilege.GM1, Privilege.SM)]
	[RouteArea("Int")]
	[RoutePrefix("Admin/EvaluationForm")]
	public class EvaluationFormController : BaseController
	{
		private readonly IEvaluationFormService _evaluationFormService;
		private readonly IAttachmentService _attachmentService;

		/// <summary>
		/// Creates a new instance of a <paramtyperef name="GrantStreamController"/> object.
		/// </summary>
		/// <param name="controllerService"></param>
		/// <param name="evaluationFormService"></param>
		/// <param name="attachmentService"></param>
		public EvaluationFormController(
			IControllerService controllerService,
			IEvaluationFormService evaluationFormService,
			IAttachmentService attachmentService) : base(controllerService.Logger)
		{
			_evaluationFormService = evaluationFormService;
			_attachmentService = attachmentService;
		}

		/// <summary>
		/// Evaluation form dashboard endpoint.
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[Route("View")]
		public ActionResult EvaluationFormView()
		{
			return View();
		}

		[HttpGet]
		[Route("QuestionTypes")]
		public JsonResult GetQuestionTypes()
		{
			List<KeyValuePair<int, string>> questionTypes = null;
			try
			{
				questionTypes = new List<KeyValuePair<int, string>>
				{
					new KeyValuePair<int, string>((int) EvaluationFormQuestionType.Header, "Header"),
					new KeyValuePair<int, string>((int) EvaluationFormQuestionType.YesNoOrNa, "Yes/No/NA"),
					new KeyValuePair<int, string>((int) EvaluationFormQuestionType.Employment, "Employment")
				};
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}
			return Json(questionTypes, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Route("Questions")]
		public JsonResult GetQuestions()
		{
			var model = new EvaluationFormListViewModel();
			try
			{
				model.Questions = _evaluationFormService
					.GetQuestions()
					.Select(t => new EvaluationFormQuestionViewModel(t))
					.ToList();
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpPut]
		[ValidateRequestHeader]
		[PreventSpam]
		[Route("Questions")]
		public JsonResult UpdateChecklist(EvaluationFormListViewModel model)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					HandleModelStateValidation(model);
				}
				else
				{
					var questionList = _evaluationFormService.GetQuestions().ToList();
					var postedItemIds = model.Questions
						.Where(i => i.Id > 0)
						.Select(i => i.Id)
						.ToList();

					var itemsToRemove = questionList
						.Where(i => !postedItemIds.Contains(i.Id))
						.ToList();

					foreach (var question in model.Questions)
					{
						EvaluationFormQuestion evaluationFormQuestion;
						if (question.Id != 0)
						{
							evaluationFormQuestion = questionList.First(x => x.Id == question.Id);

							evaluationFormQuestion.Text = question.Text ?? string.Empty;
							evaluationFormQuestion.EvaluationFormQuestionType = question.EvaluationFormQuestionType;
							evaluationFormQuestion.IsActive = true;
							evaluationFormQuestion.RowSequence = question.RowSequence;
							evaluationFormQuestion.RowVersion = Convert.FromBase64String(question.RowVersion);
						}
						else
						{
							evaluationFormQuestion = new EvaluationFormQuestion();
							evaluationFormQuestion.Text = question.Text;
							evaluationFormQuestion.EvaluationFormQuestionType = question.EvaluationFormQuestionType;
							evaluationFormQuestion.IsActive = true;
							evaluationFormQuestion.RowSequence = question.RowSequence;

							questionList.Add(evaluationFormQuestion);
						}
					}

					foreach (var toRemove in itemsToRemove)
					{
						questionList.Remove(toRemove);
					}

					_evaluationFormService.UpdateQuestions(questionList, itemsToRemove);
				}
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}

            // Fix up validation errors.
            // An array returns validation errors in the text form of "Questions[0].Text" and then the error text.
            // Fixup without the [] since that is not easily addressable inside javascript.
            if (model.ValidationErrors != null)
            {
                var originalCount = model.ValidationErrors.Count;
                for (int aIdx = 0; aIdx < originalCount; aIdx++)
                {
                    var newValue = model.ValidationErrors[aIdx].Key.Replace('[', '_');
                    newValue = newValue.Replace(']', '_');
                    model.ValidationErrors.Add(new KeyValuePair<string, string>(newValue, model.ValidationErrors[aIdx].Value));
                }
            }

            model.Questions = _evaluationFormService
	            .GetQuestions()
	            .Select(t => new EvaluationFormQuestionViewModel(t))
	            .ToList();

			return Json(model);
		}
		
		[HttpGet]
		[Route("ClaimQuestionTypes")]
		public JsonResult GetClaimQuestionTypes()
		{
			List<KeyValuePair<int, string>> questionTypes = null;
			try
			{
				questionTypes = new List<KeyValuePair<int, string>>
				{
					new KeyValuePair<int, string>((int) ClaimEvaluationFormQuestionType.Header, "Header"),
					new KeyValuePair<int, string>((int) ClaimEvaluationFormQuestionType.YesNoOrNa, "Yes/No/NA"),
				};
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}
			return Json(questionTypes, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Route("ClaimQuestions")]
		public JsonResult GetClaimQuestions()
		{
			var model = new ClaimEvaluationFormListViewModel();
			try
			{
				model.Questions = _evaluationFormService
					.GetClaimQuestions()
					.Select(t => new ClaimEvaluationFormQuestionViewModel(t))
					.ToList();
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpPut]
		[ValidateRequestHeader]
		[PreventSpam]
		[Route("ClaimQuestions")]
		public JsonResult UpdateClaimChecklist(ClaimEvaluationFormListViewModel model)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					HandleModelStateValidation(model);
				}
				else
				{
					var questionList = _evaluationFormService.GetClaimQuestions().ToList();
					var postedItemIds = model.Questions
						.Where(i => i.Id > 0)
						.Select(i => i.Id)
						.ToList();

					var itemsToRemove = questionList
						.Where(i => !postedItemIds.Contains(i.Id))
						.ToList();

					foreach (var question in model.Questions)
					{
						ClaimEvaluationFormQuestion evaluationFormQuestion;
						if (question.Id != 0)
						{
							evaluationFormQuestion = questionList.First(x => x.Id == question.Id);

							evaluationFormQuestion.Text = question.Text ?? string.Empty;
							evaluationFormQuestion.ClaimEvaluationFormQuestionType = question.ClaimEvaluationFormQuestionType;
							evaluationFormQuestion.IsActive = true;
							evaluationFormQuestion.RowSequence = question.RowSequence;
							evaluationFormQuestion.RowVersion = Convert.FromBase64String(question.RowVersion);
						}
						else
						{
							evaluationFormQuestion = new ClaimEvaluationFormQuestion();
							evaluationFormQuestion.Text = question.Text;
							evaluationFormQuestion.ClaimEvaluationFormQuestionType = question.ClaimEvaluationFormQuestionType;
							evaluationFormQuestion.IsActive = true;
							evaluationFormQuestion.RowSequence = question.RowSequence;

							questionList.Add(evaluationFormQuestion);
						}
					}

					foreach (var toRemove in itemsToRemove)
					{
						questionList.Remove(toRemove);
					}

					_evaluationFormService.UpdateClaimQuestions(questionList, itemsToRemove);
				}
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}

            // Fix up validation errors.
            // An array returns validation errors in the text form of "Questions[0].Text" and then the error text.
            // Fixup without the [] since that is not easily addressable inside javascript.
            if (model.ValidationErrors != null)
            {
                var originalCount = model.ValidationErrors.Count;
                for (int aIdx = 0; aIdx < originalCount; aIdx++)
                {
                    var newValue = model.ValidationErrors[aIdx].Key.Replace('[', '_');
                    newValue = newValue.Replace(']', '_');
                    model.ValidationErrors.Add(new KeyValuePair<string, string>(newValue, model.ValidationErrors[aIdx].Value));
                }
            }

            var claimEvaluationFormQuestions = _evaluationFormService
	            .GetClaimQuestions();

            model.Questions = claimEvaluationFormQuestions
	            .Select(t => new ClaimEvaluationFormQuestionViewModel(t))
	            .ToList();

			return Json(model);
		}

		[HttpGet]
		[Route("Attachments")]
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
        [Route("Resource/{attachmentId}")]
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

		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Attachments")]
		public JsonResult UpdateApplicationAttachments(HttpPostedFileBase[] files, string attachments)
		{
			var model = new EvaluationFormAttachmentsViewModel();
			try
			{
				// Deserialize model. This is required because it isn't easy to deserialize an array when including files in a multipart data form.
				var data = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<UpdateAttachmentViewModel>>(attachments);

				foreach (var attachment in data)
				{
					if (attachment.Delete) // Delete
					{
						var existing = _evaluationFormService.GetAttachment(attachment.Id);
						existing.RowVersion = Convert.FromBase64String(attachment.RowVersion);
						_evaluationFormService.DeleteResource(null, existing);
					}
					else if (attachment.Index.HasValue == false) // Update data only
					{
						var existing = _evaluationFormService.GetAttachment(attachment.Id);
						existing.RowVersion = Convert.FromBase64String(attachment.RowVersion);
						attachment.MapToEntity(existing);
						_evaluationFormService.UpdateResource(null, existing, true);
					}
					else if (files.Length > attachment.Index.Value && files[attachment.Index.Value] != null && attachment.Id == 0) // Add
					{
						var file = files[attachment.Index.Value].UploadFile(attachment.Description, attachment.FileName, permittedFileTypesKey: "EvaluationFormPermittedAttachmentTypes");
						_evaluationFormService.AddResource(file, true);
					}
					else if (files.Length > attachment.Index.Value && files[attachment.Index.Value] != null && attachment.Id != 0) // Update with file
					{
						var file = files[attachment.Index.Value].UploadFile(attachment.Description, attachment.FileName, permittedFileTypesKey: "EvaluationFormPermittedAttachmentTypes");
						var existing = _attachmentService.Get(attachment.Id);
						existing.RowVersion = Convert.FromBase64String(attachment.RowVersion);
						attachment.MapToEntity(existing);
						existing.AttachmentData = file.AttachmentData;
						_evaluationFormService.UpdateResource(null, existing, true);
					}
				}

				var resources = _evaluationFormService.GetResources().ToList();
				model = new EvaluationFormAttachmentsViewModel(resources);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}
	}
}
