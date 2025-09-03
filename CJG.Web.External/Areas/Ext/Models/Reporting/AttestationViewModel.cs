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

		public bool IsComplete { get; set; }
		public IEnumerable<AttachmentViewModel> Attachments { get; set; }

		public ProgramTitleLabelViewModel ProgramTitleLabel { get; set; }

		public AttestationViewModel() { }

		public AttestationViewModel(GrantApplication grantApplication)
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

			AttestationNotApplicable = attestation.AttestationNotApplicable;
			IsComplete = attestation.State == AttestationState.Complete;

			Attachments = attestation.Documents
				.Select(a => new AttachmentViewModel(a));
		}
	}
}