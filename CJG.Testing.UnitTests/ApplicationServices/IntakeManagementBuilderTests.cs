using System;
using System.Collections.Generic;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CJG.Testing.UnitTests.ApplicationServices
{
	[TestClass]
    public class IntakeManagementBuilderTests
    {
        private readonly Mock<IStaticDataService> _staticDataServiceMock = new Mock<IStaticDataService>();
        private readonly Mock<IGrantProgramService> _grantProgramServiceMock = new Mock<IGrantProgramService>();
        private readonly Mock<IGrantStreamService> _grantStreamServiceMock = new Mock<IGrantStreamService>();
        private readonly Mock<IGrantOpeningService> _grantOpeningServiceMock = new Mock<IGrantOpeningService>();
        private readonly Mock<ITrainingPeriodService> _trainingPeriodServiceMock = new Mock<ITrainingPeriodService>();

        private IntakeManagementBuilder CreateIntakeManagementBuilder()
        {
            return new IntakeManagementBuilder(_staticDataServiceMock.Object, _grantProgramServiceMock.Object, _grantStreamServiceMock.Object, _grantOpeningServiceMock.Object, _trainingPeriodServiceMock.Object);
        }

        private void MockTrainingPeriods()
        {
            _staticDataServiceMock.Setup(x => x.GetTrainingPeriods(1)).Returns(CreateTrainingPeriods);
        }

        private static List<TrainingPeriod> CreateTrainingPeriods()
        {
            return new List<TrainingPeriod>
            {
                new TrainingPeriod {StartDate = new DateTime(2016, 1, 1), EndDate = new DateTime(2016, 1, 31), Id = 1},
                new TrainingPeriod {StartDate = new DateTime(2017, 1, 1), EndDate = new DateTime(2017, 1, 31), Id = 2},
                new TrainingPeriod {StartDate = new DateTime(2017, 2, 1), EndDate = new DateTime(2017, 2, 28), Id = 3},
                new TrainingPeriod {StartDate = new DateTime(2017, 3, 1), EndDate = new DateTime(2017, 3, 31), Id = 4},
                new TrainingPeriod {StartDate = new DateTime(2018, 1, 1), EndDate = new DateTime(2018, 3, 31), Id = 5}
            };
        }
    }
}
