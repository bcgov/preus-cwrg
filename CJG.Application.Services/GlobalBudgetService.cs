using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using CJG.Application.Business.Models.GlobalBudget;
using CJG.Core.Entities;
using CJG.Core.Interfaces;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Entities;
using NLog;

namespace CJG.Application.Services
{
	public class GlobalBudgetService : Service, IGlobalBudgetService
	{
		private readonly IFiscalYearService _fiscalYearService;

		public GlobalBudgetService(IDataContext context, IFiscalYearService fiscalYearService, HttpContextBase httpContext, ILogger logger) : base(context, httpContext, logger)
		{
			_fiscalYearService = fiscalYearService;
		}

		public List<IntakePeriodSlotModel> GetIntakePeriodStreams(int? fiscalYearId)
		{
			var defaultProgramId = GetDefaultGrantProgramId();
			var fiscalYear = !fiscalYearId.HasValue
				? _fiscalYearService.GetCurrentFiscalYear()
				: _fiscalYearService.Get(fiscalYearId.Value);

			var trainingPeriods = _dbContext.TrainingPeriods
				.AsNoTracking()
				.Where(tp => tp.FiscalYearId == fiscalYear.Id)
				.Where(tp => tp.GrantStream.GrantProgramId == defaultProgramId);

			var openings = trainingPeriods
				.SelectMany(tp => tp.GrantOpenings)
				.Include(go => go.GrantApplications)
				.AsNoTracking()
				.OrderBy(go => go.GrantStream.Name);

			var grantProgramBudgets = _dbContext.GlobalProgramBudgets
				.AsNoTracking()
				.ToList();

			var streams = new List<GrantStreamBudgetModel>();
			foreach (var opening in openings)
			{
				var grantApplications = opening.GrantApplications.ToList();

				var underAssessment = GetTotalsFor(grantApplications, CommitmentType.Requested, new List<ApplicationStateInternal> { ApplicationStateInternal.UnderAssessment, ApplicationStateInternal.PendingAssessment, ApplicationStateInternal.ReturnedToAssessment });
				var committedUnclaimed = GetTotalsFor(grantApplications, CommitmentType.Agreed, new List<ApplicationStateInternal>
				{
					ApplicationStateInternal.AgreementAccepted
				});
				var committedClaimed = GetTotalsFor(grantApplications, CommitmentType.Agreed, new List<ApplicationStateInternal>
				{
					ApplicationStateInternal.NewClaim,
					ApplicationStateInternal.ClaimAssessEligibility,
					ApplicationStateInternal.ClaimAssessReimbursement,
					ApplicationStateInternal.ClaimApproved,
					ApplicationStateInternal.ClaimReturnedToApplicant,
					ApplicationStateInternal.CompletionReporting,
					ApplicationStateInternal.Closed
				});

				var forecast = GetCommitmentForecast(grantApplications);
				var totalPaid = GetTotalPaid(grantApplications);
				var committedAmount = committedUnclaimed.Value + committedClaimed.Value;
				var slippageAmount = committedAmount - totalPaid;
				var slippagePercent = committedAmount > 0 ? slippageAmount / committedAmount : 0;

				var existingBudget = grantProgramBudgets
					                     .Where(b => b.TrainingPeriodId == opening.TrainingPeriodId)
					                     .FirstOrDefault(b => b.GrantStreamId == opening.GrantStreamId)
					                     ?.IntakeBudget ?? 0;

				streams.Add(new GrantStreamBudgetModel
				{
					GrantStreamId = opening.GrantStream.Id,
					StreamName = opening.GrantStream.Name,
					TrainingPeriodId = opening.TrainingPeriodId,
					StreamBudget = existingBudget,
					ForecastedCommitment = forecast,
					ApplicationsUnderAssessment = underAssessment.Number,
					CommittedAmount = committedAmount,
					CommittedUnclaimedAmount = committedUnclaimed.Value,
					CommittedClaimedAmount = committedClaimed.Value,
					ApplicationsApproved = committedClaimed.Number + committedUnclaimed.Number,
					TotalSpent = totalPaid,

					SlippageAmount = slippageAmount,
					SlippagePercent = slippagePercent,

					SortOrder = opening.GrantStream.Name.ToLower() == "community response" ? 1 : 0
				});
			}

			return GroupIntakeModels(streams);
		}

		public void UpdateBudget(int? fiscalYearId, List<IntakePeriodSlotModel> intakePeriodsSlots)
		{
			if (!fiscalYearId.HasValue || !intakePeriodsSlots.Any())
				return;

			var openingModels = intakePeriodsSlots.SelectMany(i => i.Streams);

			var grantProgramBudgets = _dbContext
				.GlobalProgramBudgets
				.ToList();

			foreach (var opening in openingModels)
			{
				var globalBudget = grantProgramBudgets
					.Where(b => b.TrainingPeriodId == opening.TrainingPeriodId)
					.FirstOrDefault(b => b.GrantStreamId == opening.GrantStreamId);

				if (globalBudget == null)
				{
					globalBudget = new GlobalProgramBudget
					{
						TrainingPeriodId = opening.TrainingPeriodId,
						GrantStreamId = opening.GrantStreamId
					};
					_dbContext.GlobalProgramBudgets.Add(globalBudget);
				}

				globalBudget.IntakeBudget = opening.StreamBudget;
			}

			Commit();
		}

		private static List<IntakePeriodSlotModel> GroupIntakeModels(List<GrantStreamBudgetModel> streams)
		{
			var intakes = new List<IntakePeriodSlotModel>();
			var currentIntakeSlot = 0;
			var currentGrantStreamId = 0;

			var steamModels = streams
				.OrderBy(s => s.SortOrder)
				.ThenBy(s => s.StreamName);

			foreach (var stream in steamModels)
			{
				if (currentGrantStreamId != stream.GrantStreamId)
				{
					currentGrantStreamId = stream.GrantStreamId;
					currentIntakeSlot = 1;
				}

				var intakeSlot = intakes.FirstOrDefault(i => i.SlotSequence == currentIntakeSlot);
				if (intakeSlot == null)
				{
					intakeSlot = new IntakePeriodSlotModel
					{
						SlotSequence = currentIntakeSlot,
						Streams = new List<GrantStreamBudgetModel>()
					};

					intakes.Add(intakeSlot);
				}

				intakeSlot.Streams.Add(stream);
				currentIntakeSlot++;
			}

			return intakes;
		}

		private decimal GetTotalPaid(IEnumerable<GrantApplication> grantApplications)
		{
			var applicationClaims = grantApplications
				.SelectMany(ga => ga.Claims)
				.Where(q => q.ClaimState
					.In(ClaimState.ClaimApproved, ClaimState.PaymentRequested, ClaimState.ClaimPaid, ClaimState.AmountReceived))
				.ToList();

			// Sum the two claim types, SingleAmendableClaim and the rest.
			var singleAmendablePayments = applicationClaims.Where(c => c.ClaimTypeId == ClaimTypes.SingleAmendableClaim)
				.Sum(q => q.TotalAssessedReimbursement
				          - q.GrantApplication.PaymentRequests
					          .Where(o => o.ClaimVersion != q.ClaimVersion)
					          .Sum(o => o.PaymentAmount));

			var totalAmendablePayments = applicationClaims
				.Where(c => c.ClaimTypeId == ClaimTypes.MultipleClaimsWithoutAmendments)
				.Sum(q => q.TotalAssessedReimbursement);

			return singleAmendablePayments + totalAmendablePayments;
		}

		private static decimal GetCommitmentForecast(IReadOnlyCollection<GrantApplication> grantApplications)
		{
			var total = GetTotalsFor(grantApplications, CommitmentType.Requested, new List<ApplicationStateInternal> { ApplicationStateInternal.New, ApplicationStateInternal.UnderAssessment, ApplicationStateInternal.PendingAssessment, ApplicationStateInternal.ReturnedToAssessment }).Value
						+ GetTotalsFor(grantApplications, CommitmentType.Agreed, new List<ApplicationStateInternal>
			            {
				            ApplicationStateInternal.RecommendedForApproval,
							ApplicationStateInternal.OfferIssued,

							ApplicationStateInternal.AgreementAccepted,
				            ApplicationStateInternal.NewClaim,
				            ApplicationStateInternal.ClaimAssessEligibility,
				            ApplicationStateInternal.ClaimAssessReimbursement,
				            ApplicationStateInternal.ClaimApproved,
				            ApplicationStateInternal.ClaimReturnedToApplicant,
				            ApplicationStateInternal.CompletionReporting,
				            ApplicationStateInternal.Closed
			            }).Value;

			return total;
		}

		private static GrantOpeningIntakeViewModel GetTotalsFor(IEnumerable<GrantApplication> grantApplications, CommitmentType commitmentType, IEnumerable<ApplicationStateInternal> internalStates)
		{
			var applicationsWithStatus = grantApplications
				.Where(g => internalStates.Contains(g.ApplicationStateInternal))
				.ToList();

			var count = applicationsWithStatus.Count;
			var total = applicationsWithStatus.Sum(ga => commitmentType == CommitmentType.Requested ? ga.GetEstimatedReimbursement() : ga.GetAgreedCommitment());

			return new GrantOpeningIntakeViewModel(count, total);
		}

		internal enum CommitmentType
		{
			Requested,
			Agreed
		}

		public class GrantOpeningIntakeViewModel
		{
			public GrantOpeningIntakeViewModel(int number, decimal value)
			{
				Number = number;
				Value = value;
			}

			public int Number { get; }
			public decimal Value { get; }
		}
	}
}