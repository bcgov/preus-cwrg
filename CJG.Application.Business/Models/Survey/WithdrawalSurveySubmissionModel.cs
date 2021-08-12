using System;
using System.Collections.Generic;

namespace CJG.Application.Business.Models.Survey
{
	public class WithdrawalSurveySubmissionModel
	{
		public Guid InvitationKey { get; set; }
		public Guid WithdrawalKey { get; set; }
//		public int? ParticipantFormId { get; set; }  // Gets set via ng lookup
		public DateTime WithdrawalDate { get; set; }

		public List<SurveyQuestionModel> Questions { get; set; }
	}
}