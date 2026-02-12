using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CJG.Core.Entities;
using CJG.Web.External.Areas.Part.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CJG.Testing.UnitTests.Models
{
	[TestClass]
	public class ParticipantInfoStepModelValidatorTests
	{
		private ParticipantInfoStep4ViewModel _model;

		[TestInitialize]
		public void SetUp()
		{
			_model = new ParticipantInfoStep4ViewModel();
		}

		[DataTestMethod]
		[DataRow(1, true)]
		[DataRow(2, false)]
		[DataRow(3, false)]
		[DataRow(4, true)]
		[DataRow(5, false)]
		[DataRow(6, false)]
		public void HaveYouEverBeenEmployedShouldBeRequired(int employmentStatus, bool required)
		{
			_model.EmploymentStatus = employmentStatus;

			var results = ValidateModel(_model);
			var errorToLookFor = "The Have you ever been employed field is required.";

			Assert.AreEqual(required, results.Any(x => x.ErrorMessage == errorToLookFor));
		}

		[DataTestMethod]
		[DataRow(1, false)]
		[DataRow(2, true)]
		[DataRow(3, true)]
		[DataRow(4, false)]
		[DataRow(5, false)]
		[DataRow(6, true)]
		public void MultipleEmploymentPositionsShouldBeRequired(int employmentStatus, bool required)
		{
			_model.EmploymentStatus = employmentStatus;

			var results = ValidateModel(_model);
			var errorToLookFor = "The Multiple Employment Positions field is required.";

			Assert.AreEqual(required, results.Any(x => x.ErrorMessage == errorToLookFor));
		}

		[DataTestMethod]
		[DataRow(1, true)]
		[DataRow(2, false)]
		[DataRow(3, false)]
		[DataRow(4, true)]
		[DataRow(5, false)]
		[DataRow(6, false)]
		public void LastWorkedDateShouldBeRequired(int employmentStatus, bool required)
		{
			_model.EmploymentStatus = employmentStatus;
			_model.PreviousEmploymentLastDayOfWork = null;
			_model.HaveYouEverBeenEmployed = true;
			
			var results = ValidateModel(_model);
			var errorToLookFor = "The End date of most recent employment is required.";

			Assert.AreEqual(required, results.Any(x => x.ErrorMessage == errorToLookFor));
		}

		[DataTestMethod]
		[DataRow(-5, false)]
		[DataRow(5, true)]
		public void LastWorkedCannotBeBeforeToday(int dayOffset, bool hasError)
		{
			var currentDate = AppDateTime.UtcNow;

			_model.EmploymentStatus = 1;
			_model.PreviousEmploymentLastDayOfWork = currentDate.AddDays(dayOffset);
			_model.HaveYouEverBeenEmployed = true;

			var results = ValidateModel(_model);
			var errorToLookFor = "The End date of most recent employment cannot be greater than today.";

			Assert.AreEqual(hasError, results.Any(x => x.ErrorMessage == errorToLookFor));
		}

		[DataTestMethod]
		[DataRow(1, true)]
		[DataRow(2, false)]
		[DataRow(3, false)]
		[DataRow(4, true)]
		public void PreviousAverageWageShouldBeRequired(int employmentStatus, bool required)
		{
			_model.EmploymentStatus = employmentStatus;
			_model.PreviousHourlyWage = null;
			_model.HaveYouEverBeenEmployed = true;

			var results = ValidateModel(_model);
			var errorToLookFor = "The Previous Hourly Wage field is required.";

			Assert.AreEqual(required, results.Any(x => x.ErrorMessage == errorToLookFor));
		}

		[DataTestMethod]
		[DataRow(-50, true)]
		[DataRow(0, false)]
		[DataRow(50, false)]
		public void PreviousAverageWageShouldBeNoZero(int wage, bool hasError)
		{
			_model.EmploymentStatus = 1; // Required state
			_model.PreviousHourlyWage = wage;
			_model.HaveYouEverBeenEmployed = true;

			var results = ValidateModel(_model);
			var errorToLookFor = "The Previous Hourly Wage field must be greater than or equal to 0.";

			Assert.AreEqual(hasError, results.Any(x => x.ErrorMessage == errorToLookFor));
		}

		[DataTestMethod]
		[DataRow(1, true)]
		[DataRow(2, false)]
		[DataRow(3, false)]
		[DataRow(4, true)]
		public void PreviousAverageHoursShouldBeRequired(int employmentStatus, bool required)
		{
			_model.EmploymentStatus = employmentStatus;
			_model.PreviousAvgHoursPerWeek = null;
			_model.HaveYouEverBeenEmployed = true;

			var results = ValidateModel(_model);
			var errorToLookFor = "The Previous Average Hour field is required.";

			Assert.AreEqual(required, results.Any(x => x.ErrorMessage == errorToLookFor));
		}

		[DataTestMethod]
		[DataRow(-5, true)]
		[DataRow(0, false)]
		[DataRow(50, false)]
		[DataRow(120, false)]
		[DataRow(168, false)]
		[DataRow(170, true)]
		public void PreviousAverageHoursShouldBeWithinRange(int rate, bool showError)
		{
			_model.EmploymentStatus = 1;
			_model.PreviousAvgHoursPerWeek = rate;
			_model.HaveYouEverBeenEmployed = true;

			var results = ValidateModel(_model);
			var errorToLookFor = "The previous average hours per week must be within 0 to 168.";

			Assert.AreEqual(showError, results.Any(x => x.ErrorMessage == errorToLookFor));
		}

		[DataTestMethod]
		[DataRow(1, true)]
		[DataRow(2, false)]
		[DataRow(3, false)]
		[DataRow(4, true)]
		[DataRow(5, false)]
		[DataRow(6, false)]
		public void LastPreviousEmployerNameShouldBeRequired(int employmentStatus, bool required)
		{
			_model.EmploymentStatus = employmentStatus;
			_model.PreviousEmployerFullName = null;
			_model.HaveYouEverBeenEmployed = true;

			var results = ValidateModel(_model);
			var errorToLookFor = "The business name of your most recent previous employer is required.";

			Assert.AreEqual(required, results.Any(x => x.ErrorMessage == errorToLookFor));
		}

		public IList<ValidationResult> ValidateModel(object model)
		{
			var results = new List<ValidationResult>();
			var validationContext = new ValidationContext(model, null, null);

			Validator.TryValidateObject(model, validationContext, results, true);

			if (model is IValidatableObject validatableModel)
				results.AddRange(validatableModel.Validate(validationContext));

			return results;
		}
	}
}