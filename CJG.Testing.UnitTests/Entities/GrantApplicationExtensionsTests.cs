﻿using System;
using CJG.Core.Entities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CJG.Testing.UnitTests.Entities
{
    [TestClass]
    public class GrantApplicationExtensionsTests
    {
        [TestMethod]
        public void CopyApplicant_WithNewPhysicalAndNewMailingAddress_CopiesBothAddresses()
        {
            var app = new GrantApplication();
            var user = new User
            {
                PhysicalAddress = new Address {AddressLine1 = "a1"},
                PhysicalAddressId = 1,
                MailingAddress = new Address {AddressLine1 = "a2"},
                MailingAddressId = 2
            };

            app.CopyApplicant(user);

            app.ApplicantPhysicalAddress.AddressLine1.Should().Be("a1");
            app.ApplicantMailingAddress.AddressLine1.Should().Be("a2");
        }

        [TestMethod]
        public void CopyApplicant_WithNewPhysicalAndSameMailingAddress_CopiesAddressesAndSetReference()
        {
            var app = new GrantApplication();
            var physicalAddress = new Address {AddressLine1 = "a1"};
            var user = new User
            {
                PhysicalAddress = physicalAddress,
                PhysicalAddressId = 1,
                MailingAddress = physicalAddress,
                MailingAddressId = 1
            };

            app.CopyApplicant(user);

            app.ApplicantPhysicalAddress.AddressLine1.Should().Be("a1");
            app.ApplicantMailingAddress.AddressLine1.Should().Be("a1");
        }

        [TestMethod]
        public void CopyApplicant_WithExistPhysicalAndSameMailingAddress_CopiesAddressesAndSetReference()
        {
            var appPhysicalAddress = new ApplicationAddress {AddressLine1 = "a1"};

            var physicalAddress = new Address {AddressLine1 = "b1"};

            var app = new GrantApplication
            {
                ApplicantPhysicalAddress = new ApplicationAddress(),
                ApplicantPhysicalAddressId = 1,
                ApplicantMailingAddress = appPhysicalAddress,
                ApplicantMailingAddressId = 1
            };

            var user = new User
            {
                PhysicalAddress = physicalAddress,
                PhysicalAddressId = 1,
                MailingAddress = physicalAddress,
                MailingAddressId = 1
            };

            app.CopyApplicant(user);

            app.ApplicantPhysicalAddress.AddressLine1.Should().Be("b1");
            app.ApplicantMailingAddress.AddressLine1.Should().Be("b1");
        }

        [TestMethod]
        public void
            CopyApplicant_WithExistPhysicalMailingAddressAndUserHasDifferentAddresses_CopiesAddressesAndCreatesNewMailing
            ()
        {
            var appPhysicalAddress = new ApplicationAddress {AddressLine1 = "a1"};

            var physicalAddress = new Address {AddressLine1 = "b1"};

            var app = new GrantApplication
            {
                ApplicantPhysicalAddress = new ApplicationAddress(),
                ApplicantPhysicalAddressId = 1,
                ApplicantMailingAddress = appPhysicalAddress,
                ApplicantMailingAddressId = 1
            };

            var user = new User
            {
                PhysicalAddress = physicalAddress,
                PhysicalAddressId = 1,
                MailingAddress = new Address {AddressLine1 = "b2"},
                MailingAddressId = 2
            };

            app.CopyApplicant(user);

            app.ApplicantPhysicalAddress.AddressLine1.Should().Be("b1");
            app.ApplicantMailingAddress.AddressLine1.Should().Be("b2");
        }

        [TestMethod]
        public void FiscalCheckWithApplicationNumber_PreviousFiscal()
        {
			var application = new GrantApplication
			{
				FileNumber = "2312345",
				GrantOpening = new GrantOpening
				{
					TrainingPeriod = new TrainingPeriod
					{
						FiscalYear = new FiscalYear
						{
							StartDate = new DateTime(2023, 4, 1),
							EndDate = new DateTime(2024, 4, 1),
						}
					}
				}
			};

			var result = application.IsFileNumberWithinFiscalYear();

			result.Should().BeFalse("File number starts with 23");
        }

        [TestMethod]
        public void FiscalCheckWithApplicationNumber_CurrentFiscal()
        {
			var application = new GrantApplication
			{
				FileNumber = "2412345",
				GrantOpening = new GrantOpening
				{
					TrainingPeriod = new TrainingPeriod
					{
						FiscalYear = new FiscalYear
						{
							StartDate = new DateTime(2023, 4, 1),
							EndDate = new DateTime(2024, 4, 1),
						}
					}
				}
			};

			var result = application.IsFileNumberWithinFiscalYear();

			result.Should().BeTrue("File number starts with 24");
        }

        [TestMethod]
        public void GetFurthestEndDateIsAccurate()
        {
			var application = new GrantApplication();
			var trainingProgramEarlier = new TrainingProgram
			{
				Id = 37,
				StartDate = new DateTime(2025, 5, 1),
				EndDate = new DateTime(2025, 8, 1)
			};

			var trainingProgramLater = new TrainingProgram
			{
				Id = 57,
				StartDate = new DateTime(2025, 5, 1),
				EndDate = new DateTime(2025, 10, 1)
			};

			application.TrainingPrograms.Add(trainingProgramEarlier);
			application.TrainingPrograms.Add(trainingProgramLater);

			var endDate = application.GetFurthestTrainingEndDate();

			endDate.Should().Be(trainingProgramLater.EndDate);
        }
    }
}