using System.Collections.Generic;
using System.Linq;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models
{
    public class ServiceLineViewModel : LookupTableViewModel
	{
		public IEnumerable<LookupTableViewModel> ServiceLineBreakdowns { get; set; } = new List<LookupTableViewModel>();
		public string BreakdownCaption { get; set; }
		public bool EnableCost { get; set; }
		public bool EnableCustomTitle { get; set; }

		public ServiceLineViewModel() { }

		public ServiceLineViewModel(ServiceLine serviceLine)
		{
			Utilities.MapProperties(serviceLine, this);

			AllowDelete = !serviceLine.EligibleExpenseBreakdowns.Any(eeb => eeb.EligibleCostBreakdowns.Any() || eeb.EligibleCostBreakdowns.Any(ecb => ecb.TrainingPrograms.Any()));
			ServiceLineBreakdowns = serviceLine.ServiceLineBreakdowns.OrderBy(slb => slb.RowSequence).ThenBy(slb => slb.Caption).Select(slb => new LookupTableViewModel(slb)
			{
				AllowDelete = !slb.TrainingPrograms.Any()
			}).ToArray();
		}
	}
}