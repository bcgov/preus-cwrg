﻿using System;
using System.Linq;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.Agreements
{
	public class GrantAgreementScheduleAViewModel : BaseViewModel
	{
		public int GrantApplicationId { get; set; }
		public string Body { get; set; }

		public GrantAgreementScheduleAViewModel() { }
		public GrantAgreementScheduleAViewModel(GrantApplication grantApplication, int? version)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			GrantApplicationId = grantApplication.Id;
			var body = version != null && grantApplication.GrantAgreement.ScheduleA.Versions.Any()
				? grantApplication.GrantAgreement.ScheduleA.Versions.FirstOrDefault(o => o.VersionNumber == version)?.Body ?? grantApplication.GrantAgreement.ScheduleA.Body
				: grantApplication.GrantAgreement.ScheduleA.Body;
			Body = body.Replace("::RequestChangeTrainingDates::", "").Replace("::RequestChangeTrainingProvider::", "");
		}
	}
}
