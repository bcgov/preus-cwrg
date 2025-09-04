using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CJG.Application.Business.Models.DirectorsReport;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Entities;
using NLog;

namespace CJG.Application.Services
{
	public class DirectorReportsService : Service, IDirectorReportsService
	{
		private readonly IAccountsReceivableService _accountsReceivableService;
		private readonly IProgramInitiativeService _programInitiativeService;

		internal enum CommitmentType
		{
			Requested,
			Agreed
		}

		public DirectorReportsService(
			IAccountsReceivableService accountsReceivableService,
			IProgramInitiativeService programInitiativeService,
			IDataContext context,
			HttpContextBase httpContext,
			ILogger logger) : base(context, httpContext, logger)
		{
			_accountsReceivableService = accountsReceivableService;
			_programInitiativeService = programInitiativeService;
		}

		public IEnumerable<DirectorBudget> GetDirectorBudgets(FiscalYear fiscalYear)
		{
			CreateDirectorBudgets(fiscalYear);
			CreateBudgetRowsForFiscal(fiscalYear);

			var directorBudgets = _dbContext.DirectorBudgets
				.Where(d => d.FiscalYearId == fiscalYear.Id)
				.Where(d => d.ProgramInitiativeId != null)
				.OrderBy(d => d.ProgramInitiative.RowSequence)
				.AsEnumerable();

			return directorBudgets;
		}

		private void CreateBudgetRowsForFiscal(FiscalYear fiscalYear)
		{
			var openingBudgetRows = _dbContext.DirectorBudgetRows
				.Where(dbr => dbr.FiscalYearId == fiscalYear.Id)
				.Any(dbr => dbr.EntryType == DirectorBudgetEntryType.Opening);

			var closingBudgetRows = _dbContext.DirectorBudgetRows
				.Where(dbr => dbr.FiscalYearId == fiscalYear.Id)
				.Any(dbr => dbr.EntryType == DirectorBudgetEntryType.Closing);

			if (!openingBudgetRows)
			{
				CreateBudgetRow(fiscalYear, 0, DirectorBudgetEntryType.Opening);
				CreateBudgetRow(fiscalYear, 1, DirectorBudgetEntryType.Opening);
				CreateBudgetRow(fiscalYear, 2, DirectorBudgetEntryType.Opening);
			}

			if (!closingBudgetRows)
			{
				CreateBudgetRow(fiscalYear, 0, DirectorBudgetEntryType.Closing);
				CreateBudgetRow(fiscalYear, 1, DirectorBudgetEntryType.Closing);
				CreateBudgetRow(fiscalYear, 2, DirectorBudgetEntryType.Closing);
				CreateBudgetRow(fiscalYear, 3, DirectorBudgetEntryType.Closing);
				CreateBudgetRow(fiscalYear, 4, DirectorBudgetEntryType.Closing);
			}

			_dbContext.SaveChanges();
		}

		private void CreateBudgetRow(FiscalYear fiscalYear, int i, DirectorBudgetEntryType entryType)
		{
			// Possibly put in a guard here for fiscal, sequence and entry to fill in gaps
			_dbContext.DirectorBudgetRows.Add(new DirectorBudgetRow
			{
				FiscalYearId = fiscalYear.Id,
				EntryType = entryType,
				Sequence = i,
				DateAdded = AppDateTime.UtcNow
			});
		}

		private void CreateDirectorBudgets(FiscalYear fiscalYear)
		{
			var initiatives = _programInitiativeService.GetAll()
				.Where(p => p.IsActive)
				.ToList();

			var initiativeIds = initiatives
				.Select(p => p.Id)
				.ToList();

			var haveDirectorBudgetsForFiscal = _dbContext.DirectorBudgets
				.Where(d => d.FiscalYearId == fiscalYear.Id)
				.Where(d => d.ProgramInitiative != null)
				.Select(d => d.ProgramInitiativeId.Value)
				.ToList()
				.SequenceEqual(initiativeIds);
			
			if (haveDirectorBudgetsForFiscal)
				return;

			foreach (var initiative in initiatives)
			{
				var budget = _dbContext.DirectorBudgets
					.Where(db => db.FiscalYearId == fiscalYear.Id)
					.FirstOrDefault(db => db.ProgramInitiativeId == initiative.Id);

				if (budget != null)
					continue;

				_dbContext.DirectorBudgets.Add(new DirectorBudget
				{
					BudgetEntryType = BudgetEntryType.CoreStream,
					FiscalYearId = fiscalYear.Id,
					StreamFilter = initiative.ToString(),
					ProgramInitiative = initiative,
					DateAdded = AppDateTime.UtcNow
				});
			}

			_dbContext.SaveChanges();
		}

		public IEnumerable<BudgetSummaryModel> GetBudgetSummaryModels(FiscalYear fiscalYear, List<DirectorBudget> directorBudgets)
		{
			var defaultGrantProgram = GetDefaultGrantProgram();

			var directorsReport = new List<BudgetSummaryModel>();

			foreach (var budget in directorBudgets)
			{
				var budgetTitle = budget.ProgramInitiative?.Name ?? "Not Assigned";
				var budgetStreams = budget.ProgramInitiative?.Code ?? "No Program Initiative";

				var grantApplications = GetApplicationsFor(fiscalYear, budget.ProgramInitiative).ToList();

				var nonDraftApplications = grantApplications
					.Where(ga => ga.ApplicationStateInternal != ApplicationStateInternal.Draft
					             || ga.ApplicationStateInternal == ApplicationStateInternal.Draft && ga.ReturnedToDraft != null)
					.ToList();

				var newApplicationsPreAssignment = GetIntakeNumbersFor(grantApplications, CommitmentType.Requested, new List<ApplicationStateInternal>
				{
					ApplicationStateInternal.New,
					ApplicationStateInternal.PendingAssessment
				});
				var newApplicationsPostAssignment = GetIntakeNumbersFor(grantApplications, CommitmentType.Agreed, new List<ApplicationStateInternal>
				{
					ApplicationStateInternal.UnderAssessment,
					ApplicationStateInternal.ReturnedToAssessment,
					ApplicationStateInternal.RecommendedForApproval
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
					ApplicationStateInternal.AgreementRejected,
					ApplicationStateInternal.ReturnedToDraft
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

				var (receivablesTotal, receivablesNumber) = GetReceivableValues(fiscalYear, budget.ProgramInitiative);

				//var cancelledTotal = cancelledAgreed.ValueOfApplications + cancelledRequests.ValueOfApplications;
				var cancelledTotalCost = cancelledAgreed.TotalCostOfApplications + cancelledRequests.TotalCostOfApplications;
				var claimTotal = GetClaimTotal(grantApplications, defaultGrantProgram);
				//var unclaimedTotal = GetUnclaimedTotal(grantApplications, defaultGrantProgram);
				//var numberOfClaimsSubmitted = GetTotalClaimsSubmitted(grantApplications);

				var commitmentAmount = committedWithNoClaims.NumberOfApplications + committedWithClaims.NumberOfApplications;
				var commitmentTotal = committedWithNoClaims.ValueOfApplications + committedWithClaims.ValueOfApplications;
				var commitmentTotalCost = committedWithNoClaims.TotalCostOfApplications + committedWithClaims.TotalCostOfApplications;

				var intakeCount = nonDraftApplications.Count;
				//var intakeTotal = newApplicationsPreAssignment.ValueOfApplications
				//					+ newApplicationsPostAssignment.ValueOfApplications +
				//				    + commitmentTotal
				//					+ cancelledTotal;
				var intakeTotalCost = newApplicationsPreAssignment.TotalCostOfApplications
									+ newApplicationsPostAssignment.TotalCostOfApplications +
								    + commitmentTotal
									+ cancelledTotalCost;
				// Slippage is the committed Schedule A amount of agreements with claims submitted minus Claims $ Processed  
				//var slippageTotal = committedWithClaims.ValueOfApplications - claimTotal;



				/*
				Slippage = Schedule A of all agreements that have a submitted a claim
							(minus)
						   the approved Claimed Amount (files in Completion Reporting status only)

				Unclaimed amount = Schedule A amount of all agreements
									(minus)
								   the approved Claimed Amount (files in Completion Reporting status only)
									(minus)
								   Slippage

				 */
				var directorsReportCommittedScheduleA = commitmentTotalCost;
				var directorsReportClaimsProcessed = claimTotal;
				var directorsReportSlippage = committedWithClaims.TotalCostOfApplications - claimTotal;
				var directorsReportUnclaimed = commitmentTotalCost - (claimTotal + committedWithNoClaims.ValueOfApplications) - directorsReportSlippage;
				var directorsReportReceivables = receivablesTotal;
				var directorsReportYtdActual = directorsReportClaimsProcessed - directorsReportReceivables;

				var budgetModel = new BudgetSummaryModel
				{
					DirectorBudgetId = budget.Id,

					GroupingName = budgetTitle,
					GroupingStreams = budgetStreams,
					IncludeInSlippageCalculation = true,  //budget.BudgetEntryType == BudgetEntryType.CoreStream,

					Budget = budget.Budget ?? 0m,
					//NewApplicationsTotal = newApplicationsPreAssignment.ValueOfApplications + newApplicationsPostAssignment.ValueOfApplications,

					//ApplicationsReceived = intakeCount,
					//ApplicationsReceivedTotal = intakeTotal,
					//CancelledApplicationsTotal = cancelledTotal,
					//ForecastCommitmentAmount = intakeTotal - cancelledTotal,
					//ApplicationsApproved = commitmentAmount,
					//ApprovedCommitmentAmount = commitmentTotalCost,

					//ClaimsProcessedTotal = claimTotal,
					//NumberOfClaimsSubmitted = numberOfClaimsSubmitted,
					//NumberOfClaimsLeftToSubmit = commitmentAmount - numberOfClaimsSubmitted,
					//ClaimsUnclaimedTotal = commitmentTotal - claimTotal,
					//ReceivablesSetupNumber = receivablesNumber,
					//ReceivablesSetupTotal = receivablesTotal,

					//SlippageTotal = slippageTotal,
					//ForecastBudget = budget.ForecastBudget ?? 0m,

					SummaryReportApplicationsReceived = intakeCount,
					SummaryReportApplicationsApproved = commitmentAmount,
					SummaryReportApprovedCommitmentAmount = commitmentTotalCost,
					SummaryReportApplicationsPendingDecision = newApplicationsPreAssignment.TotalCostOfApplications + newApplicationsPostAssignment.TotalCostOfApplications,
					SummaryReportCancelledApplicationsTotal = cancelledTotalCost,
					SummaryReportApplicationsReceivedTotal = intakeTotalCost,

					DirectorsReportCommittedScheduleA = directorsReportCommittedScheduleA,
					DirectorsReportClaimsProcessed = directorsReportClaimsProcessed,
					DirectorsReportUnclaimed = directorsReportUnclaimed,
					DirectorsReportReceivables = directorsReportReceivables,
					DirectorsReportSlippage = directorsReportSlippage,
					DirectorsReportYtdActual = directorsReportYtdActual,
					// This is the hardcoded part of the calculation - Front end should subtract this value from the 'Adjusted Budget'
					DirectorsReportPartialAvailableBudget = directorsReportCommittedScheduleA + directorsReportReceivables + directorsReportSlippage,

					//Unclaimed $ Calculation should be as follows:
					//  Committed $ Approved from Directors Report
					//   MINUS Claims $ Processed from Finance Report
					//   MINUS Slippage $ from Finance Report.
					//ClaimsUnclaimedTotal = commitmentTotal - claimTotal - slippageTotal,

					/*
						"Sum of all schedule A of agreements that have not had a claim submitted & sum of Schedule A of agreements
						which have a claim in but are still in the statuses below:
							NewClaim
							ClaimAssessEligibility
							ClaimAssessReimbursement
							Claim Returned to Applicant"
					 
					 */

					LastUpdated = budget.DateUpdated
				};

				directorsReport.Add(budgetModel);
			}

			return directorsReport;
		}

		private (decimal receivablesTotal, int receivablesNumber) GetReceivableValues(FiscalYear fiscalYear, ProgramInitiative programInitiative)
		{
			var receivableData = _accountsReceivableService.GetAccountsReceivableReportDataAsInitiatives(fiscalYear.Id, programInitiative);

			return receivableData != null
				? (receivableData.Total, receivableData.Number)
				: (0, 0);
		}

		//private (decimal receivablesTotal, int receivablesNumber) GetReceivableValues(FiscalYear fiscalYear, ProgramInitiative programInitiative)
		//{
		//	var receivableData = _accountsReceivableService.GetAccountsReceivableReportData(fiscalYear.Id);
		//	var receivableItem = receivableData.FirstOrDefault(r => r.FiscalYearId == fiscalYear.Id) ?? new AccountsReceivableBreakdown();
		//	var receivablesTotal = 0m;
		//	var receivablesNumber = 0;

		//	if (budgetBudgetEntryType == BudgetEntryType.CoreStream)
		//	{
		//		receivablesTotal = receivableItem.CoreApplicationTotal;
		//		receivablesNumber = receivableItem.CoreApplicationNumber;
		//	}

		//	if (budgetBudgetEntryType == BudgetEntryType.NonCore)
		//	{
		//		receivablesTotal = receivableItem.CRSApplicationTotal;
		//		receivablesNumber = receivableItem.CRSApplicationNumber;
		//	}

		//	return (receivablesTotal, receivablesNumber);
		//}

		private decimal GetClaimTotal(List<GrantApplication> grantApplications, GrantProgram grantProgram)
		{
			var statesForTotalNumberOfAgreements = StateExtensions.GetInternalStatesForSummary();

			var statesForProcessedPayments = new List<ApplicationStateInternal> {
				//ApplicationStateInternal.ClaimApproved,
				//ApplicationStateInternal.Closed,
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

		private decimal GetUnclaimedTotal(List<GrantApplication> grantApplications, GrantProgram grantProgram)
		{
			var statesForTotalNumberOfAgreements = StateExtensions.GetInternalStatesForSummary();

			var statesForProcessedPayments = new List<ApplicationStateInternal> {
				ApplicationStateInternal.NewClaim,
				ApplicationStateInternal.ClaimAssessEligibility,
				ApplicationStateInternal.ClaimAssessReimbursement,
				ApplicationStateInternal.ClaimReturnedToApplicant,
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

		private IEnumerable<GrantApplication> GetApplicationsFor(FiscalYear fiscalYear, ProgramInitiative programInitiative)
		{
			var programInitiativeId = programInitiative?.Id;  // Allows null initiatives allows us to report on applications with no assigned initiative

			return _dbContext.GrantApplications
				.Where(a => a.GrantOpening.TrainingPeriod.FiscalYearId == fiscalYear.Id)
				.Where(a => a.ProgramInitiativeId == programInitiativeId);
		}

		private (int NumberOfApplications, decimal ValueOfApplications, decimal TotalCostOfApplications) GetIntakeNumbersFor(IEnumerable<GrantApplication> grantApplications, CommitmentType commitmentType, IEnumerable<ApplicationStateInternal> internalStates)
		{
			var checkStates = internalStates.ToList();
			var applications = grantApplications.ToList();

			var checkForReturnedToDraft = checkStates.Contains(ApplicationStateInternal.ReturnedToDraft);

			var applicationsWithStatus = applications
				.Where(g => checkStates.Contains(g.ApplicationStateInternal))
				.ToList();

			var count = applicationsWithStatus.Count;
			var totalValue = applicationsWithStatus.Sum(ga => commitmentType == CommitmentType.Requested
				? ga.GetEstimatedReimbursement()
				: ga.GetAgreedCommitment());
			var totalCost = GetIntakeCost(applicationsWithStatus, commitmentType);

			if (!checkForReturnedToDraft)
				return (count, totalValue, totalCost);

			var applicationsReturnedToDraft = applications
				.Where(g => g.ApplicationStateInternal == ApplicationStateInternal.Draft)
				.Where(g => g.ReturnedToDraft != null)
				.ToList();

			var draftCount = applicationsReturnedToDraft.Count;
			var draftTotalValue = applicationsReturnedToDraft.Sum(ga => commitmentType == CommitmentType.Requested
				? ga.GetEstimatedReimbursement()
				: ga.GetAgreedCommitment());
			var draftTotalCost = GetIntakeCost(applicationsReturnedToDraft, commitmentType);

			count += draftCount;
			totalValue += draftTotalValue;
			totalCost += draftTotalCost;

			return (count, totalValue, totalCost);
		}

		private static decimal GetIntakeCost(List<GrantApplication> grantApplications, CommitmentType commitmentType)
		{
			var trainingCost = 0m;

			if (commitmentType == CommitmentType.Requested)
				trainingCost = grantApplications.Sum(ga => ga?.TrainingCost?.TotalEstimatedCost) ?? 0;

			if (commitmentType == CommitmentType.Agreed)
				trainingCost = grantApplications.Sum(ga => ga?.TrainingCost?.TotalAgreedMaxCost) ?? 0;

			return Math.Round(trainingCost, 2);
		}

		public static decimal GetEstimatedReimbursement(GrantApplication grantApplication)
		{
			return Math.Round(grantApplication?.TrainingCost?.TotalEstimatedReimbursement ?? 0, 2);
		}

		/// <summary>
		/// Get the total agreed commitment of all training programs in this grant application.
		/// </summary>
		/// <param name="grantApplication"></param>
		/// <returns></returns>
		public static decimal GetAgreedCommitment(GrantApplication grantApplication)
		{
			return Math.Round(grantApplication?.TrainingCost?.AgreedCommitment ?? 0, 2);
		}

		public void UpdateBudget(List<BudgetSummaryModel> budgetSummaries, List<BudgetRowModel> openingRows, List<BudgetRowModel> closingRows)
		{
			if (!budgetSummaries.Any())
				return;

			foreach (var budgetEntry in budgetSummaries)
			{
				var budget = _dbContext.DirectorBudgets.Find(budgetEntry.DirectorBudgetId);

				if (budget == null)
					continue;

				budget.Budget = budgetEntry.Budget;

				_dbContext.Update(budget);
			}

			UpdateBudgetEntriesFor(openingRows);
			UpdateBudgetEntriesFor(closingRows);

			CommitTransaction();
		}

		private void UpdateBudgetEntriesFor(IEnumerable<BudgetRowModel> budgetRowModels)
		{
			foreach (var row in budgetRowModels)
			{
				var budgetRow = _dbContext.DirectorBudgetRows.Find(row.BudgetRowId);

				if (budgetRow == null) // Shouldn't get this condition, but guard against it
					continue;

				if (budgetRow.Name != row.Name)
				{
					budgetRow.Name = row.Name;
					_dbContext.Update(budgetRow);
				}

				foreach (var entry in row.DirectorBudgetEntries)
				{
					var budgetEntry = _dbContext.DirectorBudgetEntries
						.Where(e => e.DirectorBudgetId == entry.DirectorBudgetId)
						.Where(e => e.DirectorBudgetRowId == entry.BudgetRowId)
						.FirstOrDefault();

					if (budgetEntry == null)
						continue;

					budgetEntry.Budget = entry.Budget;
					_dbContext.Update(budgetEntry);
				}
			}
		}

		public List<BudgetRowModel> GetBudgetRows(FiscalYear fiscalYear, List<DirectorBudget> directorBudgets, DirectorBudgetEntryType budgetEntryType)
		{
			var rows = _dbContext.DirectorBudgetRows
				.Where(d => d.FiscalYearId == fiscalYear.Id)
				.Where(d => d.EntryType == budgetEntryType)
				.OrderBy(d => d.Sequence)
				.ToList()
				.Select(d => new BudgetRowModel
				{
					BudgetRowId = d.Id,
					Name = d.Name,
					Sequence = d.Sequence,
					DirectorBudgetEntries = GetDirectorBudgetEntries(d, directorBudgets)
				});

			return rows.ToList();
		}

		private List<BudgetEntryModel> GetDirectorBudgetEntries(DirectorBudgetRow budgetRow, List<DirectorBudget> directorBudgets)
		{
			foreach (var directorBudget in directorBudgets)
			{
				if (directorBudget.ProgramInitiative == null) // We don't want to try to save budget entries for unassigned DirectorBudgets
					continue;

				var hasEntry = _dbContext.DirectorBudgetEntries
					.Where(e => e.DirectorBudgetId == directorBudget.Id)
					.Where(e => e.DirectorBudgetRowId == budgetRow.Id)
					.Any();

				if (hasEntry)
					continue;

				var entry = new DirectorBudgetEntry
				{
					DirectorBudgetId = directorBudget.Id,
					DirectorBudgetRowId = budgetRow.Id,
					DateAdded = AppDateTime.UtcNow
				};

				_dbContext.DirectorBudgetEntries.Add(entry);
			}

			_dbContext.SaveChanges();
			
			var entries = _dbContext.DirectorBudgetEntries
				.Where(e => e.DirectorBudgetRowId == budgetRow.Id)
				.OrderBy(e => e.DirectorBudget.ProgramInitiative.RowSequence)
				.Select(e => new BudgetEntryModel
				{
					BudgetRowId = e.DirectorBudgetRowId,
					DirectorBudgetId = e.DirectorBudgetId,
					Budget = e.Budget
				});

			return entries.ToList();
		}
	}
}