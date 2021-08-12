using System;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Entities;
using CJG.Testing.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CJG.Testing.UnitTests.ApplicationServices
{
	[TestClass]
    public class GrantAgreementServiceTests : ServiceUnitTestBase
    {
	    private User _applicationAdministrator;
	    private ServiceHelper _helper;
	    private GrantAgreementService _service;
	    private GrantOpening _grantOpening;

	    [TestInitialize]
        public void Setup()
        {
	        _applicationAdministrator = EntityHelper.CreateExternalUser();
	        _helper = new ServiceHelper(typeof(GrantAgreementService), _applicationAdministrator);
	        _service = _helper.Create<GrantAgreementService>();

	        _grantOpening = EntityHelper.CreateGrantOpening();

	        AppDateTime.ResetNow();
	    }

		[TestMethod, TestCategory("Grant Agreement"), TestCategory("Service")]
        public void AcceptGrantAgreement_WithAgreementId_UpdatesApplicationToAgreementAccepted()
        {
	        // Arrange
            var grantApplication = EntityHelper.CreateGrantApplication(_grantOpening, _applicationAdministrator, ApplicationStateInternal.OfferIssued);

            var trainingProvider = new TrainingProvider(grantApplication);
            var trainingProgram = new TrainingProgram(grantApplication, trainingProvider) { StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(3) };
            grantApplication.TrainingProviders.Add(trainingProvider);
            grantApplication.TrainingPrograms.Add(trainingProgram);

            grantApplication.GrantAgreement = new GrantAgreement(grantApplication);
			
			_helper.MockDbSet(grantApplication);

			AppDateTime.SetNow(DateTime.UtcNow.AddDays(2));

			var agreementDate = AppDateTime.UtcNow;

			// Act
			_service.AcceptGrantAgreement(grantApplication);

            // Assert
            grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.AgreementAccepted);
            grantApplication.GrantAgreement.DateAccepted.Should().Be(agreementDate);
		}

		[TestMethod, TestCategory("Grant Agreement"), TestCategory("Service")]
        public void AcceptGrantAgreement_WithLaterAgreementDate_ChangesStartDate()
        {
	        // Arrange
            var grantApplication = EntityHelper.CreateGrantApplication(_grantOpening, _applicationAdministrator, ApplicationStateInternal.OfferIssued);

            var trainingProvider = new TrainingProvider(grantApplication);
            var trainingProgram = new TrainingProgram(grantApplication, trainingProvider) { StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(3) };
            grantApplication.TrainingProviders.Add(trainingProvider);
            grantApplication.TrainingPrograms.Add(trainingProgram);

            grantApplication.GrantAgreement = new GrantAgreement(grantApplication);

			_helper.MockDbSet(grantApplication);

			AppDateTime.SetNow(DateTime.UtcNow.AddDays(2));

			var agreementDate = AppDateTime.UtcNow;

			// Act
			_service.AcceptGrantAgreement(grantApplication);

            // Assert
            grantApplication.GrantAgreement.DateAccepted.Should().Be(agreementDate);
            grantApplication.GrantAgreement.StartDate.Should().Be(agreementDate);

            grantApplication.StartDate.Should().BeSameDateAs(agreementDate);
		}

		[TestMethod, TestCategory("Grant Agreement"), TestCategory("Service")]
        public void AcceptGrantAgreement_WithEarlierAgreementDate_LeavesStartDate()
        {
	        // Arrange
            var grantApplication = EntityHelper.CreateGrantApplication(_grantOpening, _applicationAdministrator, ApplicationStateInternal.OfferIssued);

            var trainingProvider = new TrainingProvider(grantApplication);
            var startDate = DateTime.UtcNow;
            var trainingProgram = new TrainingProgram(grantApplication, trainingProvider) { StartDate = startDate, EndDate = DateTime.UtcNow.AddMonths(3) };
            grantApplication.TrainingProviders.Add(trainingProvider);
            grantApplication.TrainingPrograms.Add(trainingProgram);

            grantApplication.GrantAgreement = new GrantAgreement(grantApplication);

			_helper.MockDbSet(grantApplication);

			AppDateTime.SetNow(startDate.AddDays(-15));

			var agreementDate = AppDateTime.UtcNow;

			// Act
			_service.AcceptGrantAgreement(grantApplication);

            // Assert
            grantApplication.GrantAgreement.DateAccepted.Should().Be(agreementDate);
            grantApplication.GrantAgreement.StartDate.Should().BeSameDateAs(startDate);
		}

        [TestMethod, TestCategory("Grant Agreement"), TestCategory("Service")]
        public void CancelGrantAgreement_WithAgreementAccepted_UpdatesApplicationToCancelledByAgreementHolder()
        {
			// Arrange
			var grantApplication = EntityHelper.CreateGrantApplication(_grantOpening, _applicationAdministrator, ApplicationStateInternal.AgreementAccepted);

            _helper.MockDbSet(grantApplication);

			// Act
            _service.CancelGrantAgreement(grantApplication, "reason");

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.CancelledByAgreementHolder);
        }

        [TestMethod, TestCategory("Grant Agreement"), TestCategory("Service")]
        public void RejectGrantAgreement_WithOfferIssued_UpdatesApplicationToAgreementRejected()
        {
			// Arrange
			_helper.MockContext();
            var grantApplication = EntityHelper.CreateGrantApplication(_grantOpening, _applicationAdministrator, ApplicationStateInternal.OfferIssued);

            _helper.MockDbSet(grantApplication);

			// Act
            _service.RejectGrantAgreement(grantApplication, "reason");

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.AgreementRejected);
        }

        [TestMethod, TestCategory("Grant Agreement"), TestCategory("Service")]
        public void AddGrantAgreement_WithAgreementAccepted_UpdatesApplicationToCancelledByAgreementHolder()
        {
			// Arrange
			var dbSetMock = _helper.MockDbSet<GrantAgreement>();

            var grantApplication = EntityHelper.CreateGrantApplication(_grantOpening, _applicationAdministrator, ApplicationStateInternal.New);

			// Act
            _service.Add(new GrantAgreement { GrantApplication = grantApplication });

			// Assert
            dbSetMock.Verify(x=>x.Add(It.IsAny<GrantAgreement>()), Times.Exactly(1));
            _helper.GetMock<IDataContext>().Verify(x=>x.Commit(), Times.Exactly(1));
        }
    }
}
