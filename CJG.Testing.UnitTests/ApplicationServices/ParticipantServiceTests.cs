using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Infrastructure.Entities;
using CJG.Testing.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CJG.Testing.UnitTests.ApplicationServices
{
	[TestClass]
    public class ParticipantServiceTests : ServiceUnitTestBase
    {
	    private User _user;
	    private ServiceHelper _helper;
	    private ParticipantService _service;

	    [TestInitialize]
        public void Setup()
        {
	        _user = EntityHelper.CreateExternalUser();
	        _helper = new ServiceHelper(typeof(ParticipantService), _user.CreateIdentity());
	        _service = _helper.Create<ParticipantService>();
		}

		[TestMethod, TestCategory("Participant"), TestCategory("Service")]
        public void AddParticipantConst_WithNewParticipantCost_AddsNewParticipantCostToRepository()
        {
			// Arrange
            _helper.MockDbSet<ParticipantCost>();

			// Act
			_service.Add(new ParticipantCost());

			// Assert
            _helper.GetMock<IDataContext>().Verify(x => x.Commit(), Times.Once);
        }

        [TestMethod, TestCategory("Participant"), TestCategory("Service")]
        public void AddParticipantForm_WithNewParticipantForm_AddsNewParticipantFormToRepository()
        {
			// Arrange
            var invitationKey = Guid.NewGuid();
            var participantForm = new ParticipantForm()
            {
                InvitationKey = invitationKey
            };
            var grantApplication = new GrantApplication()
            {
                Id = 1,
                ApplicationStateInternal = ApplicationStateInternal.AgreementAccepted,
                InvitationKey = invitationKey,
                ParticipantForms = new List<ParticipantForm>()
                {
                    participantForm
                }
            };
            participantForm.GrantApplicationId = grantApplication.Id;
            
            var trainingProgram = new TrainingProgram(grantApplication);
            grantApplication.TrainingPrograms.Add(trainingProgram);
            _helper.MockDbSet<ParticipantCost>();
            _helper.MockDbSet<ParticipantForm>();
            _helper.MockDbSet( new[] { grantApplication} );

			// Act
            _service.Add(grantApplication.ParticipantForms.First());

			// Assert
            _helper.GetMock<IDataContext>().Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Adding a participantForm with a Claim should return a ParticipantForm
        /// </summary>
        [TestMethod, TestCategory("Participant"), TestCategory("Service")]
        public void AddParticipantForm_WithClaim_ShouldReturnParticipantForm()
        {
            // Arrange
            var invitationKey = Guid.NewGuid();

            var grantApplication = EntityHelper.CreateGrantApplicationWithAgreement(EntityHelper.CreateGrantOpening(), _user, EntityHelper.CreateInternalUser(), ApplicationStateInternal.AgreementAccepted);
            grantApplication.InvitationKey = invitationKey;
            var claim = EntityHelper.CreateClaim(grantApplication);
            claim.EligibleCosts.Add(new ClaimEligibleCost(claim)
            {
                EligibleExpenseType = EntityHelper.CreateEligibleExpenseType()
            });
            grantApplication.Claims.Add(claim);
            var participantForm = new ParticipantForm(grantApplication, invitationKey);
            grantApplication.ParticipantForms.Add(participantForm);

			_helper.MockDbSet<ParticipantForm>();
			_helper.MockDbSet( new[] { grantApplication });

            // Act
            var participantFormResult = _service.Add(participantForm);

            // Assert
            Assert.IsInstanceOfType(participantFormResult, typeof(ParticipantForm));
            _helper.GetMock<IDataContext>().Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Adding a null participantForm should throw an ArgumentNullException.
        /// </summary>
        [TestMethod, TestCategory("Participant"), TestCategory("Service")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddParticipantFormNullShouldThrowArgumentNullException()
        {
            // Arrange
            ParticipantForm participantForm = null;

            // Act
            _service.Add(participantForm);

            // Assert (Handled by decorator)
        }

        /// <summary>
        /// Adding a null participantCost should throw an ArgumentNullException.
        /// </summary>
        [TestMethod, TestCategory("Participant"), TestCategory("Service")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddParticipantCostNullShouldThrowArgumentNullException()
        {
            // Arrange
            ParticipantCost participantCost = null;

            // Act
            _service.Add(participantCost);
            // Assert (Handled by decorator)
        }

        /// <summary>
        /// Updating a null participantCost should throw an ArgumentNullException.
        /// </summary>
        [TestMethod, TestCategory("Participant"), TestCategory("Service")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateParticipantCostNullShouldThrowArgumentNullException()
        {
            // Arrange
            ParticipantCost participantCost = null;

            // Act
            _service.Update(participantCost);

            // Assert (Handled by decorator)
        }

        [TestMethod, TestCategory("Participant"), TestCategory("Service")]
        public void UpdateParticipantCost_WithParticipantCost_UpdatesParticipantCostInRepository()
        {
			// Arrange
            _helper.MockDbSet<ParticipantCost>();

			// Act
            _service.Update(new ParticipantCost());

			// Assert
			_helper.GetMock<IDataContext>().Verify(x => x.Update(It.IsAny<ParticipantCost>()), Times.Once);
            _helper.GetMock<IDataContext>().Verify(x => x.Commit(), Times.Once);
        }

        [TestMethod, TestCategory("Participant"), TestCategory("Service")]
		[DataRow(-30, 1)]
		[DataRow(-7, 1)]
		[DataRow(0, 0)]
		[DataRow(4, 0)]
		[DataRow(6, 0)]
		[DataRow(7, 0)]
		[DataRow(30, 0)]
        public void EiCheckParticipantsAreCorrectlyReturned(int trainingDaysToOffset, int expectedResults)
        {
	        var currentDate = AppDateTime.UtcNow;
	        var offsetStartDate = currentDate.AddDays(trainingDaysToOffset);

	        var grantApplication = EntityHelper.CreateGrantApplicationWithAgreement(EntityHelper.CreateGrantOpening(), _user, EntityHelper.CreateInternalUser(), ApplicationStateInternal.AgreementAccepted);
	        grantApplication.DateAdded = offsetStartDate;
	        grantApplication.StartDate = offsetStartDate;

	        grantApplication.TrainingPrograms.Add(new TrainingProgram
			{
				StartDate = offsetStartDate
			});

	        var participantForm = new ParticipantForm(grantApplication, Guid.NewGuid())
	        {
		        PreviousEmploymentNoc = new NationalOccupationalClassification("12345", "Test NOC", 5, 0, 0, 0),
		        PreviousEmploymentNaics = new NaIndustryClassificationSystem("12345", "Test NAICS", 5, 0, 0, 0, 2021),
		        AffectedByTariffs = true,
		        EiEligibilityReportedOn = null,
		        DateAdded = offsetStartDate
			};

	        grantApplication.ParticipantForms.Add(participantForm);

	        _helper.MockDbSet(new[] { participantForm });
			_helper.MockDbSet(new[] { grantApplication });

            var results = _service.GetParticipantsEnrollmentsForEiCheck(currentDate, 1000, new DateTime(2026, 4, 1));

            Assert.AreEqual(expectedResults, results.Count());
        }

        [TestMethod, TestCategory("Participant"), TestCategory("Service")]
        public void EiCheckParticipantsIgnoresReportedOn()
        {
	        var trainingDaysToOffset = 7;
			var currentDate = AppDateTime.UtcNow;
	        var offsetStartDate = currentDate.AddDays(trainingDaysToOffset);

	        var grantApplication = EntityHelper.CreateGrantApplicationWithAgreement(EntityHelper.CreateGrantOpening(), _user, EntityHelper.CreateInternalUser(), ApplicationStateInternal.AgreementAccepted);
	        grantApplication.DateAdded = offsetStartDate;
	        grantApplication.StartDate = offsetStartDate;

	        grantApplication.TrainingPrograms.Add(new TrainingProgram
			{
				StartDate = offsetStartDate
			});

	        var participantForm1 = new ParticipantForm(grantApplication, Guid.NewGuid())
	        {
		        PreviousEmploymentNoc = new NationalOccupationalClassification("12345", "Test NOC", 5, 0, 0, 0),
		        PreviousEmploymentNaics = new NaIndustryClassificationSystem("12345", "Test NAICS", 5, 0, 0, 0, 2021),
		        AffectedByTariffs = true,
		        EiEligibilityReportedOn = null,
		        DateAdded = offsetStartDate,
				FirstName = "Fred"
			};

	        var participantForm2 = new ParticipantForm(grantApplication, Guid.NewGuid())
	        {
		        PreviousEmploymentNoc = new NationalOccupationalClassification("12345", "Test NOC", 5, 0, 0, 0),
		        PreviousEmploymentNaics = new NaIndustryClassificationSystem("12345", "Test NAICS", 5, 0, 0, 0, 2021),
		        AffectedByTariffs = true,
		        EiEligibilityReportedOn = currentDate.AddDays(-2),
		        DateAdded = offsetStartDate,
		        FirstName = "Bob"
			};

	        grantApplication.ParticipantForms.Add(participantForm1);
	        grantApplication.ParticipantForms.Add(participantForm2);

	        _helper.MockDbSet(new[] { participantForm1, participantForm2 });
			_helper.MockDbSet(new[] { grantApplication });

            var results = _service.GetParticipantsEnrollmentsForEiCheck(currentDate, 1000, new DateTime(2026, 4, 1))
	            .ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(true, results.All(r => r.FirstName == "Fred"));
        }
    }
}
