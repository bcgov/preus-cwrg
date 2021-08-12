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
using CJG.Web.External.Areas.Ext.Models.Attachments;
using CJG.Web.External.Areas.Int.Models.TrainingProviders;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Controllers
{
    /// <summary>
    /// <typeparamref name="TrainingProviderHistoryController"/> class, provides endpoints to manage Training Provider History.
    /// </summary>
    [RouteArea("Int")]
	[RoutePrefix("Training/Provider")]
	[AuthorizeAction(Privilege.AM1, Privilege.AM2, Privilege.AM3, Privilege.AM4, Privilege.AM5)]
	public class TrainingProviderHistoryController : BaseController
	{
		private readonly ITrainingProviderInventoryService _trainingProviderInventoryService;
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IAttachmentService _attachmentService;

		/// <summary>
		/// Creates a new instance of a <typeparamref name="TrainingProviderHistoryController"/> object.
		/// </summary>
		/// <param name="controllerService"></param>
		/// <param name="trainingProviderInventoryService"></param>
		/// <param name="grantApplicationService"></param>
		/// <param name="attachmentService"></param>
		public TrainingProviderHistoryController(
			IControllerService controllerService,
			ITrainingProviderInventoryService trainingProviderInventoryService,
			IGrantApplicationService grantApplicationService,
			IAttachmentService attachmentService) : base(controllerService.Logger)
		{
			_trainingProviderInventoryService = trainingProviderInventoryService;
			_grantApplicationService = grantApplicationService;
			_attachmentService = attachmentService;
		}

		/// <summary>
		/// Returns the training provider grant file history view.
		/// </summary>
		/// <param name="trainingProviderId"></param>
		/// <returns></returns>
		[HttpGet, Route("History/View/{trainingProviderId}")]
		public ActionResult TrainingProviderGrantFileHistoryView(int trainingProviderId)
		{
			ViewBag.TrainingProviderId = trainingProviderId;
			return View();
		}

		/// <summary>
		/// Get the training provider grant file history view data.
		/// </summary>
		/// <param name="trainingProviderId"></param>
		/// <returns></returns>
		[HttpGet, Route("History/{trainingProviderId}")]
		public JsonResult GetTrainingProviderGrantFileHistory(int trainingProviderId)
		{
			var model = new TrainingProviderGrantFileHistoryViewModel();
			try
			{
				var trainingProviderInventory = _trainingProviderInventoryService.Get(trainingProviderId);

				model = new TrainingProviderGrantFileHistoryViewModel(trainingProviderInventory)
				{
					AllowDeleteTrainingProvider = User.HasPrivilege(Privilege.TP2) && (_grantApplicationService.GetTotalGrantApplications(trainingProviderId) == 0),
					UrlReferrer = Request.UrlReferrer?.AbsolutePath ??
					   new UrlHelper(this.ControllerContext.RequestContext).Action(nameof(TrainingProviderInventoryController.TrainingProvidersView), nameof(TrainingProviderInventoryController).Replace("Controller", ""))
				};
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Get grant file histories for training provider inventory.
		/// </summary>
		/// <param name="trainingProviderInventoryId"></param>
		/// <param name="page"></param>
		/// <param name="quantity"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		[HttpGet, Route("History/Search/{trainingProviderInventoryId}/{page}/{quantity}")]
		[ValidateRequestHeader]
		[AuthorizeAction(Privilege.IA1)]
		public JsonResult GetTrainingProviderGrantFileHistory(int trainingProviderInventoryId, int page, int quantity, string search)
		{
			var model = new BaseViewModel();
			try
			{
				var grantApplications = _grantApplicationService.GetGrantApplications(trainingProviderInventoryId, page, quantity, search);
				var result = new
				{
					RecordsFiltered = grantApplications.Items.Count(),
					RecordsTotal = grantApplications.Total,
					Data = grantApplications.Items.Select(o => new TrainingProviderGrantFileHistoryDataTableModel(o)).ToArray()
				};
				return Json(result, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Update the training provider inventory notes.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="notesText"></param>
		/// <param name="rowVersion"></param>
		/// <returns></returns>
		[HttpPut, Route("History/Note/{id}")]
		[AuthorizeAction(Privilege.TP1, Privilege.TP2)]
		public JsonResult UpdateNote(int id, string notesText, string rowVersion)
		{
			var model = new TrainingProviderGrantFileHistoryViewModel();
			try
			{
				var trainingProviderInventory = _trainingProviderInventoryService.Get(id);

				trainingProviderInventory.RowVersion = Convert.FromBase64String(rowVersion.Replace(" ", "+"));
				trainingProviderInventory.Notes = notesText;

				_trainingProviderInventoryService.Update(trainingProviderInventory);

				model.RowVersion = Convert.ToBase64String(trainingProviderInventory.RowVersion);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Return all of the document data for the specified training provider inventory.
		/// </summary>
		/// <param name="trainingProviderInventoryId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("History/Documents/{trainingProviderInventoryId}")]
		public JsonResult GetDocuments(int trainingProviderInventoryId)
		{
			var model = new TrainingProviderDocumentsViewModel();
			try
			{
				var organization = _trainingProviderInventoryService.Get(trainingProviderInventoryId);
				model = new TrainingProviderDocumentsViewModel(organization);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Update the documents (delete/update/create) for the specified training provider.
		/// </summary>
		/// <param name="trainingProviderInventoryId"></param>
		/// <param name="files">Encoded file data</param>
		/// <param name="documents">Metadata about the documents</param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("History/Documents/")]
		public JsonResult SaveDocuments(int trainingProviderInventoryId, HttpPostedFileBase[] files, string documents)
		{
			var model = new TrainingProviderDocumentsViewModel();
			try
			{
				var trainingProviderInventory = _trainingProviderInventoryService.Get(trainingProviderInventoryId);

				// Deserialize model.  This is required because it isn't easy to deserialize an array when including files in a multipart data form.
				var data = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Models.Attachments.UpdateAttachmentViewModel>>(documents);

				foreach (var attachment in data)
				{
					if (attachment.Delete && attachment.Id == 0) // If we're trying to delete a freshly uploaded document, just bypass it
						continue;

					if (attachment.Delete && attachment.Id > 0) // Delete
					{
						var existing = _attachmentService.Get(attachment.Id);
						existing.RowVersion = Convert.FromBase64String(attachment.RowVersion);
						trainingProviderInventory.Documents.Remove(existing);
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
						var file = files[attachment.Index.Value].UploadFile(attachment.Description, attachment.FileName, "HistoryPermittedAttachmentTypes");
						trainingProviderInventory.Documents.Add(file);
						_attachmentService.Add(file, true);
					}
					else if (files.Length > attachment.Index.Value && files[attachment.Index.Value] != null && attachment.Id != 0) // Update with file
					{
						var file = files[attachment.Index.Value].UploadFile(attachment.Description, attachment.FileName, "HistoryPermittedAttachmentTypes");
						var existing = _attachmentService.Get(attachment.Id);
						existing.RowVersion = Convert.FromBase64String(attachment.RowVersion);
						attachment.MapToEntity(existing);
						existing.AttachmentData = file.AttachmentData;
						_attachmentService.Update(existing, true);
					}
				}

				model = new TrainingProviderDocumentsViewModel(trainingProviderInventory);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Return the attachment data for the specified document.
		/// </summary>
		/// <param name="trainingProviderInventoryId"></param>
		/// <param name="attachmentId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("History/Document/{trainingProviderInventoryId}/{attachmentId}/")]
		public JsonResult GetAttachment(int trainingProviderInventoryId, int attachmentId)
		{
			var model = new TrainingProviderInventoryDocumentViewModel();
			try
			{
				var trainingProviderInventory = _trainingProviderInventoryService.Get(trainingProviderInventoryId);
				var attachment = attachmentId > 0 ? _attachmentService.Get(attachmentId) : new Attachment();
				model = new TrainingProviderInventoryDocumentViewModel(trainingProviderInventory, attachment);
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
		/// <param name="trainingProviderInventoryId"></param>
		/// <param name="attachmentId"></param>
		/// <returns></returns>
		[HttpGet]
		[PreventSpam]
		[Route("History/Document/Download/{trainingProviderInventoryId}/{attachmentId}")]
		public ActionResult DownloadAttachment(int trainingProviderInventoryId, int attachmentId)
		{
			var model = new BaseViewModel();
			try
			{
				var trainingProviderInventory = _trainingProviderInventoryService.Get(trainingProviderInventoryId);
				var attachment = _attachmentService.Get(attachmentId);
				return File(attachment.AttachmentData, MediaTypeNames.Application.Octet, $"{attachment.FileName}{attachment.FileExtension}");
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}
	}
}
