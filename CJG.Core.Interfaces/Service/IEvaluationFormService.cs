using System.Collections.Generic;
using CJG.Application.Business.Models;
using CJG.Core.Entities;

namespace CJG.Core.Interfaces.Service
{
	public interface IEvaluationFormService : IService
	{
		IEnumerable<EvaluationFormQuestion> GetQuestions();
		void UpdateQuestions(List<EvaluationFormQuestion> questionList, List<EvaluationFormQuestion> removeItems);

		IEnumerable<ClaimEvaluationFormQuestion> GetClaimQuestions();
		void UpdateClaimQuestions(List<ClaimEvaluationFormQuestion> questionList, List<ClaimEvaluationFormQuestion> removeItems);

		IEnumerable<EvaluationFormResource> GetResources();
		Attachment GetAttachment(int id);
		Attachment UpdateResource(EvaluationFormResource resource, Attachment attachment, bool commit = false);
		Attachment AddResource(Attachment attachment, bool commit = false);
		void DeleteResource(EvaluationFormResource resource, Attachment attachment);

		void UpdateAnswers(GrantApplication grantApplication, List<EvaluationAnswerModel> model);
		void SubmitEvaluation(GrantApplication grantApplication);
		void WithdrawEvaluation(GrantApplication grantApplication);

		void UpdateAnswers(Claim claim, int claimVersion, List<ClaimEvaluationAnswerModel> model);
	}
}