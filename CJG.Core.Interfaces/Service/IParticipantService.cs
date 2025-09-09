using System;
using System.Collections.Generic;
using CJG.Application.Business.Models.Survey.Job;
using CJG.Core.Entities;

namespace CJG.Core.Interfaces.Service
{
	public interface IParticipantService : IService
	{
		ParticipantForm Get(int id);
		ParticipantForm Add(ParticipantForm newParticipantForm);
		ParticipantCost Add(ParticipantCost newParticipantCost);
		ParticipantCost Update(ParticipantCost participantCost);

		void ApproveDenyParticipants(int grantApplicationId, Dictionary<int?, bool?> participantApproved);
		void ReportAttendance(Dictionary<int, bool?> participantAttended);

		IEnumerable<ParticipantForm> GetParticipantFormsForGrantApplication(int grantApplication);
		IEnumerable<ParticipantCost> GetParticipantCostsForClaimEligibleCost(int claimEligibleCostId);
		IEnumerable<ParticipantCost> GetParticipantCosts(ClaimEligibleCost eligibleCost);
		IEnumerable<ParticipantForm> GetUnemployedParticipantEnrollments(DateTime currentDate, int take, DateTime cutoffDate);
		IEnumerable<ParticipantForm> GetTemporaryResidentParticipantEnrollments(DateTime currentDate, int take, DateTime cutoffDate);
		IEnumerable<DuplicateSINParticipants> GetParticipantEnrollmentsWithDuplicatedSIN(DateTime currentDate, int take, DateTime cutoffDate, DateTime fiscalStart, DateTime fiscalEnd);

		int GetParticipantsWithClaimEligibleCostCount(int claimId, int claimVersion);
		IDictionary<string, decimal> GetParticipantYTD(GrantApplication grantApplication);
		IDictionary<string, bool> GetParticipantMultipleInstances(GrantApplication grantApplication);

		void UpdateReportedDate(IEnumerable<ParticipantForm> participantEnrollments, DateTime reportedDate);
		void UpdateSINReportedDate(IEnumerable<ParticipantForm> participantEnrollments, DateTime reportedDate);
		void UpdateDuplicateSINReportedDate(IEnumerable<ParticipantForm> primaryForms, DateTime reportedDate);
		void UpdateExitSurveyReportedDate(List<SurveyParticipantModel> exitFormParticipants, DateTime reportedDate);

		void RemoveParticipant(ParticipantForm participantForm);
		void WithdrawParticipant(ParticipantForm participantForm);
		void IncludeParticipant(ParticipantForm participant);
		void ExcludeParticipant(ParticipantForm participant);
		void UpdateWithdrawalSurveyReportedDate(List<SurveyParticipantModel> withdrawnParticipants, DateTime reportedDate);
	}
}
