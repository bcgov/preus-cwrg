using System;
using System.Linq.Expressions;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Ext.Models.Agreements
{
    public class GrantAgreementDocumentViewModel : BaseViewModel
	{
		public string RowVersion { get; set; }

		public string Body { get; set; }

		public bool Confirmation { get; set; }

		public GrantAgreementDocumentViewModel() { }
		public GrantAgreementDocumentViewModel(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			Id = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);
		}

		public GrantAgreementDocumentViewModel(GrantApplication grantApplication, string documentName) : this(grantApplication)
		{
			switch (documentName)
			{
				case "CoverLetter":
					Body = grantApplication.GrantAgreement?.CoverLetter?.Body;
					Confirmation = grantApplication.GrantAgreement.CoverLetterConfirmed;
					break;
				case "ScheduleA":
					Body = grantApplication.GrantAgreement?.ScheduleA?.Body;
					Confirmation = grantApplication.GrantAgreement.ScheduleAConfirmed;

					// either replace the placeholders with the view or an empty string
					Body = Body.Replace("::RequestChangeTrainingProvider::", "");
					Body = Body.Replace("::RequestChangeTrainingDates::", "");
					break;
				case "ScheduleB":
					Body = grantApplication.GrantAgreement?.ScheduleB?.Body;
					Confirmation = grantApplication.GrantAgreement.ScheduleBConfirmed;
					break;
			}
		}

		public GrantAgreementDocumentViewModel(GrantApplication grantApplication, Expression<Func<GrantAgreement, Document>> expression) : this(grantApplication)
		{
			var document = expression.Compile().Invoke(grantApplication.GrantAgreement);
			Body = document.Body;
		}
	}
}