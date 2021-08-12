using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CJG.Core.Entities;

namespace CJG.Web.External.Areas.Ext.Models.ParticipantReporting
{
	public class ParticipantViewModel
	{
		public int Id { get; set; }
		public string RowVersion { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string EmailAddress { get; set; }
		public string PhoneNumber1 { get; set; }
		public string PhoneExtension1 { get; set; }
		public string PrimaryCity { get; set; }
		public bool ClaimReported { get; set; }
		public bool CanBeWithdrawn { get; set; }
		public bool HasWithdrawn { get; set; }
		public string WithdrawalUrl { get; set; }
		public string WithdrawalStatus { get; set; }
		public bool IsIncludedInClaim { get; set; }
		public DateTime DateAdded { get; set; }
		public bool IsLate { get; set; }

		public bool? Approved { get; set; }
		public bool ShowEligibility { get; set; }

		public ParticipantViewModel()
		{
		}

		public ParticipantViewModel(ParticipantForm participantForm, bool showEligibility, Claim claim = null, string baseUrlFragment = null)
		{
			if (participantForm == null)
				throw new ArgumentNullException(nameof(participantForm));

			Id = participantForm.Id;
			RowVersion = Convert.ToBase64String(participantForm.RowVersion);
			FirstName = participantForm.FirstName;
			LastName = participantForm.LastName;
			EmailAddress = participantForm.EmailAddress;
			PhoneNumber1 = participantForm.PhoneNumber1;
			PhoneExtension1 = participantForm.PhoneExtension1;
			PrimaryCity = participantForm.PrimaryCity;

			// We are only looking to see if they were reported in another claim, not the current one.
			ClaimReported = participantForm.ClaimReported || participantForm.Claims.Any(c => c.Id != claim?.Id && c.ClaimVersion != claim?.ClaimVersion);

			var withdrawAbleStatuses = new List<ApplicationStateInternal>
			{
				ApplicationStateInternal.ClaimApproved,
				ApplicationStateInternal.CompletionReporting
			};

			CanBeWithdrawn = withdrawAbleStatuses.Contains(participantForm.GrantApplication.ApplicationStateInternal)
			                 && participantForm.WithdrawalKey == null;
			HasWithdrawn = participantForm.WithdrawalKey.HasValue && !participantForm.ParticipantWithdrawalSurveyAnswers.Any();
			WithdrawalStatus = participantForm.WithdrawalKey.HasValue && participantForm.ParticipantWithdrawalSurveyAnswers.Any() ? "Withdrawn" : string.Empty;
			WithdrawalUrl = participantForm.WithdrawalKey.HasValue
				? $"{baseUrlFragment}/Ext/Reporting/Participant/Withdrawal/{HttpUtility.UrlEncode(participantForm.GrantApplication.InvitationKey.ToString())}/{HttpUtility.UrlEncode(participantForm.WithdrawalKey.ToString())}"
				: string.Empty;
			IsIncludedInClaim = !participantForm.IsExcludedFromClaim;
			DateAdded = participantForm.DateAdded.ToLocalTime();
			IsLate = DateAdded > participantForm.GrantApplication.GetParticipantReportingDueDate();
			Approved = participantForm.Approved;
			ShowEligibility = showEligibility;
		}
	}
}