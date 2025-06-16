using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using CJG.Application.Services;
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
	/// ProofOfPaymentController class, provides a controller endpoints for managing external grant application proof of payment attachments.
	/// </summary>
	[RouteArea("Ext")]
	[ExternalFilter]
	public class ProofOfPaymentController : BaseController
	{
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IAttachmentService _attachmentService;

		/// <summary>
		/// Creates a new instance of a ProofOfPaymentController object.
		/// </summary>
		/// <param name="grantApplicationService"></param>
		/// <param name="attachmentService"></param>
		/// <param name="controllerService"></param>
		public ProofOfPaymentController(
			IGrantApplicationService grantApplicationService,
			IAttachmentService attachmentService,
			IControllerService controllerService)
			: base(controllerService.Logger)
		{
			_grantApplicationService = grantApplicationService;
			_attachmentService = attachmentService;
		}

		/// <summary>
		/// Display the Proof of Payment View.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[Route("Application/ProofOfPayment/View/{grantApplicationId}")]
		public ActionResult ProofOfPaymentView(int grantApplicationId)
		{
			var grantApplication = _grantApplicationService.Get(grantApplicationId);
			ViewBag.GrantApplicationId = grantApplicationId;
			return View();
		}

		/// <summary>
		/// Get the data for the ProofOfPaymentView page.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/ProofOfPayment/Documents/{grantApplicationId}")]
		public JsonResult GetDocuments(int grantApplicationId)
		{
			var model = new ProofOfPaymentsDocumentsViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				model = new ProofOfPaymentsDocumentsViewModel(grantApplication);
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
		[Route("Application/ProofOfPayment/Document/{grantApplicationId}/{attachmentId}")]
		public JsonResult GetDocument(int grantApplicationId, int attachmentId)
		{
			var model = new GrantApplicationAttachmentViewModel();
			try
			{
				// Should check that the proof of payment doc belongs to application here
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
		/// Update the proof of payment document attachments (delete/update/create) for the specified grant application.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="proofNotApplicable"></param>
		/// <param name="completeProofOfPayment"></param>
		/// <param name="files"></param>
		/// <param name="attachments"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/ProofOfPayment/Documents/{grantApplicationId}")]
		public JsonResult UpdateProofOfPaymentDocuments(int grantApplicationId, bool? proofNotApplicable, bool? completeProofOfPayment, HttpPostedFileBase[] files, string attachments)
		{
			var model = new ProofOfPaymentsDocumentsViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);

				// Deserialize model.  This is required because it isn't easy to deserialize an array when including files in a multipart data form.
				var data = JsonConvert.DeserializeObject<IEnumerable<UpdateAttachmentViewModel>>(attachments);

				if (grantApplication.ProofOfPayment == null)
				{
					grantApplication.ProofOfPayment = new ProofOfPayment
					{
						State = ProofOfPaymentState.Incomplete,
						DateAdded = AppDateTime.UtcNow,
						Documents = new List<Attachment>()
					};

					_grantApplicationService.UpdateProofOfPayment(grantApplication);
				}

				foreach (var attachment in data)
				{
					if (attachment.Delete) // Delete
					{
						var existing = _attachmentService.Get(attachment.Id);
						existing.RowVersion = Convert.FromBase64String(attachment.RowVersion);
						grantApplication.ProofOfPayment.Documents.Remove(existing);

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

				if (proofNotApplicable.HasValue)
					grantApplication.ProofOfPayment.ProofNotApplicable = proofNotApplicable.Value;

				var sendCompletionNotification = false;
				if (completeProofOfPayment.HasValue && completeProofOfPayment.Value)
				{
					//var viewUrl = Url.Action("ApplicationDetailsView", "Application", new { Area = "Int", grantApplicationId = grantApplication.Id });
					grantApplication.ProofOfPayment.State = ProofOfPaymentState.Complete;
					sendCompletionNotification = true;
				}

				_grantApplicationService.UpdateProofOfPayment(grantApplication, sendCompletionNotification);

				model = new ProofOfPaymentsDocumentsViewModel(grantApplication);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Downloads specified proof of payment attachment
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
