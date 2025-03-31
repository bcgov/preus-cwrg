using System;
using System.ComponentModel.DataAnnotations;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.ProgramInitiatives
{
	public class ProgramInitiativeModel : BaseViewModel
    {
		[Required]
	    public string Name { get; set; }

	    [Required]
	    public string Code { get; set; }

		public int RowSequence { get; set; }

		public string RowVersion { get; set; }

		// Set externally on model 
		public bool? IsInUse { get; set; }

		public ProgramInitiativeModel()
		{
		}

		public ProgramInitiativeModel(ProgramInitiative initiative)
		{
			Id = initiative.Id;
			Name = initiative.Name;
			Code = initiative.Code;
			RowSequence = initiative.RowSequence;
			RowVersion = Convert.ToBase64String(initiative.RowVersion);
		}
	}
}