using System.ComponentModel;
using CJG.Core.Entities;

namespace CJG.Web.External.Areas.Ext.Models
{
	public class AttestationViewModel
	{
		public string LabelClass { get; set; }
		public string StatusText { get; set; }

		public AttestationReportStatus AttestationReportStatus
		{
			set
			{
				switch(value)
				{
					case AttestationReportStatus.NotStarted:
					case AttestationReportStatus.Incomplete:
					case AttestationReportStatus.Complete:
						LabelClass = value.ToString().ToLower();
						break;
					case AttestationReportStatus.Submitted:
						LabelClass = "complete";
						break;
				}

				StatusText = value.GetDescription().ToUpper();
			}
		}
	}

	public enum AttestationReportStatus
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