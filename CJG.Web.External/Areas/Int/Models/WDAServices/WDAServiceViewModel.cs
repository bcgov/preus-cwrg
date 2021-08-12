using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models
{
    public class WDAServiceViewModel : BaseViewModel
	{
		public IEnumerable<ServiceCategoryViewModel> ServiceCategories { get; set; } = new List<ServiceCategoryViewModel>();
		public bool Deleted { get; set; }

		public WDAServiceViewModel() { }

		public WDAServiceViewModel(IEnumerable<ServiceCategory> serviceCategories) 
		{
			ServiceCategories = serviceCategories.Select(sc => new ServiceCategoryViewModel(sc)).ToArray();
		}
	}
}
