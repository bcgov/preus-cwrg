using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Int.Models;

namespace CJG.Web.External.Helpers
{
    /// <summary>
    /// IntakeManagementBuilder class, provides methods to initialize GrantApplication Intake.
    /// </summary>
    public class IntakeManagementBuilder
    {
        private readonly IStaticDataService _staticDataService;
        private readonly IGrantProgramService _grantProgramService;
        private readonly IGrantStreamService _grantStreamService;
        private readonly IGrantOpeningService _grantOpeningService;
        private readonly ITrainingPeriodService _trainingPeriodService;

        public IntakeManagementBuilder(IStaticDataService staticDataService, IGrantProgramService grantProgramService, IGrantStreamService grantStreamService, IGrantOpeningService grantOpeningService, ITrainingPeriodService trainingPeriodService)
        {
            _staticDataService = staticDataService;
            _grantProgramService = grantProgramService;
            _grantStreamService = grantStreamService;
            _grantOpeningService = grantOpeningService;
            _trainingPeriodService = trainingPeriodService;
        }

        public IntakeManagementViewModel Build(int? fiscalYearId, int? grantStreamId)
        {
            var fiscalYears = _staticDataService
                .GetFiscalYears()
                .Select(fy => new KeyValuePair<int, string>(fy.Id, fy.Caption))
                .ToList();

            if (!fiscalYearId.HasValue)
                fiscalYearId = _staticDataService.GetFiscalYear(0).Id;

            var grantProgramId = _grantProgramService.GetDefaultGrantProgramId();

            var grantPrograms = GetAllGrantPrograms().ToList();
            var grantStreams = GetActiveGrantStreams(grantProgramId).ToList();
            grantStreamId = GetDefaultGrantStreamId(grantStreams, grantStreamId);
			grantStreams.Add(new KeyValuePair<int, string>(-1, "WDA Streams"));

            var periods = new List<IntakeManagementViewModel.TrainingPeriodViewModel>();

			// Specific Grant Stream
            if (grantStreamId.Value > 0)
            {
	            var trainingPeriods = _trainingPeriodService.GetAllFor(fiscalYearId.Value, grantProgramId, grantStreamId.Value);
	            periods = trainingPeriods.Select(x => LoadPeriod(x, grantStreamId.Value)).ToList();
			}

			// "WDA Streams" only
			if (grantStreamId.Value == -1)
			{
				var wdaGrantStreams = GetActiveWDAGrantStreams(grantProgramId);
				var wdaTrainingPeriods = new List<TrainingPeriod>();
				var wdaPeriods = new List<IntakeManagementViewModel.TrainingPeriodViewModel>();

				foreach (var stream in wdaGrantStreams)
				{
					var streamId = stream.Key;
					wdaTrainingPeriods.AddRange(_trainingPeriodService.GetAllFor(fiscalYearId.Value, grantProgramId, streamId));
					wdaPeriods.AddRange(wdaTrainingPeriods.Select(x => LoadPeriod(x, streamId)).ToList());
				}
				
				periods = wdaPeriods
					.GroupBy(p => p.TrainingPeriodName)
					//.ToList()
					.Select(c => new IntakeManagementViewModel.TrainingPeriodViewModel
					{
						TrainingPeriodName = c.Key,
						TotalApplicationsIntake = c.Sum(d => d.TotalApplicationsIntake),
						TotalApplicationsIntakeAmt = c.Sum(d => d.TotalApplicationsIntakeAmt),
						DateRange = string.Join(Environment.NewLine, c.OrderBy(d => d.StartDate)
							.ThenBy(d => d.EndDate)
							.Select(d => d.DateRange)
							.Distinct()),
						GrantOpeningIntakes = CombineIntakes(c.SelectMany(d => d.GrantOpeningIntakes))
					}).ToList();
			}

            return new IntakeManagementViewModel
            {
                TrainingPeriods = periods,
                FiscalYearId = fiscalYearId,
                FiscalYears = fiscalYears,
                GrantPrograms = grantPrograms,
                GrantStreams = grantStreams,
                GrantProgramId = grantProgramId,
                GrantStreamId = grantStreamId,
            };
        }

        private Dictionary<int, IntakeManagementViewModel.GrantOpeningIntakeViewModel> CombineIntakes(IEnumerable<KeyValuePair<int, IntakeManagementViewModel.GrantOpeningIntakeViewModel>> allIntakes)
        {
	        var output = new Dictionary<int, IntakeManagementViewModel.GrantOpeningIntakeViewModel>();
			foreach (var dictionary in allIntakes)
	        {
		        if (!output.ContainsKey(dictionary.Key))
			        output.Add(dictionary.Key, new IntakeManagementViewModel.GrantOpeningIntakeViewModel(dictionary.Value.StateName, 0, 0));

		        var intakeEntry = output[dictionary.Key];
		        intakeEntry.Value += dictionary.Value.Value;
		        intakeEntry.Number += dictionary.Value.Number;
	        }

	        return output;
		}

		private IEnumerable<KeyValuePair<int, string>> GetAllGrantPrograms()
        {
            return _grantProgramService.GetAll()
                .Select(x => new KeyValuePair<int, string>(x.Id, x.Name))
                .OrderBy(x => x.Value);
        }

        private IEnumerable<KeyValuePair<int, string>> GetActiveGrantStreams(int grantProgramId)
        {
            return _grantStreamService.GetAll()
                .Where(x => x.GrantProgramId == grantProgramId && x.IsActive)
                .Select(x => new KeyValuePair<int, string>(x.Id, x.Name))
                .OrderBy(x => x.Value);
        }

        private IEnumerable<KeyValuePair<int, string>> GetActiveWDAGrantStreams(int grantProgramId)
        {
            return _grantStreamService.GetAll()
                .Where(x => x.GrantProgramId == grantProgramId && x.IsActive)
                .Where(x => x.IsCoreStream)
                .Select(x => new KeyValuePair<int, string>(x.Id, x.Name))
                .OrderBy(x => x.Value);
        }

        private IntakeManagementViewModel.TrainingPeriodViewModel LoadPeriod(TrainingPeriod trainingPeriod, int grantStreamId)
        {
            var grantOpening = _grantOpeningService.GetGrantOpeningWithApplications(grantStreamId, trainingPeriod.Id);
            var grantApplications = grantOpening?.GrantApplications.ToList() ?? new List<GrantApplication>();

            var grantIntakes = LoadGrantOpeningIntakes(grantOpening, grantApplications);
            var intakeTotalApplications = grantIntakes.Values.Sum(a => a.Number);
            var intakeTotalAmount = grantIntakes.Values.Sum(a => a.Value);

            var trainingPeriodViewModel = new IntakeManagementViewModel.TrainingPeriodViewModel
            {
                Id = trainingPeriod.Id,
                FiscalYearName = trainingPeriod.FiscalYear.Caption,
                TrainingPeriodName = trainingPeriod.Caption,
                StartDate = trainingPeriod.StartDate,
                EndDate = trainingPeriod.EndDate,
				DateRange = $"{trainingPeriod.StartDate.ToString("yyyy-MM-dd")} to {trainingPeriod.EndDate.ToString("yyyy-MM-dd")}",
                Status = grantOpening?.State.ToString(),
                GrantOpeningIntakes = grantIntakes,
                TotalApplicationsIntake = intakeTotalApplications,
                TotalApplicationsIntakeAmt = intakeTotalAmount
            };

			return trainingPeriodViewModel;
        }

        private static Dictionary<int, IntakeManagementViewModel.GrantOpeningIntakeViewModel> LoadGrantOpeningIntakes(GrantOpening grantOpening, List<GrantApplication> grantApplications)
        {
            return new Dictionary<int, IntakeManagementViewModel.GrantOpeningIntakeViewModel>
            {
                {
                    1, GetIntakeModelFor(grantApplications, "New", CommitmentType.Requested, new List<ApplicationStateInternal> { ApplicationStateInternal.New })
                },
                {
                    2, GetIntakeModelFor(grantApplications, "Under Assessment *", CommitmentType.Requested, new List<ApplicationStateInternal> { ApplicationStateInternal.UnderAssessment, ApplicationStateInternal.PendingAssessment, ApplicationStateInternal.ReturnedToAssessment })
                },
                {
                    3, GetIntakeModelFor(grantApplications, "Recommended for Approval", CommitmentType.Agreed, new List<ApplicationStateInternal> { ApplicationStateInternal.RecommendedForApproval })
                },
                {
                    4, GetIntakeModelFor(grantApplications, "Recommended for Denial", CommitmentType.Requested, new List<ApplicationStateInternal> { ApplicationStateInternal.RecommendedForDenial })
                },
                {
                    5, GetIntakeModelFor(grantApplications, "Offer Issued", CommitmentType.Agreed, new List<ApplicationStateInternal> { ApplicationStateInternal.OfferIssued })
                },
                {
                    6, GetIntakeModelFor(grantApplications, "Denied", CommitmentType.Requested, new List<ApplicationStateInternal> { ApplicationStateInternal.ApplicationDenied })
                },
                {
                    7, GetIntakeModelFor(grantApplications, "Withdrawn", CommitmentType.Requested, new List<ApplicationStateInternal> { ApplicationStateInternal.ApplicationWithdrawn })
                },
                {
                    8, GetIntakeModelFor(grantApplications, "Offer Withdrawn", CommitmentType.Requested, new List<ApplicationStateInternal> { ApplicationStateInternal.OfferWithdrawn })
                },
                {
                    9, GetIntakeModelFor(grantApplications, "Cancelled by Agreement Holder", CommitmentType.Agreed, new List<ApplicationStateInternal> { ApplicationStateInternal.CancelledByAgreementHolder })
                },
                {
                    10, GetIntakeModelFor(grantApplications, "Cancelled by Ministry", CommitmentType.Agreed, new List<ApplicationStateInternal> { ApplicationStateInternal.CancelledByMinistry })
                },
                {
                    11, GetIntakeModelFor(grantApplications, "Agreement Rejected", CommitmentType.Agreed, new List<ApplicationStateInternal> { ApplicationStateInternal.AgreementRejected })
                },
                {
                    12, GetIntakeModelFor(grantApplications, "Committed **", CommitmentType.Agreed, new List<ApplicationStateInternal>
                    {
                        ApplicationStateInternal.AgreementAccepted,
                        ApplicationStateInternal.NewClaim,
                        ApplicationStateInternal.ClaimAssessEligibility,
                        ApplicationStateInternal.ClaimAssessReimbursement,
                        ApplicationStateInternal.ClaimApproved,
                        ApplicationStateInternal.ClaimReturnedToApplicant,
                        ApplicationStateInternal.CompletionReporting,
                        ApplicationStateInternal.Closed
                    })
                }
            };
        }

        private static IntakeManagementViewModel.GrantOpeningIntakeViewModel GetIntakeModelFor(IEnumerable<GrantApplication> grantApplications, string intakeGroupName, CommitmentType commitmentType, IEnumerable<ApplicationStateInternal> internalStates)
        {
            var applicationsWithStatus = grantApplications
                .Where(g => internalStates.Contains(g.ApplicationStateInternal))
                .ToList();

            var count = applicationsWithStatus.Count;
            var total = applicationsWithStatus.Sum(ga => commitmentType == CommitmentType.Requested ? ga.GetEstimatedReimbursement() : ga.GetAgreedCommitment());

            return new IntakeManagementViewModel.GrantOpeningIntakeViewModel(intakeGroupName, count, total);
        }

        public int GetDefaultGrantStreamId(IEnumerable<KeyValuePair<int, string>> grantStreams, int? grantStreamId)
        {
	        if (grantStreamId.HasValue && grantStreamId.Value == -1)
		        return -1;

            KeyValuePair<int, string> result;
            return (result = grantStreams.FirstOrDefault(o => o.Key == grantStreamId)).Equals(default(KeyValuePair<int, string>)) &&
                   (result = grantStreams.FirstOrDefault()).Equals(default(KeyValuePair<int, string>)) ?
                   0 : result.Key;
        }
    }

    internal enum CommitmentType
    {
        Requested,
        Agreed
    }
}