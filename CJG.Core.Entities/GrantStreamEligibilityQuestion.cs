using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	/// <summary>
	/// <typeparamref name="GrantStreamEligibilityQuestion"/> class, provides ORM a way to manage stream grants.
	/// </summary>
	public class GrantStreamEligibilityQuestion : EntityBase
	{
		/// <summary>
		/// get/set - Primary key uses IDENTITY.
		/// </summary>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		/// <summary>
		/// get/set - Foreign key to the parent grant program.
		/// </summary>
		[Index("IX_GrantStreamEligibilityQuestions", 1)]
		public int GrantStreamId { get; set; }

		/// <summary>
		/// get/set - The parent grant program.
		/// </summary>
		[ForeignKey(nameof(GrantStreamId))]
		public virtual GrantStream GrantStream { get; set; }

		/// <summary>
		/// get/set - The grant stream Eligibility Requirements text.
		/// </summary>
		[MaxLength(2000)]
		public string EligibilityRequirements { get; set; }

		/// <summary>
		/// get/set - The grant stream Eligibility Question.
		/// </summary>
		[MaxLength(2000)]
		[Required]
		public string EligibilityQuestion { get; set; }

		/// <summary>
		/// get/set - Whether question is enabled.
		/// </summary>
		[Required]
		public bool IsActive { get; set; }

		/// <summary>
		/// get/set - Whether question is required to be answered YES.
		/// </summary>
		[Required]
		public bool EligibilityPositiveAnswerRequired { get; set; }

		/// <summary>
		/// get/set - Do we collect contact information when applicant answered YES.
		/// </summary>
		[Required]
		public bool CollectContactInformation { get; set; }

		/// <summary>
		/// get/set - The order questions will be presented.
		/// </summary>
		[Required]
		public int RowSequence { get; set; }

		/// <summary>
		/// Creates a new instance of a <typeparamref name="GrantStream"/> object.
		/// </summary>
		public GrantStreamEligibilityQuestion()
		{
		}

		/// <summary>
		/// Creates a new instance of a <typeparamref name="GrantStreamEligibilityQuestions"/> object.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="objective"></param>
		/// <param name="program"></param>
		/// <param name="payment"></param>
		public GrantStreamEligibilityQuestion(string eligibilityRequirements, string eligibilityQuestion, bool isActive,
			bool eligibilityPositiveAnswerRequired, int rowSequence, GrantStream grantStream)
		{
			EligibilityRequirements = eligibilityRequirements;
			EligibilityQuestion = eligibilityQuestion ?? throw new ArgumentNullException(nameof(eligibilityQuestion));
			IsActive = isActive;
			EligibilityPositiveAnswerRequired = eligibilityPositiveAnswerRequired;
			RowSequence = rowSequence;
			GrantStreamId = grantStream.Id;
		}

		/// <summary>
		/// Validate this grant stream before making changes to the datasource.
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

			if (GrantStreamId > 0 && GrantStream == null)
				GrantStream = context.Set<GrantStream>().Find(GrantStreamId);

			// Must be linked to an existing grant program.
			if (GrantStreamId == 0)
				yield return new ValidationResult("The eligibility question must be associated to an existing grant stream.", new[] { nameof(GrantStreamId) });

			// EligibilityQuestion must have a value.
			if (string.IsNullOrEmpty(EligibilityQuestion))
				yield return new ValidationResult("The Eligibility Question must be defined.", new[] { nameof(EligibilityQuestion) });

			foreach (var validation in base.Validate(validationContext))
			{
				yield return validation;
			}
		}

		public bool RequiresGrantWriter()
		{
			return EligibilityQuestion.ToLower().Contains("grant writer");
		}
	}
}
