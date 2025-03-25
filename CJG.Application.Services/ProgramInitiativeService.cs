using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Entities;
using NLog;

namespace CJG.Application.Services
{
	public class ProgramInitiativeService : Service, IProgramInitiativeService
	{
		public ProgramInitiativeService(IDataContext context, HttpContextBase httpContext, ILogger logger)
			: base(context, httpContext, logger)
		{
		}

		public ProgramInitiative Get(int id)
		{
			return Get<ProgramInitiative>(id);
		}

		public IEnumerable<ProgramInitiative> GetAll()
		{
			return _dbContext.ProgramInitiatives
				.OrderBy(pi => pi.RowSequence);
		}

		public IEnumerable<ProgramInitiative> GetAll(bool isActive)
		{
			var programInitiatives = _dbContext.ProgramInitiatives
				.AsNoTracking()
				.Where(pi => pi.IsActive == isActive)
				.OrderBy(pi => pi.RowSequence)
				.AsQueryable();

			return programInitiatives.ToList();
		}

		public bool IsInUse(int programInitiativeId)
		{
			return _dbContext.GrantApplications.Any(ga => ga.ProgramInitiativeId == programInitiativeId);
		}

		public void UpdateInitiatives(List<ProgramInitiative> initiativeList, List<ProgramInitiative> removeItems)
		{
			if (initiativeList == null)
				throw new ArgumentNullException(nameof(initiativeList));

			foreach (var initiative in initiativeList)
			{
				if (initiative.Id == 0)
					_dbContext.ProgramInitiatives.Add(initiative);
			}

			// Have to explicitly remove these from the dbContext so they get deleted rather than disconnected
			foreach (var removeItem in removeItems)
			{
				if (IsInUse(removeItem.Id))
					continue;

				RemoveDirectorReports(removeItem);

				_dbContext.ProgramInitiatives.Remove(removeItem);
			}

			_dbContext.CommitTransaction();
		}

		private void RemoveDirectorReports(ProgramInitiative programInitiative)
		{
			var directorReports = _dbContext.DirectorBudgets
				.Where(dr => dr.ProgramInitiativeId == programInitiative.Id);

			foreach (var directorReport in directorReports)
			{
				var budgetRows = directorReport.BudgetEntries.Select(be => be.DirectorBudgetRow);
				_dbContext.DirectorBudgetEntries.RemoveRange(directorReport.BudgetEntries);
				_dbContext.DirectorBudgetRows.RemoveRange(budgetRows);
				_dbContext.DirectorBudgets.Remove(directorReport);
			}
		}
	}
}