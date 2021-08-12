using System;
using System.Collections.Generic;
using System.Linq;

namespace CJG.Core.Entities
{
	/// <summary>
	/// <typeparamref name="TrainingProgramExtensions"/> static class, provides extension methods for <typeparamref name="TrainingProgram"/> objects.
	/// </summary>
	public static class TrainingProgramExtensions
	{
		/// <summary>
		/// The Training Program start date must be between the grant application start and end dates.
		/// </summary>
		/// <param name="trainingProgram"></param>
		/// <returns></returns>
		public static bool HasStartDateWithinDeliveryDates(this TrainingProgram trainingProgram)
		{
			return trainingProgram.StartDate.ToLocalTime().Date >= trainingProgram.GrantApplication.StartDate.ToLocalTime().Date
				&& trainingProgram.StartDate.ToLocalTime().Date <= trainingProgram.GrantApplication.EndDate.ToLocalTime().Date;
		}

		public static bool HasStartDateWithinExtendedDeliveryDates(this TrainingProgram trainingProgram, DateTime extendedStartDate)
		{
			var trainingStartDate = trainingProgram.StartDate.ToLocalTime().Date;
			return trainingStartDate >= trainingProgram.GrantApplication.StartDate.ToLocalTime().Date && trainingStartDate <= extendedStartDate;
		}

		/// <summary>
		/// The Training Program end date must be between the grant application start and end dates.
		/// </summary>
		/// <param name="trainingProgram"></param>
		/// <returns></returns>
		public static bool HasEndDateWithinDeliveryDates(this TrainingProgram trainingProgram)
		{
			return trainingProgram.EndDate.ToLocalTime().Date >= trainingProgram.GrantApplication.StartDate.ToLocalTime().Date
				&& trainingProgram.EndDate.ToLocalTime().Date <= trainingProgram.GrantApplication.EndDate.ToLocalTime().Date;
		}

		public static bool HasEndDateWithinExtendedDeliveryDates(this TrainingProgram trainingProgram, DateTime extendedEndDate)
		{
			var trainingEndDate = trainingProgram.EndDate.ToLocalTime().Date;
			return trainingEndDate >= trainingProgram.GrantApplication.StartDate.ToLocalTime().Date && trainingEndDate <= extendedEndDate;
		}

		/// <summary>
		/// The Training Program start date must be between the grant application start and end dates.
		/// </summary>
		/// <param name="trainingProgram"></param>
		/// <returns></returns>
		public static bool HasStartDateWithin30Days(this TrainingProgram trainingProgram)
		{
			bool isEarliestDeliveryDate = true;

			if (trainingProgram.IsSkillsTraining)
			{
				isEarliestDeliveryDate = ((trainingProgram.StartDate.ToLocalTime().Date - trainingProgram.GrantApplication.StartDate.ToLocalTime().Date).TotalDays >= 30);
			}
			return isEarliestDeliveryDate;
		}

		/// <summary>
		/// The Training Program end date must be between the grant application start and end dates.
		/// </summary>
		/// <param name="trainingProgram"></param>
		/// <returns></returns>
		public static bool HasEndDateWithin30Days(this TrainingProgram trainingProgram)
		{
			bool isEarliestDeliveryDate = true;
			if (trainingProgram.IsSkillsTraining)
			{
				isEarliestDeliveryDate = ((trainingProgram.GrantApplication.EndDate.ToLocalTime().Date - trainingProgram.EndDate.ToLocalTime().Date).TotalDays >= 30);
			}

			return isEarliestDeliveryDate;
		}
		/// <summary>
		/// Validates both the start date and end dates.
		/// </summary>
		/// <param name="trainingProgram"></param>
		/// <returns></returns>
		public static bool HasValidDates(this TrainingProgram trainingProgram)
		{
			return HasStartDateWithinDeliveryDates(trainingProgram) && HasEndDateWithinDeliveryDates(trainingProgram);
		}

		/// <summary>
		/// Get the maximum allowed Dates 
		/// </summary>
		/// <param name="trainingProgram"></param>
		/// <param name="fiscalYears"></param>
		/// <returns>Tuple of DateTime, DateTime. Item1: Maximum Allowed Start Date   Item2: Maximum Allowed End Date</returns>
		public static Tuple<DateTime, DateTime> GetMaxDates(this TrainingProgram trainingProgram, List<FiscalYear> fiscalYears)
		{
			var deliveryStart = trainingProgram.GrantApplication.StartDate.ToLocalTime().Date;
			var deliveryEnd = trainingProgram.GrantApplication.EndDate.ToLocalTime().Date;

			var fiscalYear = trainingProgram.GrantApplication.GrantOpening.TrainingPeriod.FiscalYear;

			var nextFiscalYearIndex = fiscalYears.IndexOf(fiscalYear);
			var nextFiscalYear = fiscalYears.ElementAtOrDefault(nextFiscalYearIndex + 1) ?? fiscalYear;

			var maxStartDate = deliveryStart.AddYears(1);
			if (maxStartDate > fiscalYear.EndDate.ToLocalTime().Date)
				maxStartDate = fiscalYear.EndDate.ToLocalTime().Date;

			var maxEndDate = deliveryEnd.AddYears(1);
			if (maxEndDate > nextFiscalYear.EndDate.ToLocalTime().Date)
				maxEndDate = nextFiscalYear.EndDate.ToLocalTime().Date;

			return new Tuple<DateTime, DateTime>(maxStartDate, maxEndDate);
		}

		/// <summary>
		/// Validate whether the skills training component information is complete.
		/// </summary>
		/// <param name="trainingProgram"></param>
		/// <returns></returns>
		public static bool SkillsTrainingConfirmed(this TrainingProgram trainingProgram)
		{
			return trainingProgram.EligibleCostBreakdown?.EstimatedCost > 0;
		}

		/// <summary>
		/// The Training Program start date must be within current fiscal year.
		/// </summary>
		/// <param name="trainingProgram"></param>
		/// <returns></returns>
		public static bool HasStartDateWithinCurrentFiscalYear(this TrainingProgram trainingProgram)
		{
			return trainingProgram.StartDate.ToLocalTime().Date >= AppDateTime.CurrentFYStartDateMorning.Date
				&& trainingProgram.StartDate.ToLocalTime().Date <= AppDateTime.CurrentFYEndDateMidnight.Date;
		}
	}
}
