using System.Collections.Generic;
using System.Linq;
using CJG.Application.Business.Models.Survey;
using CJG.Core.Entities;

namespace CJG.Application.Services.Extensions
{
	public static class SurveyModelExtensions
	{
		public static IEnumerable<SurveyQuestionModel> ToViewModel(this IEnumerable<ParticipantExitSurveyQuestion> questions, GrantApplication grantApplication)
		{
			var agreementHolderName = grantApplication.OrganizationLegalName;
			var trainingProviderNames = grantApplication.TrainingPrograms
				.ToList()
				.Select(tp => tp.TrainingProvider)
				.Select(tp => tp.Name);

			var trainingProviderName = string.Join(" or ", trainingProviderNames);

			var questionModels = new List<SurveyQuestionModel>();
			var questionIndex = 0;

			foreach (var question in questions)
			{
				questionIndex++;

				var questionModel = new SurveyQuestionModel
				{
					QuestionId = question.Id,
					QuestionType = question.QuestionType,
					QuestionText = question.QuestionType == SurveyQuestionType.FreeText
						? question.Question
						: $"{questionIndex}. {question.Question}",
					Options = new List<SurveyQuestionOptionModel>()
				};

				foreach (var option in question.Options.Where(o => o.IsActive).OrderBy(o => o.Sequence))
				{
					var optionModel = new SurveyQuestionOptionModel
					{
						QuestionId = option.ParticipantExitSurveyQuestionId,
						OptionId = option.Id,
						OptionText = option.OptionText,
						AllowOther = option.AllowOther
					};

					var replacementToken = option.ReplacementToken;
					if (!string.IsNullOrWhiteSpace(replacementToken))
					{
						// Available tokens currently are: [TrainingProviderName] and [AgreementHolderName]
						if (replacementToken == "[AgreementHolderName]")
							optionModel.OptionText = optionModel.OptionText.Replace(replacementToken, agreementHolderName);

						if (replacementToken == "[TrainingProviderName]")
							optionModel.OptionText = optionModel.OptionText.Replace(replacementToken, trainingProviderName);
					}

					questionModel.Options.Add(optionModel);
				}
				questionModels.Add(questionModel);
			}

			return questionModels;
		}

		public static IEnumerable<SurveyQuestionModel> ToViewModel(this IEnumerable<ParticipantWithdrawalSurveyQuestion> questions, GrantApplication grantApplication)
		{
			var agreementHolderName = grantApplication.OrganizationLegalName;
			var trainingProviderNames = grantApplication.TrainingPrograms
				.ToList()
				.Select(tp => tp.TrainingProvider)
				.Select(tp => tp.Name);

			var trainingProviderName = string.Join(", ", trainingProviderNames);

			var questionModels = new List<SurveyQuestionModel>();
			var questionIndex = 0;

			foreach (var question in questions)
			{
				questionIndex++;

				var questionModel = new SurveyQuestionModel
				{
					QuestionId = question.Id,
					QuestionType = question.QuestionType,
					QuestionText = question.QuestionType == SurveyQuestionType.FreeText
						? question.Question
						: $"{questionIndex}. {question.Question}",
					Options = new List<SurveyQuestionOptionModel>()
				};

				foreach (var option in question.Options.Where(o => o.IsActive).OrderBy(o => o.Sequence))
				{
					var optionModel = new SurveyQuestionOptionModel
					{
						QuestionId = option.ParticipantWithdrawalSurveyQuestionId,
						OptionId = option.Id,
						OptionText = option.OptionText,
						AllowOther = option.AllowOther
					};

					var replacementToken = option.ReplacementToken;
					if (!string.IsNullOrWhiteSpace(replacementToken))
					{
						// Available tokens currently are: [TrainingProviderName] and [AgreementHolderName]
						if (replacementToken == "[AgreementHolderName]")
							optionModel.OptionText = optionModel.OptionText.Replace(replacementToken, agreementHolderName);

						if (replacementToken == "[TrainingProviderName]")
							optionModel.OptionText = optionModel.OptionText.Replace(replacementToken, trainingProviderName);
					}

					questionModel.Options.Add(optionModel);
				}
				questionModels.Add(questionModel);
			}

			return questionModels;
		}
	}
}