using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Identity;
using CJG.Web.External.Areas.Int.Models.ParticipantFundingStreams;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;

namespace CJG.Web.External.Areas.Int.Controllers
{
    /// <summary>
    /// <paramtyperef name="ParticipantFundingSourcesController"/> class, provides endpoints to manage program initiatives
    /// </summary>
    [AuthorizeAction(Privilege.GM1, Privilege.SM)]
	[RouteArea("Int")]
	[RoutePrefix("Admin/ParticipantFundingSources")]
	public class ParticipantFundingSourcesController : BaseController
	{
		private readonly IParticipantFundingStreamService _participantFundingStreamService;

		/// <summary>
		/// Creates a new instance of a <paramtyperef name="ParticipantFundingSourcesController"/> object.
		/// </summary>
		/// <param name="controllerService"></param>
		/// <param name="participantFundingStreamService"></param>
		public ParticipantFundingSourcesController(
			IControllerService controllerService,
			IParticipantFundingStreamService participantFundingStreamService) : base(controllerService.Logger)
		{
			_participantFundingStreamService = participantFundingStreamService;
		}

		[HttpGet]
		[Route("View")]
		public ActionResult ParticipantFundingSourcesView()
		{
			return View();
		}

		[HttpGet]
		[Route("FundingSources")]
		public JsonResult GetFundingSources()
		{
			var model = new ParticipantFundingStreamsModel();
			try
			{
				model.FundingStreams = _participantFundingStreamService
					.GetAll()
					.Select(t => new ParticipantFundingStreamModel(t))
					.ToList();

				foreach (var initiative in model.FundingStreams)
					initiative.IsInUse = _participantFundingStreamService.IsInUse(initiative.Id);
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
		[Route("FundingSources")]
		public JsonResult UpdateFundingSources(ParticipantFundingStreamsModel model)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					HandleModelStateValidation(model);
				}
				else
				{
					var fundingStreams = _participantFundingStreamService.GetAll().ToList();
					var postedItemIds = model.FundingStreams
						.Where(i => i.Id > 0)
						.Select(i => i.Id)
						.ToList();

					var itemsToRemove = fundingStreams
						.Where(i => !postedItemIds.Contains(i.Id))
						.ToList();

                    foreach (var fundingStreamModel in model.FundingStreams)
                    {
                        ParticipantFundingStream fundingStream;
                        if (fundingStreamModel.Id != 0)
                        {
                            fundingStream = fundingStreams.First(x => x.Id == fundingStreamModel.Id);

                            fundingStream.Caption= fundingStreamModel.Name ?? string.Empty;
                            fundingStream.IsActive = fundingStreamModel.IsActive;
                            fundingStream.RowSequence = fundingStreamModel.RowSequence;
                            fundingStream.RowVersion = Convert.FromBase64String(fundingStreamModel.RowVersion);
                        }
                        else
                        {
                            fundingStream = new ParticipantFundingStream();
                            fundingStream.Caption = fundingStreamModel.Name;
                            fundingStream.IsActive = fundingStreamModel.IsActive;
                            fundingStream.RowSequence = fundingStreamModel.RowSequence;

                            fundingStreams.Add(fundingStream);
                        }
                    }

                    foreach (var toRemove in itemsToRemove)
	                    fundingStreams.Remove(toRemove);

                    _participantFundingStreamService.UpdateFundingStreams(fundingStreams, itemsToRemove);
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

            model.FundingStreams = _participantFundingStreamService
	            .GetAll()
	            .Select(t => new ParticipantFundingStreamModel(t))
	            .ToList();

            foreach (var initiative in model.FundingStreams)
	            initiative.IsInUse = _participantFundingStreamService.IsInUse(initiative.Id);

			return Json(model);
		}
	}
}
