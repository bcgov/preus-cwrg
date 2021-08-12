using System.Collections.Generic;
using System.Linq;
using System.Web;
using CJG.Application.Business.Models;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Entities;
using NLog;

namespace CJG.Application.Services
{
	public class DirectorReportsService : Service, IDirectorReportsService
	{
		private readonly IAccountsReceivableService _accountsReceivableService;

		internal enum CommitmentType
		{
			Requested,
			Agreed
		}

		public DirectorReportsService(
			IAccountsReceivableService accountsReceivableService,
			IDataContext context,
			HttpContextBase httpContext,
			ILogger logger) : base(context, httpContext, logger)
		{
			_accountsReceivableService = accountsReceivableService;
		}

		public IEnumerable<DirectorBudget> GetDirectorBudgets(FiscalYear fiscalYear)
		{
			var haveDirectorBudgetsForFiscal = _dbContext.DirectorBudgets.Any(d => d.FiscalYearId == fiscalYear.Id);
			if (!haveDirectorBudgetsForFiscal)
				CreateBudgetEntries(fiscalYear);

			var directorBudgets = _dbContext.DirectorBudgets
				.Where(d => d.FiscalYearId == fiscalYear.Id)
				.OrderBy(d => d.BudgetEntryType)
				.AsEnumerable();

			return directorBudgets;
		}

		private void CreateBudgetEntries(FiscalYear fiscalYear)
		{
			_dbContext.DirectorBudgets.Add(new DirectorBudget
			{
				BudgetEntryType = BudgetEntryType.CoreStream,
				FiscalYearId = fiscalYear.Id,
				StreamFilter = null,
				DateAdded = AppDateTime.UtcNow
			});

			_dbContext.DirectorBudgets.Add(new DirectorBudget
			{
				BudgetEntryType = BudgetEntryType.NonCore,
				FiscalYearId = fiscalYear.Id,
				StreamFilter = "Community Response",  // Hardcoded until it changes
				DateAdded = AppDateTime.UtcNow
			});

			_dbContext.SaveChanges();
		}

		public IEnumerable<BudgetSummaryModel> GetBudgetSummaryModels(FiscalYear fiscalYear)
		{
			var budgets = GetDirectorBudgets(fiscalYear).ToList();

			var separatedStreamFilter = budgets
				.Where(b => b.BudgetEntryType == BudgetEntryType.NonCore)
				.Where(b => !string.IsNullOrWhiteSpace(b.StreamFilter))
				.Select(b => b.StreamFilter);

			var defaultGrantProgram = GetDefaultGrantProgram();

			// Possible limit fiscal year here
			var grantStreams = _dbContext.GrantStreams
				.Where(s => s.GrantProgramId == defaultGrantProgram.Id)
				.Where(s => s.IsActive);

			var directorsReport = new List<BudgetSummaryModel>();
			var grantApplications = new List<GrantApplication>();

			foreach (var budget in budgets)
			{
				var budgetTitle = "Stream Title";
				var budgetStreams = string.Empty;

				if (budget.BudgetEntryType == BudgetEntryType.CoreStream)
				{
					var combinedStreams = grantStreams
						.Where(s => !separatedStreamFilter.Contains(s.Name));

					budgetTitle = "Core Streams";
					budgetStreams = string.Join(", ", combinedStreams.Select(cs => cs.Name).OrderBy(cs => cs));
					grantApplications = GetApplicationsFor(fiscalYear, combinedStreams).ToList();
				}

				if (budget.BudgetEntryType == BudgetEntryType.NonCore)
				{
					var separatedStream = grantStreams
						.FirstOrDefault(s => s.Name == budget.StreamFilter);

					if (separatedStream == null)
						continue;

					budgetTitle = "CRS";
					budgetStreams = separatedStream.Name;
					grantApplications = GetApplicationsFor(fiscalYear, new List<GrantStream> { separatedStream }).ToList();
				}

				var nonDraftApplications = grantApplications
					.Where(ga => ga.ApplicationStateInternal != ApplicationStateInternal.Draft)
					.ToList();

				var newApplications = GetIntakeNumbersFor(grantApplications, CommitmentType.Requested, new List<ApplicationStateInternal>
				{
					ApplicationStateInternal.New,
					ApplicationStateInternal.PendingAssessment,
					ApplicationStateInternal.UnderAssessment,
					ApplicationStateInternal.ReturnedToAssessment
				});

				var cancelledRequests = GetIntakeNumbersFor(grantApplications, CommitmentType.Requested, new List<ApplicationStateInternal>
				{
					ApplicationStateInternal.RecommendedForDenial,
					ApplicationStateInternal.ApplicationDenied,
					ApplicationStateInternal.ApplicationWithdrawn,
					ApplicationStateInternal.OfferWithdrawn
				});

				var cancelledAgreed = GetIntakeNumbersFor(grantApplications, CommitmentType.Agreed, new List<ApplicationStateInternal>
				{
					ApplicationStateInternal.CancelledByAgreementHolder,
					ApplicationStateInternal.CancelledByMinistry,
					ApplicationStateInternal.AgreementRejected
				});

				var committedWithNoClaims = GetIntakeNumbersFor(grantApplications, CommitmentType.Agreed, new List<ApplicationStateInternal>
				{
					ApplicationStateInternal.AgreementAccepted
				});

				var committedWithClaims = GetIntakeNumbersFor(grantApplications, CommitmentType.Agreed, new List<ApplicationStateInternal>
				{
					ApplicationStateInternal.NewClaim,
					ApplicationStateInternal.ClaimAssessEligibility,
					ApplicationStateInternal.ClaimAssessReimbursement,
					ApplicationStateInternal.ClaimApproved,
					ApplicationStateInternal.ClaimReturnedToApplicant,
					ApplicationStateInternal.CompletionReporting,
					ApplicationStateInternal.Closed
				});

				var (receivablesTotal, receivablesNumber) = GetReceivableValues(fiscalYear, budget.BudgetEntryType);

				var cancelledTotal = cancelledAgreed.ValueOfApplications + cancelledRequests.ValueOfApplications;
				var claimTotal = GetClaimTotal(grantApplications, defaultGrantProgram);
				var numberOfClaimsSubmitted = GetTotalClaimsSubmitted(grantApplications);

				var commitmentAmount = committedWithNoClaims.NumberOfApplications + committedWithClaims.NumberOfApplications;
				var commitmentTotal = committedWithNoClaims.ValueOfApplications + committedWithClaims.ValueOfApplications;

				var intakeCount = nonDraftApplications.Count;
				var intakeTotal = newApplications.ValueOfApplications + commitmentTotal + cancelledTotal;
				// Slippage is the committed Schedule A amount of agreements with claims submitted minus Claims $ Processed  
				var slippageTotal = committedWithClaims.ValueOfApplications - claimTotal;

				var budgetModel = new BudgetSummaryModel
				{
					DirectorBudgetId = budget.Id,

					GroupingName = budgetTitle,
					GroupingStreams = budgetStreams,
					IncludeInSlippageCalculation = budget.BudgetEntryType == BudgetEntryType.CoreStream,

					Budget = budget.Budget ?? 0m,
					NewApplicationsTotal = newApplications.ValueOfApplications,

					ApplicationsReceived = intakeCount,
					ApplicationsReceivedTotal = intakeTotal,
					CancelledApplicationsTotal = cancelledTotal,
					ForecastCommitmentAmount = intakeTotal - cancelledTotal,
					ApplicationsApproved = commitmentAmount,
					ApprovedCommitmentAmount = commitmentTotal,

					ClaimsProcessedTotal = claimTotal,
					NumberOfClaimsSubmitted = numberOfClaimsSubmitted,
					NumberOfClaimsLeftToSubmit = commitmentAmount - numberOfClaimsSubmitted,
					//ClaimsUnclaimedTotal = commitmentTotal - claimTotal,
					ReceivablesSetupNumber = receivablesNumber,
					ReceivablesSetupTotal = receivablesTotal,

					SlippageTotal = slippageTotal,
					ForecastBudget = budget.ForecastBudget ?? 0m,

					//Unclaimed $ Calculation should be as follows:
					//  Committed $ Approved from Directors Report
					//   MINUS Claims $ Processed from Finance Report
					//   MINUS Slippage $ from Finance Report.
					ClaimsUnclaimedTotal = commitmentTotal - claimTotal - slippageTotal,

					LastUpdated = budget.DateUpdated
				};

				directorsReport.Add(budgetModel);
			}

			return directorsReport;
		}

		private (decimal receivablesTotal, int receivablesNumber) GetReceivableValues(FiscalYear fiscalYear, BudgetEntryType budgetBudgetEntryType)
		{
			var receivableData = _accountsReceivableService.GetAccountsReceivableReportData(fiscalYear.Id);
			var receivableItem = receivableData.FirstOrDefault(r => r.FiscalYearId == fiscalYear.Id) ?? new AccountsReceivableBreakdown();
			var receivablesTotal = 0m;
			var receivablesNumber = 0;

			if (budgetBudgetEntryType == BudgetEntryType.CoreStream)
			{
				receivablesTotal = receivableItem.CoreApplicationTotal;
				receivablesNumber = receivableItem.CoreApplicationNumber;
			}

			if (budgetBudgetEntryType == BudgetEntryType.NonCore)
			{
				receivablesTotal = receivableItem.CRSApplicationTotal;
				receivablesNumber = receivableItem.CRSApplicationNumber;
			}

			return (receivablesTotal, receivablesNumber);
		}

		private decimal GetClaimTotal(List<GrantApplication> grantApplications, GrantProgram grantProgram)
		{
			var statesForTotalNumberOfAgreements = StateExtensions.GetInternalStatesForSummary();

			var statesForProcessedPayments = new List<ApplicationStateInternal> {
				ApplicationStateInternal.ClaimApproved,
				ApplicationStateInternal.Closed,
				ApplicationStateInternal.CompletionReporting
			};

			var processedPaymentsTotals = 0m;

			var claimType = grantProgram?.ProgramConfiguration?.ClaimTypeId;

			// There can be multiple claims, but they will be versioned and we only care about the highest version
			if (claimType == ClaimTypes.SingleAmendableClaim)
			{
				// NOTE: For single amendable claims, these status filters may still be incorrect.
				// Current Payment Requests: Must sort payment request to get the final one.

				// Payments Processed: Must sort payment request to get the final one.
				processedPaymentsTotals = grantApplications.Where(ga => statesForProcessedPayments.Contains(ga.ApplicationStateInternal))
					.Sum(x => x.Claims.Where(c => c.IsApproved())
								  .OrderByDescending(c => c.ClaimVersion)
								  .FirstOrDefault()?.TotalAssessedReimbursement ?? 0);
			}

			// There can be multiple claims - we don't care about claim version
			if (claimType == ClaimTypes.MultipleClaimsWithoutAmendments)
			{
				// Payments Processed: must sum all claim totals where they're in an approved state
				processedPaymentsTotals = grantApplications.Where(ga => statesForTotalNumberOfAgreements.Contains(ga.ApplicationStateInternal))
					.SelectMany(x => x.Claims)
					.Where(c => c.IsApproved())
					.Sum(c => c.TotalAssessedReimbursement);
			}

			return processedPaymentsTotals;
		}

		private int GetTotalClaimsSubmitted(List<GrantApplication> grantApplications)
		{
			var statesForTotalNumberOfAgreements = StateExtensions.GetInternalStatesForSummary();
			var processedPaymentsTotals = grantApplications
				.Where(ga => statesForTotalNumberOfAgreements.Contains(ga.ApplicationStateInternal))
				.SelectMany(x => x.Claims)
				.Count(c => c.IsApproved());

			return processedPaymentsTotals;
		}

		private IEnumerable<GrantApplication> GetApplicationsFor(FiscalYear fiscalYear, IEnumerable<GrantStream> grantStreams)
		{
			var grantStreamIds = grantStreams.Select(gs => gs.Id);

			var applications = _dbContext.GrantApplications
				.Where(a => grantStreamIds.Contains(a.GrantOpening.GrantStreamId))
				.Where(a => a.GrantOpening.TrainingPeriod.FiscalYearId == fiscalYear.Id);

			return applications;
		}

		private (int NumberOfApplications, decimal ValueOfApplications) GetIntakeNumbersFor(IEnumerable<GrantApplication> grantApplications, CommitmentType commitmentType, IEnumerable<ApplicationStateInternal> internalStates)
		{
			var applicationsWithStatus = grantApplications
				.Where(g => internalStates.Contains(g.ApplicationStateInternal))
				.ToList();

			var count = applicationsWithStatus.Count;
			var total = applicationsWithStatus.Sum(ga => commitmentType == CommitmentType.Requested
				? ga.GetEstimatedReimbursement()
				: ga.GetAgreedCommitment());

			return (count, total);
		}

		public void UpdateBudget(List<BudgetSummaryModel> budgetSummaries)
		{
			if (!budgetSummaries.Any())
				return;

			foreach (var budgetEntry in budgetSummaries)
			{
				var budget = _dbContext.DirectorBudgets.Find(budgetEntry.DirectorBudgetId);

				if (budget == null)
					continue;

				budget.Budget = budgetEntry.Budget;
				budget.ForecastBudget = budgetEntry.ForecastBudget;
				//budget.WeeklyReceivablesNumber = budgetEntry.ReceivablesSetupNumber;
				//budget.WeeklyReceivablesTotal = budgetEntry.ReceivablesSetupTotal;

				_dbContext.Update(budget);
			}

			CommitTransaction();
		}
	}
}