using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Validation;
using System.Web.Mvc;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Int.Models.Applications;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;
using NLog;

namespace CJG.Web.External.Areas.Int.Controllers
{
	public class EligibilityQuestionAnswerModel
	{
		public string Question { get; set; }
		public int QuestionId { get; set; }
		public bool? EligibilityAnswer { get; set; }

		public bool CollectContactInfo { get; set; }
		public bool ShowDesignation { get; set; }

		public string Designation { get; set; }
		public string ContactName { get; set; }
		public string ContactEmailAddress { get; set; }
		public string ContactPhoneNumber { get; set; }
		
		public EligibilityQuestionAnswerModel() {}
	}

	[RouteArea("Int")]
	public class ApplicationSummaryController : BaseController
	{
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly IAuthorizationService _authorizationService;
		private readonly IRiskClassificationService _riskClassificationService;
		private readonly IProgramInitiativeService _programInitiativeService;
		private readonly IGrantAgreementService _grantAgreementService;
		private readonly IDeliveryPartnerService _deliveryPartnerService;
		private readonly IAttachmentService _attachmentService;
		private readonly IFiscalYearService _fiscalYearService;
		private readonly IGrantStreamService _grantStreamService;

		public ApplicationSummaryController(
			IControllerService controllerService,
			IAuthorizationService authorizationService,
			IGrantApplicationService grantApplicationService,
			IRiskClassificationService riskClassificationService,
			IProgramInitiativeService programInitiativeService,
			IGrantAgreementService grantAgreementService,
			IDeliveryPartnerService deliveryPartnerService,
			IAttachmentService attachmentService,
			IFiscalYearService fiscalYearService,
			IGrantStreamService grantStreamService
			) : base(controllerService.Logger)
		{
			_grantApplicationService = grantApplicationService;
			_authorizationService = authorizationService;
			_riskClassificationService = riskClassificationService;
			_programInitiativeService = programInitiativeService;
			_grantAgreementService = grantAgreementService;
			_deliveryPartnerService = deliveryPartnerService;
			_attachmentService = attachmentService;
			_fiscalYearService = fiscalYearService;
			_grantStreamService = grantStreamService;
		}

		#region Endpoints
		/// <summary>
		/// Get the summary information for the application details view.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/Summary/{grantApplicationId}")]
		public JsonResult GetSummary(int grantApplicationId)
		{
			var model = new ApplicationSummaryViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				model = new ApplicationSummaryViewModel(grantApplication, _deliveryPartnerService, _authorizationService, _grantApplicationService, _riskClassificationService, _fiscalYearService, User);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Get the checklist information for the application details view.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/Checklist/{grantApplicationId}")]
		public JsonResult GetChecklist(int grantApplicationId)
		{
			var model = new ApplicationChecklistViewModel();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);

				model = new ApplicationChecklistViewModel(grantApplication, _grantStreamService);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Get the eligibility questions and answers for the application details view.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/Eligibility/{grantApplicationId}")]
		public JsonResult GetEligibility(int grantApplicationId)
		{
			var model = new List<EligibilityQuestionAnswerModel>();
			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				var stream = grantApplication.GrantOpening.GrantStream;

				var streamEligibilityQuestions = _grantStreamService.GetGrantStreamQuestions(stream.Id)
					.Where(l => l.IsActive)
					.Select(n => new EligibilityQuestionAnswerModel
					{
						Question = n.EligibilityQuestion,
						QuestionId = n.Id,
						CollectContactInfo = n.CollectContactInformation,
						ShowDesignation = n.RequiresGrantWriter()
					}).ToList();

				foreach (var answer in grantApplication.GrantStreamEligibilityAnswers)
				{
					var questionModel = streamEligibilityQuestions.FirstOrDefault(q => q.QuestionId == answer.GrantStreamEligibilityQuestionId);
					if (questionModel == null)
						continue;

					questionModel.EligibilityAnswer = answer.EligibilityAnswer;

					if (questionModel.ShowDesignation)
					{
						var designation = answer.GrantWriterDesignation.HasValue && answer.GrantWriterDesignation.Value > 0
							? answer.GrantWriterDesignation.Value.GetDescription()
							: "--";
						questionModel.Designation = designation;
					}

					questionModel.ContactName = answer.ContactName;
					questionModel.ContactEmailAddress = answer.ContactEmailAddress;
					questionModel.ContactPhoneNumber = answer.ContactPhoneNumber;
				}

				model = streamEligibilityQuestions;
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Route("Application/Summary/ProgramInitiatives")]
		public JsonResult GetProgramInitiatives()
		{
			var programInitiatives = new KeyValuePair<int, string>[] { };
			try
			{
				programInitiatives = _programInitiativeService
					.GetAll(true)
					.Select(x => new KeyValuePair<int, string>(x.Id, x.Name))
					.ToArray();
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}
			return Json(programInitiatives, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Get an array of delivery programs.
		/// </summary>
		/// <param name="grantProgramId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/Summary/Delivery/Partners/{grantProgramId}")]
		public JsonResult GetDeliveryPartners(int grantProgramId)
		{
			var deliveryPartners = new KeyValuePair<int, string>[] { };
			try
			{
				deliveryPartners = _deliveryPartnerService.GetDeliveryPartners(grantProgramId).Select(x => new KeyValuePair<int, string>(x.Id, x.Caption)).ToArray();
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}
			return Json(deliveryPartners, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Get an array of delivery partner services.
		/// </summary>
		/// <param name="grantProgramId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Application/Summary/Delivery/Partner/Services/{grantProgramId}")]
		public JsonResult GetDeliveryPartnerServices(int grantProgramId)
		{
			var deliveryPartnerServices = new KeyValuePair<int, string>[] { };
			try
			{
				deliveryPartnerServices = _deliveryPartnerService.GetDeliveryPartnerServices(grantProgramId).Select(x => new KeyValuePair<int, string>(x.Id, x.Caption)).ToArray();
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);
			}
			return Json(deliveryPartnerServices, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Assign the specified primary assessor to the grant application.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/Summary/AssignPrimary")]
		public JsonResult AssignPrimary(ApplicationSummaryViewModel model)
		{
			try
			{
				var grantApplication = _grantApplicationService.Get(model.Id);
				grantApplication.RowVersion = Convert.FromBase64String(model.RowVersion);

				_grantApplicationService.AssignPrimaryAssessor(grantApplication, model.PrimaryAssessorId);
				model = new ApplicationSummaryViewModel(grantApplication, _deliveryPartnerService, _authorizationService, _grantApplicationService, _riskClassificationService, _fiscalYearService, User);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Assign the specified assessor to the grant application.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/Summary/Assign")]
		public JsonResult Assign(ApplicationSummaryViewModel model)
		{
			try
			{
				var grantApplication = _grantApplicationService.Get(model.Id);
				grantApplication.RowVersion = Convert.FromBase64String(model.RowVersion);
				_grantApplicationService.AssignAssessor(grantApplication, model.AssessorId);
				model = new ApplicationSummaryViewModel(grantApplication, _deliveryPartnerService, _authorizationService, _grantApplicationService, _riskClassificationService, _fiscalYearService, User);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Update the summary information in the datasource.
		/// </summary>
		/// <param name="summary"></param>
		/// <param name="file"></param>
		/// <returns></returns>
		[HttpPut]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Application/Summary")]
		public JsonResult UpdateSummary(string summary, System.Web.HttpPostedFileBase file)
		{
			var model = new ApplicationSummaryViewModel();
			try
			{
				model = Newtonsoft.Json.JsonConvert.DeserializeObject<ApplicationSummaryViewModel>(summary);

				GrantApplication grantApplication = _grantApplicationService.Get(model.Id);
				grantApplication.RowVersion = Convert.FromBase64String(model.RowVersion);

				bool deliveryDatesModified = (model.DeliveryStartDate.ToLocalTime().Date != grantApplication.StartDate.ToLocalTime().Date || model.DeliveryEndDate.ToLocalTime().Date != grantApplication.EndDate.ToLocalTime().Date);
				if (deliveryDatesModified && grantApplication.ApplicationStateInternal.In(ApplicationStateInternal.NewClaim, ApplicationStateInternal.ClaimAssessEligibility, ApplicationStateInternal.ClaimAssessReimbursement, ApplicationStateInternal.ClaimApproved))
				{
					ModelState.AddModelError(nameof(model.DeliveryStartDate) + " + " + nameof(model.DeliveryEndDate), "You cannot change the Delivery Dates when a Claim is currently submitted or a previous Claim has been approved.");
				}

				if (model.ProgramInitiativeId == null)
					ModelState.AddModelError(nameof(model.ProgramInitiativeId), "Program Initiative is required.");

				if (model.DeliveryPartnerId == null)
				{
					model.SelectedDeliveryPartnerServiceIds = new List<int>();
				}

				if ((!model.HasRequestedAdditionalFunding) ?? true)
				{
					ModelState.Remove("DescriptionOfFundingRequested");
				}

				if (ModelState.IsValid)
				{
					// when delivery start/end dates changed
					if (deliveryDatesModified)
					{
						// delivery date range must cover all training programs date ranges
						var earliest = grantApplication.DateSubmitted.Value.ToLocalMorning();
						var latest = model.DeliveryStartDate.AddYears(1);
						if (model.DeliveryStartDate < earliest || latest < model.DeliveryEndDate)
						{
							throw new InvalidOperationException($"Delivery dates must be within {earliest:yyyy-MM-dd} and {latest:yyyy-MM-dd}");
						}

						// Update Delivery dates in the Grant Application.
						grantApplication.StartDate = model.DeliveryStartDate.ToUtcMorning();
						grantApplication.EndDate = model.DeliveryEndDate.ToUtcMidnight();

						DateTime? latestStartTime = null;
						DateTime? earliestEndTime = null;

						foreach (var tp in grantApplication.TrainingPrograms)
						{
							if (latestStartTime == null)
								latestStartTime = tp.StartDate;

							if (earliestEndTime == null)
								earliestEndTime = tp.EndDate;

							if (latestStartTime > tp.StartDate)
								latestStartTime = tp.StartDate;

							if (earliestEndTime < tp.EndDate)
								earliestEndTime = tp.EndDate;
						}

						if (grantApplication.StartDate > latestStartTime || grantApplication.EndDate < earliestEndTime)
						{
							var logMessage = $"#{grantApplication.FileNumber} Skill Training Date Violation. Attempted delivery dates: {grantApplication.StartDate} to {grantApplication.EndDate}. Training Dates: Earliest: {earliestEndTime} Latest: {latestStartTime}. TP Count: {grantApplication.TrainingPrograms.Count}. Start Date Issue: {grantApplication.StartDate > latestStartTime} End Date Issue: {grantApplication.EndDate < earliestEndTime}";
							_logger.Log(LogLevel.Debug, logMessage);
							throw new InvalidOperationException("Skills training dates do not fall within your delivery period and will need to be rescheduled. Make sure all your skills training dates are accurate to your plan.");
						}
					}

					grantApplication.ProgramInitiativeId = model.ProgramInitiativeId;
					grantApplication.Organization.DoingBusinessAsMinistry = model.DoingBusinessAsMinistry;
					grantApplication.RowVersion = Convert.FromBase64String(model.RowVersion);
					//grantApplication.RiskClassificationId = model.RiskClassificationId;

					_grantApplicationService.UpdateChecklist(grantApplication, model.ChecklistItemIds);


					if (_grantAgreementService.AgreementUpdateRequired(grantApplication))
						_grantAgreementService.UpdateAgreement(grantApplication);

					if (file != null)
					{
						var attachment = file.UploadFile(grantApplication.BusinessCaseDocument?.Description ?? string.Empty, file.FileName);
						if (grantApplication.BusinessCaseDocument == null || grantApplication.BusinessCaseDocument.Id == 0)
						{
							grantApplication.BusinessCaseDocument = attachment;
							_attachmentService.Add(attachment,true);
						}
						else
						{
							attachment.Id = grantApplication.BusinessCaseDocument.Id;
							attachment.RowVersion = grantApplication.BusinessCaseDocument.RowVersion;
							_attachmentService.Update(attachment,true);
						}
					}
					if (grantApplication.GrantOpening.GrantStream.IncludeDeliveryPartner)
					{
						_grantApplicationService.UpdateDeliveryPartner(grantApplication, model.DeliveryPartnerId, model.SelectedDeliveryPartnerServiceIds);
						_grantApplicationService.Update(grantApplication, ApplicationWorkflowTrigger.EditSummary);
					}
					else
					{
						_grantApplicationService.Update(grantApplication, ApplicationWorkflowTrigger.EditSummary);
					}

					model = new ApplicationSummaryViewModel(grantApplication, _deliveryPartnerService, _authorizationService, _grantApplicationService, _riskClassificationService, _fiscalYearService, User);
				}
				else
				{
					HandleModelStateValidation(model, ModelState.GetErrorMessages("<br />"));
				}
			}
			catch (Exception e)
			{
				HandleAngularException(e, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}
		#endregion
	}
}