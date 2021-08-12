using System.Collections.Generic;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.EvaluationForm
{
	public class EvaluationFormListViewModel : BaseViewModel
	{
		public List<EvaluationFormQuestionViewModel> Questions { get; set; }
		public List<EvaluationQuestionAndAnswerViewModel> QuestionsWithAnswers { get; set; }
	}
}