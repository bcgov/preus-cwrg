using System;
using System.Linq;
using System.Web.Mvc;
using CJG.Application.Services;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Identity;
using CJG.Web.External.Areas.Int.Models;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;

namespace CJG.Web.External.Areas.Int.Controllers
{
    /// <summary>
    /// <paramtyperef name="ParticipantController"/> class. Provides endpoints to manage the Participant partial form.
    /// </summary>
    [RouteArea("Int")]
	[RoutePrefix("Application")]
	public class ParticipantController : BaseController
	{
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IUserService _userService;
		private readonly IParticipantService _participantService;
		private readonly INationalOccupationalClassificationService _nationalOccupationalClassificationService;
		private readonly ISurveyService _surveyService;
		private readonly IAttachmentService _attachmentService;

		public ParticipantController(
			IControllerService controllerService,
			IGrantApplicationService grantApplicationService,
			IUserService userService,
			IParticipantService participantService,
			INationalOccupationalClassificationService nationalOccupationalClassificationService,
			ISurveyService surveyService,
			IAttachmentService attachmentService): base(controllerService.Logger)
		{
			_grantApplicationService = grantApplicationService;
			_userService = userService;
			_participantService = participantService;
			_nationalOccupationalClassificationService = nationalOccupationalClassificationService;
			_surveyService = surveyService;
			_attachmentService = attachmentService;
		}

		/// <summary>
		/// Get participants data for the specified Grant Application.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Participants/{grantApplicationId}")]
		public JsonResult GetParticipants(int grantApplicationId)
		{
			var model = new ParticipantListViewModel();

			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				model = new ParticipantListViewModel(grantApplication, _participantService);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Get the participant consent attachment.
		/// </summary>
		/// <param name="participantId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Participant/Consent/{participantId}")]
		public ActionResult DownloadAttachment(int participantId)
		{
			var participantForm = _participantService.Get(participantId);

			if (participantForm.ParticipantConsentAttachment == null)
				return null;

			var attachment = _attachmentService.Get(participantForm.ParticipantConsentAttachment.Id);
			return File(attachment.AttachmentData, System.Net.Mime.MediaTypeNames.Application.Octet, $"{attachment.FileName}{attachment.FileExtension}");
		}

		/// <summary>
		/// Returns a view to display the participant information.
		/// </summary>
		/// <param name="participantId"></param>
		/// <returns></returns>
		[HttpGet]
		[AuthorizeAction(Privilege.IA2)]
		[Route("Participant/Info/View/{participantId}")]
		public ActionResult ParticipantInformationView(int participantId)
		{
			var model = new ParticipantInfoViewModel();
			var participantForm = _participantService.Get(participantId);

			if (participantForm != null)
				model = new ParticipantInfoViewModel(participantForm, _nationalOccupationalClassificationService, _userService, _surveyService);

#if Training || Support
			 model.ContactInfo.SIN = string.Concat(model.ContactInfo.SIN?.Substring(0, 1), "** *** ***");
#endif

			// Mask the social insurance number for anyone without privilege IA3
			if (!User.HasPrivilege(Privilege.IA3)) {
				model.ContactInfo.SIN = string.Concat(model.ContactInfo.SIN?.Substring(0, 1), "** *** ***");
			}

			return View(model);
		}

		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Participants")]
		public JsonResult ApproveDenyParticipants(ParticipantListViewModel model)
        {
			//set the Approved property
			try
			{
				if (ModelState.IsValid)
				{
					int grantApplicationId = model.Id;
					var participantsApproved = model.ParticipantInfo.Where(w=>w.ParticipantId != null).ToDictionary(d=> d.ParticipantId, d => d.Approved);
					
					_participantService.ApproveDenyParticipants(grantApplicationId, participantsApproved);

					var grantApplication = _grantApplicationService.Get(grantApplicationId);
				
					model = new ParticipantListViewModel(grantApplication, _participantService);
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

			return Json(model, JsonRequestBehavior.AllowGet);
		}
	}
}