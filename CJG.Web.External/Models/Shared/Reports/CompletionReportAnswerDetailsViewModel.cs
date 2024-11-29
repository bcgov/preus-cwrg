using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;

namespace CJG.Web.External.Models.Shared.Reports
{
    public class CompletionReportAnswerDetailsViewModel : BaseViewModel
	{
		public int QuestionId { get; set; }
		public int Level { get; set; }
		public int GrantApplicationId { get; set; }
		public int? ParticipantFormId { get; set; }
		public IEnumerable<int> EligibleCostBreakdownIds { get; set; }
		public string SelectedAnswer { get; set; }
		public string[] SelectedAnswerList { get; set; }
		public bool BoolAnswer { get; set; }
		public string StringAnswer { get; set; }

		public string NaicsAnswer { get; set; }
		public string NocAnswer { get; set; }
		public string EmployerName { get; set; }
		public string ParticipantName { get; set; }

		public CompletionReportAnswerDetailsViewModel()
		{
		}

		public CompletionReportAnswerDetailsViewModel(CompletionReportQuestion question, int level)
		{
			if (question == null)
				throw new ArgumentNullException(nameof(question));

			QuestionId = question.Id;
			Level = level;
		}

		public CompletionReportAnswerDetailsViewModel(CompletionReportQuestion question, GrantApplication grantApplication, int level)
			: this(question, level)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			GrantApplicationId = grantApplication.Id;
		}

		public CompletionReportAnswerDetailsViewModel(CompletionReportQuestion question, GrantApplication grantApplication, ParticipantForm participant, int level)
			: this(question, grantApplication, level)
		{
			if (participant == null)
				throw new ArgumentNullException(nameof(participant));

			ParticipantFormId = participant.Id;
			NaicsAnswer = participant.NaicsId == null ? null : $"{participant.Naics.Code} | {participant.Naics.Description}";
			NocAnswer = participant.NocId == null ? null : $"{participant.Noc.Code} | {participant.Noc.Description}";
			EmployerName = participant.EmployerName;
			ParticipantName = $"{participant.FirstName} {participant.LastName}";
		}

		public CompletionReportAnswerDetailsViewModel(CompletionReportQuestion question, GrantApplication grantApplication, ParticipantForm participant, ParticipantCompletionReportAnswer answer, IEnumerable<CompletionReportOption> options, int level)
			: this(question, grantApplication, participant, level)
		{
			var intAnswer = answer?.AnswerId ?? 0;
			SelectedAnswer = options.FirstOrDefault(o => o.Id == intAnswer)?.Answer;
			StringAnswer = answer?.OtherAnswer;

			// Special handling for specific question types.
			if (question.QuestionType == CompletionReportQuestionTypes.CommunityList)
			{
				SelectedAnswer = answer?.Community?.Caption;
			}
			else if (question.QuestionType == CompletionReportQuestionTypes.NAICSList)
			{
				SelectedAnswer = NaicsAnswer;
			}
			else if (question.QuestionType == CompletionReportQuestionTypes.NOCList)
			{
				SelectedAnswer = NocAnswer;
			}
			else if (question.QuestionType == CompletionReportQuestionTypes.MultipleCheckbox)
			{
				if (answer?.MultAnswers == null)
					return;

				SelectedAnswerList = new string[answer.MultAnswers.Count];

				int salIdx = 0;
				foreach (var multAnswer in answer.MultAnswers)
				{
					foreach (var opt in question.Options)
					{
						if (opt.Id == multAnswer.Id)
						{
							SelectedAnswerList[salIdx++] = opt.Answer;
						}
					}
				}
			}
		}

		public CompletionReportAnswerDetailsViewModel(CompletionReportQuestion question, GrantApplication grantApplication, EmployerCompletionReportAnswer answer, IEnumerable<CompletionReportOption> options, int level)
			: this(question, grantApplication, level)
		{
			var intAnswer = answer?.AnswerId ?? 0;
			SelectedAnswer = options.FirstOrDefault(o => o.Id == intAnswer)?.Answer;
			StringAnswer = answer?.OtherAnswer;
		}
	}
}
