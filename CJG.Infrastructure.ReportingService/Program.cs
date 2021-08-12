using System;
using CJG.Infrastructure.ReportingService.Properties;
using Fclp;
using NLog;

namespace CJG.Infrastructure.ReportingService
{
	internal class Program
    {
        private static int Main(string[] args)
        {
            var resultCode = SystemExitCode.FatalError;

            ILogger logger = null;
            Options options = null;

            try
            {
                options = ParseArgs(args);

                var appFactory = new AppFactory();
                logger = appFactory.GetLogger();

				CreateSection25Report(appFactory, options);
				CreateTemporaryResidentReport(appFactory, options);
				CreateDuplicateSinReport(appFactory, options);
				CreateExitSurveyReport(appFactory, options);
				CreateWithdrawalSurveyReport(appFactory, options);

				resultCode = SystemExitCode.Success;
				
			}
            catch (Exception e)
            {
                logger?.Fatal(e);
                Console.WriteLine(e);
            }

            if (options?.PauseBeforeExit ?? false)
            {
                Console.WriteLine("Press any key to exit");
                Console.Read();
            }

			return (int)resultCode;
        }

        private static void CreateSection25Report(AppFactory appFactory, Options options)
        {
			// Get the participants that are on EI
	        var job = appFactory.GetSection25ReportJob();
	        job.Start(options.CurrentDate,
		        string.Format(Settings.Default.CsvFilePathTemplate, options.CurrentDate),
		        string.Format(Settings.Default.HtmlFilePathTemplate, options.CurrentDate),
		        Settings.Default.NumDaysBefore,
		        $"{AppDomain.CurrentDomain.BaseDirectory}\\Templates\\SDSI-Report-Template.html",
		        Settings.Default.ReportCutoffDate,
		        Settings.Default.MaxParticipants,
		        Settings.Default.CsvAddReportHeader);
        }

        private static void CreateTemporaryResidentReport(AppFactory appFactory, Options options)
        {
	        // Get the participants that have are Temporary Residents (have a SIN that starts with 9)
	        var sinJob = appFactory.GetTemporaryResidentReportJob();
	        sinJob.Start(options.CurrentDate,
		        string.Format(Settings.Default.CsvFilePathTemplateSin, options.CurrentDate),
		        string.Format(Settings.Default.HtmlFilePathTemplateSin, options.CurrentDate),
		        Settings.Default.NumDaysBefore,
		        $"{AppDomain.CurrentDomain.BaseDirectory}\\Templates\\SIN-Report-Template.html",
		        Settings.Default.ReportCutoffDate,
		        Settings.Default.MaxParticipants,
		        Settings.Default.CsvAddReportHeader);
        }

        private static void CreateDuplicateSinReport(AppFactory appFactory, Options options)
        {
	        var sinJob = appFactory.GetDuplicateSinReportJob();
	        sinJob.Start(options.CurrentDate,
		        string.Format(Settings.Default.CsvFilePathTemplateDuplicateSin, options.CurrentDate),
		        string.Format(Settings.Default.HtmlFilePathTemplateDuplicateSin, options.CurrentDate),
		        Settings.Default.NumDaysBefore,
		        $"{AppDomain.CurrentDomain.BaseDirectory}\\Templates\\DuplicateSIN-Report-Template.html",
		        Settings.Default.ReportCutoffDate,
		        Settings.Default.MaxParticipants,
		        Settings.Default.CsvAddReportHeader);
        }

        private static void CreateExitSurveyReport(AppFactory appFactory, Options options)
        {
	        var exitJob = appFactory.GetExitSurveyReportJob();
	        exitJob.Start(options.CurrentDate,
		        string.Format(Settings.Default.CsvFilePathTemplateExitSurvey, options.CurrentDate),
		        Settings.Default.NumDaysBefore,
		        Settings.Default.ReportCutoffDate,
		        Settings.Default.MaxParticipants,
		        Settings.Default.CsvAddReportHeader);
        }

        private static void CreateWithdrawalSurveyReport(AppFactory appFactory, Options options)
        {
	        var withdrawalJob = appFactory.GetWithdrawalSurveyReportJob();
	        withdrawalJob.Start(options.CurrentDate,
		        string.Format(Settings.Default.CsvFilePathTemplateWithdrawalSurvey, options.CurrentDate),
		        Settings.Default.NumDaysBefore,
		        Settings.Default.ReportCutoffDate,
		        Settings.Default.MaxParticipants,
		        Settings.Default.CsvAddReportHeader);
        }

		private static Options ParseArgs(string[] args)
        {
            var options = new Options();

            var p = new FluentCommandLineParser();

            p.Setup<bool>('p', "pause")
                .Callback(pause => options.PauseBeforeExit = pause);

            p.Setup<DateTime>('d', "date")
                .SetDefault(DateTime.UtcNow)
                .Callback(date => options.CurrentDate = date);

            var parseResult = p.Parse(args);

            if (parseResult.HasErrors)
                throw new ApplicationException("Can't parse command line arguments: " + parseResult.ErrorText);

            return options;
        }
    }

    internal enum SystemExitCode
    {
        Success = 0,
        FatalError = 1
    }

    public class Options
    {
        public bool PauseBeforeExit { get; set; }
        public DateTime CurrentDate { get; set; }
    }
}
