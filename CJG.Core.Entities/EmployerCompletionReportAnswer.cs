using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
    /// <summary>
    /// EmployerCompletionReportAnswer class, provides a way to capture the employer answers to a completion report.
    /// </summary>
    public class EmployerCompletionReportAnswer : EntityBase
    {
	    private bool _validationEnabled = true;

	    /// <summary>
        /// get/set - The primary key and foreign key to the grant application.
        /// </summary>
        [Key, Column(Order = 0)]
        public int GrantApplicationId { get; set; }

        /// <summary>
        /// get/set - The grant application to which this answer belongs.
        /// </summary>
        [ForeignKey(nameof(GrantApplicationId))]
        public virtual GrantApplication GrantApplication { get; set; }

        /// <summary>
        /// get/set - The primary key and foreign key to the completion report question.
        /// </summary>
        [Key, Column(Order = 1)]
        public int QuestionId { get; set; }

        /// <summary>
        /// get/set - The completion report question to which this answer belongs.
        /// </summary>
        [ForeignKey(nameof(QuestionId))]
        public virtual CompletionReportQuestion Question { get; set; }

        /// <summary>
        /// get/set - The foreign key to the completion report option given as the answer.
        /// </summary>
        public int? AnswerId { get; set; }

        /// <summary>
        /// get/set - The completion report option given as the answer.
        /// </summary>
        [ForeignKey(nameof(AnswerId))]
        public virtual CompletionReportOption Answer { get; set; }

        /// <summary>
        /// get/set - The free-format text answer when "other" was the option chosen.
        /// </summary>
        [MaxLength(2000, ErrorMessage = "Maximum length of text fields is 2000 characters")]
        public string OtherAnswer { get; set; }


		public void DisableRequiredValidation()
		{
			_validationEnabled = false;
		}

        /// <summary>
        /// Creates a new instance of an EmployerCompletionReportAnswer object.
        /// </summary>
        public EmployerCompletionReportAnswer()
        {
        }

		/// <summary>
		/// Creates a new instance of an EmployerCompletionReportAnswer object and initializes it.
		/// </summary>
		/// <param name="grantApplication"></param>
		/// <param name="question"></param>
		/// <param name="answer"></param>
		public EmployerCompletionReportAnswer(GrantApplication grantApplication, CompletionReportQuestion question, CompletionReportOption answer)
        {
            GrantApplication = grantApplication ?? throw new ArgumentNullException(nameof(grantApplication));
            GrantApplicationId = grantApplication.Id;
            Question = question ?? throw new ArgumentNullException(nameof(question));
            QuestionId = question.Id;
            Answer = answer ?? throw new ArgumentNullException(nameof(answer));
            AnswerId = answer.Id;
        }

		/// <summary>
		/// Creates a new instance of an EmployerCompletionReportAnswer object and initializes it.
		/// </summary>
		/// <param name="grantApplication"></param>
		/// <param name="question"></param>
		/// <param name="answer"></param>
		public EmployerCompletionReportAnswer(GrantApplication grantApplication, CompletionReportQuestion question, string answer)
        {
            if (string.IsNullOrWhiteSpace(answer))
                throw new ArgumentException("The answer is required.", nameof(answer));

            GrantApplication = grantApplication ?? throw new ArgumentNullException(nameof(grantApplication));
            GrantApplicationId = grantApplication.Id;
            Question = question ?? throw new ArgumentNullException(nameof(question));
            QuestionId = question.Id;
            OtherAnswer = answer;
        }

        /// <summary>
        /// Validates the EmployerCompletionReportAnswer property values.
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
			// An answer must be provided when not doing a 'save for later' on step 4.
			if (_validationEnabled)
				if (AnswerId == null && string.IsNullOrWhiteSpace(OtherAnswer))
					yield return new ValidationResult("An answer must be entered.", new[] { nameof(Answer) });

			foreach (var validation in base.Validate(validationContext))
	            yield return validation;
        }
    }
}
