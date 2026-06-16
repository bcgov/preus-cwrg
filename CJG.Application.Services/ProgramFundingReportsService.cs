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
	public class ProgramFundingReportsService : Service, IProgramFundingReportsService
	{
		private readonly IAccountsReceivableService _accountsReceivableService;
		private readonly IProgramInitiativeService _programInitiativeService;

		internal enum CommitmentType
		{
			Requested,
			Agreed
		}

		public ProgramFundingReportsService(
			IAccountsReceivableService accountsReceivableService,
			IProgramInitiativeService programInitiativeService,
			IDataContext context,
			HttpContextBase httpContext,
			ILogger logger) : base(context, httpContext, logger)
		{
			_accountsReceivableService = accountsReceivableService;
			_programInitiativeService = programInitiativeService;
		}

		public IEnumerable<ProgramFundingBudget> GetProgramFundingBudgets(FiscalYear fiscalYear)
		{
			CreateProgramFundingBudgets(fiscalYear);
			CreateBudgetRowsForFiscal(fiscalYear);

			var programFundingBudgets = _dbContext.ProgramFundingBudgets
				.Where(d => d.FiscalYearId == fiscalYear.Id)
				.Where(d => d.ProgramInitiativeId != null && d.ProgramInitiative.ShowInProgramFundingReport)
				.OrderBy(d => d.ProgramInitiative.RowSequence)
				.AsEnumerable();

			return programFundingBudgets;
		}

		private void CreateBudgetRowsForFiscal(FiscalYear fiscalYear)
		{
			var openingBudgetRows = _dbContext.ProgramFundingBudgetRows
				.Where(dbr => dbr.FiscalYearId == fiscalYear.Id)
				.Any(dbr => dbr.EntryType == ProgramFundingBudgetEntryType.Opening);

			var closingBudgetRows = _dbContext.ProgramFundingBudgetRows
				.Where(dbr => dbr.FiscalYearId == fiscalYear.Id)
				.Any(dbr => dbr.EntryType == ProgramFundingBudgetEntryType.Closing);

			if (!openingBudgetRows)
			{
				CreateBudgetRow(fiscalYear, 0, ProgramFundingBudgetEntryType.Opening);
				CreateBudgetRow(fiscalYear, 1, ProgramFundingBudgetEntryType.Opening);
				CreateBudgetRow(fiscalYear, 2, ProgramFundingBudgetEntryType.Opening);
			}

			if (!closingBudgetRows)
			{
				CreateBudgetRow(fiscalYear, 0, ProgramFundingBudgetEntryType.Closing);
				CreateBudgetRow(fiscalYear, 1, ProgramFundingBudgetEntryType.Closing);
				CreateBudgetRow(fiscalYear, 2, ProgramFundingBudgetEntryType.Closing);
				CreateBudgetRow(fiscalYear, 3, ProgramFundingBudgetEntryType.Closing);
				CreateBudgetRow(fiscalYear, 4, ProgramFundingBudgetEntryType.Closing);
			}

			_dbContext.SaveChanges();
		}

		private void CreateBudgetRow(FiscalYear fiscalYear, int i, ProgramFundingBudgetEntryType entryType)
		{
			// Possibly put in a guard here for fiscal, sequence and entry to fill in gaps
			_dbContext.ProgramFundingBudgetRows.Add(new ProgramFundingBudgetRow
			{
				FiscalYearId = fiscalYear.Id,
				EntryType = entryType,
				Sequence = i,
				DateAdded = AppDateTime.UtcNow
			});
		}

		private void CreateProgramFundingBudgets(FiscalYear fiscalYear)
		{
			var initiatives = _programInitiativeService.GetAll()
				.Where(p => p.IsActive)
				.ToList();

			var initiativeIds = initiatives
				.Select(p => p.Id)
				.ToList();

			var haveProgramFundingBudgetsForFiscal = _dbContext.ProgramFundingBudgets
				.Where(d => d.FiscalYearId == fiscalYear.Id)
				.Where(d => d.ProgramInitiative != null)
				.Select(d => d.ProgramInitiativeId.Value)
				.ToList()
				.SequenceEqual(initiativeIds);

			if (haveProgramFundingBudgetsForFiscal)
				return;

			foreach (var initiative in initiatives)
			{
				var budget = _dbContext.ProgramFundingBudgets
					.Where(db => db.FiscalYearId == fiscalYear.Id)
					.FirstOrDefault(db => db.ProgramInitiativeId == initiative.Id);

				if (budget != null)
					continue;

				_dbContext.ProgramFundingBudgets.Add(new ProgramFundingBudget
				{
					FiscalYearId = fiscalYear.Id,
					StreamFilter = initiative.ToString(),
					ProgramInitiative = initiative,
					DateAdded = AppDateTime.UtcNow
				});
			}

			_dbContext.SaveChanges();
		}

		public IEnumerable<BudgetSummaryModel> GetBudgetSummaryModels(FiscalYear fiscalYear, List<ProgramFundingBudget> programFundingBudgets)
		{
			var defaultGrantProgram = GetDefaultGrantProgram();

			var directorsReport = new List<BudgetSummaryModel>();

			foreach (var budget in programFundingBudgets)
			{
				var budgetTitle = budget.ProgramInitiative?.Name ?? "Not Assigned";
				var budgetStreams = budget.ProgramInitiative?.Code ?? "No Program Initiative";

				var grantApplications = GetApplicationsFor(fiscalYear).ToList();

				var commitmentTotal = GetCommitmentAmountFor(grantApplications, budget.ProgramInitiative, new List<ApplicationStateInternal>
				{
					ApplicationStateInternal.AgreementAccepted,
					ApplicationStateInternal.NewClaim,
					ApplicationStateInternal.ClaimAssessEligibility,
					ApplicationStateInternal.ClaimAssessReimbursement,
					ApplicationStateInternal.ClaimApproved,
					ApplicationStateInternal.ClaimReturnedToApplicant,
					ApplicationStateInternal.CompletionReporting,
					ApplicationStateInternal.Closed
				});

				var receivablesTotal = GetReceivableValues(fiscalYear, budget.ProgramInitiative);
				var claimTotal = GetClaimTotal(grantApplications, defaultGrantProgram, budget.ProgramInitiative);

				var directorsReportCommittedScheduleA = commitmentTotal;
				var directorsReportClaimsProcessed = claimTotal;
				var directorsReportSlippage = commitmentTotal - claimTotal;
				var directorsReportReceivables = receivablesTotal;
				var directorsReportYtdActual = directorsReportClaimsProcessed - directorsReportReceivables;

				var budgetModel = new BudgetSummaryModel
				{
					DirectorBudgetId = budget.Id,

					GroupingName = budgetTitle,
					GroupingStreams = budgetStreams,
					IncludeInSlippageCalculation = true,

					Budget = budget.Budget ?? 0m,

					DirectorsReportCommittedScheduleA = directorsReportCommittedScheduleA,
					DirectorsReportClaimsProcessed = directorsReportClaimsProcessed,
					DirectorsReportReceivables = directorsReportReceivables,
					DirectorsReportSlippage = directorsReportSlippage,
					DirectorsReportYtdActual = directorsReportYtdActual,

					// This is the hardcoded part of the calculation - Front end should subtract this value from the 'Adjusted Budget'
					DirectorsReportPartialAvailableBudget = directorsReportCommittedScheduleA + directorsReportReceivables + directorsReportSlippage,

					LastUpdated = budget.DateUpdated
				};

				directorsReport.Add(budgetModel);
			}

			return directorsReport;
		}

		private decimal GetReceivableValues(FiscalYear fiscalYear, ProgramInitiative programInitiative)
		{
			if (programInitiative == null)
				return 0;

			var receivableData = _accountsReceivableService.GetAccountsReceivableReportDataUnfiltered(fiscalYear.Id);

			var programCode = programInitiative.Code.ToUpper();

			if (programInitiative.IsLMDA)
				return receivableData.TotalLMDA;

			if (programInitiative.IsWDA)
				return receivableData.TotalWDA;

			return 0;
		}

		private decimal GetClaimTotal(List<GrantApplication> grantApplications, GrantProgram grantProgram, ProgramInitiative programInitiative)
		{
			if (programInitiative == null)
				return 0;

			var statesForTotalNumberOfAgreements = StateExtensions.GetInternalStatesForSummary();

			var statesForProcessedPayments = new List<ApplicationStateInternal> {
				ApplicationStateInternal.CompletionReporting
			};

			var processedPaymentsTotals = 0m;
			var claimType = grantProgram?.ProgramConfiguration?.ClaimTypeId;

			// There can be multiple claims, but they will be versioned and we only care about the highest version
			if (claimType == ClaimTypes.SingleAmendableClaim)
			{
				if (programInitiative.IsLMDA)
					processedPaymentsTotals = grantApplications.Where(ga => statesForProcessedPayments.Contains(ga.ApplicationStateInternal))
						.Sum(x => x.Claims.Where(c => c.IsApproved())
									  .OrderByDescending(c => c.ClaimVersion)
									  .FirstOrDefault()?.ClaimPayment?.PaidLMDA ?? 0);

				if (programInitiative.IsWDA)
					processedPaymentsTotals = grantApplications.Where(ga => statesForProcessedPayments.Contains(ga.ApplicationStateInternal))
						.Sum(x => x.Claims.Where(c => c.IsApproved())
									  .OrderByDescending(c => c.ClaimVersion)
									  .FirstOrDefault()?.ClaimPayment?.PaidWDA ?? 0);
			}

			// There can be multiple claims - we don't care about claim version
			if (claimType == ClaimTypes.MultipleClaimsWithoutAmendments)
			{
				if (programInitiative.IsLMDA)
					processedPaymentsTotals = grantApplications.Where(ga => statesForTotalNumberOfAgreements.Contains(ga.ApplicationStateInternal))
												  .SelectMany(x => x.Claims)
												  .Where(c => c.IsApproved())
												  .Sum(c => c.ClaimPayment?.PaidLMDA) ?? 0;

				if (programInitiative.IsWDA)
					processedPaymentsTotals = grantApplications.Where(ga => statesForTotalNumberOfAgreements.Contains(ga.ApplicationStateInternal))
												  .SelectMany(x => x.Claims)
												  .Where(c => c.IsApproved())
												  .Sum(c => c.ClaimPayment?.PaidWDA) ?? 0;
			}

			return processedPaymentsTotals;
		}

		private IEnumerable<GrantApplication> GetApplicationsFor(FiscalYear fiscalYear)
		{
			return _dbContext.GrantApplications
				.Where(a => a.GrantOpening.TrainingPeriod.FiscalYearId == fiscalYear.Id);
		}

		private decimal GetCommitmentAmountFor(IEnumerable<GrantApplication> grantApplications, ProgramInitiative programInitiative, IEnumerable<ApplicationStateInternal> internalStates)
		{
			if (programInitiative == null)
				return 0;

			var checkStates = internalStates.ToList();
			var applications = grantApplications.ToList();

			var applicationsWithStatus = applications
				.Where(g => checkStates.Contains(g.ApplicationStateInternal))
				.ToList();

			if (programInitiative.IsLMDA)
				return applicationsWithStatus.Sum(ga => ga.TrainingCost.CommittedLMDA) ?? 0;

			if (programInitiative.IsWDA)
				return applicationsWithStatus.Sum(ga => ga.TrainingCost.CommittedWDA) ?? 0;

			return 0;
		}

		public void UpdateBudget(List<BudgetSummaryModel> budgetSummaries, List<BudgetRowModel> openingRows, List<BudgetRowModel> closingRows)
		{
			if (!budgetSummaries.Any())
				return;

			foreach (var budgetEntry in budgetSummaries)
			{
				var budget = _dbContext.ProgramFundingBudgets.Find(budgetEntry.DirectorBudgetId);

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
				var budgetRow = _dbContext.ProgramFundingBudgetRows.Find(row.BudgetRowId);

				if (budgetRow == null) // Shouldn't get this condition, but guard against it
					continue;

				if (budgetRow.Name != row.Name)
				{
					budgetRow.Name = row.Name;
					_dbContext.Update(budgetRow);
				}

				foreach (var entry in row.DirectorBudgetEntries)
				{
					var budgetEntry = _dbContext.ProgramFundingBudgetEntries
						.Where(e => e.ProgramFundingBudgetId == entry.DirectorBudgetId)
						.Where(e => e.ProgramFundingBudgetRowId == entry.BudgetRowId)
						.FirstOrDefault();

					if (budgetEntry == null)
						continue;

					budgetEntry.Budget = entry.Budget;
					_dbContext.Update(budgetEntry);
				}
			}
		}

		public List<BudgetRowModel> GetBudgetRows(FiscalYear fiscalYear, List<ProgramFundingBudget> programFundingBudgets, ProgramFundingBudgetEntryType budgetEntryType)
		{
			var rows = _dbContext.ProgramFundingBudgetRows
				.Where(d => d.FiscalYearId == fiscalYear.Id)
				.Where(d => d.EntryType == budgetEntryType)
				.OrderBy(d => d.Sequence)
				.ToList()
				.Select(d => new BudgetRowModel
				{
					BudgetRowId = d.Id,
					Name = d.Name,
					Sequence = d.Sequence,
					DirectorBudgetEntries = GetDirectorBudgetEntries(d, programFundingBudgets)
				});

			return rows.ToList();
		}

		private List<BudgetEntryModel> GetDirectorBudgetEntries(ProgramFundingBudgetRow budgetRow, List<ProgramFundingBudget> programFundingBudgets)
		{
			foreach (var programFundingBudget in programFundingBudgets)
			{
				if (programFundingBudget.ProgramInitiative == null) // We don't want to try to save budget entries for unassigned ProgramFundingBudgets
					continue;

				var hasEntry = _dbContext.ProgramFundingBudgetEntries
					.Where(e => e.ProgramFundingBudgetId == programFundingBudget.Id)
					.Where(e => e.ProgramFundingBudgetRowId == budgetRow.Id)
					.Any();

				if (hasEntry)
					continue;

				var entry = new ProgramFundingBudgetEntry
				{
					ProgramFundingBudgetId = programFundingBudget.Id,
					ProgramFundingBudgetRowId = budgetRow.Id,
					DateAdded = AppDateTime.UtcNow
				};

				_dbContext.ProgramFundingBudgetEntries.Add(entry);
			}

			_dbContext.SaveChanges();

			var entries = _dbContext.ProgramFundingBudgetEntries
				.Where(e => e.ProgramFundingBudgetRowId == budgetRow.Id)
				.OrderBy(e => e.ProgramFundingBudget.ProgramInitiative.RowSequence)
				.Select(e => new BudgetEntryModel
				{
					BudgetRowId = e.ProgramFundingBudgetRowId,
					DirectorBudgetId = e.ProgramFundingBudgetId,
					Budget = e.Budget
				});

			return entries.ToList();
		}
	}
}