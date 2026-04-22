using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models
{
	public class ParticipantLmdaEligibilityModel : BaseViewModel
	{
		public int? GrantApplicationId { get; set; }
		public int? ParticipantId { get; set; }
	}
}