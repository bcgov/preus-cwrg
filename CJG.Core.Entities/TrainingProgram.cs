using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using CJG.Core.Entities.Attributes;
using DataAnnotationsExtensions;

namespace CJG.Core.Entities
{
	/// <summary>
	/// <typeparamref name="TrainingProgram"/> class, provides the ORM with a way to manage training program information.  Ian wanted to merge TrainingCostEstimates with TrainingPrograms...
	/// </summary>
	public class TrainingProgram : EntityBase
	{
		/// <summary>
		/// get/set - Primary key to identify this training program.
		/// </summary>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		/// <summary>
		/// get/set - The foreign key to the grant application.
		/// </summary>
		[Index("IX_TrainingPrograms", Order = 4)]
		public int GrantApplicationId { get; set; }

		/// <summary>
		/// get/set - The parent grant application.
		/// </summary>
		[ForeignKey(nameof(GrantApplicationId))]
		public virtual GrantApplication GrantApplication { get; set; }

		/// <summary>
		/// get/set - The foreign key to the in-demand occupation.
		/// </summary>
		[Index("IX_TrainingPrograms", Order = 7)]
		public int? InDemandOccupationId { get; set; }

		/// <summary>
		/// get/set - The in-demand occupation.
		/// </summary>
		[ForeignKey(nameof(InDemandOccupationId))]
		public virtual InDemandOccupation InDemandOccupation { get; set; }

		/// <summary>
		/// get/set - The foreign key to the skill level.
		/// </summary>
		[Index("IX_TrainingPrograms", Order = 8)]
		[Obsolete("This field is no longer in use on the form. Consider future removal.")]
		public int? SkillLevelId { get; set; }

		/// <summary>
		/// get/set - The skill level.
		/// </summary>
		[ForeignKey(nameof(SkillLevelId))]
		[Obsolete("This property is no longer in use on the form. Consider future removal.")]
		public virtual SkillLevel SkillLevel { get; set; }

		/// <summary>
		/// get/set - The foreign key to the skill focus.
		/// </summary>
		[Index("IX_TrainingPrograms", Order = 9)]
		public int? SkillFocusId { get; set; }

		/// <summary>
		/// get/set - The skill focus.
		/// </summary>
		[ForeignKey(nameof(SkillFocusId))]
		public virtual SkillsFocus SkillFocus { get; set; }

		/// <summary>
		/// get/set - The foreign key to the expected qualification.
		/// </summary>
		[Index("IX_TrainingPrograms", Order = 10)]
		public int ExpectedQualificationId { get; set; }

		/// <summary>
		/// get/set - The expected qualification.
		/// </summary>
		[ForeignKey(nameof(ExpectedQualificationId))]
		public virtual ExpectedQualification ExpectedQualification { get; set; }

		/// <summary>
		/// get/set - The foreign key to the training level.
		/// </summary>
		[Index("IX_TrainingPrograms", Order = 11)]
		public int? TrainingLevelId { get; set; }

		/// <summary>
		/// get/set - The training level.
		/// </summary>
		[ForeignKey(nameof(TrainingLevelId))]
		public virtual TrainingLevel TrainingLevel { get; set; }

		/// <summary>
		/// get/set - The course title.
		/// </summary>
		[Required, MaxLength(500)]
		public string CourseTitle { get; set; }

		/// <summary>
		/// get/set - The training business case description.
		/// </summary>
		public string TrainingBusinessCase { get; set; }

		/// <summary>
		/// get/set - The start date for the training program.
		/// </summary>
		[DateTimeKind(DateTimeKind.Utc)]
		[Required(ErrorMessage = "You must select a start date."), Index("IX_TrainingPrograms", Order = 5)]
		[Column(TypeName = "DATETIME2")]
		public DateTime StartDate { get; set; }

		/// <summary>
		/// get/set - The end date for the training program.
		/// </summary>
		[DateTimeKind(DateTimeKind.Utc)]
		[Required(ErrorMessage = "You must select a end date."), Index("IX_TrainingPrograms", Order = 6)]
		[Column(TypeName = "DATETIME2")]
		public DateTime EndDate { get; set; }

		/// <summary>
		/// get/set - The total number of training hours.
		/// </summary>
		[Display(Name = "Total training hours")]
		[Required, Min(1, ErrorMessage = "Total training hours must be greater than or equal to 1."), Max(3000, ErrorMessage = "Total training hours cannot exceed 3,000")]
		public int TotalTrainingHours { get; set; }

		/// <summary>
		/// get/set - The title of the qualification that will be received upon completion.
		/// </summary>
		[MaxLength(500)]
		public string TitleOfQualification { get; set; }

		/// <summary>
		/// get/set - Whether this training has been offered before.
		/// </summary>
		[Required(ErrorMessage = "You must select whether you have offered this type of training before.")]
		public bool HasOfferedThisTypeOfTrainingBefore { get; set; }

		/// <summary>
		/// get/set - Whether additional funding has been requested for this training.
		/// </summary>
		[Required(ErrorMessage = "You must select whether you have previously received or are requesting additional funding.")]
		public bool HasRequestedAdditionalFunding { get; set; }

		/// <summary>
		/// get/set - The description of the funding requested.
		/// </summary>
		public string DescriptionOfFundingRequested { get; set; }

		/// <summary>
		/// get/set - Whether training is for a member of an under represented group.
		/// </summary>
		public bool? MemberOfUnderRepresentedGroup { get; set; }

		/// <summary>
		/// Skills Training Focus
		///        Will your training project include Short-term Occupational Certificate
		///        Effective on Mar 15, 2022
		/// </summary>
		public bool? ShortTermOccupationalCert { get; set; }

		/// <summary>
		/// Skills Training Focus
		///        Will your training project include on-the-job training?
		///        Effective on Mar 15, 2022
		/// </summary>
		public bool? OnTheJobTraining { get; set; }

		/// <summary>
		/// Skills Training Focus (one of the following two choices)
		///		null nothing selected
		///      0   Occupational skills training
		///      1   Apprenticeship Foundation Program
		///        Effective on Mar 15, 2022
		/// </summary>
		public int? SkillsTrainingFocusType { get; set; }

		/// <summary>
		/// get/set - The current state of this training program information collection.
		/// </summary>
		[Required, Index("IX_TrainingPrograms", Order = 1), DefaultValue(TrainingProgramStates.Incomplete)]
		public TrainingProgramStates TrainingProgramState { get; set; } = TrainingProgramStates.Incomplete;

		/// <summary>
		/// get/set - The foreign key to the service line.
		/// </summary>
		public int? ServiceLineId { get; set; }

		/// <summary>
		/// get/set - The service line associated with this training program.
		/// </summary>
		[ForeignKey(nameof(ServiceLineId))]
		public virtual ServiceLine ServiceLine { get; set; }

		/// <summary>
		/// get/set - The foreign key to the service line breakdown
		/// </summary>
		public int? ServiceLineBreakdownId { get; set; }

		/// <summary>
		/// get/set - The service line breakdown associated with this training program.
		/// </summary>
		[ForeignKey(nameof(ServiceLineBreakdownId))]
		public virtual ServiceLineBreakdown ServiceLineBreakdown { get; set; }

		/// <summary>
		/// get/set - The foreign key to the eligible cost breakdown.  Skills training components can have multiple training programs, each is an eligible cost breakdown within a single eligible cost.
		/// </summary>
		public int? EligibleCostBreakdownId { get; set; }

		/// <summary>
		/// get/set - The parent eligible cost breakdown.  Skills training components can have multiple training programs, each is an eligible cost breakdown within a single eligible cost.
		/// </summary>
		[ForeignKey(nameof(EligibleCostBreakdownId))]
		public virtual EligibleCostBreakdown EligibleCostBreakdown { get; set; }

		/// <summary>
		/// get/set - A collection of delivery methods associated with this training program.
		/// </summary>
		public virtual ICollection<DeliveryMethod> DeliveryMethods { get; set; } = new List<DeliveryMethod>();

		/// <summary>
		/// get/set - A collection of under represented groups participating in this training program.
		/// </summary>
		public virtual ICollection<UnderRepresentedGroup> UnderRepresentedGroups { get; set; } = new List<UnderRepresentedGroup>();

		/// <summary>
		/// get/set - A collection of the training providers associated to this training program.
		/// </summary>
		public virtual ICollection<TrainingProvider> TrainingProviders { get; set; } = new List<TrainingProvider>();

		/// <summary>
		/// get/set - The foreign key to the cips code
		/// </summary>
		public int? TargetCipsCodeId { get; set; }

		// <summary>
		/// get/set - the optional course link URL
		/// </summary>
		public string CourseLink { get; set; }

		/// <summary>
		/// get/set - The CIPS Code
		/// </summary>
		[ForeignKey(nameof(TargetCipsCodeId))]
		public virtual ClassificationOfInstructionalProgram CipsCode { get; set; }

		/// <summary>
		/// get - The current training provider for this training program.
		/// </summary>
		[NotMapped]
		public TrainingProvider TrainingProvider
		{
			get
			{
				// Get the currently validated training provider.
				var validated = TrainingProviders.OrderByDescending(tp => tp.DateAdded).FirstOrDefault(tp => tp.TrainingProviderInventoryId != null && tp.TrainingProviderState == TrainingProviderStates.Complete);

				// If there is not a validated training provider, get the most recent requested one.
				// Generally only original training providers are attached to a training program, if a change request is created then we want to get that one instead.
				return validated?.ApprovedTrainingProvider
					?? validated
					?? TrainingProviders.OrderByDescending(tp => tp.DateAdded).FirstOrDefault(tp => tp.TrainingProviderInventoryId == null && tp.TrainingProviderState != TrainingProviderStates.Requested)
					?? TrainingProviders.OrderByDescending(tp => tp.DateAdded).FirstOrDefault(tp => tp.TrainingProviderState == TrainingProviderStates.Incomplete);
			}
		}

		/// <summary>
		/// get - The currently requested training provider for this training program.  This will only contain a value if a Change Request has been submitted.
		/// </summary>
		[NotMapped]
		public TrainingProvider RequestedTrainingProvider
		{
			get
			{
				// If the training program does not have a training provider look for one that is only linked to this training provider.
				if (TrainingProvider == null)
					return null;

				var requested = TrainingProviders.OrderByDescending(tp => tp.DateAdded).ThenByDescending(o => o.Id).FirstOrDefault(tp => tp.TrainingProviderState.In(TrainingProviderStates.Requested, TrainingProviderStates.RequestApproved, TrainingProviderStates.RequestDenied) && tp.DateAdded >= TrainingProvider.DateAdded);
				return TrainingProvider.RequestedTrainingProvider ?? requested?.RequestedTrainingProvider ?? requested;
			}
		}
		/// <summary>
		/// get/set - Used to identify the request is coming from Skills Training
		/// </summary>
		[NotMapped]
		public bool IsSkillsTraining { get; set; } = false;

		/// <summary>
		/// Creates a new instance of a <typeparamref name="TrainingProgram"/> object.
		/// </summary>
		public TrainingProgram()
		{

		}

		/// <summary>
		/// Creates a new instance of a <typeparamref name="TrainingProgram"/> object and initializes it with the specified property values.
		/// </summary>
		/// <param name="grantApplication"></param>
		public TrainingProgram(GrantApplication grantApplication)
		{
			if (grantApplication == null) throw new ArgumentNullException(nameof(grantApplication));
			GrantApplicationId = grantApplication.Id;
			GrantApplication = grantApplication;
			grantApplication.TrainingPrograms.Add(this);
		}

		/// <summary>
		/// Creates a new instance of a <typeparamref name="TrainingProgram"/> object and initializes it with the specified property values.
		/// </summary>
		/// <param name="trainingProvider"></param>
		public TrainingProgram(GrantApplication grantApplication, TrainingProvider trainingProvider) : this(grantApplication)
		{
			if (trainingProvider == null)
				throw new ArgumentNullException(nameof(trainingProvider));

			TrainingProviders.Add(trainingProvider);
		}

		/// <summary>
		/// Validate the training program.
		/// </summary>
		/// <param name="validationContext"></param>
		/// <returns></returns>
		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var context = validationContext.GetDbContext();
			var entry = validationContext.GetDbEntityEntry();
			var httpContext = validationContext.GetHttpContext();

			var isExternalUser = httpContext?.User.IsExternalUser() ?? false;  // If we have no HttpContext, assume we are not an external user. This crops up when a Service runs the validate methods.

			// This is done to stop errors from being thrown when developers use EF entities in ViewModels.
			if (entry == null || context == null)
				yield break;

			if (GrantApplication == null && GrantApplicationId == 0)
				GrantApplication = context.Set<GrantApplication>().First(ga => ga.Id == GrantApplicationId);
			else if (GrantApplication == null)
			{
				// Do this to force load the grant application for the current training program.
				GrantApplication = context.Set<GrantApplication>().Find(GrantApplicationId);
			}

			if (GrantApplication.GrantOpening == null)
				context.Set<GrantOpening>().Find(GrantApplication.GrantOpeningId);

			// Must have DeliveryMethods.
			var deliveryMethods = context.Set<TrainingProgram>().Include(m => m.DeliveryMethods).FirstOrDefault(o => o.Id == Id);
			if (!DeliveryMethods.Any())
				yield return new ValidationResult("You must select at least one primary delivery method.", new[] { nameof(DeliveryMethods) });

			// Internal Users are not required to adhere to Training Start/End Date requirements
			if (isExternalUser)
			{
				// StartDate must be within the Delivery dates (compares original values, as well).
				if (!this.HasStartDateWithinDeliveryDates())
					yield return new ValidationResult($"The training start date must fall within the project delivery dates {GrantApplication.StartDate.ToLocalMorning():yyyy-MM-dd} to {GrantApplication.EndDate.ToLocalMidnight():yyyy-MM-dd}", new[] { nameof(StartDate) });

				// EndDate must be within the Delivery dates (compares original values, as well).
				if (!this.HasEndDateWithinDeliveryDates())
					yield return new ValidationResult($"The training end date must fall within the project delivery dates {GrantApplication.StartDate.ToLocalMorning():yyyy-MM-dd} to {GrantApplication.EndDate.ToLocalMidnight():yyyy-MM-dd}", new[] { nameof(EndDate) });
			}

			//if (!isExternalUser)
			//{
			//	var fiscalYears = context.FiscalYears.OrderBy(fy => fy.StartDate).ThenBy(fy => fy.Caption).ToList();
			//	// Force load training period and Fiscal year
			//	context.Set<TrainingPeriod>()
			//		.Include(t => t.FiscalYear)
			//		.FirstOrDefault(t => t.Id == GrantApplication.GrantOpening.TrainingPeriodId);

			//	var maxDates = this.GetMaxDates(fiscalYears);

			//if (!this.HasStartDateWithinExtendedDeliveryDates(maxDates.Item1))
			//	yield return new ValidationResult($"The training start date must fall within the extended project delivery dates { GrantApplication.StartDate.ToLocalMorning():yyyy-MM-dd} to {maxDates.Item1.ToLocalMidnight():yyyy-MM-dd}", new[] { nameof(StartDate) });

			//if (!this.HasEndDateWithinExtendedDeliveryDates(maxDates.Item2))
			//	yield return new ValidationResult($"The training end date must fall within the extended project delivery dates { GrantApplication.StartDate.ToLocalMorning():yyyy-MM-dd} to {maxDates.Item2.ToLocalMidnight():yyyy-MM-dd}", new[] { nameof(EndDate) });
			//}

			//// StartDate must be within the Delivery dates (compares original values, as well).
			//if (false && GrantApplication.GrantOpening.GrantStream.GrantProgram.ProgramTypeId == ProgramTypes.WDAService && !this.HasStartDateWithin30Days())
			//	yield return new ValidationResult("Training start date must be at least 30 days after the delivery start date.", new[] { nameof(StartDate) });

			//// EndDate must be within the Delivery dates (compares original values, as well).
			//if (false && GrantApplication.GrantOpening.GrantStream.GrantProgram.ProgramTypeId == ProgramTypes.WDAService &&
			//	 !this.HasEndDateWithin30Days())
			//	yield return new ValidationResult("Training end date must be at least 30 days before the delivery end date.", new[] { nameof(EndDate) });

			// StartDate must be before or on EndDate.
			if (StartDate.ToLocalTime().Date > EndDate.ToLocalTime().Date)
				yield return new ValidationResult($"The end date must occur on or after the start date '{StartDate.ToLocalMorning():yyyy-MM-dd}'.", new[] { nameof(EndDate) });

			if (ServiceLineId.HasValue)
			{
				// Need to force load any of the related entities.
				context.Set<GrantOpening>()
					.Include(m => m.GrantStream)
					.Include(m => m.GrantStream.ProgramConfiguration)
					.Include(m => m.GrantStream.ProgramConfiguration.EligibleExpenseTypes)
					.Include($"{nameof(GrantStream)}.{nameof(ProgramConfiguration)}.{nameof(ProgramConfiguration.EligibleExpenseTypes)}.{nameof(EligibleExpenseType.Breakdowns)}")
					.FirstOrDefault(g => g.Id == GrantApplication.GrantOpeningId);

				var eligibleExpenseTypeIds = GrantApplication.GrantOpening.GrantStream.ProgramConfiguration.EligibleExpenseTypes.Select(eet => eet.Id).ToArray();
				context.Set<EligibleExpenseBreakdown>().Where(eeb => eligibleExpenseTypeIds.Contains(eeb.EligibleExpenseTypeId));
				context.Set<EligibleCostBreakdown>().Include(m => m.EligibleCost).Include(m => m.EligibleCost.EligibleExpenseType).FirstOrDefault(ecb => ecb.Id == EligibleCostBreakdownId);
				context.Set<ServiceLine>().FirstOrDefault(sl => sl.Id == ServiceLineId);

				// Ensure that the selected service line is valid for the selected grant opening.
				if (!GrantApplication.GrantOpening.GrantStream.ProgramConfiguration.EligibleExpenseTypes.Any(eet => eet.Breakdowns.Any(b => b.ServiceLineId == ServiceLineId)))
					yield return new ValidationResult($"The '{EligibleCostBreakdown.EligibleCost.EligibleExpenseType.Caption}' '{ServiceLine.Caption}' is not a valid service for this grant opening.", new[] { nameof(ServiceLineId) });
			}

			// If certain SkillFocus then there must be other fields provided.
			if (new[] { 5, 6 }.Contains(SkillFocus == null ? 0 : (int)SkillFocusId))
			{
				// Must contain MemberOfUnderRepresentedGroup.
				if (MemberOfUnderRepresentedGroup == null)
					yield return new ValidationResult("You must select whether you are a member of a under represented group.", new[] { nameof(MemberOfUnderRepresentedGroup) });

				// Must contain InDemandOccupation.
				if (InDemandOccupation == null && InDemandOccupationId == null)
					yield return new ValidationResult("You must select an in demand occupation.", new[] { nameof(InDemandOccupationId) });

				// Must contain TrainingLevel.
				if (TrainingLevel == null && TrainingLevelId == null)
					yield return new ValidationResult("You must select a training level.", new[] { nameof(TrainingLevelId) });
			}

			// If a MemberOfUnderRepresentedGroup then must have UnderRepresentedGroups.
			var trainingProgram = context.Set<TrainingProgram>().Include(t => t.UnderRepresentedGroups).FirstOrDefault(x => x.Id == Id);

			if (MemberOfUnderRepresentedGroup.HasValue &&
				MemberOfUnderRepresentedGroup.Value &&
				new[] { 5, 6 }.Contains(SkillFocusId.Value) &&
				(Id > 0 && !trainingProgram.UnderRepresentedGroups.Any())) // only if this is an existing training program
				yield return new ValidationResult("You must select under represented groups that apply.", new[] { nameof(UnderRepresentedGroups) });

			// If ExpectedQualifications provided then must have TitleOfQualification.
			if (!new[] { 1, 5 }.Contains(ExpectedQualification == null ? ExpectedQualificationId : ExpectedQualification.Id) && String.IsNullOrEmpty(TitleOfQualification))
				yield return new ValidationResult("If you a have expected qualifications you must include the title of the qualification.", new[] { nameof(TitleOfQualification) });

			// If HasRequestedAdditionalFunding then must have DescriptionOfFundingRequested.
			if (HasRequestedAdditionalFunding && String.IsNullOrEmpty(DescriptionOfFundingRequested))
				yield return new ValidationResult("If you have received or requested additional funding you must include a description of the funding request.", new[] { nameof(DescriptionOfFundingRequested) });

			if (EligibleCostBreakdownId.HasValue && EligibleCostBreakdownId.Value != 0 || Id == 0 && GrantApplication.GrantOpening.GrantStream.GrantProgram.ProgramTypeId == ProgramTypes.WDAService)
			{
				if (EligibleCostBreakdown == null)
					EligibleCostBreakdown = context.Set<EligibleCostBreakdown>().FirstOrDefault(ecb => ecb.Id == EligibleCostBreakdownId);

				// When the training program references an eligible cost breakdown, the service line must be selected.
				if (!ServiceLineId.HasValue)
					yield return new ValidationResult("You must select a skills training focus for this project.", new[] { nameof(ServiceLineId) });
				else
				{
					// If a service line has breakdowns then one must be selected.
					var breakdowns = context.Set<ServiceLineBreakdown>().Where(slb => slb.ServiceLineId == ServiceLineId);
					if (!ServiceLineBreakdownId.HasValue && breakdowns.Any())
						yield return new ValidationResult("You must select an essential skills training type for this project.", new[] { nameof(ServiceLineBreakdownId) });
				}

				if (TrainingProgramState == TrainingProgramStates.Complete)
				{
					// Must include a cost for the training program.
					if (GrantApplication.ApplicationStateInternal.In(ApplicationStateInternal.Draft) && EligibleCostBreakdown.EstimatedCost <= 0)
						yield return new ValidationResult("Project total estimated cost is required.", new[] { nameof(EligibleCostBreakdown.EstimatedCost) });
					else if (GrantApplication.ApplicationStateInternal.In(ApplicationStateInternal.RecommendedForApproval, ApplicationStateInternal.OfferIssued) && EligibleCostBreakdown.IsEligible && EligibleCostBreakdown.AssessedCost <= 0)
						yield return new ValidationResult("Project total agreed cost is required.", new[] { nameof(EligibleCostBreakdown.AssessedCost) });
					else if (GrantApplication.ApplicationStateInternal.In(ApplicationStateInternal.RecommendedForApproval, ApplicationStateInternal.OfferIssued) && !EligibleCostBreakdown.IsEligible && EligibleCostBreakdown.AssessedCost > 0)
						yield return new ValidationResult("Project total agreed cost cannot be greater than $0 if it is not eligible.", new[] { nameof(EligibleCostBreakdown.AssessedCost) });
				}
			}

			if (entry.State == EntityState.Modified)
			{
				// If a service line becomes disabled, the training program must throw a validation error.
				if (isExternalUser && EligibleCostBreakdownId.HasValue && GrantApplication.ApplicationStateInternal.In(ApplicationStateInternal.Draft, ApplicationStateInternal.ApplicationWithdrawn))
				{
					var eligibleCostBreakdown = context.Set<EligibleCostBreakdown>().Include(ecb => ecb.EligibleExpenseBreakdown).FirstOrDefault(ecb => ecb.Id == EligibleCostBreakdownId.Value);
					if (!eligibleCostBreakdown?.EligibleExpenseBreakdown?.IsActive ?? false)
					{
						yield return new ValidationResult($"The service line '{eligibleCostBreakdown.EligibleExpenseBreakdown.Caption}' is no longer available, please select another.", new[] { nameof(EligibleCostBreakdownId) });
					}
				}
			}

			foreach (var validation in base.Validate(validationContext))
			{
				yield return validation;
			}
		}

		public void Clone(TrainingProgram tp)
		{
			InDemandOccupationId = tp.InDemandOccupationId;
			SkillLevelId = tp.SkillLevelId;
			SkillFocusId = tp.SkillFocusId;
			ExpectedQualificationId = tp.ExpectedQualificationId;
			TrainingLevelId = tp.TrainingLevelId;
			CourseTitle = tp.CourseTitle;
			TrainingBusinessCase = tp.TrainingBusinessCase;

			StartDate = tp.StartDate;
			EndDate = tp.EndDate;

			TotalTrainingHours = tp.TotalTrainingHours;
			TitleOfQualification = tp.TitleOfQualification;
			HasOfferedThisTypeOfTrainingBefore = tp.HasOfferedThisTypeOfTrainingBefore;
			HasRequestedAdditionalFunding = tp.HasRequestedAdditionalFunding;
			DescriptionOfFundingRequested = tp.DescriptionOfFundingRequested;
			MemberOfUnderRepresentedGroup = tp.MemberOfUnderRepresentedGroup;
			TrainingProgramState = tp.TrainingProgramState;

			ServiceLineId = tp.ServiceLineId;
			ServiceLineBreakdownId = tp.ServiceLineBreakdownId;
			EligibleCostBreakdownId = tp.EligibleCostBreakdownId;

			TargetCipsCodeId = tp.TargetCipsCodeId;
			CourseLink = tp.CourseLink;

			foreach(var dm in tp.DeliveryMethods)
				DeliveryMethods.Add(dm);

			foreach(var grp in tp.UnderRepresentedGroups)
				UnderRepresentedGroups.Add(grp);
		}
	}
}
