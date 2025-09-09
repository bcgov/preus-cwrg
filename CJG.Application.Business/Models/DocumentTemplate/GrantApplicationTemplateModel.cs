using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CJG.Core.Entities;

namespace CJG.Application.Business.Models.DocumentTemplate
{
	public class GrantApplicationTemplateModel
	{
		public int Id { get; set; }
		public string FileNumber { get; set; }
		public string ApplicantFirstName { get; set; }
		public string ApplicantLastName { get; set; }
		public string ApplicantPhysicalAddress { get; set; }
		public string ApplicantPhoneNumber { get; set; }
		public string ReimbursementRate { get; set; }
		public string MaxReimbursementAmt { get; set; }
		public string StartDate { get; set; }
		public string EndDate { get; set; }

		public string OrganizationLegalName { get; set; }

		public string GrantProgramName { get; set; }
		public string GrantProgramCode { get; set; }
		public string GrantStreamName { get; set; }
		public string ProgramInitiativeName { get; set; }
		public string GrantProgramEmail { get; set; }

		public string GrantAgreementStartDate { get; set; }
		public string GrantAgreementEndDate { get; set; }
		public string GrantAgreementParticipantReportingDueDate { get; set; }
		public string GrantAgreementReimbursementClaimDueDate { get; set; }
		public string GrantAgreementAcceptedDate { get; set; }

		public ProgramTypes ProgramType { get; set; }
		public bool ShowAgreedCosts { get; set; }
		public bool ShowEmployerContribution { get; set; }
		public string BaseURL { get; set; }

		public IEnumerable<TrainingProgramTemplateModel> TrainingPrograms { get; set; }
		public TrainingCostTemplateModel TrainingCost { get; set; }

		[RegularExpression("^[a-zA-Z '-]*$", ErrorMessage = "Invalid Format")]
		public string AlternateJobTitle { get; set; }
		public string AlternatePhoneExtension { get; set; }
		public string AlternatePhoneNumber { get; set; }

		[RegularExpression("^[a-zA-Z '-]*$", ErrorMessage = "Invalid Format")]
		public string AlternateLastName { get; set; }

		[RegularExpression("^[a-zA-Z '-]*$", ErrorMessage = "Invalid Format")]
		public string AlternateFirstName { get; set; }

		[RegularExpression("^[a-zA-Z '-]*$", ErrorMessage = "Invalid Format")]
		public string AlternateSalutation { get; set; }

		[RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Invalid Email Address")]
		public string AlternateEmail { get; set; }
		public string AlternatePhysicalAddress { get; set; }
		public string AlternateMailingAddress { get; set; }
		public bool IsAlternateContact { get; set; }
		public string ClaimDeadline { get; set; }
		public string AgreementFiscalYear { get; set; }
		public List<ParticipantFormModel> Participants { get; set; }
		public bool RequireAllParticipantsBeforeSubmission { get; set; }

		public GrantApplicationTemplateModel()
		{
		}

		public GrantApplicationTemplateModel(GrantApplication grantApplication, Action<GrantApplicationTemplateModel, GrantApplication> additionalSetup = null)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			FileNumber = grantApplication.FileNumber;
			ApplicantFirstName = grantApplication.ApplicantFirstName;
			ApplicantLastName = grantApplication.ApplicantLastName;

			// Load the Organization address - non-submitted applications will have a null address until they're submitted
			if (grantApplication.OrganizationAddress != null)
				ApplicantPhysicalAddress = grantApplication.OrganizationAddress.ToString();
			else if (grantApplication.Organization.HeadOfficeAddress != null)
				ApplicantPhysicalAddress = grantApplication.Organization.HeadOfficeAddress.ToString();

			ApplicantPhoneNumber = grantApplication.ApplicantPhoneNumber;
			MaxReimbursementAmt = grantApplication.MaxReimbursementAmt.ToString("C0");
			StartDate = grantApplication.StartDate.ToLocalTime().ToString("MMMM dd, yyyy");
			EndDate = grantApplication.EndDate.ToLocalTime().ToString("MMMM dd, yyyy");

			OrganizationLegalName = grantApplication.Organization.LegalName;
			ProgramInitiativeName = grantApplication.ProgramInitiative?.Name;

			GrantProgramName = grantApplication.GrantOpening.GrantStream.GrantProgram.Name;
			GrantProgramCode = grantApplication.GrantOpening.GrantStream.GrantProgram.ProgramCode;
			GrantStreamName = grantApplication.GrantOpening.GrantStream.Name;
			GrantProgramEmail = $"{GrantProgramCode}@gov.bc.ca";

			GrantAgreementStartDate = grantApplication.GrantAgreement?.StartDate.ToLocalTime().ToString("MMMM dd, yyyy");
			GrantAgreementEndDate = grantApplication.GrantAgreement?.ConvertEndDateToLocalTime().ToString("MMMM dd, yyyy");
			GrantAgreementParticipantReportingDueDate = grantApplication.GrantAgreement?.ParticipantReportingDueDate.ToLocalTime().ToString("MMMM dd, yyyy");
			GrantAgreementReimbursementClaimDueDate = grantApplication.GrantAgreement?.ReimbursementClaimDueDate.ToLocalTime().ToString("MMMM dd, yyyy");
			GrantAgreementAcceptedDate = grantApplication.GrantAgreement?.DateAccepted?.ToLocalTime().ToString("MMMM dd, yyyy");

			ProgramType = grantApplication.GrantOpening.GrantStream.GrantProgram.ProgramTypeId;
			ShowAgreedCosts = grantApplication.TrainingPrograms.FirstOrDefault().GrantApplication.ApplicationStateInternal.ShowAgreedCosts();
			ShowEmployerContribution = !(grantApplication.ReimbursementRate == 1 && grantApplication.CalculateAgreedEmployerContribution() == 0);

			TrainingPrograms = grantApplication.TrainingPrograms.Where(tp => tp.EligibleCostBreakdown?.IsEligible ?? true).Select(o => new TrainingProgramTemplateModel(o)).ToArray();
			TrainingCost = new TrainingCostTemplateModel(grantApplication.TrainingCost, ShowAgreedCosts);

			AlternateJobTitle = grantApplication.AlternateJobTitle;
			AlternatePhoneExtension = grantApplication.AlternatePhoneExtension;
			AlternatePhoneNumber = grantApplication.AlternatePhoneNumber;
			AlternateLastName = grantApplication.AlternateLastName;
			AlternateFirstName = grantApplication.AlternateFirstName;
			AlternateSalutation = grantApplication.AlternateSalutation;
			AlternateEmail = grantApplication.AlternateEmail;
			IsAlternateContact = grantApplication.IsAlternateContact == null ? false : grantApplication.IsAlternateContact.Value;
			ClaimDeadline = grantApplication.GrantAgreement?.GetClaimSubmissionDeadline().ToString("MMMM dd, yyyy");
			AgreementFiscalYear = $"{grantApplication.GrantOpening.TrainingPeriod.FiscalYear.StartDate.ToLocalTime().ToString("MMMM dd, yyyy")} to {grantApplication.GrantOpening.TrainingPeriod.FiscalYear.EndDate.ToLocalTime().ToString("MMMM dd, yyyy")}";
			RequireAllParticipantsBeforeSubmission = grantApplication.RequireAllParticipantsBeforeSubmission;
			Participants = new List<ParticipantFormModel>();

			if (RequireAllParticipantsBeforeSubmission)
			{
				foreach (var participantForm in grantApplication.ParticipantForms.Where(w => w.Approved == true))
				{
					Participants.Add(new ParticipantFormModel(participantForm));
				}
			}
			else
			{
				foreach (var participantForm in grantApplication.ParticipantForms)
				{
					Participants.Add(new ParticipantFormModel(participantForm));
				}
			}

			additionalSetup?.Invoke(this, grantApplication);
		}
	}
}
