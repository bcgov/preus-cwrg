using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;

namespace CJG.Web.External.Areas.Ext.Models
{
    public class DynamicProcess
	{
		public int ServiceCategoryId { get; set; }
		public string ServiceCategoryCaption { get; set; }
		public int ServiceCategoryTypeId { get; set; }
		public int MinProvider { get; set; }
		public int MaxProvider { get; set; }
		public int MinPrograms { get; set; }
		public int MaxPrograms { get; set; }
		public int RowSequence { get; set; }
		public OverviewEligibleCost AssociatedEligibleCost { get; set; }
		public IEnumerable<OverviewTrainingProvider> TrainingProviders { get; set; }
		public IEnumerable<OverviewTrainingProgram> TrainingPrograms { get; set; }
		public string ServiceCategoryDescription { get; set; }

		public DynamicProcess(GrantApplication grantApplication, EligibleExpenseType eligibleExpenseType)
		{
			ServiceCategoryId = eligibleExpenseType.Id;
			ServiceCategoryCaption = eligibleExpenseType.Caption;
			ServiceCategoryTypeId = (int)eligibleExpenseType.ServiceCategory.ServiceTypeId;
			RowSequence = eligibleExpenseType.RowSequence;
			ServiceCategoryDescription = eligibleExpenseType.Description;

			if (eligibleExpenseType.ServiceCategory.ServiceTypeId == ServiceTypes.EmploymentServicesAndSupports)
			{
				MinProvider = eligibleExpenseType.MinProviders;
				MaxProvider = eligibleExpenseType.MaxProviders;

				if (grantApplication.TrainingCost.EligibleCosts.Any(x => x.EligibleExpenseTypeId == eligibleExpenseType.Id))
					AssociatedEligibleCost = new OverviewEligibleCost(grantApplication.TrainingCost.EligibleCosts.First(x => x.EligibleExpenseTypeId == eligibleExpenseType.Id));

				TrainingProviders = grantApplication.TrainingProviders.Where(tp => tp.EligibleCost?.EligibleExpenseTypeId == eligibleExpenseType.Id).Select(x => new OverviewTrainingProvider(x)).ToList();
			}
			else if (eligibleExpenseType.ServiceCategory.ServiceTypeId == ServiceTypes.SkillsTraining)
			{
				MinPrograms = eligibleExpenseType.ServiceCategory?.MinPrograms ?? 0;
				MaxPrograms = eligibleExpenseType.ServiceCategory?.MaxPrograms ?? 0;
				if (grantApplication.HasOfferBeenIssued())
				{
					TrainingPrograms = grantApplication.TrainingPrograms.Where(tp => (tp.EligibleCostBreakdown?.IsEligible ?? true) && tp.EligibleCostBreakdown?.EligibleCost?.EligibleExpenseTypeId == eligibleExpenseType.Id).Select(x => new OverviewTrainingProgram(x)).ToList();
				}
				else
				{
					TrainingPrograms = grantApplication.TrainingPrograms.Where(tp => !(tp.EligibleCostBreakdown?.AddedByAssessor ?? false) && tp.EligibleCostBreakdown?.EligibleCost?.EligibleExpenseTypeId == eligibleExpenseType.Id).Select(x => new OverviewTrainingProgram(x)).ToList();
				}
			}
		}
	}
}