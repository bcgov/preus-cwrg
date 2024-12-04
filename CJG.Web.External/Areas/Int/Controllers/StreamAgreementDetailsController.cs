using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using CJG.Application.Business.Models;
using CJG.Core.Entities;
using CJG.Core.Entities.Helpers;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Int.Models.SteamAgreementDetails;
using CJG.Web.External.Controllers;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Filters;
using ClosedXML.Excel;
using Newtonsoft.Json;

namespace CJG.Web.External.Areas.Int.Controllers
{
    [RouteArea("Int")]
	[RoutePrefix("Admin/Reports/StreamAgreementDetails")]
	public class StreamAgreementDetailsController : BaseController
	{
		private readonly IStaticDataService _staticDataService;
		private readonly IGrantStreamService _grantStreamService;
		private readonly ICommunityService _communityService;
		private readonly ITrainingProviderService _trainingProviderService;
		private readonly ITrainingProgramService _trainingProgramService;
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly INationalOccupationalClassificationService _nationalOccupationalClassificationService;
		private readonly IFiscalYearService _fiscalYearService;

		/// <summary>
		/// Creates a new instance of a <paramtyperef name="StreamAgreementDetailsController"/> object.
		/// </summary>
		/// <param name="controllerService"></param>
		/// <param name="grantStreamService"></param>
		/// <param name="communityService"></param>
		/// <param name="trainingProviderService"></param>
		/// <param name="trainingProgramService"></param>
		/// <param name="grantApplicationService"></param>
		/// <param name="nationalOccupationalClassificationService"></param>
		/// <param name="fiscalYearService"></param>
		public StreamAgreementDetailsController(
			IControllerService controllerService,
			IGrantStreamService grantStreamService,
			ICommunityService communityService,
			ITrainingProviderService trainingProviderService,
			ITrainingProgramService trainingProgramService,
			IGrantApplicationService grantApplicationService,
			INationalOccupationalClassificationService nationalOccupationalClassificationService,
			IFiscalYearService fiscalYearService
		) : base(controllerService.Logger)
		{
			_staticDataService = controllerService.StaticDataService;
			_grantStreamService = grantStreamService;
			_communityService = communityService;
			_trainingProviderService = trainingProviderService;
			_trainingProgramService = trainingProgramService;
			_grantApplicationService = grantApplicationService;
			_nationalOccupationalClassificationService = nationalOccupationalClassificationService;
			_fiscalYearService = fiscalYearService;
		}

		[HttpGet]
		[Route("View")]
		public ActionResult StreamAgreementDetailsView()
		{
			ViewBag.FiscalYearId = _staticDataService.GetFiscalYear(0)?.Id;

			return View();
		}

		[Route("FilterLookups")]
		public JsonResult GetFilterLookupValues()
		{
			var fiscalYears = _staticDataService.GetFiscalYears(limitEndYearRange: 2)
				.Select(t => new
				{
					t.Id,
					t.Caption
				})
				.ToArray();

			var applicationStatuses = new List<ApplicationStateInternal>
			{
				ApplicationStateInternal.New,
				ApplicationStateInternal.PendingAssessment,
				ApplicationStateInternal.ApplicationWithdrawn,
				ApplicationStateInternal.ReturnedToDraft,
				ApplicationStateInternal.UnderAssessment,
				ApplicationStateInternal.ReturnedToAssessment,
				ApplicationStateInternal.RecommendedForApproval,
				ApplicationStateInternal.RecommendedForDenial,
				ApplicationStateInternal.ApplicationDenied,
				ApplicationStateInternal.OfferIssued,
				ApplicationStateInternal.AgreementAccepted,
				ApplicationStateInternal.ChangeRequest,
				ApplicationStateInternal.ChangeReturned,
				ApplicationStateInternal.ChangeForApproval,
				ApplicationStateInternal.ChangeForDenial,
				ApplicationStateInternal.ChangeRequestDenied,
				ApplicationStateInternal.NewClaim,
				ApplicationStateInternal.ClaimAssessEligibility,
				ApplicationStateInternal.ClaimAssessReimbursement,
				ApplicationStateInternal.ClaimReturnedToApplicant,
				ApplicationStateInternal.ClaimDenied,
				ApplicationStateInternal.ClaimApproved,
				ApplicationStateInternal.OfferWithdrawn,
				ApplicationStateInternal.AgreementRejected,
				ApplicationStateInternal.CancelledByMinistry,
				ApplicationStateInternal.CancelledByAgreementHolder,
				ApplicationStateInternal.CompletionReporting,
				ApplicationStateInternal.Closed,
				ApplicationStateInternal.Unfunded,
				ApplicationStateInternal.ReturnedUnassessed
			};

			var model = new
			{
				FiscalYears = fiscalYears,
				ApplicationStatuses = applicationStatuses.Select(a => new KeyValuePair<int, string>((int) a, a.GetDescription()))
			};

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Route("DataFilterLookups/{fiscalYearId}")]
		public JsonResult GetDataFedLookups(int fiscalYearId)
		{
			var trainingProgramTitles = _trainingProgramService.GetTrainingProgramTitles(fiscalYearId);
			var intakePeriods = _fiscalYearService.GetTrainingPeriodLabels(fiscalYearId, null).ToList();

			var model = new
			{
				IntakePeriods = intakePeriods,
				SkillsTrainingCourseTitles = trainingProgramTitles
			};

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[Route("FiscalYears")]
		public JsonResult GetFiscalYears()
		{
			var fiscalYears = _staticDataService.GetFiscalYears(limitEndYearRange: 2)
				.Select(t => new
				{
					t.Id,
					t.Caption
				})
				.ToArray();

			return Json(fiscalYears, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Route("Streams/{fiscalYearId}")]
		public JsonResult GetGrantStreams(int fiscalYearId)
		{
			var fiscalYear = _staticDataService.GetFiscalYear(fiscalYearId);

			var grantStreams = fiscalYearId > 0
				? _grantStreamService.GetGrantStreams(fiscalYear.Id, null)
				: _grantStreamService.GetAll();

			var streams = grantStreams
				.Select(t => new
				{
					t.Id,
					t.Name,
					ShorterName = t.Name.TruncateWithEllipsis(45)
				})
				.ToArray();

			return Json(streams, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Route("Regions")]
		public JsonResult GetCommunityRegions()
		{
			var communities = _communityService.GetAll();
			var regions = communities
				.Select(community => community.GetRegionName())
				.ToList();

			var programs = regions
				.Distinct()
				.OrderBy(r => r)
				.Select(t => new
				{
					Id = t,
					Name = t.TruncateWithEllipsis(45)
				})
				.ToArray();

			return Json(programs, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Route("Communities")]
		public JsonResult GetCommunities()
		{
			var communities = _communityService.GetAllInUse()
				.Select(t => new
				{
					t.Id,
					Name = t.GetCommunityName().TruncateWithEllipsis(45)
				})
				.ToArray();

			return Json(communities, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Route("TrainingLocations/{fiscalYearId?}")]
		public JsonResult GetTrainingLocations(int? fiscalYearId)
		{
			var cities = _trainingProviderService.GetTrainingLocationCities(fiscalYearId)
				.Select(c => new
				{
					Id = c,
					Name = c
				})
				.ToArray();

			return Json(cities, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Route("NOCs")]
		public JsonResult GetNOCs()
		{
			var cities = _nationalOccupationalClassificationService.GetNationalOccupationalClassificationsInUse()
				.Select(c => new
				{
					Id = c.Id,
					Name = $"{ c.Code } | { c.Description } ({c.NOCVersion})",
					ShorterName = $"{ c.Code } | { c.Description.TruncateWithEllipsis(45) } ({c.NOCVersion})"
				})
				.ToArray();

			return Json(cities, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[ValidateRequestHeader]
		[Route("Applications")]
		public JsonResult GetGrantApplications(StreamAgreementDetailsFilterModel filter)
		{
			var model = new List<SteamAgreementApplicationModel>();

			try
			{
				var applicationModels = _grantApplicationService
					.GetGrantApplications(filter)
					.Select(a => new SteamAgreementApplicationModel(a))
					.AsQueryable();

				model = SortApplications(applicationModels, filter)
					.ToList();
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);

				return Json(filter);
			}

			return Json(model);
		}

		[Route("Applications/Export")]
		public ActionResult ExportApplicationsToExcel(string filter)
		{
			try
			{
				var decodedFilter = JsonConvert.DeserializeObject<StreamAgreementDetailsFilterModel>(filter)
				                    ?? new StreamAgreementDetailsFilterModel();

				var applicationModels = _grantApplicationService
					.GetGrantApplications(decodedFilter)
					.Select(a => new SteamAgreementApplicationModel(a))
					.AsQueryable();

				var excelOutput = GetExcelContent(SortApplications(applicationModels, decodedFilter));
				var fileDownloadName = $"stream_details_{AppDateTime.Now:yyyy-MM-dd_HH-mm}.xlsx";

				return File(excelOutput, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileDownloadName);
			}
			catch (Exception ex)
			{
				HandleAngularException(ex);

				return Json(filter, JsonRequestBehavior.AllowGet);
			}
		}

		private IEnumerable<SteamAgreementApplicationModel> SortApplications(IQueryable<SteamAgreementApplicationModel> applications, StreamAgreementDetailsFilterModel filter)
		{
			var orderBy = filter.OrderBy != null && filter.OrderBy.Length > 0 ? filter.OrderBy : new[] { "FileNumber desc" };
			applications = applications.OrderByProperty(orderBy);
			return applications.AsEnumerable();
		}

		private byte[] GetExcelContent(IEnumerable<SteamAgreementApplicationModel> applications)
		{
			using (var stream = new MemoryStream())
			{
				var wb = new XLWorkbook();
				var ws = wb.AddWorksheet();

				ws.ShowRowColHeaders = true;

				CreateExcelHeaders(ws);

				ws.Cell("A2")
					.InsertData(applications.Select(a => new
						{
							a.FiscalYear,
							a.GrantStreamName,
							a.FileNumber,
							a.ApplicationStateInternalCaption,
							a.DateStatusChangedToClosed,
							a.Applicant,
							a.TrainingCourseTitle,
							a.ProjectDescription,
							a.TrainingProviderName,
							a.ESSTrainingProviderName,
							a.ScheduleAAmount,
							a.TotalClaimAssessment,
							a.AverageCostPerParticipant,
							a.NumberOfPIFsInClaim,
							a.NAIC,
							a.NOC,
							a.DeliveryStartDate,
							a.DeliveryEndDate,
							a.TrainingStartDate,
							a.TrainingEndDate,
							a.RequestedNumberOfParticipants,
							a.TrainingLocation,
							a.ModeOfInstruction,
							a.CommunityNames,
							a.Region
						})
						.ToList());

				ws.Columns("G").Style.Alignment.WrapText = true;
				ws.Columns("H").Style.Alignment.WrapText = true;
				ws.ColumnsUsed().AdjustToContents(5d, 50d);

				ws.RowsUsed().CellsUsed().Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
				ws.RowsUsed().AdjustToContents();

				wb.SaveAs(stream);

				return stream.ToArray();
			}
		}

		private void CreateExcelHeaders(IXLWorksheet ws)
		{
			CreateHeaderCell(ws, "A", "Fiscal Year");
			CreateHeaderCell(ws, "B", "Stream");
			CreateHeaderCell(ws, "C", "Agreement #");
			CreateHeaderCell(ws, "D", "Status");
			CreateHeaderCell(ws, "E", "Date Status changed to Closed");
			CreateHeaderCell(ws, "F", "Agreement Holder");
			CreateHeaderCell(ws, "G", "Skills Training Course Title");
			CreateHeaderCell(ws, "H", "Project Description");
			CreateHeaderCell(ws, "I", "Training Provider");
			CreateHeaderCell(ws, "J", "ESS Training Provider");
			CreateHeaderCell(ws, "K", "Schedule A Amount");
			CreateHeaderCell(ws, "L", "Total Claim Assessment");
			CreateHeaderCell(ws, "M", "Avg. Cost per Participant");
			CreateHeaderCell(ws, "N", "Number of PIFs in Claim");
			CreateHeaderCell(ws, "O", "NAIC");
			CreateHeaderCell(ws, "P", "NOC");
			CreateHeaderCell(ws, "Q", "Delivery Start Date");
			CreateHeaderCell(ws, "R", "Delivery End Date");
			CreateHeaderCell(ws, "S", "Training Start Date");
			CreateHeaderCell(ws, "T", "Training End Date");
			CreateHeaderCell(ws, "U", "Number of Participants (requested)");
			CreateHeaderCell(ws, "V", "Training Location");
			CreateHeaderCell(ws, "W", "Mode of Instruction");
			CreateHeaderCell(ws, "X", "Community Names");
			CreateHeaderCell(ws, "Y", "Regions");
		}

		private static void CreateHeaderCell(IXLWorksheet ws, string columnIndex, string columnName)
		{
			var headerCell = ws.Cell(1, columnIndex);
			headerCell.SetValue(columnName);
			headerCell.Style.Font.Bold = true;
		}
	}
}
