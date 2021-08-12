using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using CJG.Application.Business.Models.Survey;
using CJG.Application.Business.Models.Survey.Job;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Entities;
using NLog;

namespace CJG.Application.Services
{
	public class SurveyService : Service, ISurveyService
	{
		public SurveyService(IDataContext context, HttpContextBase httpContext, ILogger logger)
			: base(context, httpContext, logger)
		{
		}

		public int FindParticipantFormForExitSurvey(Guid invitationGuid, string firstName, string lastName, DateTime dateOfBirth)
		{
			var application = GetApplication(invitationGuid);
			if (application == null)
				return -1;

			var dateOfBirthUtc = dateOfBirth;

			var pif = application.ParticipantForms
				.Where(pf => pf.FirstName == firstName)
				.Where(pf => pf.LastName == lastName)
				.Where(pf => pf.BirthDate.Date == dateOfBirthUtc.Date)
				.FirstOrDefault();

			if (pif == null)
				return -1;

			if (pif.ParticipantExitSurveyAnswers.Any())
				return 0;

			if (pif.ParticipantWithdrawalSurveyAnswers.Any())
				return 0;

			return pif.Id;
		}

		public IEnumerable<ParticipantExitSurveyQuestion> GetExitSurveyQuestions()
		{
			var questions = _dbContext.ParticipantExitSurveyQuestions
				.Where(q => q.IsActive)
				.Include(q => q.Options)
				.OrderBy(q => q.Sequence);

			return questions;
		}

		public void SubmitExitSurvey(ExitSurveySubmissionModel model)
		{
			var grantApplication = GetApplication(model.InvitationKey);

			if (grantApplication == null)
				throw new ArgumentException($"No application found for invitation {model.InvitationKey}");

			if (!model.ParticipantFormId.HasValue)
				throw new ArgumentException("No participant form id provided.");

			var participantForm = grantApplication.ParticipantForms
				.FirstOrDefault(pf => pf.Id == model.ParticipantFormId.Value);

			if (participantForm == null)
				throw new ArgumentException("No participant form found.");

			if (participantForm.ParticipantExitSurveyAnswers.Any())
				throw new Exception("This participant form already has a completed Exit Form.");

			
			participantForm.TrainingExitDate = grantApplication.GetTrainingEndDate();

			foreach (var questionModel in model.Questions)
			{
				foreach (var optionModel in questionModel.Options)
				{
					var answer = new ParticipantExitSurveyAnswer
					{
						ParticipantFormId = participantForm.Id,
						ParticipantExitSurveyQuestionOptionId = optionModel.OptionId,
						DateAdded = AppDateTime.UtcNow,
					};

					if (questionModel.QuestionType == SurveyQuestionType.FreeText)
					{
						answer.Answer = true;
						answer.TextAnswer = optionModel.TextAnswer;
					}
					else
					{
						answer.OptionTextDisplayed = optionModel.OptionText; // Capture the replaced version of the Option text
						answer.Answer = optionModel.AnswerGiven;

						if (optionModel.AllowOther && optionModel.AnswerGiven && !string.IsNullOrWhiteSpace(optionModel.TextAnswer))
							answer.TextAnswer = optionModel.TextAnswer;
					}

					participantForm.ParticipantExitSurveyAnswers.Add(answer);
				}
			}

			_dbContext.Update(participantForm);
			_dbContext.SaveChanges();
		}

		public SurveyParticipantListModel GetParticipantsWithExitSurveyAnswersToReport(DateTime currentDate, int take, DateTime cutoffDate)
		{
			var currentDateUtc = currentDate.ToUniversalTime();
			var cutoffDateUtc = cutoffDate.ToUniversalTime();
			var defaultGrantProgramId = GetDefaultGrantProgramId();

			var formsToProcess = _dbContext.ParticipantForms
				.Include(f => f.GrantApplication)
				.Where(pf => pf.GrantApplication.GrantOpening.GrantStream.GrantProgramId == defaultGrantProgramId)
				.Where(pf => !pf.ExitSurveyReportedOn.HasValue || pf.ExitSurveyReportedOn > currentDateUtc)
				.Where(pf => pf.ParticipantExitSurveyAnswers.Any())
				.Where(pf => pf.GrantApplication.ApplicationStateInternal != ApplicationStateInternal.Draft)
				.Take(take)
				.ToList();

			var participantExitSurveyQuestions = GetExitSurveyQuestions();

			var model = new SurveyParticipantListModel
			{
				Questions = participantExitSurveyQuestions
					.Select(q => new KeyValuePair<int, string>(q.Id, q.Question))
					.ToList(),
				Participants = GetParticipantModelsForExitSurvey(formsToProcess)
			};

			return model;
		}

		public SurveyParticipantListModel GetParticipantsWithWithdrawalSurveyAnswersToReport(DateTime currentDate, int take, DateTime cutoffDate)
		{
			var currentDateUtc = currentDate.ToUniversalTime();
			var cutoffDateUtc = cutoffDate.ToUniversalTime();
			var defaultGrantProgramId = GetDefaultGrantProgramId();

			var formsToProcess = _dbContext.ParticipantForms
				.Include(f => f.GrantApplication)
				.Where(pf => pf.GrantApplication.GrantOpening.GrantStream.GrantProgramId == defaultGrantProgramId)
				.Where(pf => !pf.EarlyWithdrawalReportedOn.HasValue || pf.EarlyWithdrawalReportedOn > currentDateUtc)
				.Where(pf => pf.ParticipantWithdrawalSurveyAnswers.Any())
				.Where(pf => pf.GrantApplication.ApplicationStateInternal != ApplicationStateInternal.Draft)
				.Take(take)
				.ToList();

			var questions = GetWithdrawalSurveyQuestions().ToList();
			var participants = GetParticipantModelsForWithdrawalSurvey(formsToProcess);

			var model = new SurveyParticipantListModel
			{
				Questions = questions
					.Select(q => new KeyValuePair<int, string>(q.Id, q.Question))
					.ToList(),
				Participants = participants
			};

			return model;
		}

		public int FindParticipantFormForWithdrawalSurvey(Guid invitationGuid, Guid withdrawalGuid)
		{
			var application = GetApplication(invitationGuid);
			if (application == null)
				return -1;

			var pif = application.ParticipantForms
				.FirstOrDefault(pf => pf.WithdrawalKey == withdrawalGuid);

			if (pif == null)
				return -1;

			if (pif.ParticipantWithdrawalSurveyAnswers.Any())
				return 0;

			if (pif.ParticipantExitSurveyAnswers.Any())
				return 0;

			return pif.Id;
		}

		public ParticipantForm FindParticipantFormForWithdrawalSurvey(GrantApplication grantApplication, Guid withdrawalGuid)
		{
			var pif = grantApplication.ParticipantForms
				.FirstOrDefault(pf => pf.WithdrawalKey == withdrawalGuid);

			return pif;
		}

		public IOrderedQueryable<ParticipantWithdrawalSurveyQuestion> GetWithdrawalSurveyQuestions()
		{
			var questions = _dbContext.ParticipantWithdrawalSurveyQuestions
				.Where(q => q.IsActive)
				.Include(q => q.Options)
				.OrderBy(q => q.Sequence);

			return questions;
		}

		public void SubmitWithdrawalSurvey(WithdrawalSurveySubmissionModel model)
		{
			var application = GetApplication(model.InvitationKey);

			if (application == null)
				throw new ArgumentException($"No application found for invitation {model.InvitationKey}");

			var participantForm = application.ParticipantForms
				.FirstOrDefault(pf => pf.WithdrawalKey == model.WithdrawalKey);

			if (participantForm == null)
				throw new ArgumentException("No participant form found.");

			if (participantForm.ParticipantWithdrawalSurveyAnswers.Any())
				throw new Exception("This participant form already has a completed Withdrawal Form.");

			participantForm.TrainingWithdrawalDate = model.WithdrawalDate;

			foreach (var questionModel in model.Questions)
			{
				foreach (var optionModel in questionModel.Options)
				{
					var answer = new ParticipantWithdrawalSurveyAnswer
					{
						ParticipantFormId = participantForm.Id,
						ParticipantWithdrawalSurveyQuestionOptionId = optionModel.OptionId,
						DateAdded = AppDateTime.UtcNow,
					};

					if (questionModel.QuestionType == SurveyQuestionType.FreeText)
					{
						answer.Answer = true;
						answer.TextAnswer = optionModel.TextAnswer;
					}
					else
					{
						answer.OptionTextDisplayed = optionModel.OptionText; // Capture the replaced version of the Option text
						answer.Answer = optionModel.AnswerGiven;

						if (optionModel.AllowOther && optionModel.AnswerGiven && !string.IsNullOrWhiteSpace(optionModel.TextAnswer))
							answer.TextAnswer = optionModel.TextAnswer;
					}

					participantForm.ParticipantWithdrawalSurveyAnswers.Add(answer);
				}
			}

			_dbContext.Update(participantForm);
			_dbContext.SaveChanges();
		}

		private static List<SurveyParticipantModel> GetParticipantModelsForExitSurvey(List<ParticipantForm> formsToProcess)
		{
			var participants = new List<SurveyParticipantModel>();

			foreach (var participantForm in formsToProcess)
			{
				var answers = participantForm.ParticipantExitSurveyAnswers
					.Where(a => a.Answer)
					.Where(a => a.ParticipantExitSurveyQuestionOption.IsActive)
					.Where(a => a.ParticipantExitSurveyQuestionOption.ParticipantExitSurveyQuestion.IsActive)
					.OrderBy(a => a.ParticipantExitSurveyQuestionOption.ParticipantExitSurveyQuestion.Sequence)
					.ThenBy(a => a.ParticipantExitSurveyQuestionOption.Sequence)
					.Select(a => new
					{
						Question = a.ParticipantExitSurveyQuestionOption.ParticipantExitSurveyQuestionId,
						Answers = GetTextAnswer(a)
					})
					.GroupBy(b => b.Question)
					.Select(qq => new KeyValuePair<int, List<string>>(qq.Key, qq.Select(g => g.Answers).ToList()));

				var model = new SurveyParticipantModel
				{
					ParticipantFormId = participantForm.Id,
					AgreementNumber = participantForm.GrantApplication.FileNumber,
					AgreementHolder = participantForm.GrantApplication.OrganizationLegalName,
					Email = participantForm.EmailAddress,
					FirstName = participantForm.FirstName,
					LastName = participantForm.LastName,
					ExitDate = participantForm.TrainingExitDate,
					TrainingStartDate = participantForm.ProgramStartDate,
					TrainingEndDate = participantForm.GrantApplication.GetTrainingEndDate(),
					Answers = answers.ToList()
				};

				participants.Add(model);
			}

			return participants;
		}

		private static List<SurveyParticipantModel> GetParticipantModelsForWithdrawalSurvey(List<ParticipantForm> formsToProcess)
		{
			var participants = new List<SurveyParticipantModel>();

			foreach (var participantForm in formsToProcess)
			{
				var answers = participantForm.ParticipantWithdrawalSurveyAnswers
					.Where(a => a.Answer)
					.Where(a => a.ParticipantWithdrawalSurveyQuestionOption.IsActive)
					.Where(a => a.ParticipantWithdrawalSurveyQuestionOption.ParticipantWithdrawalSurveyQuestion.IsActive)
					.OrderBy(a => a.ParticipantWithdrawalSurveyQuestionOption.ParticipantWithdrawalSurveyQuestion.Sequence)
					.ThenBy(a => a.ParticipantWithdrawalSurveyQuestionOption.Sequence)
					.Select(a => new
					{
						Question = a.ParticipantWithdrawalSurveyQuestionOption.ParticipantWithdrawalSurveyQuestionId,
						Answers = GetTextAnswer(a)
					})
					.GroupBy(b => b.Question)
					.Select(qq => new KeyValuePair<int, List<string>>(qq.Key, qq.Select(g => g.Answers).ToList()));

				var model = new SurveyParticipantModel
				{
					ParticipantFormId = participantForm.Id,
					AgreementNumber = participantForm.GrantApplication.FileNumber,
					AgreementHolder = participantForm.GrantApplication.OrganizationLegalName,
					Email = participantForm.EmailAddress,
					FirstName = participantForm.FirstName,
					LastName = participantForm.LastName,
					WithdrawalDate = participantForm.TrainingWithdrawalDate,
					TrainingStartDate = participantForm.ProgramStartDate,
					TrainingEndDate = participantForm.GrantApplication.GetTrainingEndDate(),
					Answers = answers.ToList()
				};

				participants.Add(model);
			}

			return participants;
		}

		private static string GetTextAnswer(ISurveyAnswer a)
		{
			if (string.IsNullOrWhiteSpace(a.OptionTextDisplayed) && !string.IsNullOrWhiteSpace(a.TextAnswer))
				return a.TextAnswer;

			if (!string.IsNullOrWhiteSpace(a.OptionTextDisplayed) && !string.IsNullOrWhiteSpace(a.TextAnswer))
				return $"{a.OptionTextDisplayed}: {a.TextAnswer}";

			return a.OptionTextDisplayed;
		}

		// This is a clone of GrantApplicationService.Get to reduce DI in the Job Service
		private GrantApplication GetApplication(Guid invitationKey)
		{
			if (invitationKey == Guid.Empty)
				return null;

			return _dbContext.GrantApplications
				.FirstOrDefault(o => o.InvitationKey.ToString().Equals(invitationKey.ToString(), StringComparison.OrdinalIgnoreCase));
		}
	}
}