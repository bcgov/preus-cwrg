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
using CJG.Web.External.Areas.Int.Models.Attestation;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Controllers
{
    [RouteArea("Int")]
	[Authorize(Roles = "Assessor, System Administrator, Director, Financial Clerk")]
	public class AttestationController : BaseController
	{
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IAttachmentService _attachmentService;
		private readonly ISuccessStoryService _successStoryService;
		private readonly INoteService _noteService;

		public AttestationController(
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
		/// Toggle the Attestation from Open to Closed or Closed to Open.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="rowVersion"></param>
		/// <returns></returns>
		[HttpPut]
		[ValidateRequestHeader]
		[PreventSpam]
		[Route("Application/Attestation/Toggle/{grantApplicationId}")]
		public JsonResult ToggleAttestation(int grantApplicationId, string rowVersion)
		{
			var model = new ApplicationDetailsViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				grantApplication.ToggleAttestation();
				_grantApplicationService.UpdateAttestation(grantApplication);

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
		/// Return all of the attestation data for the specified grant application.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/Attestation/{grantApplicationId}")]
		public JsonResult GetAttestation(int grantApplicationId)
		{
			var model = new AttestationViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				model = new AttestationViewModel(grantApplication);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Update the Attestation for the specified grant application.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="allocatedCosts"></param>
		/// <param name="attestationNotApplicable"></param>
		/// <param name="completeAttestation"></param>
		/// <param name="files"></param>
		/// <param name="documents"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/Attestation/")]
		public JsonResult SaveAttestation(int grantApplicationId, decimal allocatedCosts, bool? attestationNotApplicable, bool? completeAttestation, HttpPostedFileBase[] files, string documents)
		{
			var model = new AttestationViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);

				model = new AttestationViewModel
				{
					AllocatedCosts = allocatedCosts,
					AttestationNotApplicable = attestationNotApplicable,
					CompleteAttestation = completeAttestation
				};

				// Deserialize model.  This is required because it isn't easy to deserialize an array when including files in a multipart data form.
				var data = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Models.Attachments.UpdateAttachmentViewModel>>(documents);

				if (grantApplication.Attestation == null)
				{
					grantApplication.Attestation = new Attestation
					{
						State = AttestationState.Incomplete,
						DateAdded = AppDateTime.UtcNow
					};

					_grantApplicationService.UpdateAttestation(grantApplication);
				}

				var claimedCosts = grantApplication.GetTotalForAllAssessedCostsInCategory(ServiceCategoryEnum.ParticipantFinancialSupports);

				var unusedFunds = claimedCosts - model.AllocatedCosts;
				if (unusedFunds < 0)
					unusedFunds = 0;

				grantApplication.Attestation.ClaimedCosts = claimedCosts;
				grantApplication.Attestation.AllocatedCosts = model.AllocatedCosts;
				grantApplication.Attestation.UnusedFunds = unusedFunds;

				if (model.AttestationNotApplicable.HasValue)
					grantApplication.Attestation.AttestationNotApplicable = model.AttestationNotApplicable.Value;

				if (model.CompleteAttestation.HasValue && model.CompleteAttestation.Value)
					grantApplication.Attestation.State = AttestationState.Complete;

				_grantApplicationService.UpdateAttestation(grantApplication);

				foreach (var attachment in data)
				{
					if (attachment.Delete) // Delete
					{
						var existing = _attachmentService.Get(attachment.Id);
						existing.RowVersion = Convert.FromBase64String(attachment.RowVersion);
						grantApplication.Attestation.Documents.Remove(existing);
						_noteService.AddSystemNote(grantApplication, $"Attestation document \'{existing.FileName}\' deleted.");
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
						grantApplication.Attestation.Documents.Add(file);
						_noteService.AddSystemNote(grantApplication, $"Attestation document \'{file.FileName}\' uploaded.");
						_attachmentService.Add(file, true);
					}
					else if (files.Length > attachment.Index.Value && files[attachment.Index.Value] != null && attachment.Id != 0) // Update with file
					{
						var file = files[attachment.Index.Value].UploadFile(attachment.Description, attachment.FileName);
						var existing = _attachmentService.Get(attachment.Id);
						existing.RowVersion = Convert.FromBase64String(attachment.RowVersion);
						_noteService.AddSystemNote(grantApplication, $"Attestation document \'{existing.FileName}\' replaced with \'{file.FileName}\'.");
						attachment.MapToEntity(existing);
						existing.AttachmentData = file.AttachmentData;
						_attachmentService.Update(existing, true);
					}
				}

				model = new AttestationViewModel(grantApplication);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Return the attachment data for the specified Attestation document.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="attachmentId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/Attestation/Document/{grantApplicationId}/{attachmentId}/")]
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
		[Route("Application/Attestation/Download/{grantApplicationId}/{attachmentId}")]
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
