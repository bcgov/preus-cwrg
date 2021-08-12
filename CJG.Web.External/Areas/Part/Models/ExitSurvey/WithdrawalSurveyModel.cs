using System;
using System.Collections.Generic;
using CJG.Application.Business.Models.Survey;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Part.Models.ExitSurvey
{
	public class WithdrawalSurveyModel : BaseViewModel
	{
		public Guid InvitationKey { get; set; }
		public Guid WithdrawalKey { get; set; }
		public DateTime? WithdrawalDate { get; set; }
		public DateTime AppDateTimeNow { get; set; } = AppDateTime.UtcNow;

		public bool RecaptchaEnabled { get; set; }
		public string RecaptchaSiteKey { get; set; }
		public string TimeoutPeriod { get; set; }

		public string RecaptchaEncodedResponse { get; set; }

		public DateTime EarliestWithdrawal { get; set; }
		public DateTime LatestWithdrawal { get; set; }
		public string AgreementNumber { get; set; }
		public string TrainingProgramTitle { get; set; }

		public List<SurveyQuestionModel> Questions { get; set; }

		public WithdrawalSurveySubmissionModel GetSubmissionModel()
		{
			var model = new WithdrawalSurveySubmissionModel
			{
				InvitationKey = InvitationKey,
				WithdrawalKey = WithdrawalKey,
				WithdrawalDate = WithdrawalDate.Value,
				Questions = Questions
			};

			return model;
		}
	}
}