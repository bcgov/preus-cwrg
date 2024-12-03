using System;
using System.Web.Mvc;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Ext.Models.Reporting;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;

namespace CJG.Web.External.Areas.Ext.Controllers
{
    /// <summary>
    /// ProofOfPaymentController class, provides a controller endpoints for managing external grant application proof of payment attachments.
    /// </summary>
    [RouteArea("Ext")]
	[ExternalFilter]
	public class AttestationController : BaseController
	{
		private readonly IGrantApplicationService _grantApplicationService;

		/// <summary>
		/// Creates a new instance of a ProofOfPaymentController object.
		/// </summary>
		/// <param name="grantApplicationService"></param>
		/// <param name="controllerService"></param>
		public AttestationController(
			IGrantApplicationService grantApplicationService,
			IControllerService controllerService)
			: base(controllerService.Logger)
		{
			_grantApplicationService = grantApplicationService;
		}

		/// <summary>
		/// Display the Proof of Payment View.
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
		/// Get the data for the ProofOfPaymentView page.
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
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/Attestation/{grantApplicationId}")]
		public JsonResult SaveAttestation(int grantApplicationId, AttestationViewModel model)
		{
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);

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
	}
}
