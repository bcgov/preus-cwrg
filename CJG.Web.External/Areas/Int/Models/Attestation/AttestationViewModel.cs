using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;
using CJG.Web.External.Areas.Int.Models.Attachments;
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

		public int MaximumDocuments { get; set; } = 1;
		public IEnumerable<AttachmentViewModel> Documents { get; set; }
		public IEnumerable<AttestationParticipantModel> AttestationParticipants { get; set; }

		public AttestationViewModel() { }

		public AttestationViewModel(GrantApplication grantApplication, IEnumerable<EligibleExpenseBreakdown> expenseBreakdowns)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			var attestation = grantApplication.Attestation ?? new Core.Entities.Attestation();

			Id = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);

			ClaimedCosts = grantApplication.GetTotalForAllAssessedCostsInCategory(ServiceCategoryEnum.ParticipantFinancialSupports);
			AllocatedCosts = attestation.AllocatedCosts;
			UnusedFunds = ClaimedCosts - AllocatedCosts >= 0 ? ClaimedCosts - AllocatedCosts : 0;

			AttestationParticipants = GetAttestationParticipantModels(grantApplication, expenseBreakdowns);

			AttestationNotApplicable = attestation.AttestationNotApplicable;
			IsComplete = attestation.State == AttestationState.Complete;

			MaximumDocuments = 1;
			Documents = attestation.Documents
				.Select(a => new AttachmentViewModel(a));
		}

		private static List<AttestationParticipantModel> GetAttestationParticipantModels(GrantApplication grantApplication, IEnumerable<EligibleExpenseBreakdown> expenseBreakdowns)
		{
			var currentParticipants = new List<AttestationParticipant>();
			if (grantApplication.Attestation != null && grantApplication.Attestation.Participants.Any())
				currentParticipants = grantApplication.Attestation.Participants.ToList();

			var attestationParticipantModels = currentParticipants.Select(ap =>
			{
				var totalAmountSpent = ap.Costs.Sum(apc => apc.ApprovedTotalSpent);
				return new AttestationParticipantModel
				{
					Id = ap.Id,
					ParticipantFormId = ap.ParticipantFormId,
					ParticipantName = $"{ap.ParticipantForm.FirstName} {ap.ParticipantForm.LastName}",
					TotalApprovedCost = ap.TotalApprovedCost,
					TotalAmountSpent = totalAmountSpent,
					UnusedFunds = ap.TotalApprovedCost - totalAmountSpent,
					ProgramInitiativeId = ap.ProgramInitiativeId,
					TariffAffected = ap.ParticipantForm.AffectedByTariffs ?? false,
					TariffCategory = ap.ParticipantForm.AffectedByTariffs.HasValue
					                 && ap.ParticipantForm.AffectedByTariffs.Value
									 && ap.ParticipantForm.ParticipantFundingStream != null
									 ? ap.ParticipantForm.ParticipantFundingStream.Caption
									 : null,
					Costs = ap.Costs.Select(apc => new AttestationParticipantCostModel
					{
						Id = apc.Id,
						ParticipantFormId = apc.AttestationParticipant.ParticipantFormId,
						CostCategoryId = apc.EligibleExpenseBreakdown.Id,
						CostCategoryName = apc.EligibleExpenseBreakdown.Caption,
						CostCategoryOther = apc.ApprovedOtherDefinition,
						TotalSpent = apc.ApprovedTotalSpent,
						RequireOther = apc.EligibleExpenseBreakdown.Caption.ToLower().Contains("other")
					}).ToList()
				};
			}).ToList();

			return attestationParticipantModels;
		}
	}

	public class AttestationParticipantModel : BaseViewModel
	{
		public int AttestationParticipantId { get; set; }
		public int ParticipantFormId { get; set; } // Probably don't need this
		public string ParticipantName { get; set; }
		public decimal TotalApprovedCost { get; set; }

		public bool TariffAffected { get; set; }
		public string TariffCategory { get; set; }

		// These are only used by the ng-side models to calculate totals
		public decimal TotalAmountSpent { get; set; }
		public decimal UnusedFunds { get; set; }

		public int? ProgramInitiativeId { get; set; }
		public string ProgramInitiative { get; set; }

		public List<AttestationParticipantCostModel> Costs { get; set; }

		public AttestationParticipantModel()
		{
			UnusedFunds = TotalApprovedCost - TotalAmountSpent;
        }
    }

	public class AttestationParticipantCostModel : BaseViewModel
	{
		public int AttestationParticipantCostId { get; set; }
		public int ParticipantFormId { get; set; } // Probably don't need this
		public int CostCategoryId { get; set; }
		public string CostCategoryName { get; set; }
		public string CostCategoryOther { get; set; }
		public decimal TotalSpent { get; set; }
		public bool RequireOther { get; set; }
	}
}