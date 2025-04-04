﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CJG.Application.Services;
using CJG.Application.Services.Exceptions;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Ext.Models;
using CJG.Web.External.Areas.Ext.Models.Reports;
using CJG.Web.External.Areas.Int.Models;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;
using CJG.Web.External.Models.Shared;
using CJG.Web.External.Models.Shared.Reports;

namespace CJG.Web.External.Areas.Ext.Controllers
{
    /// <summary>
    /// CompletionReportingController class, provides endpoints for completion reporting.
    /// </summary>
    [RouteArea("Ext")]
	[ExternalFilter]
	public class CompletionReportingController : BaseController
	{
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly ICompletionReportService _completionReportService;
		private readonly INaIndustryClassificationSystemService _naIndustryClassificationSystemService;
		private readonly INationalOccupationalClassificationService _nationalOccupationalClassificationService;
		private readonly ICommunityService _communityService;

		/// <summary>
		/// Creates a new instance of a <typeparamref name="CompletionReportingController"/> object.
		/// </summary>
		/// <param name="controllerService"></param>
		/// <param name="grantApplicationService"></param>
		/// <param name="completionReportService"></param>
		/// <param name="naIndustryClassificationSystemService"></param>
		/// <param name="nationalOccupationalClassificationService"></param>
		/// <param name="communityService"></param>
		public CompletionReportingController(
			IControllerService controllerService,
			IGrantApplicationService grantApplicationService,
			ICompletionReportService completionReportService,
			INaIndustryClassificationSystemService naIndustryClassificationSystemService,
			INationalOccupationalClassificationService nationalOccupationalClassificationService,
			ICommunityService communityService) : base(controllerService.Logger)
		{
			_grantApplicationService = grantApplicationService;
			_completionReportService = completionReportService;
			_naIndustryClassificationSystemService = naIndustryClassificationSystemService;
			_nationalOccupationalClassificationService = nationalOccupationalClassificationService;
			_communityService = communityService;
		}

		/// <summary>
		/// Return a view to report a completion report.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Reporting/Completion/Report/View/{grantApplicationId}")]
		public ActionResult CompletionReportView(int grantApplicationId)
		{
			ViewBag.GrantApplicationId = grantApplicationId;
			var grantApplication = _grantApplicationService.Get(grantApplicationId);

			if (grantApplication.ApplicationStateExternal == ApplicationStateExternal.Closed)
			{
				this.SetAlert("You are not authorized to edit the completion report.", AlertType.Warning, true);
				return RedirectToAction(nameof(CompletionReportingController.CompletionReportDetailsView), new { grantApplicationId });
			}

			if (!User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.SubmitCompletionReport))
			{
				this.SetAlert("You are not authorized to submit the completion report.", AlertType.Warning, true);
				return RedirectToAction(nameof(ReportingController.GrantFileView), nameof(ReportingController).Replace("Controller", ""), new { grantApplicationId });
			}

			return View();
		}

		/// <summary>
		/// Get the data for the completion report view.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Reporting/Completion/Report/{grantApplicationId}")]
		public JsonResult GetCompletionReport(int grantApplicationId)
		{
			var model = new CompletionReportViewModel();

			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				model = new CompletionReportViewModel(grantApplication, _completionReportService);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Get the completion report group data.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <param name="completionReportGroupId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Reporting/Completion/Report/Group/{grantApplicationId}/{completionReportGroupId}")]
		public JsonResult GetCompletionReportGroup(int grantApplicationId, int completionReportGroupId)
		{
			var model = new CompletionReportGroupViewModel();

			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				var completionReportGroup = _completionReportService.GetCompletionReportGroup(grantApplication.CompletionReportId, completionReportGroupId);
				model = new CompletionReportGroupViewModel(grantApplication, completionReportGroup, _naIndustryClassificationSystemService, _nationalOccupationalClassificationService);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Get the completion report ESS data.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Reporting/Completion/Report/ESS/{grantApplicationId}")]
		public JsonResult GetEmploymentServicesAndSupports(int grantApplicationId)
		{
			var model = new CompletionReportDynamicCheckboxViewModel();

			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);
				model = new CompletionReportDynamicCheckboxViewModel(grantApplication);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Update and save the specified completion report group answers in the grant application.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Reporting/Completion/Report/")]
		public JsonResult UpdateCompletionReport(CompletionReportGroupViewModel model)
		{
			try
			{
				var grantApplication = _grantApplicationService.Get(model.GrantApplicationId);
				if (!User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.SubmitCompletionReport))
					throw new NotAuthorizedException("You are not authorized to edit the completion report.");

				if (grantApplication.CompletionReportId != Core.Entities.Constants.CompletionReportCWRG)
					throw new Exception("Cannot report on non-CWRG completion report.");

				UpdateCompletionReport(model, grantApplication);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Update and save the specified completion report group answers in the grant application.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		[PreventSpam]
		[ValidateRequestHeader]
		[Route("Reporting/Completion/Report/SaveForLater")]
		public JsonResult SaveForLater(CompletionReportGroupViewModel model)
		{
			try
			{
				var grantApplication = _grantApplicationService.Get(model.GrantApplicationId);
				if (!User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.SubmitCompletionReport))
					throw new NotAuthorizedException("You are not authorized to edit the completion report.");

				if (grantApplication.CompletionReportId != Core.Entities.Constants.CompletionReportCWRG)
					throw new Exception("Cannot report on non-CWRG completion report.");

				UpdateCompletionReport(model, grantApplication);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Update and save the specified completion report group answers in the grant application.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="grantApplication"></param>
		/// <returns></returns>
		private CompletionReportGroupViewModel UpdateCompletionReport(CompletionReportGroupViewModel model, GrantApplication grantApplication)
		{
			var participantAnswers = new List<ParticipantCompletionReportAnswer>();
			var employerAnswers = new List<EmployerCompletionReportAnswer>();
			var completionReportGroupId = model.Id;
			var doNotIncludeParticipants = new int[0];
			var participantFormsForReport = new int[0];
			var allParticipantFormsForReport = grantApplication.ParticipantForms.Select(pf => pf.Id).ToArray();
			var participantIdsForStep = new int[0];

			var claim = grantApplication.GetCurrentClaim();
			var claimType = grantApplication.GetClaimType();

			if (completionReportGroupId == Core.Entities.Constants.CompletionReportCWRGPage1)
			{
				var participants = grantApplication.ParticipantForms;
				participantFormsForReport = participants
					.Select(pf => pf.Id)
					.ToArray();
			}
			else if (claimType == ClaimTypes.SingleAmendableClaim && claim != null)
			{
				participantFormsForReport = claim.EligibleCosts
					.SelectMany(ec => ec.ParticipantCosts.Select(pc => pc.ParticipantForm.Id))
					.Distinct()
					.Where(peId => !doNotIncludeParticipants.Contains(peId))
					.ToArray();
			}
			else
			{
				participantFormsForReport = grantApplication.ParticipantForms
					.Where(pf => !pf.IsExcludedFromClaim && !doNotIncludeParticipants.Contains(pf.Id))
					.Select(pe => pe.Id)
					.ToArray();
			}

			// Get the completion report
			var completionReport = _completionReportService.GetCurrentCompletionReport(grantApplication.CompletionReportId);
			var questions = _completionReportService.GetCompletionReportQuestionsForStep(completionReport.Id, completionReportGroupId);

			switch (completionReportGroupId)
			{
				case Core.Entities.Constants.CompletionReportCWRGPage1:
					foreach (var question in model.Questions)
					{
						var answer = question.Level1Answers.First();

						if (answer.BoolAnswer)
							continue;

						// record any entries where the answer was not in the affirmative
						foreach (var participantAnswer in question.Level2Answers.Where(o => o.BoolAnswer))
						{
							// make sure we don't record the same answer twice for a participant (currently only applies to reasons for training outcomes)
							if (participantAnswers.Count(pa => pa.AnswerId == participantAnswer.IntAnswer && pa.ParticipantFormId == participantAnswer.ParticipantFormId) == 0)
							{
								if (participantAnswer.IntAnswer == 0)
									throw new InvalidOperationException("A reason must be specified for all participants not completed training.");

								participantAnswers.Add(new ParticipantCompletionReportAnswer
								{
									GrantApplicationId = grantApplication.Id,
									QuestionId = question.Id,
									AnswerId = participantAnswer.IntAnswer,
									ParticipantFormId = participantAnswer.ParticipantFormId ?? 0,
									OtherAnswer = participantAnswer.StringAnswer ?? string.Empty
								});
							}
						}
						// get the ids of those that didn't answer in the affirmative
						doNotIncludeParticipants = participantAnswers.Select(pa => pa.ParticipantFormId).Distinct().ToArray();
					}
					break;

				case Core.Entities.Constants.CompletionReportCWRGPage2:
					foreach (var question in model.Questions)
					{
						var answer = question.Level1Answers.First();
						foreach (var participantAnswer in question.Level1Answers
							.Where(o => o.IntAnswer != 0 || o.Naics1Id != null || o.Noc1Id != null || o.CommunityId != null))
						{
							if (participantAnswer.ParticipantFormId != null)    // There may be a fake answer with no participant
							{
								// Adding NOCS and NAICS to the participant form (the DB record). These answers not stored on the participant answer table.
								if (question.QuestionType == CompletionReportQuestionTypes.NOCList ||
									question.QuestionType == CompletionReportQuestionTypes.NAICSList)
								{
									var participant = grantApplication.ParticipantForms.FirstOrDefault(o => o.Id == participantAnswer.ParticipantFormId);
									if (question.QuestionType == CompletionReportQuestionTypes.NOCList)
									{
										participant.NocId = participantAnswer.Noc5Id ?? participantAnswer.Noc4Id ?? participantAnswer.Noc3Id ?? participantAnswer.Noc2Id ?? participantAnswer.Noc1Id;
									}
									if (question.QuestionType == CompletionReportQuestionTypes.NAICSList)
									{
										participant.NaicsId = participantAnswer.Naics5Id ?? participantAnswer.Naics4Id ?? participantAnswer.Naics3Id ?? participantAnswer.Naics2Id ?? participantAnswer.Naics1Id;
									}

									participantAnswers.Add(new ParticipantCompletionReportAnswer
									{
										GrantApplicationId = grantApplication.Id,
										QuestionId = question.Id,
										AnswerId = null,
										ParticipantFormId = participantAnswer.ParticipantFormId ?? 0,
										OtherAnswer = string.Empty
									});
								}
								else
								{
									participantAnswers.Add(new ParticipantCompletionReportAnswer
									{
										GrantApplicationId = grantApplication.Id,
										QuestionId = question.Id,
										AnswerId = participantAnswer.IntAnswer == 0 ? (int?)null : participantAnswer.IntAnswer,
										CommunityId = participantAnswer.CommunityId == 0 ? null : participantAnswer.CommunityId,
										ParticipantFormId = participantAnswer.ParticipantFormId ?? 0,
										OtherAnswer = participantAnswer.StringAnswer ?? string.Empty
									});
								}
							}
						}
					}
					break;

				case Core.Entities.Constants.CompletionReportCWRGPage3:
					{
						// There is only 1 question on page 3. MultipleCheckbox, answers are uploaded per participant in the StringAnswer.
						var question = model.Questions.First();
						List<CompletionReportOption> optList = questions.First().Options.ToList();
						if (optList == null)
							throw new Exception("Cannot locate options for question.");

						foreach (var participantAnswer in question.Level1Answers.Where(o => o.StringAnswer != null && o.StringAnswer.Length != 0))
						{
							if (participantAnswer.ParticipantFormId != null)
							{
								var newAnswer = new ParticipantCompletionReportAnswer
								{
									GrantApplicationId = grantApplication.Id,
									QuestionId = question.Id,
									AnswerId = null,
									CommunityId = null,
									ParticipantFormId = participantAnswer.ParticipantFormId ?? 0,
									OtherAnswer = null
								};

								var allIds = participantAnswer.StringAnswer.Split(',');		// All the OptionIds, comma separated
								var selectedAnswerList = allIds?.Select(Int32.Parse).ToArray();
								if (selectedAnswerList?.Count() > 0)
								{
									var filteredSelections = _completionReportService.GetCompletionReportOptions().Where(o => selectedAnswerList.Contains(o.Id)).ToList();
									newAnswer.MultAnswers.Clear();

									foreach (var selection in filteredSelections)
									{
										newAnswer.MultAnswers.Add(selection);
									}
								}
								participantAnswers.Add(newAnswer);
							}
						}
					}
					break;

				case Core.Entities.Constants.CompletionReportCWRGPage4: // The employer questions page
					foreach (var question in model.Questions)
					{
						foreach (var employerAnswer in question.Level1Answers.OrEmptyIfNull())
						{
							var employerCompletionReportAnswer = new EmployerCompletionReportAnswer
							{
								QuestionId = employerAnswer.QuestionId,
								AnswerId = employerAnswer.IntAnswer == 0 ? question.DefaultAnswerId : employerAnswer.IntAnswer,
								GrantApplicationId = grantApplication.Id,
								OtherAnswer = employerAnswer.StringAnswer
							};

							employerAnswers.Add(employerCompletionReportAnswer);
						}
					}
					break;
			}

			// Page 1 is where the user indicates which participants dropped out.
			// If the user goes back to page 1, the code below clears out answers for dropouts in the following pages.
			if (completionReportGroupId == Core.Entities.Constants.CompletionReportCWRGPage1)
			{
				// exclude any participants that didn't answer in the affirmative from those on the claim
				participantIdsForStep = participantFormsForReport.ToList().Where(id => !doNotIncludeParticipants.Contains(id)).ToArray();

				// Delete any answers for these participants that are currently saved.
				_completionReportService.DeleteAnswersFor(doNotIncludeParticipants);

				// Delete saved answers if they originally were not included(?)
				var questionIds = questions.Select(q => q.Id).ToArray();
				_completionReportService.DeleteAnswersFor(participantFormsForReport, questionIds);
			}
			else if (completionReportGroupId == Core.Entities.Constants.CompletionReportCWRGPage2 ||
				completionReportGroupId == Core.Entities.Constants.CompletionReportCWRGPage3)
			{
				// exclude any participants that didn't answer in the affirmative from those on the claim
				participantIdsForStep = participantFormsForReport.ToList().Where(id => !doNotIncludeParticipants.Contains(id)).ToArray();

				// Delete saved answers for this step so that new answers can replace them.
				var questionIds = questions.Select(q => q.Id).ToArray();
				_completionReportService.DeleteAnswersFor(allParticipantFormsForReport, questionIds);
			}

			// Now add all of the participants that were dropped out, with the affirmative answer
			// This is done differently than ETG. We have a number of questions that will have answers for some participants
			// but not all participants. So, Page 1's question 15 will have a "dropped out/completed" answer, but page 2's
			// answers will not exist for dropped out participants.
			foreach (var participantId in participantIdsForStep)
			{
				switch (completionReportGroupId)
				{
					case Core.Entities.Constants.CompletionReportCWRGPage1:
						{
							participantAnswers.Add(new ParticipantCompletionReportAnswer
							{
								GrantApplicationId = grantApplication.Id,
								QuestionId = questions.First().Id,
								AnswerId = questions.First().DefaultAnswerId ?? 0,
								ParticipantFormId = participantId,
								OtherAnswer = string.Empty
							});
						}
						break;
				}
			}

			if (_completionReportService.RecordCompletionReportAnswersForStep(completionReportGroupId, participantAnswers, employerAnswers, completionReport.Id, participantIdsForStep, model.SaveOnly))
			{
				var allParticipantsHaveCompletedReport = _completionReportService.AllParticipantsHaveCompletedReport(participantIdsForStep, completionReport.Id, Core.Entities.Constants.CompletionReportCWRGPage1);

				// Locate the last page ID, and if it is the last page, close the grant file.
				var lastGroup = grantApplication.CompletionReport.Groups
					.OrderByDescending(g => g.RowSequence)
					.FirstOrDefault();

				if (lastGroup != null && lastGroup.Id >= 0)
				{
					if (!model.SaveOnly
						&& completionReportGroupId == lastGroup.Id
						&& grantApplication.ApplicationStateExternal == ApplicationStateExternal.ReportCompletion
						&& allParticipantsHaveCompletedReport)
					{
						_grantApplicationService.SubmitCompletionReportToCloseGrantFile(grantApplication);
					}
				}
			}
			return model;
		}

		/// <summary>
		/// Get an array of NAICS for the specified level and parent.
		/// </summary>
		/// <param name="level"></param>
		/// <param name="parentId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Reporting/Completion/Report/NAICS/{level}/{parentId?}")]
		public JsonResult GetNAICS(int level, int? parentId)
		{
			try
			{
				var model = _naIndustryClassificationSystemService.GetNaIndustryClassificationSystemChildren(parentId ?? 0, level).Select(n => new KeyValuePair<int, string>(n.Id, $"{n.Code} | {n.Description}")).ToArray();
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
		/// Get an array of NOCS for the specified level and parent.
		/// </summary>
		/// <param name="level"></param>
		/// <param name="parentId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Reporting/Completion/Report/NOCs/{level}/{parentId?}")]
		public JsonResult GetNOCs(int level, int? parentId)
		{
			try
			{
				var model = _nationalOccupationalClassificationService.GetNationalOccupationalClassificationChildren(parentId ?? 0, level).Select(n => new KeyValuePair<int, string>(n.Id, $"{n.Code} | {n.Description}")).ToArray();
				return Json(model, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				var model = new BaseViewModel();
				HandleAngularException(ex, model);
				return Json(model, JsonRequestBehavior.AllowGet);
			}
		}

		#region Complete Report Details
		/// <summary>
		/// Get the read only view for the completion report.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Reporting/Completion/Report/Details/{grantApplicationId}")]
		public ActionResult CompletionReportDetailsView(int grantApplicationId)
		{
			ViewBag.GrantApplicationId = grantApplicationId;
			return View();
		}

		/// <summary>
		/// Get the data for the completion report view.
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Reporting/Completion/Report/Details/Data/{grantApplicationId}")]
		public JsonResult GetCompletionReportDetails(int grantApplicationId)
		{
			var model = new CompletionReportDetailsViewModel();

			try
			{
				var grantApplication = _grantApplicationService.Get(grantApplicationId);

				model = new CompletionReportDetailsViewModel(grantApplication, _completionReportService);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, model);
			}

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Show cmpletion report status
		/// </summary>
		/// <param name="grantApplicationId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("Reporting/Completion/Status/{grantApplicationId}")]
		public PartialViewResult ShowCompletionReportStatus(int grantApplicationId)
		{
			var model = new CompletionReportStatusViewModel();

			var completionReportStatus = _completionReportService.GetCompletionReportStatus(grantApplicationId);

			model.CompletionReportStatus = (CompletionReportStatus)Enum.Parse(typeof(CompletionReportStatus), completionReportStatus.Replace(" ", string.Empty));

			return PartialView("_ShowCompletionReportStatus", model);
		}

		/// <summary>
		/// Returns a Json result of all the Communities.
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[PreventSpam]
		[Route("Reporting/Completion/Communities")]
		public ActionResult GetCommunities()
		{
			var communities = new CommunityManagementViewModel();
			try
			{
				communities = new CommunityManagementViewModel(_communityService.GetAll());
			}
			catch (Exception ex)
			{
				HandleAngularException(ex, communities);
			}
			return Json(communities, JsonRequestBehavior.AllowGet);
		}

		#endregion
	}
}
