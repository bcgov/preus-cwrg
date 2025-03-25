using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using CJG.Application.Business.Models;
using CJG.Application.Services.Exceptions;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Entities;
using NLog;

namespace CJG.Application.Services
{
	public class AccountsReceivablesService : Service, IAccountsReceivableService
	{
		private readonly INoteService _noteService;
		private readonly IFiscalYearService _fiscalYearService;

		public AccountsReceivablesService(
			INoteService noteService,
			IFiscalYearService fiscalYearService,
			IDataContext context,
			HttpContextBase httpContext,
			ILogger logger)
			: base(context, httpContext, logger)
		{
			_noteService = noteService;
			_fiscalYearService = fiscalYearService;
		}

		public AccountsReceivable GetByApplication(GrantApplication grantApplication)
		{
			return _dbContext.AccountsReceivables.FirstOrDefault(ar => ar.GrantApplication.Id == grantApplication.Id);
		}

		public void UpdateAccountsReceivables(ApplicationAccountsReceivableUpdateModel model)
		{
			var grantApplication = _dbContext.GrantApplications.Find(model.GrantApplicationId);

			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			if (!_httpContext.User.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditClaimAR))
				throw new NotAuthorizedException($"User does not have permission to edit application '{grantApplication?.Id}' accounts receivables.");


			var currentReceivable = _dbContext.AccountsReceivables
				.FirstOrDefault(ar => ar.GrantApplication.Id == grantApplication.Id);

			if (currentReceivable == null)
			{
				currentReceivable = new AccountsReceivable
				{
					GrantApplication = grantApplication,
					DateAdded = AppDateTime.UtcNow
				};

				_dbContext.AccountsReceivables.Add(currentReceivable);
			}

			currentReceivable.PaidDate = model.PaidDate;

			foreach (var ar in model.ReceivablesByServiceCategory)
			{
				var serviceCategoryId = ar.Key;
				var overpayment = ar.Value;

				var serviceCategory = _dbContext.ServiceCategories.Find(serviceCategoryId);
				if (serviceCategory == null)
					continue;

				var entry = currentReceivable.AccountsReceivableEntries.FirstOrDefault(e => e.ServiceCategory.Id == serviceCategoryId);

				if (entry != null)
					_dbContext.Update(entry);

				if (entry == null)
				{
					entry = new AccountsReceivableEntry
					{
						AccountsReceivable = currentReceivable,
						ServiceCategory = serviceCategory,
						DateAdded = AppDateTime.UtcNow
					};
					currentReceivable.AccountsReceivableEntries.Add(entry);
				}

				entry.Overpayment = overpayment;
			}

			_noteService.GenerateUpdateNote(grantApplication);

			_dbContext.Update(grantApplication);
			CommitTransaction();
		}

		public List<AccountsReceivableBreakdown> GetAccountsReceivableReportData(int? fiscalYearId = null)
		{
			var fiscalYears = new List<FiscalYear>();

			if (fiscalYearId.HasValue)
			{
				var fiscalYear = _fiscalYearService.Get(fiscalYearId.Value);
				fiscalYears.Add(fiscalYear);
			}
			else
			{
				var currentFiscalYear = _fiscalYearService.GetCurrentFiscalYear();
				fiscalYears = _fiscalYearService
					.GetFiscalYears()
					.Where(fy => fy.StartDate <= currentFiscalYear.EndDate)
					.ToList();
			}

			var defaultProgramId = GetDefaultGrantProgramId();

			var claimAccountsReceivableApplications = _dbContext.AccountsReceivables
				.Include(ar => ar.GrantApplication)
				.Include(ar => ar.AccountsReceivableEntries)
				.AsNoTracking()
				.Where(ar => ar.AccountsReceivableEntries.Any(c => c.Overpayment != 0))
				.Where(ar => ar.GrantApplication.GrantOpening.GrantStream.GrantProgramId == defaultProgramId)
				.Where(ar => ar.GrantApplication.GrantOpening.GrantStream.IsActive)
				.Where(ar => ar.GrantApplication.ProgramInitiative != null)
				.Select(ar => new
				{
					StreamName = ar.GrantApplication.GrantOpening.GrantStream.Name.ToLower(),
					AREntries = ar.AccountsReceivableEntries.Where(are => are.Overpayment != 0)
				})
				.ToList();

			var breakdowns = new List<AccountsReceivableBreakdown>();
			const string crsFilter = "community response";

			foreach (var fiscalYear in fiscalYears)
			{
				var fiscalStart = fiscalYear.StartDate;
				var fiscalEnd = fiscalYear.EndDate;

				var coreAccountsReceivables = claimAccountsReceivableApplications
					.Where(c => c.StreamName != crsFilter)
					.SelectMany(c => c.AREntries
						.Where(ar => ar.AccountsReceivable.PaidDate >= fiscalStart)
						.Where(ar => ar.AccountsReceivable.PaidDate <= fiscalEnd))
					.ToList();

				var crsAccountsReceivables = claimAccountsReceivableApplications
					.Where(c => c.StreamName == crsFilter)
					.SelectMany(c => c.AREntries
						.Where(ar => ar.AccountsReceivable.PaidDate >= fiscalStart)
						.Where(ar => ar.AccountsReceivable.PaidDate <= fiscalEnd))
					.ToList();

				var breakdown = new AccountsReceivableBreakdown
				{
					FiscalYearId = fiscalYear.Id,
					FiscalYear = fiscalYear.Caption,

					CoreApplicationNumber = coreAccountsReceivables.Select(ar => ar.AccountsReceivable.GrantApplicationId).Distinct().Count(),
					CRSApplicationNumber = crsAccountsReceivables.Select(ar => ar.AccountsReceivable.GrantApplicationId).Distinct().Count(),

					CoreApplicationTotal = coreAccountsReceivables.Sum(ar => ar.Overpayment),
					CRSApplicationTotal = crsAccountsReceivables.Sum(ar => ar.Overpayment)
				};

				breakdowns.Add(breakdown);
			}

			return breakdowns;
		}

		public AccountsReceivableInitiativeData GetAccountsReceivableReportDataAsInitiatives(int fiscalYearId, ProgramInitiative programInitiative)
		{
			var fiscalYear = _fiscalYearService.Get(fiscalYearId);
			var defaultProgramId = GetDefaultGrantProgramId();

			var fiscalStart = fiscalYear.StartDate;
			var fiscalEnd = fiscalYear.EndDate;

			var claimAccountsReceivableApplications = _dbContext.AccountsReceivables
				.Include(ar => ar.GrantApplication)
				.Include(ar => ar.AccountsReceivableEntries)
				.AsNoTracking()
				.Where(ar => ar.AccountsReceivableEntries.Any(c => c.Overpayment != 0))
				.Where(ar => ar.PaidDate >= fiscalStart)
				.Where(ar => ar.PaidDate <= fiscalEnd)
				.Where(ar => ar.GrantApplication.GrantOpening.GrantStream.GrantProgramId == defaultProgramId)
				.Where(ar => ar.GrantApplication.GrantOpening.GrantStream.IsActive)
				.Where(ar => ar.GrantApplication.ProgramInitiativeId == programInitiative.Id)
				.SelectMany(ar => ar.AccountsReceivableEntries.Where(are => are.Overpayment != 0))
				.ToList();

			var arBreakDown = new AccountsReceivableInitiativeData
			{
				Number = claimAccountsReceivableApplications.Select(ar => ar.AccountsReceivable.GrantApplication).Distinct().Count(),
				Total = claimAccountsReceivableApplications.Sum(ar => ar.Overpayment)
			};

			return arBreakDown;
		}

		public List<AccountsReceivableApplicationBreakdownModel> GetAccountsReceivableBreakdownData(int fiscalYearId)
		{
			var fiscalYear = _fiscalYearService.GetFiscalYear(fiscalYearId);
			var defaultProgramId = GetDefaultGrantProgramId();
			var fiscalStart = fiscalYear.StartDate;
			var fiscalEnd = fiscalYear.EndDate;

			var costs = _dbContext.AccountsReceivableEntries
				.Include(are => are.AccountsReceivable)
				.Include(are => are.AccountsReceivable.GrantApplication)
				.AsNoTracking()
				.Where(kek => kek.Overpayment != 0)
				.Where(kek => kek.AccountsReceivable.PaidDate >= fiscalStart)
				.Where(kek => kek.AccountsReceivable.PaidDate <= fiscalEnd)
				.Where(kek => kek.AccountsReceivable.GrantApplication.GrantOpening.GrantStream.GrantProgramId == defaultProgramId)
				.Where(kek => kek.AccountsReceivable.GrantApplication.GrantOpening.GrantStream.IsActive)
				.Select(c => new AccountsReceivableApplicationBreakdownModel
				{
					FiscalYear = fiscalYear.Caption,
					StreamName = c.AccountsReceivable.GrantApplication.GrantOpening.GrantStream.Name,
					DateCreated = c.AccountsReceivable.PaidDate,
					GrantApplicationId = c.AccountsReceivable.GrantApplication.Id,
					ApplicationNumber = c.AccountsReceivable.GrantApplication.FileNumber,
					OverpaymentCategory = c.ServiceCategory.Caption,
					Overpayment = c.Overpayment
				})
				.ToList();

			return costs;
		}
	}
}