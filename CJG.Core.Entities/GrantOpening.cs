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
	/// <typeparamref name="GrantOpening"/> class, provides the ORM a way to manage grant openings.  A grant opening is an allotment of funds for a specific stream and training period.
	/// </summary>
	public class GrantOpening : EntityBase
	{
		/// <summary>
		/// get/set - The primary key for this Grant Opening.
		/// </summary>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		/// <summary>
		/// get/set - The current state of this Grant Opening.
		/// </summary>
		[DefaultValue(GrantOpeningStates.Unscheduled)]
		[Index("IX_GrantOpening", 1)]
		public GrantOpeningStates State { get; set; } = GrantOpeningStates.Unscheduled;

		/// <summary>
		/// get/set - The amount of money targeted to use the whole budget based on denied, withdrawn, reduction, slippage and cancellation rates.
		/// </summary>
		[Required, Min(0, ErrorMessage = "The intake target must be greater than or equal to 0.")]
		public decimal IntakeTargetAmt { get; set; }

		/// <summary>
		/// get/set - The amount of money that has been allocated to this Grant Opening.
		/// </summary>
		[Required, Min(0, ErrorMessage = "The budget allocation amount must be greater than or equal to 0.")]
		public decimal BudgetAllocationAmt { get; set; }

		/// <summary>
		/// get/set - The expected percentage of Grant Applications that will be denied.
		/// </summary>
		[Min(0, ErrorMessage = "The plan denied rate cannot be less than 0."), Max(1, ErrorMessage = "The plan denied rate cannot be greater than 1.")]
		public double PlanDeniedRate { get; set; }

		/// <summary>
		/// get/set - The expected percentage of Grant Application that will be withdrawn.
		/// </summary>
		[Min(0, ErrorMessage = "The plan withdrawn rate cannot be less than 0."), Max(1, ErrorMessage = "The plan withdrawn rate cannot be greater than 1.")]
		public double PlanWithdrawnRate { get; set; }

		/// <summary>
		/// get/set - The expected percentage of Grant Applications that will be reduced.
		/// </summary>
		[Min(0, ErrorMessage = "The plan reduction rate cannot be less than 0."), Max(1, ErrorMessage = "The plan reduction rate cannot be greater than 1.")]
		public double PlanReductionRate { get; set; }

		/// <summary>
		/// get/set - The expected percentage of Grant Applications that will have slippage.
		/// </summary>
		[Min(0, ErrorMessage = "The plan slippage rate cannot be less than 0."), Max(1, ErrorMessage = "The plan slippage rate cannot be greater than 1.")]
		public double PlanSlippageRate { get; set; }

		/// <summary>
		/// get/set - The expected percentage of Grant Applications that will be cancelled.
		/// </summary>
		[Min(0, ErrorMessage = "The plan cancellation rate cannot be less than 0."), Max(1, ErrorMessage = "The plan cancellation rate cannot be greater than 1.")]
		public double PlanCancellationRate { get; set; }

		/// <summary>
		/// get/set - When this Grant Opening will be published for the public to pick when creating Grant Applications.
		/// </summary>
		[DateTimeKind(DateTimeKind.Utc)]
		[Required]
		[Index("IX_GrantOpening", 2)]
		[Column(TypeName = "DATETIME2")]
		public DateTime PublishDate { get; set; }

		/// <summary>
		/// get/set - When this Grant Opening will be available for submitting Grant Applications.
		/// </summary>
		[DateTimeKind(DateTimeKind.Utc)]
		[Required]
		[Index("IX_GrantOpening", 3)]
		[Column(TypeName = "DATETIME2")]
		public DateTime OpeningDate { get; set; }

		/// <summary>
		/// get/set - When this Grant Opening will no longer be available for selecting or submitting Grant Applications.
		/// </summary>
		[DateTimeKind(DateTimeKind.Utc)]
		[Required]
		[Index("IX_GrantOpening", 4)]
		[Column(TypeName = "DATETIME2")]
		public DateTime ClosingDate { get; set; }

		/// <summary>
		/// get/set - The foreign key to the Training Period this Grant Opening will be available in.
		/// </summary>
		[Index("IX_GrantOpening", 5)]
		public int TrainingPeriodId { get; set; }

		/// <summary>
		/// get/set - The Training Period this Grant Opening will be available in.
		/// </summary>
		[ForeignKey(nameof(TrainingPeriodId))]
		public virtual TrainingPeriod TrainingPeriod { get; set; }

		/// <summary>
		/// get/set - The foreign key to the Grant Stream this Grant Opening is associated with.
		/// </summary>
		[Index("IX_GrantOpening", 6)]
		public int GrantStreamId { get; set; }

		/// <summary>
		/// get/set - The Grant Stream this Grant Opening is associated with.
		/// </summary>
		[ForeignKey(nameof(GrantStreamId))]
		public virtual GrantStream GrantStream { get; set; }

		/// <summary>
		/// get/set - The intake information for this Grant Opening.
		/// </summary>
		public virtual GrantOpeningIntake GrantOpeningIntake { get; set; }

		/// <summary>
		/// get/set - The financial information for this Grant Opening.
		/// </summary>
		public virtual GrantOpeningFinancial GrantOpeningFinancial { get; set; }

		/// <summary>
		/// get/set - A collection of all the Grant Applications associated to this Grant Opening.
		/// </summary>
		public ICollection<GrantApplication> GrantApplications { get; set; } = new List<GrantApplication>();

		/// <summary>
		/// Creates a new instance of a <typeparamref name="GrantOpening" /> object.
		/// </summary>
		public GrantOpening()
		{
		}

		/// <summary>
		/// Creates a new instance of a <typeparamref name="GrantOpening"/> object and initializes it with the specified property values.
		/// Uses the training period default publish, opening and closing dates.
		/// </summary>
		/// <param name="grantStream"></param>
		/// <param name="trainingPeriod"></param>
		/// <param name="budgetAllocation"></param>
		/// <param name="intakeTarget"></param>
		public GrantOpening(GrantStream grantStream, TrainingPeriod trainingPeriod, decimal budgetAllocation)
		{
			if (budgetAllocation < 0)
				throw new ArgumentException("The budget allocation must be greater than or equal to 0.", nameof(budgetAllocation));

			GrantStream = grantStream ?? throw new ArgumentNullException(nameof(grantStream));
			GrantStreamId = grantStream.Id;
			TrainingPeriod = trainingPeriod ?? throw new ArgumentNullException(nameof(trainingPeriod));
			TrainingPeriodId = trainingPeriod.Id;
			PublishDate = trainingPeriod.DefaultPublishDate;
			OpeningDate = trainingPeriod.DefaultOpeningDate;
			ClosingDate = trainingPeriod.EndDate;
			BudgetAllocationAmt = budgetAllocation;
			State = GrantOpeningStates.Unscheduled;

			PlanDeniedRate = grantStream.DefaultDeniedRate;
			PlanWithdrawnRate = grantStream.DefaultWithdrawnRate;
			PlanReductionRate = grantStream.DefaultReductionRate;
			PlanSlippageRate = grantStream.DefaultSlippageRate;
			PlanCancellationRate = grantStream.DefaultCancellationRate;
			IntakeTargetAmt = this.CalculateIntakeTarget();
		
			GrantOpeningIntake = new GrantOpeningIntake(this);
			GrantOpeningFinancial = new GrantOpeningFinancial(this);
		}

		/// <summary>
		/// Creates a new instance of a <typeparamref name="GrantOpening"/> object and initializes it with the specified property values.
		/// </summary>
		/// <param name="grantStream"></param>
		/// <param name="trainingPeriod"></param>
		/// <param name="publishOn"></param>
		/// <param name="openOn"></param>
		/// <param name="closeOn"></param>
		/// <param name="budgetAllocation"></param>
		public GrantOpening(GrantStream grantStream, TrainingPeriod trainingPeriod, DateTime publishOn, DateTime openOn, DateTime closeOn, decimal budgetAllocation)
		{
			if (trainingPeriod == null)
				throw new ArgumentNullException(nameof(trainingPeriod));

			if (trainingPeriod.StartDate > openOn)
				throw new ArgumentException($"The opening date must not be before the training period start date '{trainingPeriod.StartDate.ToLocalMorning():yyyy-MM-dd}'.", nameof(openOn));

			if (publishOn > openOn)
				throw new ArgumentException("The publish date must be before or on the opening date.", nameof(publishOn));

			if (openOn >= closeOn)
				throw new ArgumentException("The opening date must be before or on the closing date.", nameof(openOn));

			if (closeOn.Date < AppDateTime.UtcMorning)
				throw new ArgumentException("The closing date cannot be in the past.", nameof(closeOn));

			if (trainingPeriod.EndDate < closeOn)
				throw new ArgumentException($"The closing date cannot be after the training period end date '{trainingPeriod.EndDate.ToLocalMidnight():yyyy-MM-dd}'.", nameof(closeOn));

			if (budgetAllocation < 0)
				throw new ArgumentException("The budget allocation must be greater than or equal to 0.", nameof(budgetAllocation));

			GrantStream = grantStream ?? throw new ArgumentNullException(nameof(grantStream));
			GrantStreamId = grantStream.Id;
			TrainingPeriod = trainingPeriod;
			TrainingPeriodId = trainingPeriod.Id;
			PublishDate = publishOn.ToUniversalTime();
			OpeningDate = openOn.ToUniversalTime();
			ClosingDate = closeOn.ToUniversalTime();
			BudgetAllocationAmt = budgetAllocation;
			State = GrantOpeningStates.Unscheduled;

			PlanDeniedRate = grantStream.DefaultDeniedRate;
			PlanWithdrawnRate = grantStream.DefaultWithdrawnRate;
			PlanReductionRate = grantStream.DefaultReductionRate;
			PlanSlippageRate = grantStream.DefaultSlippageRate;
			PlanCancellationRate = grantStream.DefaultCancellationRate;
			IntakeTargetAmt = this.CalculateIntakeTarget();
		}

		/// <summary>
		/// Creates a new instance of a <typeparamref name="GrantOpening"/> object and initializes it with the specified property values.
		/// </summary>
		/// <param name="grantStream"></param>
		/// <param name="trainingPeriod"></param>
		/// <param name="publishOn"></param>
		/// <param name="openOn"></param>
		/// <param name="closeOn"></param>
		/// <param name="budgetAllocation"></param>
		/// <param name="deniedRate"></param>
		/// <param name="withdrawnRate"></param>
		/// <param name="reductionRate"></param>
		/// <param name="slippageRate"></param>
		/// <param name="cancellationRate"></param>
		public GrantOpening(GrantStream grantStream, TrainingPeriod trainingPeriod, DateTime publishOn, DateTime openOn, DateTime closeOn, decimal budgetAllocation,
			double deniedRate, double withdrawnRate, double reductionRate, double slippageRate, double cancellationRate) : this(grantStream, trainingPeriod, publishOn, openOn, closeOn, budgetAllocation)
		{
			if (deniedRate < 0 || deniedRate > 1)
				throw new ArgumentException("The denied rate cannot be less than 0 or greater than 1.", nameof(deniedRate));

			if (withdrawnRate < 0 || withdrawnRate > 1)
				throw new ArgumentException("The withdrawn rate cannot be less than 0 or greater than 1.", nameof(withdrawnRate));

			if (reductionRate < 0 || reductionRate > 1)
				throw new ArgumentException("The reduction rate cannot be less than 0 or greater than 1.", nameof(reductionRate));

			if (slippageRate < 0 || slippageRate > 1)
				throw new ArgumentException("The slippage rate cannot be less than 0 or greater than 1.", nameof(slippageRate));

			if (cancellationRate < 0 || cancellationRate > 1)
				throw new ArgumentException("The cancellation rate cannot be less than 0 or greater than 1.", nameof(cancellationRate));

			PlanDeniedRate = deniedRate;
			PlanWithdrawnRate = withdrawnRate;
			PlanReductionRate = reductionRate;
			PlanSlippageRate = slippageRate;
			PlanCancellationRate = cancellationRate;
			IntakeTargetAmt = this.CalculateIntakeTarget();
		}

		/// <summary>
		/// Validate this <typeparamref name="GrantOpening"/> object.
		/// </summary>
		/// <param name="validationContext"></param>
		/// <returns></returns>
		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var context = validationContext.GetDbContext();
			var entry = validationContext.GetDbEntityEntry();

			// This is done to stop errors from being thrown when developers use EF entities in ViewModels.
			if (entry == null || context == null)
				yield break;

			context.Set<GrantOpening>().Include(go => go.TrainingPeriod).FirstOrDefault(go => go.Id == Id);

			// The earliest publish date is today.
			var earliestPublishDate = AppDateTime.UtcMorning;

			// TrainingPeriodId is required.
			if (TrainingPeriodId == 0)
				yield return new ValidationResult("The grant opening must be associated with a valid training period.", new[] { nameof(TrainingPeriodId) });

			if (entry.State == EntityState.Added)
			{
				// A new GrantOpening must begin as Unscheduled.
				if (!((PublishDate >= AppDateTime.UtcMorning &&
					   OpeningDate >= AppDateTime.UtcMorning &&
					   ClosingDate > AppDateTime.UtcNow) ||
					  (PublishDate <= AppDateTime.UtcMorning &&
					   OpeningDate <= AppDateTime.UtcMorning &&
					   ClosingDate < AppDateTime.UtcNow)))
				{
					yield return new ValidationResult("Publish date, opening date and closing date must either all in the past or all in the future.", new[] { nameof(State) });
				}

				if (State != GrantOpeningStates.Unscheduled)
					yield return new ValidationResult("A new Grant Opening must begin as unscheduled.", new[] { nameof(State) });
			}
			else if (entry.State == EntityState.Modified)
			{
				context.Entry(entry.Entity).GetDatabaseValues();

				var originalBudget = (decimal)entry.OriginalValues[nameof(BudgetAllocationAmt)];
				var originalState = (GrantOpeningStates)entry.OriginalValues[nameof(State)];
				var originalPublishDate = DateTime.SpecifyKind((DateTime)entry.OriginalValues[nameof(PublishDate)], DateTimeKind.Utc);
				var originalOpeningDate = DateTime.SpecifyKind((DateTime)entry.OriginalValues[nameof(OpeningDate)], DateTimeKind.Utc);
				var originalClosingDate = DateTime.SpecifyKind((DateTime)entry.OriginalValues[nameof(ClosingDate)], DateTimeKind.Utc);
				var originalGrantStreamId = (int)entry.OriginalValues[nameof(GrantStreamId)];
				var originalTrainingPeriodId = (int)entry.OriginalValues[nameof(TrainingPeriodId)];
				earliestPublishDate = originalPublishDate.Date < AppDateTime.UtcMorning.Date ? originalPublishDate : AppDateTime.UtcMorning;

				if (GrantStreamId != originalGrantStreamId || TrainingPeriodId != originalTrainingPeriodId)
					yield return new ValidationResult("The Grant Stream and Training Period cannot be changed on a Grant Opening.", new[] { nameof(State) });

				switch (State)
				{
					// to UNSCHEDULED
					case GrantOpeningStates.Unscheduled:
						if (originalState != GrantOpeningStates.Unscheduled
						 && originalState != GrantOpeningStates.Scheduled)
							yield return new ValidationResult("The state must go from scheduled or unscheduled, to unscheduled.", new[] { nameof(State) });
						break;

					// to SCHEDULED
					case GrantOpeningStates.Scheduled:
						if (originalState != GrantOpeningStates.Scheduled
						 && originalState != GrantOpeningStates.Unscheduled)
							yield return new ValidationResult("The state must go from scheduled or unscheduled, to scheduled.", new[] { nameof(State) });
						break;

					// to PUBLISHED
					case GrantOpeningStates.Published:
						if (originalState != GrantOpeningStates.Published
						 && originalState != GrantOpeningStates.Scheduled
						 && originalState != GrantOpeningStates.Unscheduled)
							yield return new ValidationResult("The state must go from published, scheduled, or unscheduled, to published.", new[] { nameof(State) });

						if (originalState.In(GrantOpeningStates.Published, GrantOpeningStates.Open, GrantOpeningStates.OpenForSubmit, GrantOpeningStates.Closed)
							&& originalPublishDate != PublishDate)
							yield return new ValidationResult("Publish date cannot be changed after the Grant Opening has been published.", new[] { nameof(State) });

						if (!(PublishDate <= AppDateTime.UtcNow && OpeningDate > AppDateTime.UtcNow))
							yield return new ValidationResult("Published date must be today or in the past, opening date must be in the future while in open state.", new[] { nameof(ClosingDate) });
						break;

					// to OPEN
					case GrantOpeningStates.Open:
						if (originalState != GrantOpeningStates.Open
						 && originalState != GrantOpeningStates.Scheduled
						 && originalState != GrantOpeningStates.Unscheduled
						 && originalState != GrantOpeningStates.Published
						 && originalState != GrantOpeningStates.Closed)
							yield return new ValidationResult("The state must go from open, closed, scheduled, unscheduled, or published, to open.", new[] { nameof(State) });

						if (originalState.In(GrantOpeningStates.Published, GrantOpeningStates.Open, GrantOpeningStates.OpenForSubmit, GrantOpeningStates.Closed)
							&& originalPublishDate != PublishDate)
							yield return new ValidationResult("Publish date cannot be changed after the Grant Opening has been published.", new[] { nameof(State) });

						if (originalState.In(GrantOpeningStates.Open, GrantOpeningStates.OpenForSubmit, GrantOpeningStates.Closed)
							&& originalOpeningDate != OpeningDate)
							yield return new ValidationResult("Opening date cannot be changed after the Grant Opening has been opened.", new[] { nameof(State) });

						// The TrainingPeriod has expired
						if (originalState == GrantOpeningStates.Closed && AppDateTime.UtcNow >= TrainingPeriod.EndDate)
							yield return new ValidationResult("This grant opening cannot be reopened as the training period end date has past.", new[] { nameof(ClosingDate) });

						if (!(OpeningDate <= AppDateTime.UtcNow && ClosingDate > AppDateTime.UtcNow))
							yield return new ValidationResult("Opening date must be today or in the past, closing date must be in the future while in open state.", new[] { nameof(ClosingDate) });
						break;

					// to CLOSED
					case GrantOpeningStates.Closed:
						if (originalState != GrantOpeningStates.Closed
							&& originalState != GrantOpeningStates.Open
							&& originalState != GrantOpeningStates.OpenForSubmit
							&& originalState != GrantOpeningStates.Unscheduled
							&& originalState != GrantOpeningStates.Scheduled
							&& originalState != GrantOpeningStates.Published)
							yield return new ValidationResult("The state must go from open, open for submit, unscheduled, scheduled, or published, to closed.", new[] { nameof(State) });

						if (originalState.In(GrantOpeningStates.Published, GrantOpeningStates.Open, GrantOpeningStates.OpenForSubmit, GrantOpeningStates.Closed)
							&& originalPublishDate != PublishDate)
							yield return new ValidationResult("Publish date cannot be changed after the Grant Opening has been published.", new[] { nameof(State) });

						if (originalState.In(GrantOpeningStates.Open, GrantOpeningStates.OpenForSubmit, GrantOpeningStates.Closed)
							&& originalOpeningDate != OpeningDate)
							yield return new ValidationResult("Opening date cannot be changed after the Grant Opening has been opened.", new[] { nameof(State) });

						if (originalState.In(GrantOpeningStates.OpenForSubmit, GrantOpeningStates.Closed)
							&& originalClosingDate != ClosingDate)
							yield return new ValidationResult("Closing date cannot be changed after the Grant Opening has been closed.", new[] { nameof(State) });
						break;

					// to OPENFORSUBMIT
					case GrantOpeningStates.OpenForSubmit:
						if (originalState != GrantOpeningStates.OpenForSubmit
							&& originalState != GrantOpeningStates.Closed)
							yield return new ValidationResult("The state must go from closed, to open for submit.", new[] { nameof(State) });

						if (PublishDate != originalPublishDate
						 || OpeningDate != originalOpeningDate
						 || ClosingDate != originalClosingDate)
							yield return new ValidationResult("Publish date, opening date and closing date cannot be changed while in open for submit state.", new[] { nameof(State) });
						break;
				}
			}

			if (PublishDate < earliestPublishDate
			    || PublishDate > TrainingPeriod?.EndDate)
				yield return new ValidationResult($"Publish date must be today or later, and the same or before the training period end date '{TrainingPeriod?.EndDate.ToLocalMidnight():yyyy-MM-dd}'.", new[] { nameof(PublishDate) });

			if (OpeningDate < PublishDate
			    || OpeningDate.ToLocalTime().Date > ClosingDate.ToLocalTime().Date)
				yield return new ValidationResult("Opening date must be the same or later than the publish date, and before the closing date.", new[] { nameof(OpeningDate) });

			if (ClosingDate < OpeningDate
			    || ClosingDate.Date > TrainingPeriod?.EndDate
			    || ClosingDate.Date < TrainingPeriod?.StartDate)
				yield return new ValidationResult($"Closing date must be the same or later than the opening date, and during the training period of '{TrainingPeriod?.StartDate.ToLocalMidnight():yyyy-MM-dd} to {TrainingPeriod?.EndDate.ToLocalMidnight():yyyy-MM-dd}'.", new[] { nameof(ClosingDate) });

			foreach (var validation in base.Validate(validationContext))
			{
				yield return validation;
			}
		}
	}
}
