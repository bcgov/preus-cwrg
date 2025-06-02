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
	/// AttestationController class, provides a controller endpoints for managing external grant application attestations.
	/// </summary>
	[RouteArea("Ext")]
	[ExternalFilter]
	public class AttestationController : BaseController
	{
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IAttachmentService _attachmentService;

		/// <summary>
		/// Creates a new instance of a AttestationController object.
		/// </summary>
		/// <param name="grantApplicationService"></param>
		/// <param name="attachmentService"></param>
		/// <param name="controllerService"></param>
		public AttestationController(
			IGrantApplicationService grantApplicationService,
			IAttachmentService attachmentService,
			IControllerService controllerService)
			: base(controllerService.Logger)
		{
			_grantApplicationService = grantApplicationService;
			_attachmentService = attachmentService;
		}

		/// <summary>
		/// Display the Attestation View.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[Route("Application/Attestation/View/{grantApplicationId}")]
		public ActionResult AttestationView(int grantApplicationId)
		{
			var grantApplication = _grantApplicationService.Get(grantApplicationId);
			ViewBag.GrantApplicationId = grantApplicationId;
			return View();
		}

		/// <summary>
		/// Get the data for the Attestation page.
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
		/// Update the attestation for the specified grant application.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="allocatedCosts"></param>
		/// <param name="attestationNotApplicable"></param>
		/// <param name="completeAttestation"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/Attestation/{grantApplicationId}")]
		public JsonResult SaveAttestation(int grantApplicationId, decimal allocatedCosts, bool? attestationNotApplicable, bool? completeAttestation, HttpPostedFileBase[] files, string attachments)
		{
			var model = new AttestationViewModel
			{
				AllocatedCosts = allocatedCosts,
				AttestationNotApplicable = attestationNotApplicable,
				CompleteAttestation = completeAttestation
			};

			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);

				// Deserialize model.  This is required because it isn't easy to deserialize an array when including files in a multipart data form.
				var data = JsonConvert.DeserializeObject<IEnumerable<UpdateAttachmentViewModel>>(attachments);

				if (grantApplication.Attestation == null)
				{
					grantApplication.Attestation = new Attestation
					{
						State = AttestationState.Incomplete,
						DateAdded = AppDateTime.UtcNow
					};

					_grantApplicationService.UpdateAttestation(grantApplication);
				}

				foreach (var attachment in data)
				{
					if (attachment.Delete)
					{
						var existing = _attachmentService.Get(attachment.Id);
						existing.RowVersion = Convert.FromBase64String(attachment.RowVersion);
						grantApplication.Attestation.Documents.Remove(existing);

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

				var claimedCosts = grantApplication.GetTotalForAllAssessedCostsInCategory(ServiceCategoryEnum.ParticipantFinancialSupports);

				var attestationAllocatedCosts = claimedCosts - model.AllocatedCosts;
				if (attestationAllocatedCosts < 0)
					attestationAllocatedCosts = 0;

				grantApplication.Attestation.ClaimedCosts = claimedCosts;
				grantApplication.Attestation.AllocatedCosts = model.AllocatedCosts;
				grantApplication.Attestation.UnusedFunds = attestationAllocatedCosts;

				if (model.AttestationNotApplicable.HasValue)
					grantApplication.Attestation.AttestationNotApplicable = model.AttestationNotApplicable.Value;

				var sendCompletionNotification = false;

				if (model.CompleteAttestation.HasValue && model.CompleteAttestation.Value)
				{
					grantApplication.Attestation.State = AttestationState.Complete;
					sendCompletionNotification = true;
				}

				_grantApplicationService.UpdateAttestation(grantApplication, sendCompletionNotification);

				model = new AttestationViewModel(grantApplication);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}


		/// <summary>
		/// Downloads specified Attestation attachment
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
