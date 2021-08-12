using System;
using System.Collections.Generic;
using CJG.Application.Business.Models.Survey;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Part.Models.ExitSurvey
{
    public class ExitSurveyModel : BaseViewModel
    {
	    public Guid InvitationKey { get; set; }
	    public DateTime EarliestExit { get; set; }
	    public DateTime LatestExit { get; set; }
	    public DateTime AppDateTimeNow { get; set; } = AppDateTime.UtcNow;

		public bool RecaptchaEnabled { get; set; }
	    public string RecaptchaSiteKey { get; set; }
	    public string TimeoutPeriod { get; set; }

	    public string RecaptchaEncodedResponse { get; set; }

		public string FirstName { get; set; }
	    public string LastName { get; set; }
	    public DateTime? DateOfBirth { get; set; }

	    public int? ExitDay { get; set; }
	    public int? ExitMonth { get; set; }
	    public int? ExitYear { get; set; }

		public int? BirthDay { get; set; }
	    public int? BirthMonth { get; set; }
	    public int? BirthYear { get; set; }

	    public DateTime ParticipantOldestAge { get; set; }
	    public DateTime ParticipantYoungestAge { get; set; }

		public int? ParticipantFormId { get; set; }  // Gets set via ng lookup
		public string AgreementNumber { get; set; }
		public DateTime? ExitDate { get; set; }
		public List<SurveyQuestionModel> Questions { get; set; }

		public ExitSurveySubmissionModel GetSubmissionModel()
		{
			var model = new ExitSurveySubmissionModel
			{
				InvitationKey = InvitationKey,
				ParticipantFormId = ParticipantFormId,
				ExitDate = ExitDate,
				Questions = Questions
			};

			return model;
		}
    }
}
