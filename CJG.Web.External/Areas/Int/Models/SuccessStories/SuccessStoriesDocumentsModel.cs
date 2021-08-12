using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;
using CJG.Web.External.Areas.Int.Models.Attachments;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.SuccessStories
{
	public class SuccessStoriesDocumentsModel : BaseViewModel
	{
		public string RowVersion { get; set; }
		public int MaximumDocuments { get; set; } = 20;
		public SuccessStoryState State { get; set; }
		public string StateDescription { get; set; }
		public bool? SuccessfulOutcome { get; set; }
		public string NoOutcomeReason { get; set; }

		public IEnumerable<AttachmentViewModel> Documents { get; set; }
		public IEnumerable<KeyValuePair<int, string>> AttachmentTypes { get; set; } = new List<KeyValuePair<int, string>>();

		public SuccessStoriesDocumentsModel() { }

		public SuccessStoriesDocumentsModel(GrantApplication grantApplication, SuccessStory successStory)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			Id = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);

			if (successStory != null)
			{
				State = successStory.State;
				StateDescription = successStory.State.GetDescription();
				SuccessfulOutcome = successStory.SuccessfulOutcome;
				NoOutcomeReason = successStory.NoOutcomeReason;
				Documents = successStory.Documents
					.Select(a => new AttachmentViewModel(a));
			}
			else
			{
				State = SuccessStoryState.Incomplete;
				StateDescription = SuccessStoryState.Incomplete.GetDescription();
				SuccessfulOutcome = null;
				NoOutcomeReason = string.Empty;
				Documents = new List<AttachmentViewModel>();
			}

			MaximumDocuments = 20;
			var attachmentTypes = new List<AttachmentType>
			{
				AttachmentType.SuccessStoryConsentForm,
				AttachmentType.SuccessStoryTestimonial
			};
			AttachmentTypes = attachmentTypes
				.Select(a => new KeyValuePair<int, string>((int) a, a.GetDescription()));
		}
	}
}