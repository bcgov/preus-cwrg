using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Identity;
using CJG.Web.External.Areas.Int.Models.ProgramInitiatives;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;

namespace CJG.Web.External.Areas.Int.Controllers
{
    /// <summary>
    /// <paramtyperef name="ProgramInitiativesController"/> class, provides endpoints to manage program initiatives
    /// </summary>
    [AuthorizeAction(Privilege.GM1, Privilege.SM)]
	[RouteArea("Int")]
	[RoutePrefix("Admin/ProgramInitiatives")]
	public class ProgramInitiativesController : BaseController
	{
		private readonly IProgramInitiativeService _programInitiativeService;

		/// <summary>
		/// Creates a new instance of a <paramtyperef name="ProgramInitiativesController"/> object.
		/// </summary>
		/// <param name="controllerService"></param>
		/// <param name="programInitiativeService"></param>
		public ProgramInitiativesController(
			IControllerService controllerService,
			IProgramInitiativeService programInitiativeService) : base(controllerService.Logger)
		{
			_programInitiativeService = programInitiativeService;
		}

		[HttpGet]
		[Route("View")]
		public ActionResult ProgramInitiativesView()
		{
			return View();
		}

		[HttpGet]
		[Route("Initiatives")]
		public JsonResult GetInitiatives()
		{
			var model = new ProgramInitiativesModel();
			try
			{
				model.Initiatives = _programInitiativeService
					.GetAll()
					.Select(t => new ProgramInitiativeModel(t))
					.ToList();

				foreach (var initiative in model.Initiatives)
					initiative.IsInUse = _programInitiativeService.IsInUse(initiative.Id);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpPut]
		[ValidateRequestHeader]
		[PreventSpam]
		[Route("Initiatives")]
		public JsonResult UpdateInitiatives(ProgramInitiativesModel model)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					HandleModelStateValidation(model);
				}
				else
				{
					var initiativeList = _programInitiativeService.GetAll().ToList();
					var postedItemIds = model.Initiatives
						.Where(i => i.Id > 0)
						.Select(i => i.Id)
						.ToList();

					var itemsToRemove = initiativeList
						.Where(i => !postedItemIds.Contains(i.Id))
						.ToList();

                    foreach (var question in model.Initiatives)
                    {
                        ProgramInitiative programInitiative;
                        if (question.Id != 0)
                        {
                            programInitiative = initiativeList.First(x => x.Id == question.Id);

                            programInitiative.Name = question.Name ?? string.Empty;
                            programInitiative.Code = question.Code ?? string.Empty;
                            programInitiative.IsActive = true;
                            programInitiative.RowSequence = question.RowSequence;
                            programInitiative.RowVersion = Convert.FromBase64String(question.RowVersion);
                        }
                        else
                        {
                            programInitiative = new ProgramInitiative();
                            programInitiative.Name = question.Name;
                            programInitiative.Code = question.Code;
                            programInitiative.IsActive = true;
                            programInitiative.RowSequence = question.RowSequence;

                            initiativeList.Add(programInitiative);
                        }
                    }

                    foreach (var toRemove in itemsToRemove)
	                    initiativeList.Remove(toRemove);

                    _programInitiativeService.UpdateInitiatives(initiativeList, itemsToRemove);
                }
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}

            // Fix up validation errors.
            // An array returns validation errors in the text form of "Questions[0].Text" and then the error text.
            // Fixup without the [] since that is not easily addressable inside javascript.
            if (model.ValidationErrors != null)
            {
                var originalCount = model.ValidationErrors.Count;
                for (int aIdx = 0; aIdx < originalCount; aIdx++)
                {
                    var newValue = model.ValidationErrors[aIdx].Key.Replace('[', '_');
                    newValue = newValue.Replace(']', '_');
                    model.ValidationErrors.Add(new KeyValuePair<string, string>(newValue, model.ValidationErrors[aIdx].Value));
                }
            }

            model.Initiatives = _programInitiativeService
	            .GetAll()
	            .Select(t => new ProgramInitiativeModel(t))
	            .ToList();

            foreach (var initiative in model.Initiatives)
	            initiative.IsInUse = _programInitiativeService.IsInUse(initiative.Id);

			return Json(model);
		}
	}
}
