using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using CJG.Application.Services;
using CJG.Core.Entities;

namespace CJG.Web.External.Areas.Int.Models.Applications
{
	public class ProgramComponentViewModel
	{
		public int Id { get; set; }
		public string RowVersion { get; set; }
		public string Caption { get; set; }
		public bool CanEdit { get; set; }
		public bool CanChangeEligibility { get; set; }
		public bool CanRemove { get; set; }
		public bool IsEligible { get; set; }
		public TrainingProgramStates State { get; set; }
		public bool AddedByAssessor { get; set; }
		public IEnumerable<ProviderComponentViewModel> Providers { get; set; }
		public ProviderComponentViewModel Provider { get; set; }

		public ProgramComponentViewModel() { }

		public ProgramComponentViewModel(TrainingProgram trainingProgram, IPrincipal user)
		{
			if (trainingProgram == null) throw new ArgumentNullException(nameof(trainingProgram));
			if (user == null) throw new ArgumentNullException(nameof(user));

			Id = trainingProgram.Id;
			RowVersion = Convert.ToBase64String(trainingProgram.RowVersion);
			Caption = trainingProgram.CourseTitle;
			State = trainingProgram.TrainingProgramState;
			IsEligible = trainingProgram.EligibleCostBreakdown?.IsEligible ?? true;
			AddedByAssessor = trainingProgram.EligibleCostBreakdown?.AddedByAssessor ?? false;
			CanEdit = user.CanPerformAction(trainingProgram.GrantApplication, ApplicationWorkflowTrigger.EditTrainingProgram);
			CanChangeEligibility = user.CanPerformAction(trainingProgram.GrantApplication, ApplicationWorkflowTrigger.EditTrainingCosts);

			var providers = new List<ProviderComponentViewModel>();
			var provider = trainingProgram.TrainingProvider.ApprovedTrainingProvider ?? trainingProgram.TrainingProvider;
			if (provider.RequestedTrainingProvider != null && trainingProgram.GrantApplication.ApplicationStateInternal.In(ApplicationStateInternal.ChangeRequest, ApplicationStateInternal.ChangeForApproval, ApplicationStateInternal.ChangeForDenial, ApplicationStateInternal.ChangeReturned))
			{
				providers.Add(new ProviderComponentViewModel(provider.RequestedTrainingProvider, user, true)); // Change Request.
			}
			Provider = new ProviderComponentViewModel(provider, user);
			providers.Add(Provider);
			Providers = providers.ToArray();

			CanRemove = user.CanPerformAction(trainingProgram.GrantApplication, ApplicationWorkflowTrigger.AddOrRemoveTrainingProgram)
				&& Providers.All(p => p.IsValidated && !p.IsChangeRequest)
				&& AddedByAssessor;
		}
	}
}