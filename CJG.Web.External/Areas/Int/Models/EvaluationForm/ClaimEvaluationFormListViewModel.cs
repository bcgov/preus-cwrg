using System.Collections.Generic;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.EvaluationForm
{
	public class ClaimEvaluationFormListViewModel : BaseViewModel
	{
		public int ClaimVersion { get; set; }
		public bool EvaluationIsComplete { get; set; }

		public List<ClaimEvaluationFormQuestionViewModel> Questions { get; set; }
		public List<ClaimEvaluationQuestionAndAnswerViewModel> QuestionsWithAnswers { get; set; }
	}
}