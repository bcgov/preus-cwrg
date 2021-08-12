using System;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.Agreements
{
	public class GrantAgreementViewModel : BaseViewModel
	{
		public int GrantApplicationId { get; set; }
		public string RowVersion { get; set; }
		public DateTime DeliveryStartDate { get; set; }
		public bool CoverLetterConfirmed { get; set; }
		public bool ScheduleAConfirmed { get; set; }
		public bool ScheduleBConfirmed { get; set; }
		public int Versions { get; set; }

		public ApplicationWorkflowViewModel ApplicationWorkflowViewModel { get; set; }

		public GrantAgreementViewModel() { }
		public GrantAgreementViewModel(GrantApplication grantApplication)
		{
			if (grantApplication == null) throw new ArgumentNullException(nameof(grantApplication));

			Id = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);
			DeliveryStartDate = grantApplication.StartDate.ToLocalTime();
			CoverLetterConfirmed = grantApplication.GrantAgreement.CoverLetterConfirmed;
			ScheduleAConfirmed = grantApplication.GrantAgreement.ScheduleAConfirmed;
			ScheduleBConfirmed = grantApplication.GrantAgreement.ScheduleBConfirmed;
			ApplicationWorkflowViewModel = new ApplicationWorkflowViewModel(grantApplication);
			Versions = grantApplication.GrantAgreement.ScheduleA.VersionNumber;
		}
	}
}
