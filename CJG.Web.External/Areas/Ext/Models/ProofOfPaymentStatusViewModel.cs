using System.ComponentModel;
using CJG.Core.Entities;

namespace CJG.Web.External.Areas.Ext.Models
{
	public class ProofOfPaymentStatusViewModel
	{
		public string LabelClass { get; set; }
		public string StatusText { get; set; }

		public ProofOfPaymentReportStatus ProofOfPaymentStatus
		{
			set
			{
				switch(value)
				{
					case ProofOfPaymentReportStatus.NotStarted:
					case ProofOfPaymentReportStatus.Incomplete:
					case ProofOfPaymentReportStatus.Complete:
						LabelClass = value.ToString().ToLower();
						break;
					case ProofOfPaymentReportStatus.Submitted:
						LabelClass = "complete";
						break;
				}

				StatusText = value.GetDescription().ToUpper();
			}
		}
	}

	public enum ProofOfPaymentReportStatus
	{
		[Description("Not Started")]
		NotStarted = 0,
		[Description("Incomplete")]
		Incomplete = 1,
		[Description("Complete")]
		Complete = 2,
		[Description("Submitted")]
		Submitted = 3
	}
}