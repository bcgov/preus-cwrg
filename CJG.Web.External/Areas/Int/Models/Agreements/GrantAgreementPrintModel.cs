using System.Linq;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.Agreements
{
	public class GrantAgreementPrintModel : BaseViewModel
	{
		public int GrantApplicationId { get; set; }

		public string CoverLetterBody { get; set; }
		public string ScheduleABody { get; set; }
		public string ScheduleBBody { get; set; }

		public GrantAgreementPrintModel()
		{
		}

		public GrantAgreementPrintModel(GrantApplication grantApplication, int version = 1)
		{
			GrantApplicationId = grantApplication.Id;

			var coverLetter = grantApplication.GrantAgreement.CoverLetter.Versions.Any()
				? grantApplication.GrantAgreement.CoverLetter.Versions.FirstOrDefault(o => o.VersionNumber == version)?.Body ?? grantApplication.GrantAgreement.CoverLetter.Body
				: grantApplication.GrantAgreement.CoverLetter.Body;

			var scheduleA = grantApplication.GrantAgreement.ScheduleA.Versions.Any()
				? grantApplication.GrantAgreement.ScheduleA.Versions.FirstOrDefault(o => o.VersionNumber == version)?.Body ?? grantApplication.GrantAgreement.ScheduleA.Body
				: grantApplication.GrantAgreement.ScheduleA.Body;
			scheduleA = scheduleA.Replace("::RequestChangeTrainingDates::", "").Replace("::RequestChangeTrainingProvider::", "");

			var scheduleB = grantApplication.GrantAgreement.ScheduleB.Versions.Any()
				? grantApplication.GrantAgreement.ScheduleB.Versions.FirstOrDefault(o => o.VersionNumber == version)?.Body ?? grantApplication.GrantAgreement.ScheduleB.Body
				: grantApplication.GrantAgreement.ScheduleB.Body;

			CoverLetterBody = coverLetter;
			ScheduleABody = scheduleA;
			ScheduleBBody = scheduleB;
		}
	}
}