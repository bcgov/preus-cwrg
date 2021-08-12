using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;
using CJG.Web.External.Areas.Ext.Models.Attachments;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Ext.Models.Reporting
{
	public class SuccessStoryViewModel : BaseViewModel
	{
		public string RowVersion { get; set; }
		public int AttachmentsMaximum { get; set; }

		public bool? SuccessfulOutcome { get; set; }
		public string NoOutcomeReason { get; set; }
		public bool IsComplete { get; set; }

		public IEnumerable<AttachmentViewModel> Attachments { get; set; }
		public ProgramTitleLabelViewModel ProgramTitleLabel { get; set; }
		public IEnumerable<KeyValuePair<int, string>> AttachmentTypes { get; set; }

		public SuccessStoryViewModel() { }

		public SuccessStoryViewModel(GrantApplication grantApplication, SuccessStory successStory)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			ProgramTitleLabel = new ProgramTitleLabelViewModel(grantApplication, false);

			Id = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);
			AttachmentsMaximum = 20;

			var documents = new List<Attachment>();

			if (successStory != null)
			{ 
				SuccessfulOutcome = successStory.SuccessfulOutcome;
				NoOutcomeReason = successStory.NoOutcomeReason;
				IsComplete = successStory.State == SuccessStoryState.Complete;
				documents = successStory.Documents.ToList();
			}

			Attachments = documents
				.Select(a => new AttachmentViewModel(a));

			var attachmentTypes = new List<AttachmentType>
			{
				AttachmentType.SuccessStoryConsentForm,
				AttachmentType.SuccessStoryTestimonial
			};

			AttachmentTypes = attachmentTypes.Select(a => new KeyValuePair<int, string>((int)a, a.GetDescription()));
		}
	}
}