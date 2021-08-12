using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Ext.Models.Attachments;
using CJG.Web.External.Areas.Int.Models;
using CJG.Web.External.Areas.Int.Models.SuccessStories;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;
using CJG.Web.External.Models.Shared;
using Newtonsoft.Json;

namespace CJG.Web.External.Areas.Int.Controllers
{
    [RouteArea("Int")]
	[Authorize(Roles = "Assessor, System Administrator, Director, Financial Clerk")]
	public class SuccessStoriesController : BaseController
	{
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IAttachmentService _attachmentService;
		private readonly ISuccessStoryService _successStoryService;
		private readonly INoteService _noteService;

		public SuccessStoriesController(
			IControllerService controllerService,
			IGrantApplicationService grantApplicationService,
			IAttachmentService attachmentService,
			ISuccessStoryService successStoryService,
			INoteService noteService
		   ) : base(controllerService.Logger)
		{
			_grantApplicationService = grantApplicationService;
			_attachmentService = attachmentService;
			_successStoryService = successStoryService;
			_noteService = noteService;
		}

		/// <summary>
		/// Toggle the Success Stories from Open to Closed or Closed to Open.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="rowVersion"></param>
		/// <returns></returns>
		[HttpPut]
		[ValidateRequestHeader]
		[PreventSpam]
		[Route("Application/SuccessStories/Toggle/{grantApplicationId}")]
		public JsonResult ToggleSuccessStory(int grantApplicationId, string rowVersion)
		{
			var model = new ApplicationDetailsViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				_successStoryService.ToggleSuccessStory(grantApplication);

				grantApplication.RowVersion = Convert.FromBase64String(rowVersion.Replace(" ", "+"));

				model = new ApplicationDetailsViewModel(grantApplication, User, x => Url.RouteUrl(x), _successStoryService);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Return all of the success story document data for the specified grant application.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/SuccessStories/Documents/{grantApplicationId}")]
		public JsonResult GetDocuments(int grantApplicationId)
		{
			var model = new SuccessStoriesDocumentsModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				var successStory = _successStoryService.GetSuccessStory(grantApplication.Id);

				model = new SuccessStoriesDocumentsModel(grantApplication, successStory);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Update the Success Story + Attachments (delete/update/create) for the specified grant application.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="successfulOutcome"></param>
		/// <param name="noOutcomeReason"></param>
		/// <param name="files"></param>
		/// <param name="attachments"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/SuccessStories")]
		public JsonResult UpdateSuccessStory(int grantApplicationId, bool? successfulOutcome, string noOutcomeReason, HttpPostedFileBase[] files, string attachments)
		{
			var model = new SuccessStoriesDocumentsModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				var successStory = _successStoryService.GetSuccessStory(grantApplicationId);

				// Deserialize model.  This is required because it isn't easy to deserialize an array when including files in a multipart data form.
				var data = JsonConvert.DeserializeObject<IEnumerable<UpdateAttachmentViewModel>>(attachments);

				if (successStory == null)
				{
					successStory = new SuccessStory
					{
						GrantApplicationId = grantApplication.Id,
						State = SuccessStoryState.Incomplete,
						Documents = new List<Attachment>(),
						RowVersion = grantApplication.RowVersion
					};

					_successStoryService.AddSuccessStory(successStory);
				}

				foreach (var attachment in data)
				{
					if (attachment.Delete) // Delete
					{
						var existing = _attachmentService.Get(attachment.Id);
						existing.RowVersion = Convert.FromBase64String(attachment.RowVersion);
						successStory.Documents.Remove(existing);

						_attachmentService.Delete(existing);
					}
					else if (attachment.Index.HasValue == false) // Update data only
					{
						var existing = _attachmentService.Get(attachment.Id);
						existing.RowVersion = Convert.FromBase64String(attachment.RowVersion);
						attachment.MapToEntity(existing);
						_attachmentService.Update(existing, true);
					}
					else if (files.Length > attachment.Index.Value && files[attachment.Index.Value] != null && attachment.Id == 0) // Add
					{
						var file = files[attachment.Index.Value].UploadFile(attachment.Description, attachment.FileName);
						file.AttachmentType = attachment.AttachmentType;
						successStory.Documents.Add(file);
						_attachmentService.Add(file, true);
					}
					else if (files.Length > attachment.Index.Value && files[attachment.Index.Value] != null && attachment.Id != 0) // Update with file
					{
						var file = files[attachment.Index.Value].UploadFile(attachment.Description, attachment.FileName);
						var existing = _attachmentService.Get(attachment.Id);
						existing.RowVersion = Convert.FromBase64String(attachment.RowVersion);

						attachment.MapToEntity(existing);
						existing.AttachmentType = attachment.AttachmentType;
						existing.AttachmentData = file.AttachmentData;
						_attachmentService.Update(existing, true);
					}
				}

				successStory.SuccessfulOutcome = successfulOutcome ?? false;
				successStory.NoOutcomeReason = successStory.SuccessfulOutcome ? string.Empty : noOutcomeReason;

				//if (isComplete.HasValue && isComplete.Value)
				//	successStory.State = SuccessStoryState.Complete;

				_successStoryService.UpdateSuccessStory(successStory);

				model = new SuccessStoriesDocumentsModel(grantApplication, successStory);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Return the attachment data for the specified Success Story document.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="attachmentId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/SuccessStories/Document/{grantApplicationId}/{attachmentId}/")]
		public JsonResult GetAttachment(int grantApplicationId, int attachmentId)
		{
			var model = new GrantApplicationAttachmentViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				var attachment = attachmentId > 0 ? _attachmentService.Get(attachmentId) : new Attachment();
				model = new GrantApplicationAttachmentViewModel(grantApplication, attachment);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Downloads specified attachment
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="attachmentId"></param>
		/// <returns></returns>
		[HttpGet]
		[PreventSpam]
		[Route("Application/SuccessStories/Download/{grantApplicationId}/{attachmentId}")]
		public ActionResult DownloadAttachment(int grantApplicationId, int attachmentId)
		{
			var model = new BaseViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				var attachment = _attachmentService.Get(attachmentId);
				return File(attachment.AttachmentData, MediaTypeNames.Application.Octet, $"{attachment.FileName}");
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}
	}
}
