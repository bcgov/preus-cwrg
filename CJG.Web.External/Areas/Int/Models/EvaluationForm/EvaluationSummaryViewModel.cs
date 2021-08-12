using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.EvaluationForm
{
    public class EvaluationSummaryViewModel : BaseViewModel
	{
		public InternalUser Assessor { get; set; }

		public string FileNumber { get; set; }

		public int GrantProgramId { get; set; }
		public string GrantStreamFullName { get; set; }

		public DateTime TrainingPeriodStartDate { get; set; }
		public DateTime TrainingPeriodEndDate { get; set; }

		public int OrganizationId { get; set; }
		public string OrganizationLegalName { get; set; }
		public string StreamIntent { get; set; }

		public List<TrainingProviderItem> SkillTrainingProviders { get; set; }
		public List<TrainingProviderItem> ESSTrainingProviders { get; set; }

		[Required(ErrorMessage = "High Level Rationale is required.")]
		public string HighLevelRationale { get; set; }
		public string ApplicationNotes { get; set; }

		public string RowVersion { get; set; }

		public bool EditSummary { get; set; }

		public EvaluationSummaryViewModel()
		{
		}

		public EvaluationSummaryViewModel(GrantApplication grantApplication, IEvaluationFormService evaluationFormService, IPrincipal user)
		{
			if (user == null)
				throw new ArgumentNullException(nameof(user));

			Id = grantApplication.Id;

			FileNumber = grantApplication.FileNumber;

			GrantStreamFullName = grantApplication.GrantOpening.GrantStream.Name;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);

			Assessor = grantApplication.Assessor == null ? null : new InternalUser
			{
				Id = grantApplication.Assessor.Id,
				LastName = grantApplication.Assessor.LastName,
				FirstName = grantApplication.Assessor.FirstName,
				IDIR = grantApplication.Assessor.IDIR,
				Email = grantApplication.Assessor.Email
			};

			SkillTrainingProviders = new List<TrainingProviderItem>();
			foreach (var program in grantApplication.TrainingPrograms)
			{
				if (program.TrainingProvider != null)
				{
					var trainingProviderId = program.TrainingProvider.TrainingProviderInventoryId ?? 0;
					var trainingProviderName = program.TrainingProvider.Name;
					SkillTrainingProviders.Add(new TrainingProviderItem { Id = trainingProviderId, Name = trainingProviderName });
				}
			}

			ESSTrainingProviders = new List<TrainingProviderItem>();
			foreach (var provider in grantApplication.TrainingProviders)
			{
				if (provider != null)
				{
					var trainingProviderId = provider.TrainingProviderInventoryId ?? 0;
					var trainingProviderName = provider.Name;
					ESSTrainingProviders.Add(new TrainingProviderItem { Id = trainingProviderId, Name = trainingProviderName });
				}
			}

			TrainingPeriodStartDate = grantApplication.GrantOpening.TrainingPeriod.StartDate.ToLocalTime();
			TrainingPeriodEndDate = grantApplication.GrantOpening.TrainingPeriod.EndDate.ToLocalTime();

			OrganizationId = grantApplication.Organization.Id;
			OrganizationLegalName = grantApplication.OrganizationLegalName;

			StreamIntent = grantApplication.GrantOpening.GrantStream.Intent ?? string.Empty;

			var evaluation = grantApplication.GrantApplicationEvaluation ?? new GrantApplicationEvaluation();
			HighLevelRationale = evaluation.HighLevelRationale;
			ApplicationNotes = evaluation.ApplicationNotes;

			EditSummary = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditApplication);
		}
	}

    public class TrainingProviderItem
    {
	    public int Id { get; set; }
	    public string Name { get; set; }
    }
}