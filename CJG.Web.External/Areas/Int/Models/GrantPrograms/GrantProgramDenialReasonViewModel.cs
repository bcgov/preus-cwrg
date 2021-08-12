using System;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.GrantPrograms
{
    public class GrantProgramDenialReasonViewModel : BaseViewModel
	{
		public int GrantProgramId { get; set; }
		public string Caption { get; set; }
		public bool IsActive { get; set; }

		public GrantProgramDenialReasonViewModel() { }

		public GrantProgramDenialReasonViewModel(DenialReason denialReason)
		{
			if (denialReason == null)
				throw new ArgumentNullException(nameof(denialReason));

			GrantProgramId = denialReason.GrantProgramId;
			Caption = denialReason.Caption;
			IsActive = denialReason.IsActive;
			Id = denialReason.Id;
		}
	}
}