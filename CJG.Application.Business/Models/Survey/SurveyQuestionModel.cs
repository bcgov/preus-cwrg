using System.Collections.Generic;
using CJG.Core.Entities;

namespace CJG.Application.Business.Models.Survey
{
	public class SurveyQuestionModel
	{
		public int QuestionId { get; set; }

		public string QuestionText { get; set; }
		public SurveyQuestionType QuestionType { get; set; }

		public List<SurveyQuestionOptionModel> Options { get; set; } =  new List<SurveyQuestionOptionModel>();
	}
}