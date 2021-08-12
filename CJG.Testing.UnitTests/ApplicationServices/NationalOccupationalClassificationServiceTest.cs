using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Infrastructure.Entities;
using CJG.Testing.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace CJG.Testing.UnitTests.ApplicationServices
{
    [TestClass]
    public class NationalOccupationalClassificationServiceTest : ServiceUnitTestBase
    {
        private Mock<ILogger> _loggerMock;
        private Mock<HttpContextBase> _httpContextMock;
        private Mock<IDataContext> _dbContextMock;
        private NationalOccupationalClassificationService _service;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger>();
            _httpContextMock = new Mock<HttpContextBase>();
            _dbContextMock = new Mock<IDataContext>();
            _dbContextMock.Setup(x => x.NationalOccupationalClassifications.Add(It.IsAny<NationalOccupationalClassification>()));
            _service = new NationalOccupationalClassificationService(_dbContextMock.Object, _httpContextMock.Object, _loggerMock.Object);
        }

        [TestMethod, TestCategory("NOC"), TestCategory("Service")]
        public void AddNationalOccupationalClassification_WhenNewNocIsPassed_ShouldAddTheNoc()
        {
            // Arrange
            var newNoc = new NationalOccupationalClassification()
            {
                Code = "Code",
                Description = "Description"
            };

            // Act
            _service.AddNationalOccupationalClassification(newNoc);

            // Assert
            _dbContextMock.Verify(x => x.NationalOccupationalClassifications.Add(It.IsAny<NationalOccupationalClassification>()), Times.Exactly(1));
            _dbContextMock.Verify(x => x.Commit(), Times.Exactly(1));
        }

        [TestMethod, TestCategory("NOC"), TestCategory("Service")]
        public void UpdateNationalOccupationalClassification_WhenModifiedNocIsPassed_ShouldUpdateTheNoc()
        {
            // Arrange
            var modifiedNoc = new NationalOccupationalClassification()
            {
                Code = "Code",
                Description = "Description"
            };
            _dbContextMock.Setup(x => x.Update(It.IsAny<NationalOccupationalClassification>()));

            // Act
            _service.UpdateNationalOccupationalClassification(modifiedNoc);

            // Assert
            _dbContextMock.Verify(x => x.Update(It.IsAny<NationalOccupationalClassification>()), Times.Exactly(1));
            _dbContextMock.Verify(x => x.Commit(), Times.Exactly(1));
        }

        [TestMethod, TestCategory("NOC"), TestCategory("Service")]
        public void GetNationalOccupationalClassification_WhenNullIDAsParameter_ShouldReturnNull()
        {
            // Act
            var results = _service.GetNationalOccupationalClassification(null);

            // Assert
            results.Should().Be(null);
        }

        [TestMethod, TestCategory("NOC"), TestCategory("Service")]
        public void GetNationalOccupationalClassification_WhenPassInNocID_ShouldReturnNocData()
        {
            // Arrange
            var noc = new NationalOccupationalClassification()
            {
                Code = "Code",
                Description = "Description"
            };
            _dbContextMock.Setup(x => x.NationalOccupationalClassifications.Find(It.IsAny<int?>())).Returns(noc);

            // Act
            var results = _service.GetNationalOccupationalClassification(1);

            // Assert
            results.Code.Should().Be("Code");
            results.Description.Should().Be("Description");
        }

        [TestMethod, TestCategory("NOC"), TestCategory("Service")]
        public void GetNationalOccupationalClassifications_WhenPassInNullID_ReturnEmptyList()
        {
            // Act
            var results = _service.GetNationalOccupationalClassifications(null);

            // Assert
            results.Count().Should().Be(0);
        }

        [TestMethod, TestCategory("NOC"), TestCategory("Service")]
        public void GetNationalOccupationalClassifications_WhenPassInNocID_ShouldReturnListOfNocData()
        {
            // Arrange
            var noc = new NationalOccupationalClassification
            {
                Code = "Code",
                Description = "Description",
                Left = 1,
                Right = 1,
                Level = 1,
                Id = 1
            };

            var helper = new ServiceHelper(typeof(NationalOccupationalClassificationService), EntityHelper.CreateExternalUser().CreateIdentity());
            helper.MockDbSet(noc);

            var service = helper.Create<NationalOccupationalClassificationService>();

            // Act
            var results = service.GetNationalOccupationalClassifications(1).ToList();

            // Assert
            results.Count.Should().BeGreaterThan(0);
            Assert.IsTrue(results.Contains(noc));
        }

        [TestMethod, TestCategory("NOC"), TestCategory("Service")]
        public void GetNationalOccupationalClassificationChildren_WhenPassInParentIDAndLevel_ShouldReturnListOfNocChildrenOrderByNocCode()
        {
            // Arrange
            var parent = new NationalOccupationalClassification
            {
                Code = "Parent",
                Description = "Description",
                ParentId = 3,
                Level = 4,
                Id = 3,
                Left = 1,
                Right = 3
            };
            var child = new NationalOccupationalClassification
            {
                Code = "Child",
                Description = "Description",
                ParentId = 3,
                Level = 4,
                Id = 1,
                Left = 4,
                Right = 2,
                Parent = parent
            };
            var nocs = new List<NationalOccupationalClassification>
            {
	            parent,
	            child
            };

            var helper = new ServiceHelper(typeof(NationalOccupationalClassificationService), EntityHelper.CreateExternalUser());
            var dbSetMock = helper.MockDbSet(nocs);
            _dbContextMock.Setup(x => x.NationalOccupationalClassifications).Returns(dbSetMock.Object);

            var service = helper.Create<NationalOccupationalClassificationService>();

            // Act
            var results = service.GetNationalOccupationalClassificationChildren(3, 4).ToList();

            // Assert
            results.Count.Should().BeGreaterThan(0);
            results[0].Code.Should().Be(child.Code);
        }
    }
}
