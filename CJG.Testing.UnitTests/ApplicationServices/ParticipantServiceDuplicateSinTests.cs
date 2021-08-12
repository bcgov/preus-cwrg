using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Infrastructure.BCeID.WebService.BCeID;
using CJG.Testing.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CJG.Testing.UnitTests.ApplicationServices
{
	[TestClass]
    public class ParticipantServiceDuplicateSinTests : ServiceUnitTestBase
    {
	    private ServiceHelper _helper;
	    private ParticipantService _service;
	    private DateTime _cutoffDate;
	    private GrantProgram _defaultGrantProgram;
	    private GrantApplication _grantApplication;
	    private DateTime _currentDate;
	    private DateTime _fiscalStart;
	    private DateTime _fiscalEnd;

	    [TestInitialize]
        public void Setup()
        {
	        _helper = new ServiceHelper(typeof(ParticipantService), EntityHelper.CreateExternalUser().CreateIdentity());
	        _service = _helper.Create<ParticipantService>();

	        _cutoffDate = new DateTime(2019, 4, 1);  // SEt it much earlier than fiscal just to make sure it doesn't create issues with the results
	        _currentDate = new DateTime(2022, 8, 1);

	        _fiscalStart = new DateTime(2022, 4, 1);
	        _fiscalEnd = new DateTime(2023, 4, 1).AddSeconds(-1);

			_defaultGrantProgram = new GrantProgram
	        {
		        Id = 5,
		        ProgramCode = "CWRG"
	        };

	        _grantApplication = new GrantApplication
	        {
		        Id = 37,
		        ApplicationStateInternal = ApplicationStateInternal.New,
		        InvitationKey = Guid.NewGuid(),
		        ParticipantForms = new List<ParticipantForm>(),
		        GrantOpening = new GrantOpening
		        {
			        GrantStream = new GrantStream
			        {
				        GrantProgramId = _defaultGrantProgram.Id,
				        GrantProgram = _defaultGrantProgram
			        }
		        }
	        };

	        _helper.MockDbSet(new[] { _grantApplication });
	        _helper.MockDbSet(new[] { _defaultGrantProgram });
		}

		[TestMethod, TestCategory("Participant"), TestCategory("Service")]
		public void DuplicateSINFindsNoDuplicates()
		{
			var participantForm1 = CreateParticipantForm(57, "688-272-293", 35, _grantApplication);
			var participantForm2 = CreateParticipantForm(59, "222-333-444", 50, _grantApplication);

			_grantApplication.ParticipantForms.Add(participantForm1);
			_grantApplication.ParticipantForms.Add(participantForm2);

			_helper.MockDbSet(new[] { participantForm1, participantForm2 });

			var duplicateSinResult = _service.GetParticipantEnrollmentsWithDuplicatedSIN(_currentDate, 100, _cutoffDate, _fiscalStart, _fiscalEnd);

			Assert.AreEqual(0, duplicateSinResult.Count());
		}

		[TestMethod, TestCategory("Participant"), TestCategory("Service")]
		public void UnreportedDuplicateSINFindsTwoDuplicates()
		{
			var participantForm = CreateParticipantForm(57, "688-272-293", 35, _grantApplication);
			var participantForm2 = CreateParticipantForm(59, "688-272-293", 50, _grantApplication);

			_grantApplication.ParticipantForms.Add(participantForm);
			_grantApplication.ParticipantForms.Add(participantForm2);

			_helper.MockDbSet(new[] { participantForm, participantForm2 });

			var duplicateSinResult = _service.GetParticipantEnrollmentsWithDuplicatedSIN(_currentDate, 100, _cutoffDate, _fiscalStart, _fiscalEnd).ToList();

			Assert.AreEqual(2, duplicateSinResult.Count());  // Both PFs' should come back with references to each other

			// Grab what came back as duplicates for each PF
			var firstDuplicates = duplicateSinResult
				.Where(ds => ds.PrimaryForm.Id == 57)
				.SelectMany(d => d.DuplicatedParticipantForms)
				.ToList();
			var secondDuplicates = duplicateSinResult
				.Where(ds => ds.PrimaryForm.Id == 59)
				.SelectMany(d => d.DuplicatedParticipantForms)
				.ToList();

			// Check that both have a single duplicate
			Assert.AreEqual(1, firstDuplicates.Count);
			Assert.AreEqual(1, secondDuplicates.Count);

			Assert.AreEqual(59, firstDuplicates.First().Id);  // The first form should have the second as a duplicated item
			Assert.AreEqual(57, secondDuplicates.First().Id);  // The second form should have the first as a duplicated item
		}

		[TestMethod, TestCategory("Participant"), TestCategory("Service")]
		public void UnreportedDuplicateSINFindsSingleReportedDuplicate()
		{
			var participantForm = CreateParticipantForm(57, "688-272-293", 35, _grantApplication);
			var participantForm2 = CreateParticipantForm(59, "688-272-293", 50, _grantApplication, _currentDate.AddDays(-5)); // This PF was reported on, it should not come back as a PF with duplicates

			_grantApplication.ParticipantForms.Add(participantForm);
			_grantApplication.ParticipantForms.Add(participantForm2);

			_helper.MockDbSet(new[] { participantForm, participantForm2 });

			var duplicateSinResult = _service.GetParticipantEnrollmentsWithDuplicatedSIN(_currentDate, 100, _cutoffDate, _fiscalStart, _fiscalEnd).ToList();

			Assert.AreEqual(1, duplicateSinResult.Count);  // Both PFs' should come back with references to each other
			Assert.AreEqual(57, duplicateSinResult.First().PrimaryForm.Id);  // Both PFs' should come back with references to each other
		}

		[TestMethod, TestCategory("Participant"), TestCategory("Service")]
		public void UnreportedDuplicateSINFindsMultipleForms()
		{
			var newParticipantForm = CreateParticipantForm(1, "688-272-293", 35, _grantApplication);

			var participantForm2 = CreateParticipantForm(50, "111-222-333", 50, _grantApplication, _currentDate.AddDays(-5));
			var participantForm3 = CreateParticipantForm(51, "688-272-293", 50, _grantApplication, _currentDate.AddDays(-5));
			var participantForm4 = CreateParticipantForm(52, "688-272-293", 50, _grantApplication, _currentDate.AddDays(-5));
			var participantForm5 = CreateParticipantForm(53, "688-272-293", 50, _grantApplication, _currentDate.AddDays(-5));
			
			_grantApplication.ParticipantForms.Add(newParticipantForm);
			_grantApplication.ParticipantForms.Add(participantForm2);
			_grantApplication.ParticipantForms.Add(participantForm3);
			_grantApplication.ParticipantForms.Add(participantForm4);
			_grantApplication.ParticipantForms.Add(participantForm5);

			_helper.MockDbSet(new[] { newParticipantForm, participantForm2, participantForm3, participantForm4, participantForm5 });

			var duplicateSinResult = _service.GetParticipantEnrollmentsWithDuplicatedSIN(_currentDate, 100, _cutoffDate, _fiscalStart, _fiscalEnd).ToList();

			Assert.AreEqual(1, duplicateSinResult.Count);

			var expectedDuplicatedIds = new List<int> { 51, 52, 53 };
			Assert.IsTrue(expectedDuplicatedIds.SequenceEqual(duplicateSinResult.SelectMany(d => d.DuplicatedParticipantForms).Select(f => f.Id)));
		}

		[TestMethod, TestCategory("Participant"), TestCategory("Service")]
		public void UnreportedDuplicateSINOutsideOfFiscalReturnsNothing()
		{
			var participantForm = CreateParticipantForm(57, "688-272-293", 500, _grantApplication);
			var participantForm2 = CreateParticipantForm(59, "688-272-293", 505, _grantApplication);

			_grantApplication.ParticipantForms.Add(participantForm);
			_grantApplication.ParticipantForms.Add(participantForm2);

			_helper.MockDbSet(new[] { participantForm, participantForm2 });

			var duplicateSinResult = _service.GetParticipantEnrollmentsWithDuplicatedSIN(_currentDate, 100, _cutoffDate, _fiscalStart, _fiscalEnd).ToList();

			Assert.AreEqual(0, duplicateSinResult.Count);
		}

		private ParticipantForm CreateParticipantForm(int id, string sin, int createDateDaysOffset, GrantApplication grantApplication, DateTime? reportedDate = null)
		{
			return new ParticipantForm
			{
				Id = id,
				InvitationKey = Guid.NewGuid(),
				SIN = sin,
				DuplicateSinReportedOn = reportedDate,
				DateAdded = _currentDate.AddDays(createDateDaysOffset),
				GrantApplication = grantApplication,
				GrantApplicationId = grantApplication.Id,
			};
		}
    }
}
