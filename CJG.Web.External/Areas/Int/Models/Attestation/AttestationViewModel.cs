using System;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.Attestation
{
	public class AttestationViewModel : BaseViewModel
	{
		public string RowVersion { get; set; }

		public decimal ClaimedCosts { get; set; }
		public decimal AssessedCosts { get; set; }
		public decimal AllocatedCosts { get; set; }
		public decimal UnusedFunds { get; set; }

		public bool? AttestationNotApplicable { get; set; }
		public bool? CompleteAttestation { get; set; }

		public bool IsComplete { get; set; }

		public AttestationViewModel() { }

		public AttestationViewModel(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			var attestation = grantApplication.Attestation ?? new Core.Entities.Attestation();

			Id = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);

			ClaimedCosts = grantApplication.GetTotalForAllAssessedCostsInCategory(ServiceCategoryEnum.ParticipantFinancialSupports);
			AllocatedCosts = attestation.AllocatedCosts;
			UnusedFunds = ClaimedCosts - AllocatedCosts >= 0 ? ClaimedCosts - AllocatedCosts : 0;

			AttestationNotApplicable = attestation.AttestationNotApplicable;
			IsComplete = attestation.State == AttestationState.Complete;
		}
	}
}