using System;
using System.Configuration;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;
using CJG.Web.External.Models.Shared.SkillsTrainings;

namespace CJG.Web.External.Areas.Ext.Models
{
	public class TrainingServiceProviderViewModel : BaseViewModel
	{
		public int GrantApplicationId { get; set; }
		public TrainingProviderDetailsViewModel TrainingProvider { get; set; }
		public ServiceProviderDetailsViewModel ServiceProvider { get; set; }
		public int MaxUploadSize { get; set; }
		public string RowVersion { get; set; }
		public string EligibleCostBreakdownRowVersion { get; set; }

		public TrainingServiceProviderViewModel()
		{
		}

		public TrainingServiceProviderViewModel(TrainingProvider trainingProvider)
		{
			if (trainingProvider == null)
				throw new ArgumentNullException(nameof(trainingProvider));

			var grantApplication = trainingProvider.GetGrantApplication();

			GrantApplicationId = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);
			TrainingProvider = new TrainingProviderDetailsViewModel(trainingProvider)
			{
				GrantApplicationId = grantApplication.Id
			};

			EligibleCostBreakdownRowVersion = trainingProvider.TrainingProgram?.EligibleCostBreakdown != null
				? Convert.ToBase64String(trainingProvider.TrainingProgram.EligibleCostBreakdown.RowVersion)
				: null;

			int maxUploadSize = int.Parse(ConfigurationManager.AppSettings["MaxUploadSizeInBytes"]);
			MaxUploadSize = maxUploadSize / 1024 / 1024;
		}

		public TrainingServiceProviderViewModel(TrainingProvider trainingProvider, EligibleExpenseType eligibleExpenseType)
		{
			if (trainingProvider == null)
				throw new ArgumentNullException(nameof(trainingProvider));

			if (eligibleExpenseType == null)
				throw new ArgumentNullException(nameof(eligibleExpenseType));

			var grantApplication = trainingProvider.GetGrantApplication();

			GrantApplicationId = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);
			ServiceProvider = new ServiceProviderDetailsViewModel(trainingProvider, eligibleExpenseType)
			{
				GrantApplicationId = grantApplication.Id
			};

			EligibleCostBreakdownRowVersion = trainingProvider.TrainingProgram?.EligibleCostBreakdown != null
				? Convert.ToBase64String(trainingProvider.TrainingProgram.EligibleCostBreakdown.RowVersion)
				: null;

			int maxUploadSize = int.Parse(ConfigurationManager.AppSettings["MaxUploadSizeInBytes"]);
			MaxUploadSize = maxUploadSize / 1024 / 1024;
		}

		public TrainingServiceProviderViewModel(GrantApplication grantApplication, EligibleExpenseType eligibleExpenseType)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			if (eligibleExpenseType == null)
				throw new ArgumentNullException(nameof(eligibleExpenseType));

			GrantApplicationId = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);
			ServiceProvider = new ServiceProviderDetailsViewModel(eligibleExpenseType)
			{
				GrantApplicationId = grantApplication.Id
			};

			int maxUploadSize = int.Parse(ConfigurationManager.AppSettings["MaxUploadSizeInBytes"]);
			MaxUploadSize = maxUploadSize / 1024 / 1024;
		}
	}
}
