using System.Collections.Generic;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.ParticipantFundingStreams
{
    public class ParticipantFundingStreamsModel : BaseViewModel
	{
		public List<ParticipantFundingStreamModel> FundingStreams { get; set; }
	}
}