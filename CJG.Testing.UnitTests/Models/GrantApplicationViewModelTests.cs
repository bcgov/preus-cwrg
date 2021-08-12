using System;
using System.Collections.Generic;
using CJG.Core.Entities;
using CJG.Web.External.Areas.Int.Models.WorkQueue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CJG.Testing.UnitTests.Models
{
	[TestClass]
	public class GrantApplicationViewModelTests
	{
		private GrantApplication _grantApplication;
		private GrantApplicationViewModel _model;
		private DateTime _trainingStartDate;

		[TestInitialize]
		public void Setup()
		{
			_model = new GrantApplicationViewModel();
			_trainingStartDate = new DateTime(2023, 6, 12, 12, 00, 00, DateTimeKind.Local);

			_grantApplication = new GrantApplication
			{
				ApplicationStateInternal = ApplicationStateInternal.New,
				TrainingCost = new TrainingCost
				{
					EstimatedParticipants = 2
				},
				TrainingPrograms = new List<TrainingProgram>
				{
					new TrainingProgram
					{
						StartDate = _trainingStartDate
					}
				},
				ParticipantForms = new List<ParticipantForm>()
			};
		}

		[TestMethod]
		public void PIFWarning_StateCheck_BypassesWarning()
		{
			AppDateTime.SetNow(new DateTime(2023, 6, 14).ToUniversalTime());
			Assert.AreEqual(false, _model.ShowPifWarning(_grantApplication));
		}

		[TestMethod]
		public void PIFWarning_StateCheck_ShowsWarning()
		{
			AppDateTime.SetNow(new DateTime(2023, 6, 14).ToUniversalTime());
			_grantApplication.ApplicationStateInternal = ApplicationStateInternal.AgreementAccepted;

			Assert.AreEqual(true, _model.ShowPifWarning(_grantApplication));
		}

		[TestMethod]
		public void PIFWarning_DateRangeChecks()
		{
			_grantApplication.ApplicationStateInternal = ApplicationStateInternal.AgreementAccepted;

			AppDateTime.SetNow(_trainingStartDate.AddDays(-1));
			Assert.AreEqual(false, _model.ShowPifWarning(_grantApplication), "Pre warning period");

			AppDateTime.SetNow(_trainingStartDate);
			Assert.AreEqual(true, _model.ShowPifWarning(_grantApplication), "On start of warning period");

			AppDateTime.SetNow(_trainingStartDate.AddDays(5));
			Assert.AreEqual(true, _model.ShowPifWarning(_grantApplication), "During warning period");

			AppDateTime.SetNow(_trainingStartDate.AddDays(9));
			Assert.AreEqual(false, _model.ShowPifWarning(_grantApplication), "After warning period");
		}

		[TestMethod]
		public void PIFWarning_RequiredPIFs()
		{
			_grantApplication.ApplicationStateInternal = ApplicationStateInternal.AgreementAccepted;
			AppDateTime.SetNow(_trainingStartDate.AddDays(5));

			Assert.AreEqual(true, _model.ShowPifWarning(_grantApplication), "Not enough PIFs");

			_grantApplication.ParticipantForms.Add(new ParticipantForm());
			Assert.AreEqual(true, _model.ShowPifWarning(_grantApplication), "Still not enough PIFs");

			_grantApplication.ParticipantForms.Add(new ParticipantForm());
			Assert.AreEqual(false, _model.ShowPifWarning(_grantApplication), "Required PIFs met");
		}
	}
}