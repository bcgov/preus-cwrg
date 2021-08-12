using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CJG.Application.Business.Models.Survey.Job;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using NLog;

namespace CJG.Infrastructure.ReportingService
{
	internal class ExitSurveyReportJob : IExitSurveyReportJob
	{
		private readonly ILogger _logger;
		private readonly IParticipantService _participantService;
		private readonly ISurveyService _surveyService;

		public ExitSurveyReportJob(IParticipantService participantService, ISurveyService surveyService, ILogger logger)
		{
			_participantService = participantService;
			_surveyService = surveyService;
			_logger = logger;
		}

		public void Start(DateTime currentDate, string csvFilePath, int daysBefore, DateTime cutoffDate, int maxParticipants = 1000, bool addHeader = false)
		{
			_logger.Info($"Exit Survey: Retrieving participants... [current date: {currentDate.ToStringLocalTime()}] [daysBefore: {daysBefore}] [maxParticipants: {maxParticipants}]");

			var exitSurveyData = _surveyService.GetParticipantsWithExitSurveyAnswersToReport(currentDate.AddDays(-daysBefore), maxParticipants, cutoffDate);

			_logger.Info("Converting and formatting data...");

			var header = addHeader ? CreateCsvHeaderString(exitSurveyData.Questions) : null;
			var placeholders = exitSurveyData.Participants.Select(s => s).Select(p => ConvertParticipantFormToPlaceholders(exitSurveyData.Questions, p)).ToList();

			_logger.Info("Exporting data into csv file...");
			var participantCount = ExportToCsvFile(placeholders.Select(ConvertPlaceholdersToCsvString), csvFilePath, header);
			_logger.Info($"Exported {participantCount} participant records into csv file: {csvFilePath}");

			_participantService.UpdateExitSurveyReportedDate(exitSurveyData.Participants, currentDate);

			_logger.Info($"All {exitSurveyData.Participants.Count} exported participant records marked with date {currentDate.ToUniversalTime()}");
		}

		private string CreateCsvHeaderString(List<KeyValuePair<int, string>> questions)
		{
			var headerBuilder = new CsvLineBuilder()
				.AppendColumn("Agreement Number")
				.AppendColumn("Agreement Holder Name")
				.AppendColumn("First Name")
				.AppendColumn("LastName")
				.AppendColumn("Participant Email")
				.AppendColumn("Training Start")
				.AppendColumn("Training End")
				.AppendColumn("Date of Exit");


			foreach (var question in questions)
				headerBuilder.AppendColumn(question.Value);

			return headerBuilder
				.ToString();
		}

		private static IDictionary<string, string> ConvertParticipantFormToPlaceholders(List<KeyValuePair<int, string>> questions, SurveyParticipantModel exitSurvey)
		{
			var placeholders = new Dictionary<string, string>
			{
				{ "AgreementNumber", exitSurvey.AgreementNumber },
				{ "AgreementHolder", exitSurvey.AgreementHolder },
				{ "FirstName", exitSurvey.FirstName },
				{ "LastName", exitSurvey.LastName },
				{ "Email", exitSurvey.Email },
				{ "TrainingStart", exitSurvey.TrainingStartDate.ToDateOnly() },
				{ "TrainingEnd", exitSurvey.TrainingEndDate.ToDateOnly() },
				{ "DateOfExit", exitSurvey.ExitDate.ToDateOnly() },
			};

			foreach (var question in questions)
			{
				var answerList = exitSurvey.Answers.FirstOrDefault(a => a.Key == question.Key);

				// We have to add answers for any answers not present in the list of questions (ie: new or removed questions)
				var answer = string.Empty;
				if (answerList.Value != null && answerList.Value.Any())
					answer = string.Join(", ", answerList.Value);

				placeholders.Add(question.Key.ToString(), answer);
			}

			return placeholders;
		}

		private static string ConvertPlaceholdersToCsvString(IDictionary<string, string> placeholders)
		{
			var builder = new CsvLineBuilder();

			foreach (var placeholder in placeholders)
				builder.AppendColumn(placeholder.Value);

			return builder.ToString();
		}

		private static int ExportToCsvFile(IEnumerable<string> lines, string filePath, string headerLine = null)
		{
			var index = 0;
			var path = new FileInfo(filePath).DirectoryName;

			// Create the directory if doesn't exist.
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			using (var w = new StreamWriter(filePath))
			{
				if (!string.IsNullOrWhiteSpace(headerLine))
					w.WriteLine(headerLine);

				foreach (var line in lines)
				{
					w.WriteLine(line);
					w.Flush();
					index++;
				}
			}

			return index;
		}
	}
}
