using System;
using System.Linq;
using System.Web.Mvc;
using CJG.Application.Services;
using CJG.Application.Services.Exceptions;
using CJG.Core.Entities;
using CJG.Core.Interfaces;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;

namespace CJG.Web.External.Areas.Ext.Controllers
{
    /// <summary>
    /// ServiceController class, provides API to manage eligible costs.
    /// </summary>
    [RouteArea("Ext")]
	[ExternalFilter]
	public class ServiceController : BaseController
	{
		#region Variables
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IEligibleExpenseTypeService _eligibleExpenseTypeService;
		private readonly IEligibleCostBreakdownService _eligibleCostBreakdownService;
		#endregion

		#region Constructors
		/// <summary>
		/// Creates a new instance of a ServiceController object.
		/// </summary>
		/// <param name="controllerService"></param>
		/// <param name="grantApplicationService"></param>
		/// <param name="eligibleExpenseTypeService"></param>
		/// <param name="eligibleCostBreakdownService"></param>
		public ServiceController(
			IControllerService controllerService,
			IGrantApplicationService grantApplicationService,
			IEligibleExpenseTypeService eligibleExpenseTypeService,
			IEligibleCostBreakdownService eligibleCostBreakdownService) : base(controllerService.Logger)
		{
			_grantApplicationService = grantApplicationService;
			_eligibleExpenseTypeService = eligibleExpenseTypeService;
			_eligibleCostBreakdownService = eligibleCostBreakdownService;
		}
		#endregion

		#region Endpoints
		[HttpGet]
		[Route("Application/{grantApplicationId:int}/Employment/Services/Supports/View/{eligibleExpenseTypeId:int}/{eligibleCostId?}")]
		public ActionResult EmploymentServicesAndSupportsView(int grantApplicationId, int eligibleExpenseTypeId, int? eligibleCostId)
		{
			var grantApplication = _grantApplicationService.Get(grantApplicationId);

			if (!User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditApplication))
				throw new NotAuthorizedException("User does not have permission to edit application");

			_eligibleExpenseTypeService.Get(eligibleExpenseTypeId);

			ViewBag.GrantApplicationId = grantApplicationId;
			ViewBag.EligibleExpenseTypeId = eligibleExpenseTypeId;
			ViewBag.EligibleCostId = eligibleCostId;
			return View();
		}

		/// <summary>
		/// Get the data for the employment services and supports view.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="eligibleExpenseTypeId"></param>
		/// <param name="eligibleCostId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/{grantApplicationId:int}/Employment/Services/Supports/{eligibleExpenseTypeId:int}/{eligibleCostId?}")]
		public JsonResult GetEmploymentServicesAndSupports(int grantApplicationId, int eligibleExpenseTypeId, int? eligibleCostId)
		{
			var model = new Models.Services.EmploymentServiceViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);

				if (!User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditApplication))
					throw new NotAuthorizedException("User does not have permission to edit application");

				var eligibleExpenseType = _eligibleExpenseTypeService.Get(eligibleExpenseTypeId);
				var eligibleCost = eligibleCostId.HasValue ? grantApplication.TrainingCost.EligibleCosts.FirstOrDefault(t => t.Id == eligibleCostId) : null;

				model = new Models.Services.EmploymentServiceViewModel(eligibleExpenseType, eligibleCost, grantApplication);
			}
			catch(Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Update the employment services and supports.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[Route("Application/Employment/Services/Supports")]
		public ActionResult UpdateEmploymentServicesAndSupports(Models.Services.EmploymentServiceViewModel model)
		{
			try
			{
				var serviceSelected = model.EligibleExpenseType.EligibleExpenseBreakdowns.FirstOrDefault(o => o.Selected) != null;
				if (serviceSelected && model.EstimatedCost == 0)
				{
					ModelState.AddModelError("EstimatedCost", "Services were selected but no service cost was entered.");
				}
				else if (!serviceSelected && model.EstimatedCost > 0)
				{
					ModelState.AddModelError("EligibleExpenseBreakdowns", "A service cost was entered but no services were selected.");
				}

				if (ModelState.IsValid)
				{
					var grantApplication = _grantApplicationService.Get(model.GrantApplicationId);
					var eligibleCost = grantApplication.TrainingCost.EligibleCosts.Where(t => t.Id == model.EligibleCostId).FirstOrDefault();
					var eligibleExpenseType = _eligibleExpenseTypeService.Get(model.EligibleExpenseType.Id);

					if (eligibleCost == null)
					{
						eligibleCost = new EligibleCost(grantApplication, eligibleExpenseType, model.EstimatedCost ?? 0m, grantApplication.TrainingCost.EstimatedParticipants);
						grantApplication.TrainingCost.EligibleCosts.Add(eligibleCost);
					}
					else
					{
						eligibleCost.RowVersion = Convert.FromBase64String(model.RowVersion);
					}

					if ((eligibleCost.Id == 0 || eligibleCost.EstimatedCost != model.EstimatedCost) && grantApplication.TrainingCost.TrainingCostState == TrainingCostStates.Complete)
					{
						grantApplication.TrainingCost.TrainingCostState = TrainingCostStates.Incomplete;
					}

					eligibleCost.EstimatedCost = model.EstimatedCost ?? 0m;

					var idsToRemove = model.EligibleExpenseType.EligibleExpenseBreakdowns.Where(t => t.Selected == false);
					var idsToAddOrModify = model.EligibleExpenseType.EligibleExpenseBreakdowns.Where(t => t.Selected);

					idsToRemove.ForEach(o =>
					{
						var breakdown = eligibleCost.Breakdowns.FirstOrDefault(t => t.EligibleExpenseBreakdownId == o.Id);
						if (breakdown != null)
						{
							_eligibleCostBreakdownService.Remove(breakdown);
						}
					});

					idsToAddOrModify.ForEach(item =>
					{
						var breakdown = eligibleCost.Breakdowns.FirstOrDefault(t => t.EligibleExpenseBreakdownId == item.Id);

						if (breakdown == null)
						{
							breakdown = new EligibleCostBreakdown
							{
								EstimatedCost = 0m,
								EligibleCostId = eligibleCost.Id,
								EligibleCost = eligibleCost,
								EligibleExpenseBreakdownId = item.Id
							};

							eligibleCost.Breakdowns.Add(breakdown);
						}

						breakdown.EstimatedCost = item.EstimatedCost;
						breakdown.CustomCostTitle = item.CustomCostTitle;

						//if (eligibleCost.Breakdowns.All(o => o.EligibleExpenseBreakdownId != item.Id))
						//{
						//	itemsToAdd.Add(new EligibleCostBreakdown
						//	{
						//		EligibleCostId = eligibleCost.Id,
						//		EligibleCost = eligibleCost,
						//		EstimatedCost = item.EstimatedCost,
						//		EligibleExpenseBreakdownId = item.Id,
						//	});
						//}
					});

					//itemsToAdd.ForEach(t => eligibleCost.Breakdowns.Add(t));

					eligibleCost.CalculateEstimatedReimbursement();
					grantApplication.TrainingCost.RecalculateEstimatedCosts();

					_grantApplicationService.Update(grantApplication);

					model.RedirectURL = Url.Action(nameof(ApplicationController.ApplicationOverviewView), new { controller = "Application", grantApplicationId = grantApplication.Id });
					model.RowVersion = Convert.ToBase64String(eligibleCost.RowVersion);
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
		#endregion
	}
}
