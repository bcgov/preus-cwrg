using System;
using System.Collections.Generic;

namespace CJG.Application.Business.Models.Survey.Job
{
	public class SurveyParticipantModel
	{
		public int ParticipantFormId { get; set; }
		public string AgreementNumber { get; set; }
		public string AgreementHolder { get; set; }
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public DateTime? TrainingStartDate { get; set; }
		public DateTime? TrainingEndDate { get; set; }
		public DateTime? WithdrawalDate { get; set; }
		public DateTime? ExitDate { get; set; }

		public List<KeyValuePair<int, List<string>>> Answers { get; set; }
	}
}