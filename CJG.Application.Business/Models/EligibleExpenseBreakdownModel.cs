using CJG.Core.Entities;

namespace CJG.Application.Business.Models
{
    public class EligibleExpenseBreakdownModel : CollectionItemModel
    {
	    public bool EnableCost { get; set; }
        public bool EnableCustomTitle { get; set; }

        public EligibleExpenseBreakdownModel()
        {

        }

        public EligibleExpenseBreakdownModel(EligibleExpenseBreakdown eligibleExpenseBreakdown)
        {
            Id = eligibleExpenseBreakdown.Id;
            Caption = eligibleExpenseBreakdown.Caption;
            EnableCost = eligibleExpenseBreakdown.EnableCost;
            EnableCustomTitle = eligibleExpenseBreakdown.EnableCustomTitle;
        }
    }
}
