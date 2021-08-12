using System;
using System.ComponentModel.DataAnnotations;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.GrantStreams
{
	public class ChecklistItemViewModel : BaseViewModel
	{
		[Required(ErrorMessage = "Checklist Item caption is required.")]
		public string Caption { get; set; }
		public int RowSequence { get; set; }
		public bool IsActive { get; set; }
		public string RowVersion { get; set; }

		public ChecklistItemViewModel()
		{
		}

		public ChecklistItemViewModel(ChecklistItem checklistItem)
		{
			Id = checklistItem.Id;
			Caption = checklistItem.Caption;
			IsActive = checklistItem.IsActive;
			RowSequence = checklistItem.RowSequence;
			RowVersion = Convert.ToBase64String(checklistItem.RowVersion);
		}
	}
}