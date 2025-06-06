﻿using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Identity;
using CJG.Web.External.Areas.Int.Models;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CJG.Application.Business.Models;

namespace CJG.Web.External.Areas.Int.Controllers
{
	[RouteArea("Int")]
	public class ApplicantContactController : BaseController
	{
		private readonly IStaticDataService _staticDataService;
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IUserService _userService;
		private readonly IApplicationAddressService _applicationAddressService;
		private readonly IGrantStreamService _grantStreamService;

		public ApplicantContactController(
			IControllerService controllerService,
			IGrantApplicationService grantApplicationService,
			IApplicationAddressService applicationAddressService,
			IGrantStreamService grantStreamService
		   ) : base(controllerService.Logger)
		{
			_userService = controllerService.UserService;
			_staticDataService = controllerService.StaticDataService;
			_grantApplicationService = grantApplicationService;
			_applicationAddressService = applicationAddressService;
			_grantStreamService = grantStreamService;
		}

		/// <summary>
		/// Get the applicant contact information for the application details view.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/ApplicantContact/{id}")]
		public JsonResult GetApplicantContact(int id)
		{
			var model = new ApplicantContactViewModel();

			try
			{
				var grantApplication = _grantApplicationService.Get(id);
				model = new ApplicantContactViewModel(grantApplication, _userService, User);
				SetApplicationProxyDetails(grantApplication, model);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		public void SetApplicationProxyDetails(GrantApplication grantApplication, ApplicantContactViewModel model)
		{
			var stream = grantApplication.GrantOpening.GrantStream;

			var grantWriterQuestion = _grantStreamService.GetGrantStreamQuestions(stream.Id)
				.Where(l => l.IsActive)
				.Where(l => l.CollectContactInformation)
				.Where(l => l.RequiresGrantWriter())
				.FirstOrDefault();

			if (grantWriterQuestion == null)
				return;

			var answer = grantApplication.GrantStreamEligibilityAnswers
				.Where(a => a.EligibilityAnswer)
				.FirstOrDefault(a => a.GrantStreamEligibilityQuestionId == grantWriterQuestion.Id);

			if (answer == null)
				return;

			model.HasProxyContact = true;
			var designation = answer.GrantWriterDesignation.HasValue && answer.GrantWriterDesignation.Value > 0
				? answer.GrantWriterDesignation.Value.GetDescription()
				: "--";

			model.ProxyContactDesignation = designation;
			model.ProxyContactName = answer.ContactName;
			model.ProxyContactEmail= answer.ContactEmailAddress;
			model.ProxyContactPhoneNumber = answer.ContactPhoneNumber;
		}

		/// <summary>
		/// Update the applicant contact information in the datasource.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPut]
		[ValidateRequestHeader]
		[PreventSpam]
		[Route("Application/ApplicantContact/")]
		public JsonResult UpdateApplicantContact(ApplicantContactViewModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var grantApplication = _grantApplicationService.Get(model.Id);
					var originalMailingAddress = grantApplication.ApplicantMailingAddress;
					model.MapToGrantApplication(grantApplication, _staticDataService, _applicationAddressService);
					if (grantApplication.ApplicantMailingAddressId == 0 && originalMailingAddress?.Id != 0 && originalMailingAddress?.Id != grantApplication.ApplicantPhysicalAddressId)
					{
						// Delete the orphaned address.
						_applicationAddressService.RemoveAddressIfNotUsed(grantApplication.ApplicantMailingAddress);
					}
					_grantApplicationService.Update(grantApplication, ApplicationWorkflowTrigger.EditApplicantContact);
					model = new ApplicantContactViewModel(grantApplication, _userService, User);
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

		/// <summary>
		/// Get all of the applicants in the organization for the specified grant application.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/Applicant/Contacts/{grantApplicationId}")]
		public JsonResult GetApplicantsForOrganization(int grantApplicationId)
		{
			var model = new List<KeyValuePair<int, string>>();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				model = _grantApplicationService.GetAvailableApplicationContacts(grantApplication).Select(a => new KeyValuePair<int, string>(a.Id, $"{a.GetUserFullName()} | {a.JobTitle} | {a.EmailAddress} | {a.PhoneNumber}")).ToList();
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Change the applicant who owns the specified grant application.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/Applicant/Contact/Change")]
		public JsonResult ChangeApplicant(Models.Applicants.ChangeApplicantContactViewModel model)
		{
			var result = new ApplicantContactViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(model.Id);
				grantApplication.RowVersion = Convert.FromBase64String(model.RowVersion);
				_grantApplicationService.ChangeApplicationAdministrator(grantApplication, model.ApplicantContactId);

				result = new ApplicantContactViewModel(grantApplication, _userService, User);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, result);
			}
			return Json(result, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Change the applicant who owns the specified grant application.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/Alternate/Contact/Change")]
		public JsonResult ChangeAlternateContact(ApplicantContactViewModel model)
		{
			var result = new ApplicantContactViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(model.Id);
				grantApplication.RowVersion = Convert.FromBase64String(model.RowVersion);

				var contactModel = new AlternateContactModel
				{
					AlternateFirstName = model.AlternateFirstName,
					AlternateLastName = model.AlternateLastName,
					AlternateEmail = model.AlternateEmail,
					AlternateJobTitle = model.AlternateJobTitle,
					AlternatePhoneNumber = model.AlternatePhoneNumberViewModel.Phone,
					AlternatePhoneExtension = model.AlternatePhoneNumberViewModel.PhoneExtension
				};

				_grantApplicationService.ChangeAlternateContact(grantApplication, contactModel);

				// set redirect url?

				result = new ApplicantContactViewModel(grantApplication, _userService, User);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, result);
			}
			return Json(result, JsonRequestBehavior.AllowGet);
		}
	}
}