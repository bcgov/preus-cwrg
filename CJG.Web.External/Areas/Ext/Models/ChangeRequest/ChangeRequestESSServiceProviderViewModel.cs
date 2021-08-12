using System;
using CJG.Core.Entities;

namespace CJG.Web.External.Areas.Ext.Models.ChangeRequest
{
    public class ChangeRequestESSServiceProviderViewModel
    {
        public int GrantApplicationId { get; set; }
        public int EligibleExpenseTypeId { get; set; }
        public string TrainingProviderName { get; set; }
        public int TrainingProviderId { get; set; }

        public string RequestTrainingProviderName { get; set; }
        public int RequestTrainingProviderId { get; set; }
        public ChangeRequestESSServiceProviderViewModel()
        {

        }
        public ChangeRequestESSServiceProviderViewModel(TrainingProvider trainingProvider, int grantApplicationId)
        {
            if (trainingProvider == null)
	            throw new ArgumentNullException(nameof(trainingProvider));

            GrantApplicationId = grantApplicationId;
            TrainingProviderId = trainingProvider.Id;
            EligibleExpenseTypeId = trainingProvider.EligibleCost.EligibleExpenseTypeId;
            TrainingProviderName = trainingProvider.Name;

            var baseTrainingProvider = trainingProvider.OriginalTrainingProvider ?? trainingProvider;
            var requestTrainingProvider = baseTrainingProvider.RequestedTrainingProvider;
            if (requestTrainingProvider != null)
            {
                RequestTrainingProviderName = requestTrainingProvider.Name;
                RequestTrainingProviderId = requestTrainingProvider.Id;
            }
        }
    }
}