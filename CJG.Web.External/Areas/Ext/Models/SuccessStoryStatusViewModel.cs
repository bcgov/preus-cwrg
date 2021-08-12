using CJG.Core.Entities;

namespace CJG.Web.External.Areas.Ext.Models
{
	public class SuccessStoryStatusViewModel
	{
		public string LabelClass { get; set; }
		public string StatusText { get; set; }

		public SuccessStoryStatusViewModel()
		{
			LabelClass = "notstarted";
			StatusText = "Not Started";
		}

		public SuccessStoryStatusViewModel(SuccessStory successStory)
		{
			LabelClass = "notstarted";
			StatusText = "Not Started";

			//TODO: Change how this is generated
			if (successStory == null)
				return;

			if (successStory.State == SuccessStoryState.Incomplete)
			{
				LabelClass = "incomplete";
				StatusText = "Incomplete";

				return;
			}

			if (successStory.State == SuccessStoryState.Complete)
			{
				LabelClass = "complete";
				StatusText = "Complete";

				return;
			}
		}
	}
}