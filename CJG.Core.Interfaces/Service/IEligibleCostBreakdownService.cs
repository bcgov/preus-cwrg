using CJG.Core.Entities;

namespace CJG.Core.Interfaces.Service
{
	public interface IEligibleCostBreakdownService : IService
    {
        EligibleCostBreakdown Get(int id);
        EligibleCostBreakdown Add(EligibleCostBreakdown eligibleCostBreakdown);
        EligibleCostBreakdown Update(EligibleCostBreakdown eligibleCostBreakdown);
        void Delete(EligibleCostBreakdown eligibleCostBreakdown);
    }
}
