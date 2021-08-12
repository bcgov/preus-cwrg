using System;

namespace CJG.Infrastructure.ReportingService
{
	interface IExitSurveyReportJob
	{
		void Start(DateTime currentDate, string csvFilePath, int daysBefore, DateTime cutoffDate, int maxParticipants = 1000, bool addHeader = false);
	}
}