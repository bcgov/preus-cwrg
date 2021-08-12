using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using CJG.Application.Services;
using CJG.Application.Services.Exceptions;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Int.Models.Notes;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Controllers
{
    [RouteArea("Int")]
	public class NoteController : BaseController
	{
		private readonly IStaticDataService _staticDataService;
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IAttachmentService _attachmentService;
		private readonly INoteService _noteService;

		public NoteController(
			IControllerService controllerService,
			IGrantApplicationService grantApplicationService,
			IAttachmentService attachmentService,
			INoteService noteService

		   ) : base(controllerService.Logger)
		{
			_staticDataService = controllerService.StaticDataService;
			_grantApplicationService = grantApplicationService;
			_attachmentService = attachmentService;
			_noteService = noteService;
		}

		/// <summary>
		/// Get the notes for the application details view.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/Notes/{grantApplicationId}")]
		public JsonResult GetNotes(int grantApplicationId)
		{
			var model = new NoteListViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				model = new NoteListViewModel(grantApplication, User);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Return a view for the specified note.
		/// </summary>
		/// <param name="noteId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/Note/View/{noteId?}")]
		public PartialViewResult NoteView(int? noteId)
		{
			return PartialView();
		}

		/// <summary>
		/// Get the note for the specified id.
		/// </summary>
		/// <param name="noteId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/Note/{noteId:int}")]
		public JsonResult GetNote(int noteId)
		{
			var model = new NoteViewModel();
			try
			{
				var note = _noteService.Get(noteId);
				var grantApplication = _grantApplicationService.Get(note.GrantApplicationId);

				model = new NoteViewModel(note, User);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Get an array of note types.
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/Note/Types")]
		public JsonResult GetNoteTypes()
		{
			try
			{
				var noteTypes = _staticDataService.GetNoteTypes().Select(x => new {x.Id, x.Description, x.IsSystem}).ToArray();
				return Json(noteTypes, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				var model = new BaseViewModel();
				HandleAngularException(ex, model);
				return Json(model, JsonRequestBehavior.AllowGet);
			}
		}

		/// <summary>
		/// Get note users
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/Note/Users/{grantApplicationId}")]
		public JsonResult GetNoteUsers(int grantApplicationId)
		{
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				var model = grantApplication.Notes
					.Select(x => new {
						CreatorId = x.CreatorId ?? 0,
						CreatorName = x.CreatorId == null ? "Applicant" : $"{x.Creator.FirstName} {x.Creator.LastName}"
					}).Distinct();
				return Json(model, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				var model = new BaseViewModel();
				HandleAngularException(ex, model);
				return Json(model, JsonRequestBehavior.AllowGet);
			}
		}

		/// <summary>
		/// Add a new note to the datasource.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/Note")]
		public JsonResult AddNote(NoteViewModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var grantApplication = _grantApplicationService.Get(model.GrantApplicationId);
					Attachment attachment = null;

					if (model.AttachmentId.HasValue)
					{
						attachment = _attachmentService.Get(model.AttachmentId.Value);
						attachment.Description = model.AttachmentDescription;
					}

					var note = _noteService.CreateNote(grantApplication, model.NoteTypeId.Value, model.Content, attachment);
					_noteService.Add(note);

					model = new NoteViewModel(note, User);
				}
				else
				{
					HandleModelStateValidation(model);
				}
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			// Replace the new lines when reloading the note after edit
			model.Content = model.Content.Replace(Environment.NewLine, "<br />");

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Update the note in the datasource.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/Note")]
		public JsonResult UpdateNote(NoteViewModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var note = _noteService.Get(model.Id);

					note.RowVersion = Convert.FromBase64String(model.RowVersion);
					note.NoteType = _staticDataService.GetNoteType(model.NoteTypeId.Value);
					note.Content = model.Content;

					// If the attachment has been updated, then create a new version.
					if (note.AttachmentId != model.AttachmentId)
					{
						if (note.Attachment != null)
							_attachmentService.Remove(note.Attachment);

						if (model.AttachmentId.HasValue)
						{
							var attachment = _attachmentService.Get(model.AttachmentId.Value);
							attachment.Description = model.AttachmentDescription;

							note.Attachment = attachment;
						}
					}
					else if (note.AttachmentId != null && (model.AttachmentId == null || model.AttachmentId == 0))
					{
						// If the attachment was removed, remove it from the note in the datasource.
						foreach (var version in note.Attachment.Versions)
						{
							_attachmentService.Remove(version);
						}

						_attachmentService.Remove(note.Attachment);

						note.Attachment = null;
						note.AttachmentId = null;
					}
					_noteService.Update(note);

					model = new NoteViewModel(note, User);
				}
				else
				{
					HandleModelStateValidation(model);
				}
			}

			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			// Replace the new lines when reloading the note after edit
			model.Content = model.Content.Replace(Environment.NewLine, "<br />");

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Delete the specified note from the datasource.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/Note/Delete")]
		public JsonResult DeleteNote(NoteViewModel model)
		{
			try
			{
				var note = _noteService.Get(model.Id);
				var grantApplication = _grantApplicationService.Get(model.GrantApplicationId);

				if (note.CreatorId != User.GetUserId() || note.NoteType.IsSystem)
					throw new NotAuthorizedException($"User does not have permission to delete this note in the application '{model.GrantApplicationId}'.");

				note.RowVersion = Convert.FromBase64String(model.RowVersion);

				if (note.AttachmentId.HasValue)
				{
					_attachmentService.Delete(note.Attachment);
				}
				_noteService.Remove(note);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Download the specified attachment.
		/// </summary>
		/// <param name="noteId"></param>
		/// <param name="attachmentId"></param>
		/// <returns></returns>
		[HttpGet]
		[PreventSpam]
		[Route("Application/Note/{noteId}/Download/{attachmentId}")]
		public ActionResult DownloadAttachment(int noteId, int attachmentId)
		{
			var model = new BaseViewModel();
			try
			{
				Note note = null;
				if (noteId > 0)
					note = _noteService.Get(noteId);

				var attachment = _attachmentService.Get(attachmentId);

				var downloadName = attachment.FileName.EndsWith(attachment.FileExtension)
					? attachment.FileName.Trim()
					: $"{attachment.FileName.Trim()}{attachment.FileExtension.Trim()}";

				return File(attachment.AttachmentData, MediaTypeNames.Application.Octet, downloadName);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[Route("Application/Note/DropzoneUpload")]
		public JsonResult DropzoneUpload(IEnumerable<HttpPostedFileBase> files)
		{
			var file = files.FirstOrDefault();
			if (file == null)
				return Json("NoFile", JsonRequestBehavior.AllowGet);

			var attachment = file.UploadPostedNoteFile("TemporaryUpload").Item3;
			_attachmentService.Add(attachment, true);

			var uploadedAttachmentId = attachment.Id;
			var model = new
			{
				AttachmentId = uploadedAttachmentId,
				FileName = attachment.FileName
			};

			return Json(model, JsonRequestBehavior.AllowGet);
		}
	}
}