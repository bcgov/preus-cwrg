namespace CJG.Application.Business.Models.Survey
{
	public class SurveyQuestionOptionModel
	{
		public int QuestionId { get; set; }
		public int OptionId { get; set; }

		public string OptionText { get; set; }
		public bool AllowOther { get; set; }

		public bool AnswerGiven { get; set; }
		public string TextAnswer { get; set; }
	}
}