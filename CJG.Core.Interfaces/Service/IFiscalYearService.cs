using System;
using System.Collections.Generic;
using CJG.Core.Entities;

namespace CJG.Core.Interfaces.Service
{
	public interface IFiscalYearService : IService
    {
        FiscalYear Get(int id);
        FiscalYear GetFiscalYear(DateTime date);
        FiscalYear GetFiscalYear(int? fiscalYearId);
        FiscalYear GetCurrentFiscalYear();

		IEnumerable<ReportRate> GetReportRates(int fiscalYearId);
        IEnumerable<FiscalYear> GetFiscalYears();

        IEnumerable<KeyValuePair<string, string>> GetTrainingPeriodLabels(int? fiscalYearId, int? grantStreamId);
        TrainingPeriod GetCurrentTrainingPeriodFor(int? fiscalYearId, int? grantStreamId);
    }
}
