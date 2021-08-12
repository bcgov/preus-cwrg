using System;
using System.ComponentModel.DataAnnotations;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.EvaluationForm
{
	public class ClaimEvaluationFormQuestionViewModel : BaseViewModel
	{
		[Required(ErrorMessage = "The question text is required")]
		public string Text { get; set; }

		[Required]
		public ClaimEvaluationFormQuestionType ClaimEvaluationFormQuestionType { get; set; }

		public int RowSequence { get; set; }

		public string RowVersion { get; set; }

		public ClaimEvaluationFormQuestionViewModel()
		{
		}

		public ClaimEvaluationFormQuestionViewModel(ClaimEvaluationFormQuestion question)
		{
			Id = question.Id;
			Text = question.Text;
			ClaimEvaluationFormQuestionType = question.ClaimEvaluationFormQuestionType;
			RowSequence = question.RowSequence;
			RowVersion = Convert.ToBase64String(question.RowVersion);
		}
	}
}