using System.Collections.Generic;
using System.Linq;
using CJG.Application.Business.Models;
using CJG.Application.Services;
using CJG.Core.Entities;

namespace CJG.Web.External.Areas.Ext.Models
{
    public class OverviewEligibleCost
    {
	    public int Id { get; set; }
        public decimal EstimatedCost { get; set; }
        public IEnumerable<CollectionItemModel> ServiceLines { get; set; } = new List<CollectionItemModel>();

        public OverviewEligibleCost()
        {
        }

        public OverviewEligibleCost(EligibleCost eligibleCost)
        {
            Utilities.MapProperties(eligibleCost, this);

            if (!eligibleCost.Breakdowns.Any())
	            return;

            ServiceLines = eligibleCost.Breakdowns.Select(x => new CollectionItemModel
            {
	            Id = x.EligibleExpenseBreakdown.ServiceLine.Id,
	            Caption = x.EligibleExpenseBreakdown.ServiceLine.Caption,
	            Description = x.EligibleExpenseBreakdown.ServiceLine.Description,
	            EstimatedCost = x.EstimatedCost,
	            CustomCostTitle = x.CustomCostTitle
            }).ToArray();
        }
    }
}