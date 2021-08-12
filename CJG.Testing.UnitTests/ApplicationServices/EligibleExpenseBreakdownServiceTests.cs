using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Infrastructure.Entities;
using CJG.Infrastructure.Identity;
using CJG.Testing.Core;
using FluentAssertions;
using Microsoft.AspNet.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using CJG.Application.Services.Exceptions;

namespace CJG.Testing.UnitTests.ApplicationServices
{
	[TestClass]
    public class EligibleExpenseBreakdownServiceTests
    {
	    private IPrincipal identity;
	    private ServiceHelper helper;

	    [TestInitialize]
	    public void Setup()
	    {
		    identity = EntityHelper.CreateInternalUser().CreateIdentity("Assessor");
		    helper = new ServiceHelper(typeof(EligibleExpenseBreakdownService), identity);
	    }

		[TestMethod]
        [TestCategory("Eligible Expense Breakdown"), TestCategory("Service")]
        public void GetAllEligibleExpenseBreakdowns_WithEligibleExpenseTypeId()
        {
            // Arrange
            var expenseBreakDownTypes = new List<EligibleExpenseBreakdown>()
            {
                new EligibleExpenseBreakdown(){ Id = 1, EligibleExpenseTypeId = 1},
                new EligibleExpenseBreakdown(){Id = 2, EligibleExpenseTypeId = 1}
            };
            var service = helper.Create<EligibleExpenseBreakdownService>();
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            helper.SetMock(new Mock<ApplicationUserManager>(userStore.Object));
			var dbSetMock = helper.MockDbSet(expenseBreakDownTypes);
			var dbContextMock = helper.GetMock<IDataContext>();
			dbContextMock.Setup(x => x.EligibleExpenseBreakdowns.AsNoTracking()).Returns(dbSetMock.Object);

			// Act
			var results = service.GetAllForEligibleExpenseType(1);

            // Assert
            results.Count().Should().Be(2);
        }

        [TestMethod]
        [TestCategory("Eligible Expense Breakdown"), TestCategory("Service")]
        public void GetAllActiveEligibleExpenseBreakdowns_WithEligibleExpenseTypeId()
        {
            // Arrange
            var expenseBreakDownTypes = new List<EligibleExpenseBreakdown>()
            {
                new EligibleExpenseBreakdown(){ Id = 1, EligibleExpenseTypeId= 1, IsActive = true},
                new EligibleExpenseBreakdown(){Id =2, EligibleExpenseTypeId= 1, IsActive = false}
            };
            var service = helper.Create<EligibleExpenseBreakdownService>();
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            helper.SetMock(new Mock<ApplicationUserManager>(userStore.Object));
            var dbSetMock = helper.MockDbSet(expenseBreakDownTypes);
			var dbContextMock = helper.GetMock<IDataContext>();
			dbContextMock.Setup(x => x.EligibleExpenseBreakdowns.AsNoTracking()).Returns(dbSetMock.Object);

			// Act
			var results = service.GetAllActiveForEligibleExpenseType(1);

            // Assert
            results.Count().Should().Be(1);
        }

        [TestMethod]
        [TestCategory("Eligible Expense Breakdown"), TestCategory("Service")]
        public void GetAllEligibleExpenseBreakdowns_WithId()
        {
            // Arrange
            var expenseBreakDownTypes = new List<EligibleExpenseBreakdown>()
            {
                new EligibleExpenseBreakdown(){ Id = 1, EligibleExpenseTypeId= 1, Description = "Expense Breakdown 1"},
                new EligibleExpenseBreakdown(){Id =2, EligibleExpenseTypeId= 1,Description = "Expense Breakdown 2"}
            };
            var service = helper.Create<EligibleExpenseBreakdownService>();
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            helper.SetMock(new Mock<ApplicationUserManager>(userStore.Object));
            helper.MockDbSet(expenseBreakDownTypes);

            // Act
            var results = service.Get(1);

            // Assert
            results.Description.Should().Be("Expense Breakdown 1");
        }

        /// <summary>
        /// GetEligibleExpenseBreakdown with non-existing Id should throw exception.
        /// </summary>
        [TestMethod]
        [TestCategory("Eligible Expense Breakdown"), TestCategory("Service")]
        public void GetAllEligibleExpenseBreakdowns_WithNonExistId_ThrowsException()
        {
            // Arrange
            var expenseBreakDownTypes = new List<EligibleExpenseBreakdown>()
            {
                new EligibleExpenseBreakdown(){ Id = 1, EligibleExpenseTypeId= 1, Description = "Expense Breakdown 1"},
                new EligibleExpenseBreakdown(){Id =2, EligibleExpenseTypeId= 1,Description = "Expense Breakdown 2"}
            };

            var service = helper.Create<EligibleExpenseBreakdownService>();
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            helper.SetMock(new Mock<ApplicationUserManager>(userStore.Object));
            helper.MockDbSet(expenseBreakDownTypes);

            // Act
            Action actionToTest = () => service.Get(3);

            // Assert
            actionToTest.Should().Throw<NoContentException>();
        }

        [TestMethod]
        [TestCategory("Eligible Expense Breakdown"), TestCategory("Service")]
        public void UpdateEligibleExpenseBreakdown()
        {
            // Arrange
            var expenseBreakDownTypes = new List<EligibleExpenseBreakdown>()
            {
                new EligibleExpenseBreakdown(){ Id = 1, EligibleExpenseTypeId= 1, Description = "Expense Breakdown 1"},
            };

            var service = helper.Create<EligibleExpenseBreakdownService>();
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            helper.SetMock(new Mock<ApplicationUserManager>(userStore.Object));
            helper.MockDbSet(expenseBreakDownTypes);

            var expenseBreakDownTypeToUpdate = expenseBreakDownTypes.First();
            expenseBreakDownTypeToUpdate.Description = "Expense Updated";
            // Act
            var results = service.Update(expenseBreakDownTypeToUpdate);

            // Assert
            results.Description.Should().Be("Expense Updated");
        }

        [TestMethod]
        [TestCategory("Eligible Expense Breakdown"), TestCategory("Service")]
        public void AddEligibleExpenseBreakdown()
        {
            // Arrange
            var expenseBreakDownTypes = new List<EligibleExpenseBreakdown>();
            var service = helper.Create<EligibleExpenseBreakdownService>();
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            helper.SetMock(new Mock<ApplicationUserManager>(userStore.Object));
            var dbSetMock = helper.MockDbSet(expenseBreakDownTypes);
            var EligibleExpenseBreakdownToAdd = new EligibleExpenseBreakdown()
            {
                Id = 1,
                Description = "Expense Breakdown 1"
            };

            // Act
            service.Add(EligibleExpenseBreakdownToAdd);

			// Assert
			dbSetMock.Verify(x => x.Add(It.Is<EligibleExpenseBreakdown>(eb => eb.DateAdded.Date == AppDateTime.UtcNow.Date)));

        }

        [TestMethod]
        [TestCategory("Eligible Expense Breakdown"), TestCategory("Service")]
        public void DeleteEligibleExpenseBreakdown()
        {
            // Arrange
            var eligibleExpenseType = new EligibleExpenseType() { };
			var expenseBreakDownTypes = new List<EligibleExpenseBreakdown>()
			{
				new EligibleExpenseBreakdown()
				{ Id = 1,
				  Description = "Expense Breakdown 1",
				  ExpenseType = eligibleExpenseType

				}
			};
            var service = helper.Create<EligibleExpenseBreakdownService>();
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            helper.SetMock(new Mock<ApplicationUserManager>(userStore.Object));
            var dbSetMock = helper.MockDbSet(expenseBreakDownTypes);
            var dbContextMock = helper.GetMock<IDataContext>();
			var expenseBreakDownTypeToDelete = expenseBreakDownTypes.First();

            // Act
            service.Delete(expenseBreakDownTypeToDelete);

			// Assert
			dbSetMock.Verify(x => x.Remove(It.IsAny<EligibleExpenseBreakdown>()), Times.Exactly(1));
            dbContextMock.Verify(x => x.Commit(), Times.Exactly(1));
        }

        /// <summary>
        /// DeleteEligibleExpenseBreakdown with a an affiliated ClaimbreakDownCost should throw an error.
        /// </summary>
        [TestMethod]
        [TestCategory("Eligible Expense Breakdown"), TestCategory("Service")]
        public void DeleteEligibleExpenseBreakdown_WithClaim_ThrowsException()
        {
            // Arrange
            var eligibleExpenseType = new EligibleExpenseType() { };
            var breakdownCosts = new List<ClaimBreakdownCost>()
            {
                new ClaimBreakdownCost(){ClaimEligibleCost = new ClaimEligibleCost()}
            };

            var expenseBreakDownTypes = new List<EligibleExpenseBreakdown>()
            {
                new EligibleExpenseBreakdown()
                { Id = 1,
                  Description = "Expense Breakdown 1",
                  ClaimBreakdownCosts = breakdownCosts,
                  ExpenseType = eligibleExpenseType

                }
            };

            var service = helper.Create<EligibleExpenseBreakdownService>();
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            helper.SetMock(new Mock<ApplicationUserManager>(userStore.Object));
            helper.MockDbSet(expenseBreakDownTypes);
            var unitOfWorkMock = helper.GetMock<IDataContext>();
            var expenseBreakDownTypeToDelete = expenseBreakDownTypes.First();

            // Act
            Action actionToTest = () => service.Delete(expenseBreakDownTypeToDelete);

            // Assert
            actionToTest.Should().Throw<InvalidOperationException>().Where(x => x.Message.Contains("This breakdown is used in existing grant file and can’t be deleted"));
        }

        /// <summary>
        /// DeleteEligibleExpenseBreakdown with a an affiliated EligibleCost should throw an exception.
        /// </summary>
        [TestMethod]
        [TestCategory("Eligible Expense Breakdown"), TestCategory("Service")]
        public void DeleteEligibleExpenseBreakdown_WithEligibleCosts_ThrowsEcxception()
        {
            // Arrange
            var eligibleCosts = new List<EligibleCost>()
            {
                new EligibleCost(){Id = 1}
            };
            var eligibleExpenseType = new EligibleExpenseType() { EligibleCosts = eligibleCosts};
            var breakdownCosts = new List<ClaimBreakdownCost>()
            {
                new ClaimBreakdownCost(){ClaimEligibleCost = new ClaimEligibleCost()}
            };
            var expenseBreakDownTypes = new List<EligibleExpenseBreakdown>()
            {
                new EligibleExpenseBreakdown()
                { Id = 1,
                  Description = "Expense Breakdown 1",
                  ClaimBreakdownCosts = breakdownCosts,
                  ExpenseType = eligibleExpenseType
                }
            };
            var service = helper.Create<EligibleExpenseBreakdownService>();
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            helper.SetMock(new Mock<ApplicationUserManager>(userStore.Object));
            helper.MockDbSet(expenseBreakDownTypes);
            var unitOfWorkMock = helper.GetMock<IDataContext>();
            var expenseBreakDownTypeToDelete = expenseBreakDownTypes.First();

            // Act
            Action actionToTest = () => service.Delete(expenseBreakDownTypeToDelete);

            // Assert
            actionToTest.Should().Throw<InvalidOperationException>().Where(x => x.Message.Contains("This breakdown is used in existing grant file and can’t be deleted"));
        }

        [TestMethod]
        [TestCategory("Eligible Expense Breakdown"), TestCategory("Service")]
        public void GetOccupationalSkillsTrainingBreakdown()
        {
	        // Arrange
	        var expenseBreakDownTypes = new List<EligibleExpenseBreakdown>
	        {
		        new EligibleExpenseBreakdown { Id = 3, EligibleExpenseTypeId = 1, Description = "Fictional Training"},
		        new EligibleExpenseBreakdown { Id = 1, EligibleExpenseTypeId = 1, Caption = "Occupational Skills Training" },
		        new EligibleExpenseBreakdown { Id = 2, EligibleExpenseTypeId = 2, Caption = "Occupational Skills Training" }
	        };

	        var service = helper.Create<EligibleExpenseBreakdownService>();
	        var userStore = new Mock<IUserStore<ApplicationUser>>();

	        helper.SetMock(new Mock<ApplicationUserManager>(userStore.Object));
	        helper.MockDbSet(expenseBreakDownTypes);

	        // Act
	        var results = service.GetForOccupationalSkillsTraining(1);

	        // Assert
	        results.Id.Should().Be(1);
	        results.Caption.Should().Be("Occupational Skills Training");
        }
	}
}
