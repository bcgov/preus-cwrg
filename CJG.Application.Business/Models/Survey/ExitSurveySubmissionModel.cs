using System;
using System.Collections.Generic;

namespace CJG.Application.Business.Models.Survey
{
	public class ExitSurveySubmissionModel
	{
		public Guid InvitationKey { get; set; }
		public int? ParticipantFormId { get; set; }  // Gets set via ng lookup
		public DateTime? ExitDate { get; set; }
		public List<SurveyQuestionModel> Questions { get; set; }
	}
}