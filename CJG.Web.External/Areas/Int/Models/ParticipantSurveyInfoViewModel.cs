using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using Environment = System.Environment;

namespace CJG.Web.External.Areas.Int.Models
{
    public class ParticipantSurveyInfoViewModel
	{
		public bool HasEarlyWithdrawal { get; set; }
		public bool HasExitSurvey { get; set; }

		public string AgreementNumber { get; set; }
		public string AgreementHolder { get; set; }
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }

		public DateTime? ExitDate { get; set; }
		public DateTime? WithdrawalDate { get; set; }

		public List<ParticipantSurveySummaryModel> ExitSurveyQuestions { get; set; } = new List<ParticipantSurveySummaryModel>();
		public List<ParticipantSurveySummaryModel> WithdrawalSurveyQuestions { get; set; } = new List<ParticipantSurveySummaryModel>();

		public ParticipantSurveyInfoViewModel()
		{
		}

		public ParticipantSurveyInfoViewModel(ParticipantForm participantForm, ISurveyService surveyService)
		{
			HasExitSurvey = participantForm.ParticipantExitSurveyAnswers.Any();
			HasEarlyWithdrawal = participantForm.ParticipantWithdrawalSurveyAnswers.Any();

			if (!HasExitSurvey && !HasEarlyWithdrawal)
				return;

			var grantApplication = participantForm.GrantApplication;

			AgreementNumber = grantApplication.FileNumber;
			AgreementHolder = grantApplication.OrganizationLegalName;
			Email = participantForm.EmailAddress;
			FirstName = participantForm.FirstName;
			LastName = participantForm.LastName;

			SetExitInfo(participantForm);
			SetWithdrawalInfo(participantForm);
		}

		private void SetExitInfo(ParticipantForm participantForm)
		{
			if (participantForm.TrainingExitDate.HasValue)
				ExitDate = participantForm.TrainingExitDate.Value;

			if (!participantForm.ParticipantExitSurveyAnswers.Any())
				return;

			ExitSurveyQuestions = participantForm.ParticipantExitSurveyAnswers
				.Where(a => a.Answer)
				.OrderBy(a => a.ParticipantExitSurveyQuestionOption.ParticipantExitSurveyQuestion.Sequence)
				.ThenBy(a => a.ParticipantExitSurveyQuestionOption.Sequence)
				.Select(a => new
				{
					Question = a.ParticipantExitSurveyQuestionOption.ParticipantExitSurveyQuestion.Question,
					Answers = GetTextAnswer(a)
				})
				.GroupBy(b => b.Question)
				.Select(qq => new ParticipantSurveySummaryModel
				{
					Question = qq.Key,
					Answers = qq
						.Select(g => g.Answers)
						.ToList()
				})
				.ToList();
		}

		private void SetWithdrawalInfo(ParticipantForm participantForm)
		{
			if (participantForm.TrainingWithdrawalDate.HasValue)
				WithdrawalDate = participantForm.TrainingWithdrawalDate.Value;

			if (!participantForm.ParticipantWithdrawalSurveyAnswers.Any())
				return;

			WithdrawalSurveyQuestions = participantForm.ParticipantWithdrawalSurveyAnswers
				.Where(a => a.Answer)
				.OrderBy(a => a.ParticipantWithdrawalSurveyQuestionOption.ParticipantWithdrawalSurveyQuestion.Sequence)
				.ThenBy(a => a.ParticipantWithdrawalSurveyQuestionOption.Sequence)
				.Select(a => new
				{
					Question = a.ParticipantWithdrawalSurveyQuestionOption.ParticipantWithdrawalSurveyQuestion.Question,
					Answers = GetTextAnswer(a)
				})
				.GroupBy(b => b.Question)
				.Select(qq => new ParticipantSurveySummaryModel
				{
					Question = qq.Key,
					Answers = qq
						.Select(g => g.Answers)
						.ToList()
				})
				.ToList();
		}

		private static string GetTextAnswer(ISurveyAnswer a)
		{
			if (string.IsNullOrWhiteSpace(a.OptionTextDisplayed) && !string.IsNullOrWhiteSpace(a.TextAnswer))
			{
				var lines = a.TextAnswer.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
				return string.Join("<br />", lines);
			}

			if (!string.IsNullOrWhiteSpace(a.OptionTextDisplayed) && !string.IsNullOrWhiteSpace(a.TextAnswer))
				return $"{a.OptionTextDisplayed}: {a.TextAnswer}";

			return a.OptionTextDisplayed;
		}

		public class ParticipantSurveySummaryModel
		{
			public string Question { get; set; }
			public List<string> Answers { get; set; }
		}
	}
}