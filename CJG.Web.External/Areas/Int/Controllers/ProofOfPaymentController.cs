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
using CJG.Web.External.Areas.Int.Models.ProofOfPayments;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Controllers
{
    [RouteArea("Int")]
	[Authorize(Roles = "Assessor, System Administrator, Director, Financial Clerk")]
	public class ProofOfPaymentController : BaseController
	{
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IAttachmentService _attachmentService;
		private readonly ISuccessStoryService _successStoryService;
		private readonly INoteService _noteService;

		public ProofOfPaymentController(
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
		/// Toggle the Proof of Payment from Open to Closed or Closed to Open.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="rowVersion"></param>
		/// <returns></returns>
		[HttpPut]
		[ValidateRequestHeader]
		[PreventSpam]
		[Route("Application/ProofOfPayment/Toggle/{grantApplicationId}")]
		public JsonResult ToggleProofOfPayment(int grantApplicationId, string rowVersion)
		{
			var model = new ApplicationDetailsViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				grantApplication.ToggleProofOfPayment();
				_grantApplicationService.Update(grantApplication, ApplicationWorkflowTrigger.ViewApplication);

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
		/// Return all of the proof of payment document data for the specified grant application.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/ProofOfPayment/Documents/{grantApplicationId}")]
		public JsonResult GetDocuments(int grantApplicationId)
		{
			var model = new ProofOfPaymentDocumentsModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				model = new ProofOfPaymentDocumentsModel(grantApplication);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}


		/// <summary>
		/// Update the Proof of Payment documents (delete/update/create) for the specified grant application.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="proofNotApplicable"></param>
		/// <param name="files">Encoded file data</param>
		/// <param name="documents">Metadata about the documents</param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/ProofOfPayment/Documents/")]
		public JsonResult SaveProofOfPaymentDocuments(int grantApplicationId, bool? proofNotApplicable, HttpPostedFileBase[] files, string documents)
		{
			var model = new ProofOfPaymentDocumentsModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);

				// Deserialize model.  This is required because it isn't easy to deserialize an array when including files in a multipart data form.
				var data = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Models.Attachments.UpdateAttachmentViewModel>>(documents);

				if (grantApplication.ProofOfPayment == null)
				{
					grantApplication.ProofOfPayment = new ProofOfPayment
					{
						State = ProofOfPaymentState.Incomplete,
						DateAdded = AppDateTime.UtcNow,
						Documents = new List<Attachment>()
					};
				}

				if (proofNotApplicable.HasValue)
					grantApplication.ProofOfPayment.ProofNotApplicable = proofNotApplicable.Value;

				_grantApplicationService.UpdateProofOfPayment(grantApplication);

				foreach (var attachment in data)
				{
					if (attachment.Delete) // Delete
					{
						var existing = _attachmentService.Get(attachment.Id);
						existing.RowVersion = Convert.FromBase64String(attachment.RowVersion);
						grantApplication.ProofOfPayment.Documents.Remove(existing);
						_noteService.AddSystemNote(grantApplication, $"Proof of payment document \'{existing.FileName}\' deleted.");
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
						grantApplication.ProofOfPayment.Documents.Add(file);
						_noteService.AddSystemNote(grantApplication, $"Proof of payment document \'{file.FileName}\' uploaded.");
						_attachmentService.Add(file, true);
					}
					else if (files.Length > attachment.Index.Value && files[attachment.Index.Value] != null && attachment.Id != 0) // Update with file
					{
						var file = files[attachment.Index.Value].UploadFile(attachment.Description, attachment.FileName);
						var existing = _attachmentService.Get(attachment.Id);
						existing.RowVersion = Convert.FromBase64String(attachment.RowVersion);
						_noteService.AddSystemNote(grantApplication, $"Proof of payment document \'{existing.FileName}\' replaced with \'{file.FileName}\'.");
						attachment.MapToEntity(existing);
						existing.AttachmentData = file.AttachmentData;
						_attachmentService.Update(existing, true);
					}
				}

				model = new ProofOfPaymentDocumentsModel(grantApplication);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Return the attachment data for the specified Proof of Payment document.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="attachmentId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/ProofOfPayment/Document/{grantApplicationId}/{attachmentId}/")]
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
		[Route("Application/ProofOfPayment/Download/{grantApplicationId}/{attachmentId}")]
		public ActionResult DownloadAttachment(int grantApplicationId, int attachmentId)
		{
			var model = new BaseViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				var attachment = _attachmentService.Get(attachmentId);
				return File(attachment.AttachmentData, MediaTypeNames.Application.Octet, attachment.FileName);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}
	}
}
