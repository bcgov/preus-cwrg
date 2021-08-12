using System.Collections.Generic;

namespace CJG.Application.Business.Models.Survey.Job
{
	public class SurveyParticipantListModel
	{
		public List<KeyValuePair<int, string>> Questions = new List<KeyValuePair<int, string>>();
		public List<SurveyParticipantModel> Participants = new List<SurveyParticipantModel>();
	}
}