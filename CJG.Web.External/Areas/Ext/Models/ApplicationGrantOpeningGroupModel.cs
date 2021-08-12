using CJG.Core.Entities;
using System;
using System.Collections.Generic;

namespace CJG.Web.External.Areas.Ext.Models
{
	public class ApplicationGrantOpeningGroupModel
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public List<ApplicationGrantOpeningViewModel> GrantOpenings { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public DateTime MaxEndDate
		{
			get
			{
				return EndDate.AddYears(1);
			}
		}
		public ApplicationGrantOpeningGroupModel()
		{
		}

		public ApplicationGrantOpeningGroupModel(DateTime startDate, DateTime endDate, ProgramTypes programType, List<ApplicationGrantOpeningViewModel> grantOpenings)
		{
			StartDate = startDate;
			EndDate = endDate;

			var localMorning = StartDate.ToLocalMorning();
			var localMidnight = EndDate.ToLocalMidnight();

			Title = $"For project delivery and training starting between {localMorning.ToString("MMMMM d, yyyy")} and {localMidnight.ToString("MMMMM d, yyyy")}";
			Description = $"Delivery and training must start in the period {localMorning.ToString("yyyy-MM-dd")} to {localMidnight.ToString("yyyy-MM-dd")} for the grant you have selected and your start date may not be before your application submission date.";
			GrantOpenings = grantOpenings;
		}
	}
}
