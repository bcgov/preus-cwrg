using System.Collections.Generic;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.ProgramInitiatives
{
	public class ProgramInitiativesModel : BaseViewModel
	{
		public List<ProgramInitiativeModel> Initiatives { get; set; }
	}
}