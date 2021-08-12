using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;
using CJG.Web.External.Areas.Ext.Models.Attachments;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Ext.Models.Reporting
{
	public class ProofOfPaymentsDocumentsViewModel : BaseViewModel
	{
		public string RowVersion { get; set; }
		public int AttachmentsMaximum { get; set; }
		public bool IsSkillsTrainingForEconomicRecoveryStream { get; set; }

		public bool? ProofNotApplicable { get; set; }
		public bool IsComplete { get; set; }

		public IEnumerable<AttachmentViewModel> Attachments { get; set; }
		public ProgramTitleLabelViewModel ProgramTitleLabel { get; set; }

		public ProofOfPaymentsDocumentsViewModel() { }

		public ProofOfPaymentsDocumentsViewModel(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			var proofOfPayment = grantApplication.ProofOfPayment ?? new ProofOfPayment();

			ProgramTitleLabel = new ProgramTitleLabelViewModel(grantApplication, false);

			Id = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);
			AttachmentsMaximum = 20;
			IsSkillsTrainingForEconomicRecoveryStream = string.Equals(grantApplication.GrantOpening.GrantStream.Name, "Skills Training for Economic Recovery", StringComparison.OrdinalIgnoreCase);

			ProofNotApplicable = proofOfPayment.ProofNotApplicable;
			IsComplete = proofOfPayment.State == ProofOfPaymentState.Complete;

			Attachments = proofOfPayment.Documents
				.Select(a => new AttachmentViewModel(a));
		}
	}
}