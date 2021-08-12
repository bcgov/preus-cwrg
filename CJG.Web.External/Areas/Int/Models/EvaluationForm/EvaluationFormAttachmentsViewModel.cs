using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;
using CJG.Web.External.Areas.Int.Models.Attachments;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.EvaluationForm
{
	public class EvaluationFormAttachmentsViewModel : BaseViewModel
	{
		public IEnumerable<AttachmentViewModel> Attachments { get; set; }

		public EvaluationFormAttachmentsViewModel() { }

		public EvaluationFormAttachmentsViewModel(IEnumerable<EvaluationFormResource> resources)
		{
			Attachments = resources.Select(a => new AttachmentViewModel(a.Attachment));
		}
	}
}