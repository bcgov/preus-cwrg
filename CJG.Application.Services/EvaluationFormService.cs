using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CJG.Application.Business.Models;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Entities;
using NLog;

namespace CJG.Application.Services
{
	public class EvaluationFormService : Service, IEvaluationFormService
	{
		public EvaluationFormService(IDataContext context, HttpContextBase httpContext, ILogger logger)
			: base(context, httpContext, logger)
		{
		}

		public IEnumerable<EvaluationFormQuestion> GetQuestions()
		{
			return _dbContext.EvaluationFormQuestions
				.OrderBy(ev => ev.RowSequence);
		}

		public IEnumerable<ClaimEvaluationFormQuestion> GetClaimQuestions()
		{
			return _dbContext.ClaimEvaluationFormQuestions
				.OrderBy(ev => ev.RowSequence);
		}

		public void UpdateQuestions(List<EvaluationFormQuestion> questionList, List<EvaluationFormQuestion> removeItems)
		{
			if (questionList == null)
				throw new ArgumentNullException(nameof(questionList));

			foreach (var question in questionList)
			{
				if (question.Id == 0)
					_dbContext.EvaluationFormQuestions.Add(question);
			}

			// Have to explicitly remove these from the dbContext so they get deleted rather than disconnected
			foreach (var removeItem in removeItems)
				_dbContext.EvaluationFormQuestions.Remove(removeItem);

			_dbContext.CommitTransaction();
		}
		
		public void UpdateClaimQuestions(List<ClaimEvaluationFormQuestion> questionList, List<ClaimEvaluationFormQuestion> removeItems)
		{
			if (questionList == null)
				throw new ArgumentNullException(nameof(questionList));

			foreach (var question in questionList)
			{
				if (question.Id == 0)
					_dbContext.ClaimEvaluationFormQuestions.Add(question);
			}

			// Have to explicitly remove these from the dbContext so they get deleted rather than disconnected
			foreach (var removeItem in removeItems)
				_dbContext.ClaimEvaluationFormQuestions.Remove(removeItem);

			_dbContext.CommitTransaction();
		}

		public IEnumerable<EvaluationFormResource> GetResources()
		{
			return _dbContext.EvaluationFormResources
				.OrderBy(ev => ev.RowSequence)
				.ThenBy(ev => ev.Attachment.Description);
		}

		public Attachment GetAttachment(int id)
		{
			return Get<Attachment>(id);
		}

		public Attachment UpdateResource(EvaluationFormResource resource, Attachment attachment, bool commit = false)
		{
			if (attachment == null)
				throw new ArgumentNullException(nameof(attachment));

			var existingAttachment = Get<Attachment>(attachment.Id);
			existingAttachment.RowVersion = attachment.RowVersion ?? existingAttachment.RowVersion;
			existingAttachment.CreateNewVersion(attachment.FileName, attachment.Description, attachment.FileExtension, attachment.AttachmentData);

			_dbContext.Update(existingAttachment);

			if (commit)
				_dbContext.CommitTransaction();
			return existingAttachment;
		}

		public Attachment AddResource(Attachment attachment, bool commit = false)
		{
			if (attachment == null)
				throw new ArgumentNullException(nameof(attachment));

			_dbContext.EvaluationFormResources.Add(new EvaluationFormResource
			{
				Attachment = attachment
			});
			//_dbContext.Attachments.Add(attachment);

			if (commit)
				_dbContext.CommitTransaction();

			return attachment;
		}

		public void DeleteResource(EvaluationFormResource resource, Attachment attachment)
		{
			if (attachment == null)
				throw new ArgumentNullException(nameof(attachment));

			var deleteResource = _dbContext.EvaluationFormResources.FirstOrDefault(r => r.Attachment.Id == attachment.Id);

			foreach (var version in attachment.Versions)
			{
				_dbContext.VersionedAttachments.Remove(version);
			}

			_dbContext.Attachments.Remove(attachment);

			if (deleteResource != null)
				_dbContext.EvaluationFormResources.Remove(deleteResource);

			_dbContext.CommitTransaction();
		}

		public void UpdateAnswers(GrantApplication grantApplication, List<EvaluationAnswerModel> model)
		{
			var evaluation = grantApplication.GrantApplicationEvaluation;
			if (evaluation == null)
			{
				evaluation = new GrantApplicationEvaluation
				{
					GrantApplication = grantApplication
				};
				grantApplication.GrantApplicationEvaluation = evaluation;
			}

			if (evaluation.EvaluationAnswers == null)
				evaluation.EvaluationAnswers = new List<GrantApplicationEvaluationAnswer>();

			var questions = GetQuestions().ToList();
			var currentAnswers = evaluation.EvaluationAnswers.ToList();

			var questionIds = questions.Select(q => q.Id).ToList();

			// Remove any questions that were since removed from the Question List
			var answersToRemove = currentAnswers.Where(ca => !questionIds.Contains(ca.EvaluationFormQuestionReferenceId));
			foreach (var answer in answersToRemove)
				_dbContext.GrantApplicationEvaluationAnswers.Remove(answer);

			foreach (var answeredQuestion in model)
			{
				var questionId = answeredQuestion.EvaluationQuestionId;
				var currentQuestion = questions.FirstOrDefault(q => q.Id == questionId);
				var currentAnswer = currentAnswers.FirstOrDefault(a => a.EvaluationFormQuestionReferenceId == questionId);

				if (currentQuestion == null)
					continue;

				if (currentAnswer == null)
				{
					currentAnswer = new GrantApplicationEvaluationAnswer
					{
						GrantApplicationEvaluation = grantApplication.GrantApplicationEvaluation,
						EvaluationFormQuestionReferenceId = currentQuestion.Id
					};

					evaluation.EvaluationAnswers.Add(currentAnswer);
				}

				currentAnswer.AnswerGiven = answeredQuestion.Answer;
				currentAnswer.QuestionAsked = currentQuestion.Text;
				currentAnswer.QuestionType = currentQuestion.EvaluationFormQuestionType;
				currentAnswer.RowSequence = currentQuestion.RowSequence;
			}
		}

		public void SubmitEvaluation(GrantApplication grantApplication)
		{
			var evaluation = grantApplication.GrantApplicationEvaluation;
			evaluation.EvaluationStatus = EvaluationStatus.Complete;
		}

		public void WithdrawEvaluation(GrantApplication grantApplication)
		{
			var evaluation = grantApplication.GrantApplicationEvaluation;
			evaluation.EvaluationStatus = EvaluationStatus.Started;
		}

		public void UpdateAnswers(Claim claim, int claimVersion, List<ClaimEvaluationAnswerModel> model)
		{
			var evaluation = claim.ClaimEvaluation;
			if (evaluation == null)
			{
				evaluation = new ClaimEvaluation
				{
					Claim = claim
				};
				claim.ClaimEvaluation = evaluation;
			}

			if (evaluation.EvaluationAnswers == null)
				evaluation.EvaluationAnswers = new List<ClaimEvaluationAnswer>();

			var questions = GetClaimQuestions().ToList();
			var currentAnswers = evaluation.EvaluationAnswers.ToList();

			var questionIds = questions.Select(q => q.Id).ToList();

			// Remove any questions that were since removed from the Question List
			var answersToRemove = currentAnswers.Where(ca => !questionIds.Contains(ca.ClaimEvaluationFormQuestionReferenceId));
			foreach (var answer in answersToRemove)
				_dbContext.ClaimEvaluationAnswers.Remove(answer);

			foreach (var answeredQuestion in model)
			{
				var questionId = answeredQuestion.ClaimEvaluationQuestionId;
				var currentQuestion = questions.FirstOrDefault(q => q.Id == questionId);
				var currentAnswer = currentAnswers.FirstOrDefault(a => a.ClaimEvaluationFormQuestionReferenceId == questionId);

				if (currentQuestion == null)
					continue;

				if (currentAnswer == null)
				{
					currentAnswer = new ClaimEvaluationAnswer
					{
						ClaimEvaluation = claim.ClaimEvaluation,
						ClaimEvaluationFormQuestionReferenceId = currentQuestion.Id
					};

					evaluation.EvaluationAnswers.Add(currentAnswer);
				}

				currentAnswer.AnswerGiven = answeredQuestion.Answer;
				currentAnswer.QuestionAsked = currentQuestion.Text;
				currentAnswer.QuestionType = currentQuestion.ClaimEvaluationFormQuestionType;
				currentAnswer.RowSequence = currentQuestion.RowSequence;
			}
		}
	}
}