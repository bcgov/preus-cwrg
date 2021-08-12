using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CJG.Application.Services;
using CJG.Application.Services.Exceptions;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Testing.Core;
using CJG.Web.External.Areas.Int.Controllers;
using CJG.Web.External.Areas.Int.Models;
using CJG.Web.External.Models.Shared;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using static CJG.Testing.Core.ServiceHelper;

namespace CJG.Testing.UnitTests.Controllers.Int
{
	[TestClass]
	public class ApplicantControllerTests
	{
		#region GetApplicant
		[TestMethod, TestCategory("Controller"), TestCategory(nameof(ApplicantController))]
		public void GetApplicant()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ControllerHelper<ApplicantController>(user, Roles.Assessor);
			var controller = helper.Create();

			var grantApplication = EntityHelper.CreateGrantApplicationWithAgreement(user, ApplicationStateInternal.AgreementAccepted);
			helper.GetMock<IGrantApplicationService>().Setup(m => m.Get(It.IsAny<int>())).Returns(grantApplication);

			var naIndustryClassificationSystems = new [] { EntityHelper.CreateNaIndustryClassificationSystem() };
			helper.GetMock<INaIndustryClassificationSystemService>().Setup(m => m.GetNaIndustryClassificationSystems(It.IsAny<int?>())).Returns(naIndustryClassificationSystems);

			// Act
			var result = controller.GetApplicant(grantApplication.Id);

			// Assert
			result.Should().NotBeNull().And.BeOfType<JsonResult>();
			result.Data.Should().NotBeNull().And.BeOfType<ApplicantViewModel>();
			var data = result.Data as ApplicantViewModel;
			data.Id.Should().Be(grantApplication.Id);
			helper.GetMock<IGrantApplicationService>().Verify(m => m.Get(It.IsAny<int>()), Times.Once);
			helper.GetMock<INaIndustryClassificationSystemService>().Verify(m => m.GetNaIndustryClassificationSystems(It.IsAny<int?>()), Times.Once);
		}

		[TestMethod, TestCategory("Controller"), TestCategory(nameof(ApplicantController))]
		public void GetApplicant_Exception()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ControllerHelper<ApplicantController>(user, Roles.Assessor);

			helper.GetMock<IGrantApplicationService>().Setup(m => m.Get(It.IsAny<int>())).Throws<NoContentException>();

			var controller = helper.Create();

			// Act
			var result = controller.GetApplicant(1);

			// Assert
			result.Should().NotBeNull().And.BeOfType<JsonResult>();
			controller.HttpContext.Response.StatusCode.Should().Be(400);
		}
		#endregion

		#region GetCommunities
		[TestMethod, TestCategory("Controller"), TestCategory(nameof(ApplicantController))]
		public void GetCommunities()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ControllerHelper<ApplicantController>(user, Roles.Assessor);
			var controller = helper.Create();

			var communities = new[] { EntityHelper.CreateCommunity("test") };
			helper.GetMock<ICommunityService>().Setup(m => m.GetAll()).Returns(communities);

			// Act
			var result = controller.GetCommunities();

			// Assert
			result.Should().NotBeNull().And.BeOfType<JsonResult>();
			result.Data.Should().NotBeNull().And.BeOfType<KeyValuePair<int, string>[]>();
			var data = result.Data as IEnumerable<KeyValuePair<int, string>>;
			data.Count().Should().Be(1);
			data.First().Key.Should().Be(1);
			helper.GetMock<ICommunityService>().Verify(m => m.GetAll(), Times.Once);
		}

		[TestMethod, TestCategory("Controller"), TestCategory(nameof(ApplicantController))]
		public void GetCommunities_Exception()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ControllerHelper<ApplicantController>(user, Roles.Assessor);

			helper.GetMock<ICommunityService>().Setup(m => m.GetAll()).Throws<NoContentException>();

			var controller = helper.Create();

			// Act
			var result = controller.GetCommunities();

			// Assert
			result.Should().NotBeNull().And.BeOfType<JsonResult>();
			controller.HttpContext.Response.StatusCode.Should().Be(400);
		}
		#endregion

		#region GetNAICS
		[TestMethod, TestCategory("Controller"), TestCategory(nameof(ApplicantController))]
		public void GetNAICS()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ControllerHelper<ApplicantController>(user, Roles.Assessor);
			var controller = helper.Create();

			var naics = new[] { EntityHelper.CreateNaIndustryClassificationSystem(1, "test", 1) };
			helper.GetMock<INaIndustryClassificationSystemService>().Setup(m => m.GetNaIndustryClassificationSystemChildren(It.IsAny<int>(), It.IsAny<int>())).Returns(naics);

			// Act
			var result = controller.GetNAICS(1, null);

			// Assert
			result.Should().NotBeNull().And.BeOfType<JsonResult>();
			var data = result.Data as IEnumerable<object>;
			var item = data.First();
			item.GetReflectedProperty("Key").Should().Be(1);
			item.GetReflectedProperty("Code").Should().Be("test");
			helper.GetMock<INaIndustryClassificationSystemService>().Verify(m => m.GetNaIndustryClassificationSystemChildren(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
		}

		[TestMethod, TestCategory("Controller"), TestCategory(nameof(ApplicantController))]
		public void GetNAICS_Exception()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ControllerHelper<ApplicantController>(user, Roles.Assessor);

			helper.GetMock<INaIndustryClassificationSystemService>().Setup(m => m.GetNaIndustryClassificationSystemChildren(It.IsAny<int>(), It.IsAny<int>())).Throws<NoContentException>();

			var controller = helper.Create();

			// Act
			var result = controller.GetNAICS(1, null);

			// Assert
			result.Should().NotBeNull().And.BeOfType<JsonResult>();
			controller.HttpContext.Response.StatusCode.Should().Be(400);
		}
		#endregion

		#region UpdateApplicant
		[TestMethod, TestCategory("Controller"), TestCategory(nameof(ApplicantController))]
		public void UpdateApplicant()
		{
			var user = EntityHelper.CreateExternalUser();
			var helper = new ControllerHelper<ApplicantController>(user);
			var grantApplication = EntityHelper.CreateGrantApplication(user);

			helper.GetMock<IGrantApplicationService>().Setup(m => m.Get(It.IsAny<int>())).Returns(grantApplication);
			helper.GetMock<IStaticDataService>().Setup(m => m.GetCountry(It.IsAny<string>())).Returns(EntityHelper.CreateCountry("CA"));
			helper.GetMock<IStaticDataService>().Setup(m => m.GetRegion(It.IsAny<string>(), It.IsAny<string>())).Returns(EntityHelper.CreateRegion("Victoria"));
			var mockNaIndustryClassificationSystemService = helper.GetMock<INaIndustryClassificationSystemService>();
			var viewModel = new ApplicantViewModel(grantApplication, mockNaIndustryClassificationSystemService.Object)
			{
				NAICSLevel1Id = grantApplication.NAICSId
			};
			var controller = helper.Create();

			// Act
			var result = controller.UpdateApplicant(viewModel);

			// Assert
			result.Should().NotBeNull().And.BeOfType<JsonResult>();
			result.Data.Should().NotBeNull().And.BeOfType<ApplicantViewModel>();
			helper.GetMock<IGrantApplicationService>().Verify(m => m.Get(It.IsAny<int>()), Times.Once);
			helper.GetMock<IGrantApplicationService>().Verify(m => m.Update(It.IsAny<GrantApplication>(), ApplicationWorkflowTrigger.EditApplicant, null), Times.Once);
			var model = result.Data as ApplicantViewModel;
			model.Id.Should().Be(grantApplication.Id);
		}

		[TestMethod, TestCategory("Controller"), TestCategory(nameof(ApplicantController))]
		public void UpdateApplicant_InValidModel()
		{
			var user = EntityHelper.CreateExternalUser();
			var helper = new ControllerHelper<ApplicantController>(user);
			var grantApplication = EntityHelper.CreateGrantApplication(user);

			helper.GetMock<IGrantApplicationService>().Setup(m => m.Get(It.IsAny<int>())).Returns(grantApplication);
			helper.GetMock<IStaticDataService>().Setup(m => m.GetCountry(It.IsAny<string>())).Returns(EntityHelper.CreateCountry("CA"));
			helper.GetMock<IStaticDataService>().Setup(m => m.GetRegion(It.IsAny<string>(), It.IsAny<string>())).Returns(EntityHelper.CreateRegion("Victoria"));
			var mockNaIndustryClassificationSystemService = helper.GetMock<INaIndustryClassificationSystemService>();
			var viewModel = new ApplicantViewModel()
			{
				BusinessLicenseNumber = "123456789012345678901"
			};
			var controller = helper.CreateWithModel(viewModel);

			// Act
			var result = controller.UpdateApplicant(viewModel);

			// Assert
			result.Should().NotBeNull().And.BeOfType<JsonResult>();
			result.Data.Should().NotBeNull().And.BeOfType<ApplicantViewModel>();
			var validationErrors = (result.Data as ApplicantViewModel).ValidationErrors;
			validationErrors.Count().Should().Be(16);
			validationErrors.Any(l => l.Key == "OrganizationLegalName").Should().BeTrue();
			validationErrors.Any(l => l.Key == "OrganizationYearEstablished").Should().BeTrue();
			validationErrors.Any(l => l.Key == "OrganizationNumberOfEmployeesWorldwide").Should().BeTrue();
			validationErrors.Any(l => l.Key == "OrganizationNumberOfEmployeesBC").Should().BeTrue();
			validationErrors.Any(l => l.Key == "OrganizationAnnualTrainingBudget").Should().BeTrue();
			validationErrors.Any(l => l.Key == "OrganizationAnnualEmployeesTrained").Should().BeTrue();
			validationErrors.Any(l => l.Key == "BusinessLicenseNumber").Should().BeTrue();
			validationErrors.Any(l => l.Key == "AddressLine1").Should().BeTrue();
			validationErrors.Any(l => l.Key == "City").Should().BeTrue();
			validationErrors.Any(l => l.Key == "Region").Should().BeTrue();
			validationErrors.Any(l => l.Key == "PostalCode").Should().BeTrue();
			validationErrors.Any(l => l.Key == "Country").Should().BeTrue();
			validationErrors.Any(l => l.Key == "NAICSLevel1Id").Should().BeTrue();
			validationErrors.Any(l => l.Key == "NAICSLevel2Id").Should().BeTrue();
			validationErrors.Any(l => l.Key == "NAICSLevel3Id").Should().BeTrue();

			helper.GetMock<IGrantApplicationService>().Verify(m => m.Get(It.IsAny<int>()), Times.Never);
			helper.GetMock<IGrantApplicationService>().Verify(m => m.Update(It.IsAny<GrantApplication>(), ApplicationWorkflowTrigger.EditApplicant, null), Times.Never);
		}

		[TestMethod, TestCategory("Controller"), TestCategory(nameof(ApplicantController))]
		public void UpdateApplicant_NotAuthorized()
		{
			// Arrange
			var user = EntityHelper.CreateExternalUser();
			var helper = new ControllerHelper<ApplicantController>(user);
			var grantApplication = EntityHelper.CreateGrantApplication(user);
			helper.GetMock<IGrantApplicationService>()
				.Setup(m => m.Get(It.IsAny<int>()))
				.Throws<NotAuthorizedException>();

			var mockNaIndustryClassificationSystemService = helper.GetMock<INaIndustryClassificationSystemService>();
			var viewModel = new ApplicantViewModel(grantApplication, mockNaIndustryClassificationSystemService.Object);
			var controller = helper.Create();

			// Act
			var result = controller.UpdateApplicant(viewModel);

			// Assert
			result.Should().NotBeNull().And.BeOfType<JsonResult>();
			result.Data.Should().NotBeNull().And.BeOfType<ApplicantViewModel>();
			controller.HttpContext.Response.StatusCode.Should().Be(403);
		}
		#endregion


		// TODO: Cleanup these test clones, but subtly different test methods. They were merged from ApplicantControllerTest.cs (which was deleted)

		#region GetApplicant
		[TestMethod, TestCategory("Controller"), TestCategory(nameof(ApplicantController))]
		public void GetApplicant_MergedFromOtherTestClass()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ControllerHelper<ApplicantController>(user, Roles.Assessor);
			var controller = helper.Create();
			var naics = EntityHelper.CreateNaIndustryClassificationSystem();
			var grantApplication = EntityHelper.CreateGrantApplication();
			grantApplication.OrganizationTypeId = 1;
			grantApplication.OrganizationLegalStructureId = 1;
			grantApplication.NAICSId = naics.Id;
			helper.GetMock<IGrantApplicationService>().Setup(m => m.Get(It.IsAny<int>())).Returns(grantApplication);
			helper.GetMock<INaIndustryClassificationSystemService>().Setup(m => m.GetNaIndustryClassificationSystems(It.IsAny<int>())).Returns(new List<NaIndustryClassificationSystem> { naics });

			// Act
			var result = controller.GetApplicant(grantApplication.Id);

			// Assert
			result.Should().NotBeNull().And.BeOfType<JsonResult>();
			result.Data.Should().NotBeNull().And.BeOfType<ApplicantViewModel>();
			var model = result.Data as ApplicantViewModel;
			model.Id.Should().Be(grantApplication.Id);
			model.OrganizationLegalName.Should().Be(grantApplication.OrganizationLegalName);
			helper.GetMock<IGrantApplicationService>().Verify(m => m.Get(It.IsAny<int>()), Times.Once);
		}

		[TestMethod, TestCategory("Controller"), TestCategory(nameof(ApplicantController))]
		public void GetApplicant_NotAuthorizedException_MergedFromOtherTestClass()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ControllerHelper<ApplicantController>(user, Roles.Assessor);
			var controller = helper.Create();
			var grantApplication = EntityHelper.CreateGrantApplication();
			helper.GetMock<IGrantApplicationService>().Setup(m => m.Get(It.IsAny<int>())).Throws<NotAuthorizedException>();

			// Act
			var result = controller.GetApplicant(grantApplication.Id);

			// Assert
			result.Should().NotBeNull().And.BeOfType<JsonResult>();
			result.Data.Should().NotBeNull().And.BeOfType<ApplicantViewModel>();
			helper.GetMock<IGrantApplicationService>().Verify(m => m.Get(It.IsAny<int>()), Times.Once);
			controller.ControllerContext.HttpContext.Response.StatusCode.Should().Be(403);
		}
		#endregion

		#region UpdateApplicant
		[TestMethod, TestCategory("Controller"), TestCategory(nameof(ApplicantController))]
		public void UpdateApplicant_MergedFromOtherTestClass()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ControllerHelper<ApplicantController>(user, Roles.Assessor);
			var grantApplication = EntityHelper.CreateGrantApplication();
			var region = EntityHelper.CreateRegion("BC");
			var country = EntityHelper.CreateCountry("CA");
			var naics = EntityHelper.CreateNaIndustryClassificationSystem();

			helper.GetMock<IGrantApplicationService>().Setup(m => m.Get(It.IsAny<int>())).Returns(grantApplication);
			helper.GetMock<IStaticDataService>().Setup(m => m.GetRegion(It.IsAny<string>(), It.IsAny<string>())).Returns(region);
			helper.GetMock<IStaticDataService>().Setup(m => m.GetCountry(It.IsAny<string>())).Returns(country);
			helper.GetMock<INaIndustryClassificationSystemService>().Setup(m => m.GetNaIndustryClassificationSystems(It.IsAny<int>())).Returns(new List<NaIndustryClassificationSystem> { naics });

			var applicantViewModel = new ApplicantViewModel()
			{
				Id = grantApplication.Id,
				OrganizationLegalName = grantApplication.OrganizationLegalName,
				OrganizationTypeId = 0,
				LegalStructureId = 0,
				OrganizationYearEstablished = 0,
				OrganizationNumberOfEmployeesWorldwide = 0,
				OrganizationNumberOfEmployeesBC = 0,
				OrganizationAnnualTrainingBudget = 0,
				OrganizationAnnualEmployeesTrained = 0,
				BusinessLicenseNumber = "",
				BusinessNumber = "",
				BusinessNumberVerified = false,
				StatementOfRegistrationNumber = "",
				JurisdictionOfIncorporation = "",
				IncorporationNumber = "",
				AddressLine1 = "",
				AddressLine2 = "",
				City = "",
				RegionId = "",
				Region = "",
				PostalCode = "",
				CountryId = "",
				Country = "",
				HasAppliedForGrantBefore = false,
				WouldTrainEmployeesWithoutGrant = false,
				NAICSLevel1Id = 0,
				NAICSLevel2Id = 0,
				NAICSLevel3Id = 0,
				NAICSLevel4Id = 0,
				NAICSLevel5Id = 0,
				GrantProgramName = "",
				RowVersion = "AgQGCAoMDhASFA=="
			};
			var controller = helper.Create();

			// Act
			var result = controller.UpdateApplicant(applicantViewModel);

			// Assert
			result.Should().NotBeNull().And.BeOfType<JsonResult>();
			result.Data.Should().NotBeNull().And.BeOfType<ApplicantViewModel>();
			var model = result.Data as ApplicantViewModel;
			model.Id.Should().Be(grantApplication.Id);
			helper.GetMock<IGrantApplicationService>()
				.Verify(m => m.Update(It.IsAny<GrantApplication>(), It.IsAny<ApplicationWorkflowTrigger>(), It.IsAny<Func<ApplicationWorkflowTrigger, bool>>()), Times.Once);
			helper.GetMock<IGrantApplicationService>().Verify(m => m.Get(It.IsAny<int>()), Times.Once);
		}

		[TestMethod, TestCategory("Controller"), TestCategory(nameof(ApplicantController))]
		public void UpdateApplicant_NotAuthorizedException_MergedFromOtherTestClass()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ControllerHelper<ApplicantController>(user, Roles.Assessor);
			helper.GetMock<IGrantApplicationService>().Setup(m => m.Get(It.IsAny<int>())).Throws<NotAuthorizedException>();

			var phoneNumberViewModel = new PhoneViewModel("604-000-0000", "454");
			var physicalAddress = new AddressViewModel()
			{
				RowVersion = "AgQGCAoMDhASFA==",
				CountryId = CJG.Core.Entities.Constants.CanadaCountryId,
				IsCanadianAddress = true
			};
			var applicantViewModel = new ApplicantViewModel()
			{
				Id = 1,
				RowVersion = "AgQGCAoMDhASFA=="
			};
			var controller = helper.Create();

			// Act
			var result = controller.UpdateApplicant(applicantViewModel);

			// Assert
			result.Should().NotBeNull().And.BeOfType<JsonResult>();
			result.Data.Should().NotBeNull().And.BeOfType<ApplicantViewModel>();
			controller.HttpContext.Response.StatusCode.Should().Be(403);
			helper.GetMock<IGrantApplicationService>()
				.Verify(m => m.Update(It.IsAny<GrantApplication>(), It.IsAny<ApplicationWorkflowTrigger>(), It.IsAny<Func<ApplicationWorkflowTrigger, bool>>()), Times.Never);
			helper.GetMock<IGrantApplicationService>().Verify(m => m.Get(It.IsAny<int>()), Times.Once);
		}

		[TestMethod, TestCategory("Controller"), TestCategory(nameof(ApplicantController))]
		public void UpdateApplicant_InvalidModel_MergedFromOtherTestClass()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ControllerHelper<ApplicantController>(user, Roles.Assessor);
			helper.GetMock<IGrantApplicationService>().Setup(m => m.Get(It.IsAny<int>())).Throws<NotAuthorizedException>();
			var phoneNumberViewModel = new PhoneViewModel("604-000-0000", "454");
			var physicalAddress = new AddressViewModel()
			{
				RowVersion = "AgQGCAoMDhASFA==",
				CountryId = CJG.Core.Entities.Constants.CanadaCountryId,
				IsCanadianAddress = true
			};
			var applicantViewModel = new ApplicantViewModel()
			{
				Id = 1,
				RowVersion = "AgQGCAoMDhASFA=="
			};
			var controller = helper.Create();
			controller.ModelState.AddModelError("ApplicantFirstName", "ApplicantFirstName is required");
			// Act
			var result = controller.UpdateApplicant(applicantViewModel);

			// Assert
			result.Should().NotBeNull().And.BeOfType<JsonResult>();
			result.Data.Should().NotBeNull().And.BeOfType<ApplicantViewModel>();
			helper.GetMock<IGrantApplicationService>()
				.Verify(m => m.Update(It.IsAny<GrantApplication>(), It.IsAny<ApplicationWorkflowTrigger>(), It.IsAny<Func<ApplicationWorkflowTrigger, bool>>()), Times.Never);
			helper.GetMock<IGrantApplicationService>().Verify(m => m.Get(It.IsAny<int>()), Times.Never);
		}
		#endregion
	}
}
