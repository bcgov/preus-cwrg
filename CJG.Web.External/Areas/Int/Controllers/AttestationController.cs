using System;
using System.Web.Mvc;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Int.Models;
using CJG.Web.External.Areas.Int.Models.Attestation;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;

namespace CJG.Web.External.Areas.Int.Controllers
{
    [RouteArea("Int")]
	[Authorize(Roles = "Assessor, System Administrator, Director, Financial Clerk")]
	public class AttestationController : BaseController
	{
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly ISuccessStoryService _successStoryService;

		public AttestationController(
			IControllerService controllerService,
			IGrantApplicationService grantApplicationService,
			ISuccessStoryService successStoryService
		   ) : base(controllerService.Logger)
		{
			_grantApplicationService = grantApplicationService;
			_successStoryService = successStoryService;
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
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/Attestation/")]
		public JsonResult SaveAttestation(AttestationViewModel model)
		{
			try
			{
				var grantApplication = _grantApplicationService.Get(model.Id);

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

				model = new AttestationViewModel(grantApplication);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}
	}
}
