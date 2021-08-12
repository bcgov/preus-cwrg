using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;
using CJG.Web.External.Areas.Int.Models.Attachments;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.ProofOfPayments
{
	public class ProofOfPaymentDocumentsModel : BaseViewModel
	{
		public string RowVersion { get; set; }
		public int MaximumDocuments { get; set; } = 20;
		public ProofOfPaymentState State { get; set; }
		public string StateDescription { get; set; }
		public bool? ProofNotApplicable { get; set; }

		public IEnumerable<AttachmentViewModel> Documents { get; set; }

		public ProofOfPaymentDocumentsModel() { }

		public ProofOfPaymentDocumentsModel(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			var proofOfPayment = grantApplication.ProofOfPayment ?? new ProofOfPayment();

			Id = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);
			State = proofOfPayment.State;

			StateDescription = proofOfPayment.State.GetDescription();
			ProofNotApplicable = proofOfPayment.ProofNotApplicable;

			MaximumDocuments = 20;
			Documents = proofOfPayment.Documents
				.Select(a => new AttachmentViewModel(a));
		}
	}
}