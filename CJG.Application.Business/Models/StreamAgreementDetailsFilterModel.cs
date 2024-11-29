using System.Collections.Generic;

namespace CJG.Application.Business.Models
{
	public class StreamAgreementDetailsFilterModel
	{
		public StreamAgreementDetailsFilterModel()
		{
			GrantStreamIds = new List<int>();
			Intake = new List<string>();
			ApplicationStatuses = new List<int>();
			RegionNames = new List<string>();
			CourseTitles = new List<string>();
		}

		public List<int> ApplicationStatuses { get; set; }
		public List<string> Intake { get; set; }
		public string AgreementHolder { get; set; }
		public string TrainingProvider { get; set; }
		public string Keywords { get; set; }
		public List<string> CourseTitles { get; set; }
		public int? FiscalYearId { get; set; }
		public List<int> GrantStreamIds { get; set; }
		public List<string> RegionNames { get; set; }
		public int? CommunityId { get; set; }
		public int? NocId { get; set; }
		public string TrainingLocationName { get; set; }
		public string[] OrderBy { get; set; }
	}
}