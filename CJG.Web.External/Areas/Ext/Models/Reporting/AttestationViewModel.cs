using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;
using CJG.Web.External.Areas.Ext.Models.Attachments;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Ext.Models.Reporting
{
    public class AttestationViewModel : BaseViewModel
	{
		public string RowVersion { get; set; }
		public int AttachmentsMaximum { get; set; }

		public decimal ClaimedCosts { get; set; }
		public decimal AllocatedCosts { get; set; }
		public decimal UnusedFunds { get; set; }

		public bool? AttestationNotApplicable { get; set; }
		public bool? CompleteAttestation { get; set; }

		public List<AttestationParticipantModel> AttestationParticipants { get; set; }

		public bool IsComplete { get; set; }
		public IEnumerable<AttachmentViewModel> Attachments { get; set; }

		public ProgramTitleLabelViewModel ProgramTitleLabel { get; set; }

		public AttestationViewModel() { }

		public AttestationViewModel(GrantApplication grantApplication, IEnumerable<EligibleExpenseBreakdown> expenseBreakdowns)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			var attestation = grantApplication.Attestation ?? new Attestation();

			ProgramTitleLabel = new ProgramTitleLabelViewModel(grantApplication, false);

			Id = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);
			AttachmentsMaximum = 5;
			ClaimedCosts = grantApplication.GetTotalForAllAssessedCostsInCategory(ServiceCategoryEnum.ParticipantFinancialSupports);
			AllocatedCosts = attestation.AllocatedCosts;
			UnusedFunds = ClaimedCosts - AllocatedCosts >= 0 ? ClaimedCosts - AllocatedCosts : 0;

			AttestationParticipants = GetAttestationParticipantModels(grantApplication, expenseBreakdowns);

			AttestationNotApplicable = attestation.AttestationNotApplicable;
			IsComplete = attestation.State == AttestationState.Complete;

			Attachments = attestation.Documents
				.Select(a => new AttachmentViewModel(a));
		}

		private static List<AttestationParticipantModel> GetAttestationParticipantModels(GrantApplication grantApplication, IEnumerable<EligibleExpenseBreakdown> expenseBreakdowns)
		{
			var participants = grantApplication.ParticipantForms
				.Where(pf => !pf.IsExcludedFromClaim)
				.OrderBy(pf => pf.LastName)
				.ThenBy(pf => pf.FirstName);

			var eligibleCosts = grantApplication
				.TrainingCost
				.EligibleCosts
				.Where(ec => ec.EligibleExpenseType.ServiceCategory != null
				             && ec.EligibleExpenseType.ServiceCategory.Caption == "Participant Financial Supports")
				.ToList();

			var approvedPfsCostsPerParticipant = eligibleCosts.Sum(ee => ee.AgreedMaxParticipantCost);
			var pfsCostCategories = eligibleCosts.Select(ec => ec.EligibleExpenseType);

			var currentParticipants = new List<AttestationParticipant>();

			if (grantApplication.Attestation != null && grantApplication.Attestation.Participants.Any())
			{
				currentParticipants = grantApplication.Attestation.Participants.ToList();
			}

			var attestationParticipantModels = participants.Select(p =>
			{
				var currentParticipant = currentParticipants.FirstOrDefault(cp => cp.ParticipantFormId == p.Id);
				var currentParticipantTotalAmountSpent = currentParticipant?.TotalAmountSpent ?? 0;

				return new AttestationParticipantModel
				{
					ParticipantFormId = p.Id,
					ParticipantName = $"{p.FirstName} {p.LastName}",
					TotalApprovedCost = approvedPfsCostsPerParticipant,
					TotalAmountSpent = currentParticipantTotalAmountSpent,
					UnusedFunds = approvedPfsCostsPerParticipant - currentParticipantTotalAmountSpent,
					ProgramInitiativeId = grantApplication.ProgramInitiativeId,
					Costs = expenseBreakdowns.Select(c =>
					{
						var currentParticipantCost = currentParticipant?.Costs
							.FirstOrDefault(cpc => cpc.EligibleExpenseBreakdownId == c.Id);

						var otherDefinition = currentParticipantCost?.RequestedOtherDefinition;
						var totalSpent = currentParticipantCost?.RequestedTotalSpent ?? 0;

						return new AttestationParticipantCostModel
						{
							ParticipantFormId = p.Id,
							CostCategoryId = c.Id,
							CostCategoryName = c.Caption,
							CostCategoryOther = otherDefinition,
							TotalSpent = totalSpent,
							RequireOther = c.Caption.ToLower().Contains("other")
						};
					}).ToList()
				};
			}).ToList();

			return attestationParticipantModels;
		}
	}
}