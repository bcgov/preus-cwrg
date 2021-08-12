using System;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.EvaluationForm
{
	public class ClaimEvaluationQuestionAndAnswerViewModel : BaseViewModel
	{
		// The Number of the question within the set. Headers have no number. 
		public int Number { get; set; }

		// The text of the Question
		public string Text { get; set; }

		public ClaimEvaluationFormQuestionType ClaimEvaluationFormQuestionType { get; set; }

		// The answer given by the user. Headers will have no answer.
		public int Answer { get; set; }
		// The answer translated to english words
		public string FullAnswer { get; set; }

		public int RowSequence { get; set; }
		public string RowVersion { get; set; }

		public ClaimEvaluationQuestionAndAnswerViewModel()
		{
		}

		public ClaimEvaluationQuestionAndAnswerViewModel(ClaimEvaluationFormQuestion question)
		{
			Id = question.Id;
			Text = question.Text;
			ClaimEvaluationFormQuestionType = question.ClaimEvaluationFormQuestionType;
			RowSequence = question.RowSequence;
			RowVersion = Convert.ToBase64String(question.RowVersion);
		}

		public ClaimEvaluationQuestionAndAnswerViewModel(ClaimEvaluationAnswer answer)
		{
			Id = answer.Id;
			Text = answer.QuestionAsked;
			ClaimEvaluationFormQuestionType = answer.QuestionType;
			RowSequence = answer.RowSequence;
			Answer = answer.AnswerGiven;
			RowVersion = Convert.ToBase64String(answer.RowVersion);
		}
	}
}