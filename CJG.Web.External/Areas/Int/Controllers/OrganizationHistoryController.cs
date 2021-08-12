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
using CJG.Web.External.Areas.Int.Models.Organizations;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Controllers
{
    /// <summary>
    /// <typeparamref name="OrganizationHistoryController"/> class, provides endpoints to manage organization History.
    /// </summary>
    [RouteArea("Int")]
	[RoutePrefix("Organization")]
	[AuthorizeAction(Privilege.AM1, Privilege.AM2, Privilege.AM3, Privilege.AM4, Privilege.AM5)]
	public class OrganizationHistoryController : BaseController
	{
		private readonly IOrganizationService _organizationService;
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IUserService _userService;
		private readonly IAttachmentService _attachmentService;
		private readonly IGrantProgramService _grantProgramService;

		/// <summary>
		/// Creates a new instance of a <typeparamref name="OrganizationHistoryController"/> object.
		/// </summary>
		/// <param name="controllerService"></param>
		/// <param name="organizationService"></param>
		/// <param name="grantApplicationService"></param>
		/// <param name="grantProgramService"></param>
		/// <param name="userService"></param>
		/// <param name="attachmentService"></param>
		public OrganizationHistoryController(
			IControllerService controllerService,
			IOrganizationService organizationService,
			IGrantApplicationService grantApplicationService,
			IGrantProgramService grantProgramService,
			IUserService userService,
			IAttachmentService attachmentService) : base(controllerService.Logger)
		{
			_organizationService = organizationService;
			_grantApplicationService = grantApplicationService;
			_grantProgramService = grantProgramService;
			_userService = userService;
			_attachmentService = attachmentService;
		}

		/// <summary>
		/// Returns the organization grant file history view.
		/// </summary>
		/// <param name="organizationId"></param>
		/// <returns></returns>
		[HttpGet, Route("History/View/{organizationId}")]
		public ActionResult OrganizationGrantFileHistoryView(int? organizationId)
		{
			ViewBag.OrganizationId = organizationId;
			//logic to handle return click - either return to organization page or application page
			ViewBag.Path = new List<string>();

			var referrer = Request.UrlReferrer;
			var path = referrer.LocalPath;
			var listOfPath = path.Split('/').ToList();

			foreach (var item in listOfPath)
				ViewBag.Path.Add(item);

			return View();
		}

		/// <summary>
		/// Get the organization grant file history view data.
		/// </summary>
		/// <param name="organizationId"></param>
		/// <returns></returns>
		[HttpGet, Route("History/{organizationId}")]
		public JsonResult GetOrganizationGrantFileHistory(int organizationId)
		{
			var model = new OrganizationGrantFileHistoryViewModel();
			try
			{
				var organization = _organizationService.Get(organizationId);
				model = new OrganizationGrantFileHistoryViewModel(organization, _organizationService)
				{
					AllowDeleteOrganization = User.IsInRole("Director") || User.IsInRole("Assessor") || User.IsInRole("System Administrator"),
					UrlReferrer = Request.UrlReferrer?.AbsolutePath ??
					   new UrlHelper(this.ControllerContext.RequestContext).Action(nameof(OrganizationController.OrganizationsView), nameof(OrganizationController).Replace("Controller", ""))
				};
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Route("HistoryYTD/{organizationId}/{grantProgramId}")]
		public JsonResult GetOrganizationGrantHistoryYTD(int organizationId, int grantProgramId)
		{
			var model = new OrganizationGrantFileYTDViewModel();
			try
			{
				if (grantProgramId == 0)
					grantProgramId = _grantProgramService.GetDefaultGrantProgramId();

				var result = _organizationService.GetOrganizationYTD(organizationId, grantProgramId);
				model.TotalRequested = result.TotalRequested;
				model.TotalApproved = result.TotalApproved;
				model.TotalPaid = result.TotalPaid;
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Route("History/Program/Types")]
		public JsonResult GetProgramTypes()
		{
			IEnumerable<KeyValuePair<int, string>> results = new KeyValuePair<int, string>[0];
			try
			{
				var grantPrograms = _grantProgramService.GetAll();
				results = grantPrograms.Select(p => new KeyValuePair<int, string>(p.Id, p.Name)).ToArray();
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}
			return Json(results, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Get grant file histories for organization.
		/// </summary>
		/// <param name="organizationId"></param>
		/// <param name="page"></param>
		/// <param name="quantity"></param>
		/// <param name="grantProgramId"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		[HttpGet, Route("History/Search/{organizationId}/{page}/{quantity}")]
		[ValidateRequestHeader]
		[AuthorizeAction(Privilege.IA1)]
		public JsonResult GetOrganizationGrantFileHistory(int organizationId,int page, int quantity, int grantProgramId, string search)
		{
			var model = new BaseViewModel();
			try
			{
				if (grantProgramId == 0)
					grantProgramId = _grantProgramService.GetDefaultGrantProgramId();

				var grantApplications = _grantApplicationService.GetGrantApplicationsForOrg(organizationId, page, quantity, grantProgramId, search);
				var result = new
				{
					RecordsFiltered = grantApplications.Items.Count(),
					RecordsTotal = grantApplications.Total,
					Data = grantApplications.Items.Select(o => new OrganizationGrantFileHistoryDataTableModel(o, _userService)).ToArray()
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
		/// Update the organization notes/risk-flag.
		/// </summary>
		/// <param name="organizationId"></param>
		/// <param name="notesText"></param>
		/// <param name="riskFlag"></param>
		/// <param name="rowVersion"></param>
		/// <returns></returns>
		[HttpPut, Route("History/Change/{organizationId}")]
		[AuthorizeAction(Privilege.TP1, Privilege.TP2)]
		public JsonResult UpdateNote(int organizationId, string notesText, bool riskFlag, string rowVersion)
		{
			var model = new OrganizationGrantFileHistoryViewModel();
			try
			{
				var organization = _organizationService.Get(organizationId);

				organization.RowVersion = Convert.FromBase64String(rowVersion.Replace(" ", "+"));
				organization.Notes = notesText;
				organization.RiskFlag = riskFlag;

				_organizationService.UpdateOrganization(organization);

				model.RowVersion = Convert.ToBase64String(organization.RowVersion);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Return all of the document data for the specified organization.
		/// </summary>
		/// <param name="organizationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("History/Documents/{organizationId}")]
		public JsonResult GetDocuments(int organizationId)
		{
			var model = new OrganizationDocumentsViewModel();
			try
			{
				var organization = _organizationService.Get(organizationId);
				model = new OrganizationDocumentsViewModel(organization);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Update the documents (delete/update/create) for the specified organization.
		/// </summary>
		/// <param name="organizationId"></param>
		/// <param name="files">Encoded file data</param>
		/// <param name="documents">Metadata about the documents</param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("History/Documents/")]
		public JsonResult SaveDocuments(int organizationId, HttpPostedFileBase[] files, string documents)
		{
			var model = new OrganizationDocumentsViewModel();
			try
			{
				var organization = _organizationService.Get(organizationId);

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
						organization.Documents.Remove(existing);
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
						organization.Documents.Add(file);
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

				model = new OrganizationDocumentsViewModel(organization);
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
		/// <param name="organizationId"></param>
		/// <param name="attachmentId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("History/Document/{organizationId}/{attachmentId}/")]
		public JsonResult GetAttachment(int organizationId, int attachmentId)
		{
			var model = new OrganizationDocumentViewModel();
			try
			{
				var organization = _organizationService.Get(organizationId);
				var attachment = attachmentId > 0 ? _attachmentService.Get(attachmentId) : new Attachment();
				model = new OrganizationDocumentViewModel(organization, attachment);
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
		/// <param name="organizationId"></param>
		/// <param name="attachmentId"></param>
		/// <returns></returns>
		[HttpGet]
		[PreventSpam]
		[Route("History/Document/Download/{organizationId}/{attachmentId}")]
		public ActionResult DownloadAttachment(int organizationId, int attachmentId)
		{
			var model = new BaseViewModel();
			try
			{
				var organization = _organizationService.Get(organizationId);
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
