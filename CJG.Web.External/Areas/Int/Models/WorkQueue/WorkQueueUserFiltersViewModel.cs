using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.WorkQueue
{
    public class WorkQueueUserFiltersViewModel : BaseViewModel
	{
		public IEnumerable<WorkQueueUserFilterViewModel> Filters { get; set; }

		public WorkQueueUserFiltersViewModel() { }

		public WorkQueueUserFiltersViewModel(IEnumerable<InternalUserFilter> filters)
		{
			Filters = filters?.Select(f => new WorkQueueUserFilterViewModel(f));
		}
	}
}