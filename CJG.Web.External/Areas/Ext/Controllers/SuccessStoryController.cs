using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CJG.Application.Services;
using CJG.Application.Services.Exceptions;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Ext.Models.Attachments;
using CJG.Web.External.Areas.Ext.Models.Reporting;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;
using CJG.Web.External.Models.Shared;
using Newtonsoft.Json;

namespace CJG.Web.External.Areas.Ext.Controllers
{
    /// <summary>
    /// SuccessStoryController class, provides a controller endpoints for managing external grant application success stories.
    /// </summary>
    [RouteArea("Ext")]
	[ExternalFilter]
	public class SuccessStoryController : BaseController
	{
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly ISuccessStoryService _successStoryService;
		private readonly IAttachmentService _attachmentService;

		/// <summary>
		/// Creates a new instance of a SuccessStoryController object.
		/// </summary>
		/// <param name="grantApplicationService"></param>
		/// <param name="successStoryService"></param>
		/// <param name="attachmentService"></param>
		/// <param name="controllerService"></param>
		public SuccessStoryController(
			IGrantApplicationService grantApplicationService,
			ISuccessStoryService successStoryService,
			IAttachmentService attachmentService,
			IControllerService controllerService)
			: base(controllerService.Logger)
		{
			_grantApplicationService = grantApplicationService;
			_successStoryService = successStoryService;
			_attachmentService = attachmentService;
		}

		/// <summary>
		/// Display the Success Story View.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[Route("Application/SuccessStory/View/{grantApplicationId}")]
		public ActionResult SuccessStoryView(int grantApplicationId)
		{
			var grantApplication = _grantApplicationService.Get(grantApplicationId);
			ViewBag.GrantApplicationId = grantApplicationId;
			return View();
		}

		/// <summary>
		/// Get the data for the SuccessStoryView page.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/SuccessStory/{grantApplicationId}")]
		public JsonResult GetSuccessStory(int grantApplicationId)
		{
			var model = new SuccessStoryViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				var successStory = _successStoryService.GetSuccessStory(grantApplicationId);

				model = new SuccessStoryViewModel(grantApplication, successStory);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Return the attachment data for the specified attachment.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="attachmentId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/SuccessStory/Document/{grantApplicationId}/{attachmentId}")]
		public JsonResult GetDocument(int grantApplicationId, int attachmentId)
		{
			var model = new GrantApplicationAttachmentViewModel();
			try
			{
				// Should check that the success story doc belongs to application here
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				var attachment = _attachmentService.Get(attachmentId);
				model = new GrantApplicationAttachmentViewModel(grantApplication, attachment);
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
		/// <param name="completeSuccessStory"></param>
		/// <param name="files"></param>
		/// <param name="attachments"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/SuccessStory/{grantApplicationId}")]
		public JsonResult UpdateSuccessStory(int grantApplicationId, bool? successfulOutcome, string noOutcomeReason, bool? completeSuccessStory, HttpPostedFileBase[] files, string attachments)
		{
			var model = new SuccessStoryViewModel();
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
						existing.AttachmentData = file.AttachmentData;
						_attachmentService.Update(existing, true);
					}
				}

				successStory.SuccessfulOutcome = successfulOutcome ?? false;
				successStory.NoOutcomeReason = successStory.SuccessfulOutcome ? string.Empty : noOutcomeReason;

                if (completeSuccessStory.HasValue && completeSuccessStory.Value)
                    successStory.State = SuccessStoryState.Complete;

                _successStoryService.UpdateSuccessStory(successStory);

				model = new SuccessStoryViewModel(grantApplication, successStory);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Downloads specified success story attachment
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="attachmentId"></param>
		/// <returns></returns>
		[HttpGet]
		[PreventSpam]
		[Route("Application/SuccessStory/Download/{grantApplicationId}/{attachmentId}")]
		public ActionResult DownloadAttachment(int grantApplicationId, int attachmentId)
		{
			var model = new BaseViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				var attachment = _attachmentService.Get(attachmentId);
				var successStory = _successStoryService.GetSuccessStory(grantApplicationId);

				var checkAttachment = successStory.Documents.FirstOrDefault(a => a.Id == attachmentId);
				if (checkAttachment == null)
					throw new NotAuthorizedException($"User does not have permission to access attachment '{attachment?.Id}'.");

				return File(attachment.AttachmentData, System.Net.Mime.MediaTypeNames.Application.Octet, attachment.FileName);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}
	}
}
