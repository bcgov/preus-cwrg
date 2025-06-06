﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using CJG.Application.Services;
using CJG.Core.Entities;

namespace CJG.Web.External.Areas.Int.Models.Applications
{
	public class ApplicationComponentViewModel
	{
		#region Properties
		public int Id { get; set; }
		public string RowVersion { get; set; }
		public bool CanEdit { get; set; }
		public bool CanAdd { get; set; }
		public string Caption { get; set; }
		public string Category { get; set; }

		#region Service Categories
		public int? ServiceCategoryId { get; set; }
		public ServiceTypes? ServiceCategoryTypeId { get; set; }
		public int? EligibleExpenseTypeId { get; set; }
		public int MaxProviders { get; set; }
		public int MinProviders { get; set; }
		public int RowSequence { get; set; }

		public IEnumerable<ProgramComponentViewModel> Programs { get; set; }
		public IEnumerable<ProviderComponentViewModel> Providers { get; set; }
		#endregion
		#endregion

		#region Constructors
		public ApplicationComponentViewModel() { }

		public ApplicationComponentViewModel(TrainingProvider trainingProvider, IPrincipal user, int rowSequence = 0)
		{
			if (trainingProvider == null) throw new ArgumentNullException(nameof(trainingProvider));
			if (user == null) throw new ArgumentNullException(nameof(user));

			var grantApplication = trainingProvider.GetGrantApplication();
			Id = trainingProvider.Id;
			RowVersion = Convert.ToBase64String(trainingProvider.RowVersion);
			CanEdit = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditTrainingProvider);
			CanAdd = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.AddRemoveTrainingProvider);
			Caption = "Training Provider";
			RowSequence = rowSequence;
			Category = "TrainingProvider";
			EligibleExpenseTypeId = trainingProvider?.EligibleCost?.EligibleExpenseTypeId;

			var providers = new List<ProviderComponentViewModel>();

			// Display a TrainingProvider change only for certain states. The ViewModel is only used for the
			// internal Application/Details view. The TP change could/should be displayed only for certain states.
			var DisplayTPChangeRequest = grantApplication?.ApplicationStateInternal.In(ApplicationStateInternal.ChangeRequest,
					ApplicationStateInternal.ChangeForApproval, ApplicationStateInternal.ChangeForDenial, ApplicationStateInternal.ChangeReturned) == true;
			if (DisplayTPChangeRequest && trainingProvider.RequestedTrainingProvider != null)
				providers.Add(new ProviderComponentViewModel(trainingProvider.RequestedTrainingProvider, user, true)); // Change Request.
			providers.Add(new ProviderComponentViewModel(trainingProvider, user));
			Providers = providers.ToArray();
		}

		public ApplicationComponentViewModel(TrainingProgram trainingProgram, IPrincipal user, int rowSequence = 0)
		{
			if (trainingProgram == null) throw new ArgumentNullException(nameof(trainingProgram));
			if (user == null) throw new ArgumentNullException(nameof(user));

			Id = trainingProgram.Id;
			RowVersion = Convert.ToBase64String(trainingProgram.RowVersion);
			CanEdit = user.CanPerformAction(trainingProgram.GrantApplication, ApplicationWorkflowTrigger.EditTrainingProgram);
			CanAdd = user.CanPerformAction(trainingProgram.GrantApplication, ApplicationWorkflowTrigger.AddOrRemoveTrainingProgram);
			Caption = "Training Program";
			RowSequence = rowSequence;
			Category = "TrainingProgram";
			EligibleExpenseTypeId = trainingProgram?.EligibleCostBreakdown?.EligibleCost?.EligibleExpenseTypeId;

			Programs = new[] { new ProgramComponentViewModel(trainingProgram, user) };
		}

		/// <summary>
		/// Creates a new instance of a ApplicationComponentViewModel object.
		/// This component may be a service category.
		/// A service category may have services, programs and/or providers.
		/// </summary>
		/// <param name="eligibleCost"></param>
		/// <param name="user"></param>
		public ApplicationComponentViewModel(EligibleCost eligibleCost, IPrincipal user)
		{
			if (eligibleCost == null) throw new ArgumentNullException(nameof(eligibleCost));
			if (user == null) throw new ArgumentNullException(nameof(user));

			Id = eligibleCost.Id;
			RowVersion = Convert.ToBase64String(eligibleCost.RowVersion);
			Caption = eligibleCost.EligibleExpenseType.Caption;
			RowSequence = eligibleCost.EligibleExpenseType.RowSequence;
			ServiceCategoryId = eligibleCost.EligibleExpenseType.ServiceCategoryId;
			ServiceCategoryTypeId = eligibleCost.EligibleExpenseType.ServiceCategory?.ServiceTypeId;
			EligibleExpenseTypeId = eligibleCost.EligibleExpenseTypeId;
			MaxProviders = eligibleCost.EligibleExpenseType.MaxProviders;
			MinProviders = eligibleCost.EligibleExpenseType.MinProviders;
			Category = ServiceCategoryTypeId?.ToString("g");

			switch (ServiceCategoryTypeId)
			{
				case ServiceTypes.SkillsTraining:
					CanAdd = (eligibleCost.EligibleExpenseType?.ServiceCategory?.MaxPrograms ?? 0) > 0
					              && user.CanPerformAction(eligibleCost.TrainingCost.GrantApplication, ApplicationWorkflowTrigger.AddRemoveTrainingProvider);
					break;
				case ServiceTypes.EmploymentServicesAndSupports:
					CanAdd = eligibleCost.EligibleExpenseType.MaxProviders > 0
					              && user.CanPerformAction(eligibleCost.TrainingCost.GrantApplication, ApplicationWorkflowTrigger.AddRemoveTrainingProvider);
					CanEdit = user.CanPerformAction(eligibleCost.TrainingCost.GrantApplication, ApplicationWorkflowTrigger.EditTrainingCosts); // Has services.
					break;
			}

			Programs = eligibleCost.Breakdowns.Where(b => b.TrainingPrograms.Any()).Select(b => new ProgramComponentViewModel(b.TrainingPrograms.FirstOrDefault(), user)).ToArray();

			var providers = new List<ProviderComponentViewModel>();
			foreach (var provider in eligibleCost.TrainingCost.GrantApplication.TrainingProviders.Where(tp => tp.EligibleCostId == eligibleCost.Id && tp.OriginalTrainingProviderId == null)) // Only return original training providers.
			{
				var trainingProvider = provider.ApprovedTrainingProvider ?? provider;
				if (trainingProvider.RequestedTrainingProvider != null && trainingProvider.GetGrantApplication().ApplicationStateInternal.In(ApplicationStateInternal.ChangeRequest, ApplicationStateInternal.ChangeForApproval, ApplicationStateInternal.ChangeForDenial, ApplicationStateInternal.ChangeReturned))
				{
					providers.Add(new ProviderComponentViewModel(trainingProvider.RequestedTrainingProvider, user, true)); // Change Request.
				}
				providers.Add(new ProviderComponentViewModel(trainingProvider, user));
			}
			Providers = providers.ToArray();
		}
		#endregion
	}
}