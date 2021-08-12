using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.GrantStreams
{
	public class GrantStreamChecklistViewModel : BaseViewModel
	{
		public int GrantStreamId { get; set; }
		[Required(ErrorMessage = "Checklist Category caption is required.")]
		public string Caption { get; set; }
		public int RowSequence { get; set; }
		public bool IsActive { get; set; }
		public string RowVersion { get; set; }

		public List<ChecklistItemViewModel> Items { get; set; } = new List<ChecklistItemViewModel>();

		public GrantStreamChecklistViewModel() { }

		public GrantStreamChecklistViewModel(ChecklistCategory checklistCategory)
		{
			if (checklistCategory == null)
				throw new ArgumentNullException(nameof(checklistCategory));

			Utilities.MapProperties(checklistCategory, this);

			Items = checklistCategory.Items
				.OrderBy(i => i.RowSequence)
				.Select(i => new ChecklistItemViewModel(i))
				.ToList();

			RowVersion = Convert.ToBase64String(checklistCategory.RowVersion);
		}
	}
}