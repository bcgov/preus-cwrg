using System;
using CJG.Application.Services;
using CJG.Core.Entities;

namespace CJG.Web.External.Models.Shared.Reports
{
    public class CompletionReportParticipantDetailsViewModel
	{
		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string ParticipantName { get { return $"{FirstName} {LastName}"; } }
		public string EmailAddress { get; set; }
		public string PhoneNumber1 { get; set; }
		public string PhoneExtension1 { get; set; }
		public string PrimaryCity { get; set; }
		public bool ClaimReported { get; set; }
		public bool IsExcludedFromClaim { get; set; }
		public DateTime DateAdded { get; set; }

		public CompletionReportParticipantDetailsViewModel()
		{
		}

		public CompletionReportParticipantDetailsViewModel(ParticipantForm participantForm)
		{
			if (participantForm == null)
				throw new ArgumentNullException(nameof(participantForm));

			Utilities.MapProperties(participantForm, this);

			// These fields present as 'Virtual' and the MapProperties doesn't want map those
			FirstName = participantForm.FirstName;
			LastName = participantForm.LastName;

			DateAdded = participantForm.DateAdded.ToLocalTime();
		}
	}
}
