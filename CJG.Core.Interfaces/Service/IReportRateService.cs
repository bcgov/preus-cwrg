using CJG.Core.Entities;

namespace CJG.Core.Interfaces.Service
{
    public interface IReportRateService : IService
    {
        ReportRate Get(int fiscalYearId, int grantProgramId, int grantStreamId);

        ReportRate Add(ReportRate reportRate);
    }
}
