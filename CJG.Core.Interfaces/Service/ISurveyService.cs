using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Application.Business.Models.Survey;
using CJG.Application.Business.Models.Survey.Job;
using CJG.Core.Entities;

namespace CJG.Core.Interfaces.Service
{
	public interface ISurveyService
	{
		/// <summary>
		/// Return -1 if no PIF found. 0 if PIF found but answers are complete. PIF Id if PIF is found and not answered.
		/// </summary>
		/// <param name="invitationGuid"></param>
		/// <param name="firstName"></param>
		/// <param name="lastName"></param>
		/// <param name="dateOfBirth"></param>
		/// <returns></returns>
		int FindParticipantFormForExitSurvey(Guid invitationGuid, string firstName, string lastName, DateTime dateOfBirth);
		IEnumerable<ParticipantExitSurveyQuestion> GetExitSurveyQuestions();
		void SubmitExitSurvey(ExitSurveySubmissionModel model);
		SurveyParticipantListModel GetParticipantsWithExitSurveyAnswersToReport(DateTime currentDate, int take, DateTime cutoffDate);

		int FindParticipantFormForWithdrawalSurvey(Guid invitationGuid, Guid withdrawalGuid);
		ParticipantForm FindParticipantFormForWithdrawalSurvey(GrantApplication grantApplication, Guid withdrawalGuid);
		IOrderedQueryable<ParticipantWithdrawalSurveyQuestion> GetWithdrawalSurveyQuestions();
		void SubmitWithdrawalSurvey(WithdrawalSurveySubmissionModel model);
		SurveyParticipantListModel GetParticipantsWithWithdrawalSurveyAnswersToReport(DateTime currentDate, int take, DateTime cutoffDate);
	}
}