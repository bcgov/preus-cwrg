﻿using System.ComponentModel;

namespace CJG.Core.Entities
{
	/// <summary>
	/// NotificationRecipientRules enum, provides a way to identify notification recipient rules.
	/// </summary>
	public enum NotificationRecipientRules
	{
		/// <summary>
		/// Applicant - Notify just the applicant.
		/// </summary>
		[Description("Applicant")]
		Applicant = 0,

		/// <summary>
		/// AllApplicantsInOrganization - Notify all applicants in organization.
		/// </summary>
		[Description("All Applicants In Organization")]
		AllApplicantsInOrganization = 1,

		/// <summary>
		/// AllExitingParticipants - Notify all participants that are to be Exiting an application (ie: not Withdrawn or Ineligible).
		/// </summary>
		[Description("All Exiting Participants")]
		AllExitingParticipants = 2
	}
}
