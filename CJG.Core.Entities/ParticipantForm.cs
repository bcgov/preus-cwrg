﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using CJG.Core.Entities.Attributes;
using CJG.Core.Entities.Interfaces;

namespace CJG.Core.Entities
{
	/// <summary>
	/// <typeparamref name="ParticipantForm"/> class, provides the ORM a way to manage participant forms.
	/// A participant form is used to collect information about the participant before they can participate in training.
	///
	/// </summary>
	public class ParticipantForm : EntityBase, INotificationRecipient
	{
		/// <summary>
		/// get/set - Primary key used IDENTITY
		/// </summary>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		/// <summary>
		/// get/set - A unique key used to identify this participant.
		/// </summary>
		[Required]
		public Guid InvitationKey { get; set; }

		/// <summary>
		/// get/set - A optional unique key used to identify this participant when they wish to withdraw.
		/// </summary>
		public Guid? WithdrawalKey { get; set; }

		/// <summary>
		/// get/set - Foreign key to the parent grant application.
		/// </summary>
		[Index("IX_ParticipantForm", 1)]
		public int? GrantApplicationId { get; set; }

		/// <summary>
		/// get/set - The parent grant application.
		/// </summary>
		[ForeignKey(nameof(GrantApplicationId))]
		public virtual GrantApplication GrantApplication { get; set; }

		/// <summary>
		/// get/set - The foreign key to the participant.
		/// </summary>
		[Index("IX_ParticipantForm", 2)]
		public int? ParticipantId { get; set; }

		/// <summary>
		/// get/set - The parent participant associated with this form.
		/// </summary>
		[ForeignKey(nameof(ParticipantId))]
		public virtual Participant Participant { get; set; }

		/// <summary>
		/// get/set - The foreign key to the canadian status of this participant.
		/// </summary>
		public int CanadianStatusId { get; set; }

		/// <summary>
		/// get/set - The canadian status of this participant.
		/// </summary>
		[ForeignKey(nameof(CanadianStatusId))]
		public virtual CanadianStatus CanadianStatus { get; set; }

		/// <summary>
		/// get/set - The foreign key to the aboriginal band of this participant.
		/// </summary>
		public int? AboriginalBandId { get; set; }

		/// <summary>
		/// get/set - The aboriginal band of this participant.
		/// </summary>
		[ForeignKey(nameof(AboriginalBandId))]
		public virtual AboriginalBand AboriginalBand { get; set; }

		/// <summary>
		/// get/set - The foreign key to the federal official language of this participant.
		/// </summary>
		public int? FederalOfficialLanguageId { get; set; }

		/// <summary>
		/// get/set - The federal official language of this participant.
		/// </summary>
		[ForeignKey(nameof(FederalOfficialLanguageId))]
		public virtual FederalOfficialLanguage FederalOfficialLanguage { get; set; }

		/// <summary>
		/// get/set - The foreign key to the marital status of this participant.
		/// </summary>
		public int? MartialStatusId { get; set; }

		/// <summary>
		/// get/set - The marital status of this participant.
		/// </summary>
		[ForeignKey(nameof(MartialStatusId))]
		public virtual MaritalStatus MaritalStatus { get; set; }

		/// <summary>
		/// get/set - The number of dependents of this participant.
		/// </summary>
		[Range(0, int.MaxValue, ErrorMessage = "The number of dependents must be greater than or equal to 0.")]
		public int NumberOfDependents { get; set; }

		/// <summary>
		/// get/set - The foreign key to the education level of this participant.
		/// </summary>
		public int? EducationLevelId { get; set; }

		/// <summary>
		/// get/set - The education level of this participant.
		/// </summary>
		[ForeignKey(nameof(EducationLevelId))]
		public virtual EducationLevel EducationLevel { get; set; }

		/// <summary>
		/// get/set - The foreign key to the employment type of this participant.
		/// </summary>
		public int? EmploymentTypeId { get; set; }

		/// <summary>
		/// get/set - The employment type of this participant.
		/// </summary>
		[ForeignKey(nameof(EmploymentTypeId))]
		public virtual EmploymentType EmploymentType { get; set; }

		/// <summary>
		/// get/set - The foreign key of the recent period of this participant.
		/// </summary>
		public int? RecentPeriodId { get; set; }

		/// <summary>
		/// get/set - The recent period of this participant.
		/// </summary>
		[ForeignKey(nameof(RecentPeriodId))]
		public virtual RecentPeriod RecentPeriod { get; set; }

		/// <summary>
		/// get/set - The foreign key of the employment status of this participant.
		/// </summary>
		public int EmploymentStatusId { get; set; }

		/// <summary>
		/// get/set - The employment status of this participant.
		/// </summary>
		[ForeignKey(nameof(EmploymentStatusId))]
		public virtual EmploymentStatus EmploymentStatus { get; set; }

		/// <summary>
		/// get/set - The foreign key of the training result for this participant.
		/// </summary>
		public int? TrainingResultId { get; set; }

		/// <summary>
		/// get/set - The training result for this participant.
		/// </summary>
		[ForeignKey(nameof(TrainingResultId))]
		public virtual TrainingResult TrainingResult { get; set; }

		/// <summary>
		/// get/set - The program sponsor name.
		/// </summary>
		[Required, MaxLength(500)]
		public string ProgramSponsorName { get; set; }

		/// <summary>
		/// get/set - The description of this program.
		/// </summary>
		[Required]
		public string ProgramDescription { get; set; }

		/// <summary>
		/// get/set - The start date of the program.
		/// </summary>
		[DateTimeKind(DateTimeKind.Utc)]
		[Required]
		[Column(TypeName = "DATETIME2")]
		public DateTime ProgramStartDate { get; set; }

		/// <summary>
		/// get/set - The first name of the participant.
		/// </summary>
		[Required, MaxLength(100)]
		public string FirstName { get; set; }

		/// <summary>
		/// get/set - The middle name of the participant.
		/// </summary>
		[MaxLength(100)]
		public string MiddleName { get; set; }

		/// <summary>
		/// get/set - The last name of the participant.
		/// </summary>
		[Required, MaxLength(100)]
		public string LastName { get; set; }

		/// <summary>
		/// get/set - The birthdate of the participant.
		/// </summary>
		[DateTimeKind(DateTimeKind.Utc)]
		[Required]
		[Column(TypeName = "DATETIME2")]
		public DateTime BirthDate { get; set; }

		/// <summary>
		/// get/set - The SIN of the participant.
		/// </summary>
		[Required, MaxLength(11), RegularExpression(@"^[0-9]{3}-[0-9]{3}-[0-9]{3}$")]
		public string SIN { get; set; }

		/// <summary>
		/// get/set - The primary phone number of the participant.
		/// </summary>
		[Required, MaxLength(20)]
		[RegularExpression(@"\(?[0-9]{3}\)?\s?[0-9]{3}\-?[0-9]{4}")]
		public string PhoneNumber1 { get; set; }

		/// <summary>
		/// get/set - The primary phone number extension of the participant.
		/// </summary>
		[MaxLength(20)]
		[RegularExpression(@"[0-9]*")]
		public string PhoneExtension1 { get; set; }

		/// <summary>
		/// get/set - The secondary phone number of the participant.
		/// </summary>
		[MaxLength(20), RegularExpression(@"\(?[0-9]{3}\)?\s?[0-9]{3}\-?[0-9]{4}")]
		public string PhoneNumber2 { get; set; }

		/// <summary>
		/// get/set - The secondary phone number extension of the participant.
		/// </summary>
		[MaxLength(20)]
		[RegularExpression(@"[0-9]*")]
		public string PhoneExtension2 { get; set; }

		/// <summary>
		/// get/set - The email address of this participant.
		/// </summary>
		[Required, MaxLength(500)]
		[EmailAddress(ErrorMessage = "Invalid Email Address")]
		public string EmailAddress { get; set; }

		/// <summary>
		/// get/set - The primary address line of the participant.
		/// </summary>
		[Required, MaxLength(500)]
		public string AddressLine1 { get; set; }

		/// <summary>
		/// get/set - The secondary address line of the participant.
		/// </summary>
		[MaxLength(500)]
		public string AddressLine2 { get; set; }

		/// <summary>
		/// get/set - The city of the participant.
		/// </summary>
		[Required, MaxLength(500)]
		public string City { get; set; }

		/// <summary>
		/// get/set - The postal code of the participant.
		/// </summary>
		[Required, MaxLength(10)]
		[RegularExpression(Constants.PostalCodeValidationRegEx, ErrorMessage = "Invalid Format")]
		public string PostalCode { get; set; }

		/// <summary>
		/// get/set - The foreign key to province/state of the participant.
		/// </summary>
		[MaxLength(10)]
		[ForeignKey(nameof(Region)), Column(Order = 2)]
		public string RegionId { get; set; }

		/// <summary>
		/// get/set - The foreign key to the country of the participant.
		/// </summary>
		[MaxLength(20)]
		[ForeignKey(nameof(Region)), Column(Order = 3)]
		public string CountryId { get; set; }

		/// <summary>
		/// get/set - The province/state of the participant.
		/// </summary>
		public virtual Region Region { get; set; }

		/// <summary>
		/// get/set - The country of the participant.
		/// </summary>
		[ForeignKey(nameof(CountryId))]
		public virtual Country Country { get; set; }

		/// <summary>
		/// get/set - The gender of the participant.
		/// </summary>
		[Required]
		public int Gender { get; set; }

		/// <summary>
		/// get/set - Whether the participant is a youth in care.
		/// </summary>
		public bool YouthInCare { get; set; }

		/// <summary>
		/// get/set - Whether this participant is a person with a disability.
		/// </summary>
		public int PersonDisability { get; set; }

		/// <summary>
		/// get/set - Whether this participant is an aboriginal.
		/// </summary>
		public int? PersonAboriginal { get; set; }

		/// <summary>
		/// get/set - Whether this participant lives on a reserve.
		/// </summary>
		public bool LiveOnReserve { get; set; }

		/// <summary>
		/// get/set - Whether this participant is a visible minority.
		/// </summary>
		public int? VisibleMinority { get; set; }

		/// <summary>
		/// get/set - Whether this participant is an immigrant to Canada.
		/// </summary>
		public bool? CanadaImmigrant { get; set; }

		/// <summary>
		/// get/set - What year this participant migrated to Canada.
		/// </summary>
		public int YearToCanada { get; set; }

		/// <summary>
		/// get/set - Whether this participant is a Canadian refugee.
		/// </summary>
		public bool? CanadaRefugee { get; set; }

		/// <summary>
		/// get/set - Which country this participant immigrated from.
		/// </summary>
		[MaxLength(200)]
		public string FromCountry { get; set; }

		/// <summary>
		/// get/set - The foreign key to the EI benefits for this participant.
		/// </summary>
		public int EIBenefitId { get; set; }

		/// <summary>
		/// get/set - The EI benefits for this participant.
		/// </summary>
		[ForeignKey(nameof(EIBenefitId))]
		public virtual EIBenefit EIBenefit { get; set; }

		/// <summary>
		/// get/set - Whether this participant is receiving EI benefit.
		/// </summary>
		public bool ReceivingEIBenefit { get; set; } = false;

		/// <summary>
		/// get/set - Whether this participant is a maternal or paternal.
		/// </summary>
		public bool MaternalPaternal { get; set; }

		/// <summary>
		/// get/set - Whether this participant is a BCEA client.
		/// </summary>
		public bool BceaClient { get; set; }

		/// <summary>
		/// get/set - Whether this participant is employed by supporting employer.
		/// </summary>
		[Obsolete("This has been deprecated")]
		public bool EmployedBySupportEmployer { get; set; }

		/// <summary>
		/// get/set - Whether this participant is the business owner.
		/// </summary>
		[Obsolete("This has been deprecated")]
		public bool BusinessOwner { get; set; }

		/// <summary>
		/// get/set - Whether this participant is an apprentice.
		/// </summary>
		public bool Apprentice { get; set; }

		/// <summary>
		/// get/set - Whether this participant is ITA registered.
		/// </summary>
		public bool ItaRegistered { get; set; }

		/// <summary>
		/// get/set - Whether this participant has applied for other programs.
		/// </summary>
		public bool OtherPrograms { get; set; }

		/// <summary>
		/// get/set - The number of years this participant has been unemployed.
		/// </summary>
		[Range(0, 50, ErrorMessage = "The number of years must be within 0 to 50.")]
		[Obsolete("This has been deprecated")]
		public int? HowLongYears { get; set; }

		/// <summary>
		/// get/set - The number of months this participant has been unemployed.
		/// </summary>
		[Range(0, 12, ErrorMessage = "The number of months must be within 0 to 12.")]
		[Obsolete("This has been deprecated")]
		public int? HowLongMonths { get; set; }

		/// <summary>
		/// get/set - The average hours per week this participant works.
		/// </summary>
		[Range(0, 168, ErrorMessage = "The average hours per week must be within 0 to 168.")]
		[Obsolete("This has been deprecated")]
		public int? AvgHoursPerWeek { get; set; }

		/// <summary>
		/// get/set - The average hours per week this participant plans to work during training.
		/// </summary>
		[Range(0, 168, ErrorMessage = "The average hours per week during training must be within 0 to 168.")]
		public int? AvgHoursPerWeekDuringTraining { get; set; }

		/// <summary>
		/// get/set - The hourly rate this participant makes.
		/// </summary>
		[Range(0, 99999, ErrorMessage = "The hourly rate must be within $0 to $99,999.")]
		public decimal? HourlyWage { get; set; }

		/// <summary>
		/// get/set - The primary city this participant works from.
		/// </summary>
		[MaxLength(250)]
		public string PrimaryCity { get; set; }

		/// <summary>
		/// get/set - Other program description information.
		/// </summary>
		public string OtherProgramDesc { get; set; }

		/// <summary>
		/// get/set - The last high school attended by this participant.
		/// </summary>
		[MaxLength(250)]
		public string LastHighSchoolName { get; set; }

		/// <summary>
		/// get/set - The last highs school city attended by this participant.
		/// </summary>
		[MaxLength(250)]
		public string LastHighSchoolCity { get; set; }

		/// <summary>
		/// get/set - The foreign key to the current NOC before training for this participant.
		/// </summary>
		public int CurrentNocId { get; set; }

		/// <summary>
		/// get/set - The current NOC before training for this participant.
		/// </summary>
		[ForeignKey(nameof(CurrentNocId))]
		public virtual NationalOccupationalClassification CurrentNoc { get; set; }

		/// <summary>
		/// get/set - The foreign key to the future NOC after training for this participant.
		/// </summary>
		public int FutureNocId { get; set; }

		/// <summary>
		/// get/set - The future NOC after training for this participant.
		/// </summary>
		[ForeignKey(nameof(FutureNocId))]
		public virtual NationalOccupationalClassification FutureNoc { get; set; }

		/// <summary>
		/// get/set - The foreign key to the NOC for this participant.
		/// </summary>
		public int? NocId { get; set; }

		/// <summary>
		/// get/set - The NOC for this participant.
		/// </summary>
		[ForeignKey(nameof(NocId))]
		public virtual NationalOccupationalClassification Noc { get; set; }

		/// <summary>
		/// get/set - The foreign key to the NAICS for this participant.
		/// </summary>
		public int? NaicsId { get; set; }

		/// <summary>
		/// get/set - The NAICS for this participant.
		/// </summary>
		[ForeignKey(nameof(NaicsId))]
		public virtual NaIndustryClassificationSystem Naics { get; set; }

		/// <summary>
		/// get/set - The employer name of the participant.
		/// </summary>
		[MaxLength(250)]
		public string EmployerName { get; set; }

		/// <summary>
		/// get/set - The date the participant consented to submitting their information.
		/// </summary>
		[DateTimeKind(DateTimeKind.Utc)]
		[Column(TypeName = "DATETIME2")]
		public DateTime ConsentDateEntered { get; set; }

		/// <summary>
		/// get/set - Whether this participant is part of a reported claim.
		/// </summary>
		[DefaultValue(false)]
		public bool ClaimReported { get; set; }

		/// <summary>
		/// get/set - Whether the participant has been excluded from future claims.
		/// </summary>
		[DefaultValue(false)]
		public bool IsExcludedFromClaim { get; set; }

		/// <summary>
		/// get/set - the consent form upload by grant application applicant
		/// </summary>
		public int? ParticipantConsentAttachmentId { get; set; }

		/// <summary>
		/// get - the consent form associated with this participant.
		/// </summary>
		[ForeignKey(nameof(ParticipantConsentAttachmentId))]
		public virtual Attachment ParticipantConsentAttachment { get; set; }

		/// <summary>
		/// get/set - The date the participant form was withdrawn from the Training Program.
		/// </summary>
		[DateTimeKind(DateTimeKind.Utc)]
		[Column(TypeName = "DATETIME2")]
		public DateTime? TrainingWithdrawalDate { get; set; }

		/// <summary>
		/// get/set - The date the participant declared their Exit via the Exit Form
		/// </summary>
		[DateTimeKind(DateTimeKind.Utc)]
		[Column(TypeName = "DATETIME2")]
		public DateTime? TrainingExitDate { get; set; }

		/// <summary>
		/// get/set - When the participant was reported on. This is the Section 25 Report job.
		/// </summary>
		public DateTime? ReportedOn { get; set; }

		/// <summary>
		/// get/set - When the participant with a SIN that starts with 9 was reported on.
		/// </summary>
		public DateTime? SinReportedOn { get; set; }

		/// <summary>
		/// get/set - Set when the PIF is checked against other PIFs in the last fiscal year for duplicate SIN usages.
		/// </summary>
		public DateTime? DuplicateSinReportedOn { get; set; }

		/// <summary>
		/// get/set - Set when the PIF is reported on by the ExitSurvey job
		/// </summary>
		public DateTime? ExitSurveyReportedOn { get; set; }

		/// <summary>
		/// get/set - Set when the PIF is reported on by the EarlyWithdrawalSurvey job
		/// </summary>
		public DateTime? EarlyWithdrawalReportedOn { get; set; }

		/// <summary>
		/// get - All the costs associated with this participant.
		/// </summary>
		public virtual ICollection<ParticipantCost> ParticipantCosts { get; set; } = new List<ParticipantCost>();

		/// <summary>
		/// get - All the completion report answers associated with this participant.
		/// </summary>
		public virtual ICollection<ParticipantCompletionReportAnswer> ParticipantCompletionReportAnswers { get; set; } = new List<ParticipantCompletionReportAnswer>();

		/// <summary>
		/// get - All the eligible cost breakdowns associated with this participant.
		/// </summary>
		public virtual ICollection<EligibleCostBreakdown> EligibleCostBreakdowns { get; set; } = new List<EligibleCostBreakdown>();
		public virtual ICollection<ParticipantExitSurveyAnswer> ParticipantExitSurveyAnswers { get; set; } = new List<ParticipantExitSurveyAnswer>();
		public virtual ICollection<ParticipantWithdrawalSurveyAnswer> ParticipantWithdrawalSurveyAnswers { get; set; } = new List<ParticipantWithdrawalSurveyAnswer>();

		/// <summary>
		/// get - The claim associated with this participantForm.
		/// </summary>
		public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

		/// <summary>
		/// indicates whether the ministry has approved this participant
		/// </summary>
		public bool? Approved { get; set; }

		/// <summary>
		/// Indicated that a participant attended (received) the training
		/// </summary>
		public bool? Attended { get; set; }

		/// <summary>
		/// Creates a new instance of a <typeparamref name="ParticipantForm"/> object.
		/// </summary>
		public ParticipantForm()
		{ }

		/// <summary>
		/// Creates a new instance of a <typeparamref name="ParticipantForm"/> object and initializes it with the specified property values.
		/// </summary>
		/// <param name="grantApplication"></param>
		/// <param name="invitationKey"></param>
		public ParticipantForm(GrantApplication grantApplication, Guid invitationKey)
		{
			if (invitationKey == Guid.NewGuid())
				throw new ArgumentException("The invitation key must be valid.", nameof(invitationKey));

			InvitationKey = invitationKey;
			GrantApplication = grantApplication ?? throw new ArgumentNullException(nameof(grantApplication));
			GrantApplicationId = grantApplication.Id;
		}

		/// <summary>
		/// Validates this ParticipantForm property values.
		/// </summary>
		/// <param name="validationContext"></param>
		/// <returns></returns>
		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var entry = validationContext.GetDbEntityEntry();

			// This is done to stop errors from being thrown when developers use EF entities in ViewModels.
			if (entry == null)
				yield break;

			if (entry.State == EntityState.Added)
			{
				// Cannot be older than or younger than the configured age.
				var oldest = AppDateTime.UtcNow.AddYears(-1 * Participant.OldestAge).Date;
				var youngest = AppDateTime.UtcNow.AddYears(-1 * Participant.YoungestAge).Date;
				if (BirthDate.Date < oldest || BirthDate.Date > youngest)
					yield return new ValidationResult($"The birthdate must be between '{oldest:yyyy}' and '{youngest:yyyy}'.", new[] { nameof(BirthDate) });

				// ConsentDateEntered must be today.
				if (ConsentDateEntered.Date != AppDateTime.UtcNow.Date)
					yield return new ValidationResult($"The consent date must be today '{AppDateTime.UtcNow:yyyy-MM-dd}'.", new[] { nameof(ConsentDateEntered) });
			}
			else if (entry.State == EntityState.Modified)
			{
				// ConsentDateEntered must be equal to the original ConsentDateEntered (no changes)
				var originalConsentDateEntered = (DateTime)entry.OriginalValues[nameof(ConsentDateEntered)];
				if (ConsentDateEntered.Date != originalConsentDateEntered.Date)
					yield return new ValidationResult($"The consent date cannot be changed from '{originalConsentDateEntered:yyyy-MM-dd}'.", new[] { nameof(ConsentDateEntered) });
			}

			foreach (var validation in base.Validate(validationContext))
			{
				yield return validation;
			}
		}
	}
}
