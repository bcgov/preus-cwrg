using System;

namespace CJG.Infrastructure.ReportingService
{
	interface ISinReportJob
	{
		void Start(DateTime currentDate,
			string csvFilePath,
			string htmlFilePathTemplate,
			int daysBefore,
			string templatePath,
			DateTime cutoffDate,
			int maxParticipants = 1000,
			bool addHeader = false);
	}
}
