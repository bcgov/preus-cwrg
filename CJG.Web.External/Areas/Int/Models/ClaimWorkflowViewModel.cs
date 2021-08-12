using System;
using CJG.Core.Entities;

namespace CJG.Web.External.Areas.Int.Models
{
	public class ClaimWorkflowViewModel
	{
		public int Id { get; set; }
		public int ClaimVersion { get; set; }
		public string RowVersion { get; set; }
		public string EligibilityAssessmentNotes { get; set; }
		public string ReimbursementAssessmentNotes { get; set; }
		public string ClaimAssessmentNotes { get; set; }

		public ClaimWorkflowViewModel()
		{
		}

		public ClaimWorkflowViewModel(Claim claim)
		{
			if (claim == null)
				throw new ArgumentNullException(nameof(claim));

			Id = claim.Id;
			ClaimVersion = claim.ClaimVersion;
			RowVersion = Convert.ToBase64String(claim.RowVersion);
			EligibilityAssessmentNotes = claim.EligibilityAssessmentNotes;
			ReimbursementAssessmentNotes = claim.ReimbursementAssessmentNotes;
			ClaimAssessmentNotes = claim.ClaimAssessmentNotes;
		}
	}
}