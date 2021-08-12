using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CJG.Application.Business.Models;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Identity;
using CJG.Web.External.Areas.Int.Models.AccountsReceivables;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;

namespace CJG.Web.External.Areas.Int.Controllers
{
    /// <summary>
    /// <paramtyperef name="ApplicationAccountsReceivableController"/> class, provides endpoints to manage the application AR values.
    /// </summary>
    [Authorize(Roles = "Assessor, Director, Financial Clerk, System Administrator")]
	[RouteArea("Int")]
	public class ApplicationAccountsReceivableController : BaseController
	{
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IAccountsReceivableService _accountsReceivableService;

		public ApplicationAccountsReceivableController(
			IControllerService controllerService,
			IGrantApplicationService grantApplicationService,
			IAccountsReceivableService accountsReceivableService
		   ) : base(controllerService.Logger)
		{
			_grantApplicationService = grantApplicationService;
			_accountsReceivableService = accountsReceivableService;
		}
		
		[HttpGet]
		[AuthorizeAction(Privilege.AM2, Privilege.AM4, Privilege.AM5)]
		[Route("Application/AccountsReceivable/{grantApplicationId}")]
		public ActionResult GetAccountsReceivables(int grantApplicationId)
		{
			var model = new ApplicationAccountsReceivableModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				var accountsReceivable = _accountsReceivableService.GetByApplication(grantApplication);
				model = new ApplicationAccountsReceivableModel(grantApplication, accountsReceivable);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

        [HttpPut]
        [ValidateRequestHeader]
        [AuthorizeAction(Privilege.AM2, Privilege.AM4, Privilege.AM5)]
        [Route("Application/AccountsReceivable")]
        public ActionResult UpdateAccountsReceivable(ApplicationAccountsReceivableModel model)
        {
            try
            {

				var grantApplication = _grantApplicationService.Get(model.Id);

				if (ModelState.IsValid)
				{

					var updateModel = new ApplicationAccountsReceivableUpdateModel
					{
						GrantApplicationId = grantApplication.Id,
						PaidDate = model.AccountsReceivablePaidDate.Value,
						ReceivablesByServiceCategory = model.Records
							.Select(r => new KeyValuePair<int, decimal>(r.ServiceCategoryId, r.Overpayment))
							.ToList()
					};

					_accountsReceivableService.UpdateAccountsReceivables(updateModel);

					var accountsReceivable = _accountsReceivableService.GetByApplication(grantApplication);
					model = new ApplicationAccountsReceivableModel(grantApplication, accountsReceivable);
				}
				else
				{
					HandleModelStateValidation(model, ModelState.GetErrorMessages("<br />"));
				}
			}
			catch (Exception ex)
            {
                HandleAngularException(ex, model);
            }
            return Json(model);
        }
    }
}
