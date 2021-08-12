using System;
using System.ComponentModel.DataAnnotations;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.EvaluationForm
{
	public class EvaluationFormQuestionViewModel : BaseViewModel
    {
		[Required]
	    public string Text { get; set; }

	    [Required]
		public EvaluationFormQuestionType EvaluationFormQuestionType { get; set; }

		public int RowSequence { get; set; }

		public string RowVersion { get; set; }

		public EvaluationFormQuestionViewModel()
		{
		}

		public EvaluationFormQuestionViewModel(EvaluationFormQuestion question)
		{
			Id = question.Id;
			Text = question.Text;
			EvaluationFormQuestionType = question.EvaluationFormQuestionType;
			RowSequence = question.RowSequence;
			RowVersion = Convert.ToBase64String(question.RowVersion);
		}
	}
}