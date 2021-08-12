using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.GrantStreams
{
	public class GrantStreamQuestionViewModel : BaseViewModel
	{
		public int GrantStreamId { get; set; }
		public string EligibilityRequirements { get; set; }
		[Required(ErrorMessage = "Eligibility question is required."), Display(Name = "Eligibility Question")]
		public string EligibilityQuestion { get; set; }
		public bool IsActive { get; set; }
		public bool EligibilityPositiveAnswerRequired { get; set; }
		public int RowSequence { get; set; }
		// Answer - returned when asking user when creating Grant Application
		public bool? EligibilityAnswer { get; set; }
		public bool CollectContactInformation { get; set; }

		public List<KeyValuePair<int, string>> GrantWriterDesignations = new List<KeyValuePair<int, string>>();

		public bool RequiresGrantWriter { get; set; }
		public int? Designation { get; set; }
		public string ContactName { get; set; }
		public string ContactEmailAddress { get; set; }
		public string ContactPhoneNumber { get; set; }

		public string RowVersion { get; set; }

		public GrantStreamQuestionViewModel() { }

		public GrantStreamQuestionViewModel(GrantStreamEligibilityQuestion grantQuestion)
		{
			if (grantQuestion == null)
				throw new ArgumentNullException(nameof(grantQuestion));

			Utilities.MapProperties(grantQuestion, this);
			RequiresGrantWriter = grantQuestion.RequiresGrantWriter();

			if (RequiresGrantWriter)
			{
				var designations = new List<GrantWriterDesignation>
				{
					GrantWriterDesignation.ProfessionalGrantWriter,
					GrantWriterDesignation.NonProfessionalGrantWriter,
					GrantWriterDesignation.TrainingServiceProvider
				};

				GrantWriterDesignations = designations
					.Select(d => new KeyValuePair<int, string>((int) d, d.GetDescription()))
					.ToList();
			}

			RowVersion = Convert.ToBase64String(grantQuestion.RowVersion);
		}
	}
}
