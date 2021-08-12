using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.Applications
{
	public class ApplicationChecklistViewModel : BaseViewModel
	{
		public class ChecklistCategoryViewModel
		{
			public int Id { get; set; }
			public string Caption { get; set; }
			public int RowSequence { get; set; }

			public List<ChecklistApplicationItem> Items { get; set; }
		}

		public class ChecklistApplicationItem
		{
			public int Id { get; set; }
			public string Caption { get; set; }
			public int RowSequence { get; set; }
			public bool IsChecked { get; set; }
		}

		public List<ChecklistCategoryViewModel> Categories { get; set; }

		public ApplicationChecklistViewModel()
		{
			
		}

		public ApplicationChecklistViewModel(GrantApplication grantApplication, IGrantStreamService grantStreamService)
		{
			var streamCheckList = grantStreamService.GetGrantStreamChecklist(grantApplication.GrantOpening.GrantStreamId);
			Categories = new List<ChecklistCategoryViewModel>();

			var checkedItems = grantApplication
				.GrantApplicationTasks
				.Where(c => c.IsChecked)
				.Select(c => c.ChecklistItemId)
				.ToList();

			foreach (var category in streamCheckList.Where(c => c.IsActive))
			{
				Categories.Add(new ChecklistCategoryViewModel
				{
					Id = category.Id,
					Caption = category.Caption,
					RowSequence = category.RowSequence,

					Items = category.Items.Select(i => new ChecklistApplicationItem
					{
						Id = i.Id,
						Caption = i.Caption,
						RowSequence = i.RowSequence,
						IsChecked = checkedItems.Contains(i.Id)
					}).ToList()
				});
			}
		}
	}

}