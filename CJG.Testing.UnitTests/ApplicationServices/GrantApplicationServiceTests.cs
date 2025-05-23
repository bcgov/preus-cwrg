﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CJG.Application.Services;
using CJG.Application.Services.Exceptions;
using CJG.Core.Entities;
using CJG.Core.Entities.Helpers;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Entities;
using CJG.Infrastructure.Identity;
using CJG.Testing.Core;
using FluentAssertions;
using Microsoft.AspNet.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CJG.Testing.UnitTests.ApplicationServices
{
	[TestClass]
	public class GrantApplicationServiceTests : ServiceUnitTestBase
	{
		private GrantProgram _defaultGrantProgram;

		[TestInitialize]
		public void Setup()
		{
			_defaultGrantProgram = new GrantProgram
			{
				Id = 57, // Temporary and ugly fix to filter issues
				ProgramCode = "CWRG"
			};
		}

		#region Tests
		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void AddGrantApplication_WithEmptyApplication_SetState()
		{
			// Arrange
			var user = EntityHelper.CreateExternalUser();
			var helper = new ServiceHelper<GrantApplicationService>(user);
			helper.MockDbSet<GrantApplication>();
			var service = helper.Create();
			var grantApplication = new GrantApplication { GrantOpening = new GrantOpening() };

			// Act
			service.Add(grantApplication);

			// Assert
			grantApplication.ApplicationStateExternal.Should().Be(ApplicationStateExternal.Incomplete);
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.Draft);
			helper.GetMock<DbSet<GrantApplication>>().Verify(m => m.Add(It.IsAny<GrantApplication>()), Times.Once());
			helper.GetMock<IDataContext>().Verify(m => m.CommitTransaction(), Times.Once());
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void UpdateGrantApplication_WithIncompleteApplication_UpdatesAndDoesNotResetFileNumber()
		{
			// Arrange
			var applicationAdministrator = EntityHelper.CreateExternalUser();
			var grantApplication = EntityHelper.CreateGrantApplication(applicationAdministrator, ApplicationStateInternal.Draft);
			grantApplication.ApplicationStateExternal = ApplicationStateExternal.Incomplete;
			grantApplication.FileNumber = "TEST";
			grantApplication.TrainingCost = new TrainingCost() { EstimatedParticipants = 5, GrantApplication = grantApplication };

			var helper = new ServiceHelper(typeof(GrantApplicationService), applicationAdministrator);
			var service = helper.Create<GrantApplicationService>();
			helper.MockDbSet(grantApplication);

			var dbContextMock = helper.GetMock<IDataContext>();
			dbContextMock.Setup(m => m.OriginalValue(It.IsAny<TrainingCost>(), It.IsAny<string>())).Returns(1);

			// Act
			service.Update(grantApplication);

			// Assert
			grantApplication.ApplicationStateExternal.Should().Be(ApplicationStateExternal.Incomplete);
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.Draft);
			grantApplication.FileNumber.Should().Be("TEST");
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void UpdateGrantApplication_InvalidApplicationAdministrator()
		{
			// Arrange
			var user = EntityHelper.CreateExternalUser();
			user.Id = 2;
			var helper = new ServiceHelper(typeof(GrantApplicationService), user);
			var applicationAdministrator = EntityHelper.CreateExternalUser();
			var grantApplication = EntityHelper.CreateGrantApplication(applicationAdministrator, ApplicationStateInternal.Draft);
			var service = helper.Create<GrantApplicationService>();

			try
			{
				// Act
				service.Update(grantApplication);
				Assert.Fail();
			}
			catch (Exception e)
			{
				// Assert
				Assert.IsInstanceOfType(e, typeof(NotAuthorizedException));
			}
		}

		/// <summary>
		/// Get Grant Application with an valid invitation should return a <typeparamref name="GrantApplication"/>.
		/// </summary>
		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void GetGrantApplication_WithValidInvitation_ShouldReturnGrantApplication()
		{
			// Arrange
			var applicationAdministrator = EntityHelper.CreateExternalUser();
			var grantOpening = EntityHelper.CreateGrantOpening();
			var helper = new ServiceHelper(typeof(GrantApplicationService), applicationAdministrator);
			Guid invitationKey = Guid.NewGuid();
			var grantApplication = EntityHelper.CreateGrantApplication(grantOpening, applicationAdministrator, ApplicationStateInternal.AgreementAccepted);
			grantApplication.InvitationKey = invitationKey;

			helper.MockDbSet<GrantApplication>(grantApplication);

			var service = helper.Create<GrantApplicationService>();

			// Act
			var actual = service.Get(invitationKey);

			// Assert
			actual.Should().NotBeNull().And.BeOfType<GrantApplication>();
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void GetGrantApplication_WithMockedId_ReturnsApplication()
		{
			// Arrange
			var applicationAdministrator = EntityHelper.CreateExternalUser();
			var grantOpening = EntityHelper.CreateGrantOpening();
			var grantApplication = EntityHelper.CreateGrantApplication(grantOpening, applicationAdministrator, ApplicationStateInternal.Draft);
			var helper = new ServiceHelper(typeof(GrantApplicationService), applicationAdministrator);
			helper.MockDbSet(grantApplication).Setup(x => x.Find(It.IsAny<int>())).Returns(grantApplication);

			var service = helper.Create<GrantApplicationService>();

			// Act

			// Assert
			service.Get(1).Should().NotBeNull();
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void GetGrantApplications_WithNewInternalState_ReturnsOneElementCollection()
		{
			// Arrange
			var user = EntityHelper.CreateExternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user);
			helper.MockDbSet(_defaultGrantProgram);

			helper.GetMock<IGrantStreamService>().Setup(m => m.GetDefaultGrantProgramId()).Returns(57);

			helper.MockDbSet( new List<GrantApplication>
			{
				new GrantApplication { ApplicationStateInternal = ApplicationStateInternal.New, FileNumber = "tesT",
					GrantOpening = new GrantOpening
					{
						GrantStream = new GrantStream
						{
							Id = _defaultGrantProgram.Id,
							GrantProgram = _defaultGrantProgram
						}
					}
				}
			});

			var service = helper.Create<GrantApplicationService>();
			var filter = new ApplicationFilter(new[] {ApplicationStateInternal.New });

			// Act
			var page = service.GetGrantApplications(1, 10, filter);

			// Assert
			page.Should().NotBeNull();
			page.Items.Should().HaveCount(1);
			page.Items.First().ApplicationStateInternal.Should().Be(ApplicationStateInternal.New);
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void SubmitGrantApplication_WithValidApplication_UpdatesStatusOnSubmitted()
		{
			// Arrange
			AppDateTime.SetNow(DateTime.UtcNow);
			var user = EntityHelper.CreateExternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user);
			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplicationWithCosts(user, ApplicationStateInternal.Draft);
			grantApplication.TrainingCost.TrainingCostState = TrainingCostStates.Complete;
			helper.MockDbSet<GrantApplication>(grantApplication);

			// Act
			service.Submit(grantApplication);

			// Assert
			grantApplication.ApplicationStateExternal.Should().Be(ApplicationStateExternal.Submitted);
		}


		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void SubmitGrantApplication_UpdatesStatusOnSubmitted()
		{
			// Arrange
			AppDateTime.SetNow(DateTime.UtcNow);
			var user = EntityHelper.CreateExternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user);
			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplicationWithCosts(user, ApplicationStateInternal.Draft);
			grantApplication.TrainingCost.TrainingCostState = TrainingCostStates.Complete;

			helper.MockDbSet<GrantApplication>(grantApplication);

			// Act
			service.Submit(grantApplication);

			// Assert
			grantApplication.ApplicationStateExternal.Should().Be(ApplicationStateExternal.Submitted);
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void SubmitChangeRequest_WithAgreementAccepted_UpdatesApplicationToChangeRequestSubmitted()
		{
			// Arrange
			var user = EntityHelper.CreateExternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user);
			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.AgreementAccepted);
			grantApplication.TrainingProviders.Add(new TrainingProvider(grantApplication) { Id = 1, TrainingProviderInventoryId = 1 });
			grantApplication.TrainingPrograms.Add(new TrainingProgram(grantApplication, grantApplication.TrainingProviders.First()) { Id = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(3) });

			// Act
			service.SubmitChangeRequest(grantApplication, "reason");

			// Assert
			grantApplication.ApplicationStateExternal.Should().Be(ApplicationStateExternal.ChangeRequestSubmitted);
		}


		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void DenyChangeRequest_WithChangeForDenial_UpdatesApplicationToChangeRequestDenied()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Director");
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(new ApplicationUser(user));
			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.ChangeForDenial);
			var trainingProgram = EntityHelper.CreateTrainingProgram(grantApplication, EntityHelper.CreateTrainingProvider(grantApplication), 5);
			var changeRequest = new TrainingProvider(trainingProgram) { DateAdded = DateTime.UtcNow.AddMinutes(5) };

			// Act
			service.DenyChangeRequest(grantApplication, "reason");

			// Assert
			grantApplication.ApplicationStateExternal.Should().Be(ApplicationStateExternal.ChangeRequestDenied);
		}


		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void ApproveChangeRequest_WithChangeForApproval_UpdatesApplicationToChangeRequestApproved()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Director");
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(new ApplicationUser(user));
			var service = helper.CreateMock<GrantApplicationService>().As<IGrantApplicationService>();

			var applicationAdministrator = EntityHelper.CreateExternalUser();
			var assessor = new ApplicationUser(user);

			helper.GetMock<IDataContext>().Setup(m => m.GrantApplications.Add(It.IsAny<GrantApplication>()));
			helper.GetMock<IUserService>().Setup(m => m.GetUser(It.IsAny<Guid>())).Returns(applicationAdministrator);
			helper.GetMock<IUserService>().Setup(m => m.GetAccountType()).Returns(AccountTypes.Internal);
			helper.GetMock<IUserService>().Setup(m => m.GetInternalUser(It.IsAny<int>())).Returns(user);
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(assessor);
			//helper.GetMock<INotificationService>().Setup(m => m.HandleWorkflowNotification(It.IsAny<GrantApplication>(), It.IsAny<NotificationTypes>())).Returns(true);
			helper.GetMock<IStaticDataService>().Setup(m => m.GetNoteType(It.IsAny<NoteTypes>())).Returns(new NoteType());
			service.CallBase = true;

			var grantApplication = EntityHelper.CreateGrantApplication(applicationAdministrator, user, ApplicationStateInternal.ChangeForApproval);

			// Original Training Provider.
			var trainingProvider = new TrainingProvider(grantApplication)
			{
				TrainingProviderState = TrainingProviderStates.Complete
			};

			grantApplication.TrainingPrograms.Add(new TrainingProgram(grantApplication, trainingProvider)
			{
				TrainingProgramState = TrainingProgramStates.Complete,
				DateAdded = DateTime.UtcNow
			});
			grantApplication.TrainingCost.TrainingCostState = TrainingCostStates.Complete;
			// Requested Training Provider
			grantApplication.TrainingPrograms.FirstOrDefault().TrainingProviders.Add(new TrainingProvider(grantApplication.TrainingPrograms.FirstOrDefault())
			{
				TrainingProviderState = TrainingProviderStates.Requested,
				DateAdded = DateTime.UtcNow.AddSeconds(5)
			});

			helper.GetMock<INoteService>().Setup(m => m.CreateWorkflowNote(It.IsAny<GrantApplication>(), It.IsAny<string>())).Returns(new Note(grantApplication, new NoteType(), "A message"));

			// Act
			service.Object.ApproveChangeRequest(grantApplication);

			// Assert
			grantApplication.ApplicationStateExternal.Should().Be(ApplicationStateExternal.ChangeRequestApproved);
		}


		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void ReturnChangeToAssessment_WithChangeForApproval_UpdatesApplicationToChangeRequestSubmitted()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Director");
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(new ApplicationUser(user));
			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.ChangeForApproval);

			// Act
			service.ReturnChangeToAssessment(grantApplication);

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.ChangeReturned);
			grantApplication.ApplicationStateExternal.Should().Be(ApplicationStateExternal.ChangeRequestSubmitted);
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void SelectForAssessment_WithNew_UpdatesApplicationToPendingAssessment()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Assessor");
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(new ApplicationUser(user));
			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.New);
			grantApplication.DateSubmitted = DateTime.Today;
			grantApplication.TrainingPrograms = new List<TrainingProgram> { new TrainingProgram { } };
			grantApplication.TrainingCost.TotalEstimatedCost = 1;
			helper.GetMock<IGrantOpeningService>().Setup(x => x.Get(It.IsAny<int>()))
				.Returns(grantApplication.GrantOpening);

			// Act
			service.SelectForAssessment(grantApplication);

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.PendingAssessment);
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void BeginAssessment_WithPendingAssessment_UpdatesApplicationToUnderAssessment()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Assessor");
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(new ApplicationUser(user));
			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.PendingAssessment);
			grantApplication.TrainingProviders.Add(new TrainingProvider(grantApplication) { Id = 1, TrainingProviderInventoryId = 1 });

			// Act
			service.BeginAssessment(grantApplication, user.Id);

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.UnderAssessment);
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void CancelApplicationAgreement_WithAgreementAccepted_UpdatesApplicationToCancelledByMinistry()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Director");
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(new ApplicationUser(user));
			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.AgreementAccepted);

			helper.GetMock<IGrantOpeningService>().Setup(x => x.Get(It.IsAny<int>()))
				.Returns(grantApplication.GrantOpening);

			// Act
			service.CancelApplicationAgreement(grantApplication, "test");

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.CancelledByMinistry);
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void ReturnUnfundedApplications_WithNew_UpdatesApplicationToUnfunded()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Director");
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(new ApplicationUser(user));
			var service = helper.Create<GrantApplicationService>();
			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.New);
			var trainingProgram = new TrainingProgram(grantApplication) { Id = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(3) };
			var trainingProvider = new TrainingProvider(trainingProgram) { Id = 1, TrainingProviderInventoryId = 1, TrainingProviderState = TrainingProviderStates.Complete };

			// Act
			service.ReturnUnfunded(grantApplication);

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.Unfunded);
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void ReturnApplicationUnaccessed_WithNew_UpdatesApplicationStateToApplicationReturnedUnassessed()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Director");
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(new ApplicationUser(user));
			var service = helper.Create<GrantApplicationService>();
			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.New);
			var trainingProgram = new TrainingProgram(grantApplication) { Id = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(3) };
			var trainingProvider = new TrainingProvider(trainingProgram) { Id = 1, TrainingProviderInventoryId = 1, TrainingProviderState = TrainingProviderStates.Complete };

			// Act
			service.ReturnUnassessed(grantApplication);

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.ReturnedUnassessed);
			grantApplication.ApplicationStateExternal.Should().Be(ApplicationStateExternal.ReturnedUnassessed);
		}

		[TestMethod]
		[TestCategory("Workflow"), TestCategory("State Machine"), TestCategory("Service")]
		public void ReturnApplicationUnassessed_WithDirector_UpdatesApplicationStateToApplicationReturnedUnassessed()
		{
			// Arrange
			var applicationAdministrator = EntityHelper.CreateExternalUser();
			var director = EntityHelper.CreateInternalUser();
			var grantOpening = EntityHelper.CreateGrantOpening();
			var grantApplication = EntityHelper.CreateGrantApplicationWithCosts(grantOpening, applicationAdministrator, ApplicationStateInternal.New);
			grantApplication.DateSubmitted = DateTime.UtcNow;
			grantApplication.FileNumber = "2150000";
			grantOpening.GrantOpeningIntake.NewCount = 1;
			grantOpening.GrantOpeningIntake.NewAmt = grantApplication.TrainingCost.TotalEstimatedReimbursement;

			var helper = new ServiceHelper(typeof(ApplicationWorkflowStateMachine), director, "Director");
			helper.MockDbSet(grantOpening);
			helper.SetMockAs<GrantOpeningService, IGrantOpeningService>().CallBase = true;

			var stateMachine = helper.Create<ApplicationWorkflowStateMachine>(grantApplication);

			// Act
			stateMachine.ReturnApplicationUnassessed();

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.ReturnedUnassessed);
			grantApplication.ApplicationStateExternal.Should().Be(ApplicationStateExternal.ReturnedUnassessed);
			grantApplication.FileNumber.Should().NotBeNull();
			grantOpening.GrantOpeningIntake.NewCount.Should().Be(0);
			grantOpening.GrantOpeningIntake.NewAmt.Should().Be(0);
			grantApplication.TrainingPrograms.Should().NotBeNullOrEmpty();
			grantApplication.TrainingPrograms.Count.Should().BeGreaterOrEqualTo(1);
			grantApplication.TrainingPrograms.Single().TrainingProvider.Should().NotBeNull();
			grantApplication.ProgramDescription.Should().NotBeNull();
			grantApplication.TrainingCost.Should().NotBeNull();
			grantApplication.TrainingCost.EligibleCosts.Should().NotBeNull();
			grantApplication.TrainingCost.TrainingCostState.Should().NotBeNull();
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		[ExpectedException(typeof(NotAuthorizedException))]
		public void ReturnApplicationUnaccessed_WithAssessor_ThrowsException()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Assessor");
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(new ApplicationUser(user));
			var service = helper.Create<GrantApplicationService>();
			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.UnderAssessment);
			var trainingProgram = new TrainingProgram(grantApplication) { Id = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(3) };
			var trainingProvider = new TrainingProvider(trainingProgram) { Id = 1, TrainingProviderInventoryId = 1, TrainingProviderState = TrainingProviderStates.Complete };

			// Act
			service.ReturnUnassessed(grantApplication);

			// Assert
			// Should throw exception with null country name as parameter
		}


		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void RemoveFromAssessment_WithPendingAssessment_UpdatesApplicationToNew()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Assessor");
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(new ApplicationUser(user));
			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.PendingAssessment);

			helper.GetMock<IGrantOpeningService>().Setup(x => x.Get(It.IsAny<int>()))
				.Returns(grantApplication.GrantOpening);

			// Act
			service.RemoveFromAssessment(grantApplication);

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.New);
		}


		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void ReturnToAssessment_WithRecommendedForApproval_UpdatesApplicationToReturnedToAssessment()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Director");
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(new ApplicationUser(user));
			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.RecommendedForApproval);

			// Act
			service.ReturnToAssessment(grantApplication);

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.ReturnedToAssessment);
		}


		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void RecommendChangeForApproval_WithChangeRequest_UpdatesApplicationToChangeForApproval()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Assessor");
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(new ApplicationUser(user));
			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.ChangeRequest);

			// Act
			service.RecommendChangeForApproval(grantApplication);

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.ChangeForApproval);
		}


		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void RecommendChangeForDenial_WithChangeRequest_UpdatesApplicationToChangeForApproval()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Assessor");
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(new ApplicationUser(user));
			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.ChangeRequest);

			// Act
			service.RecommendChangeForDenial(grantApplication, "reason");

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.ChangeForDenial);
		}


		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void WithdrawOffer_WithOfferIssued_UpdatesApplicationToOfferWithdrawn()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Director");
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(new ApplicationUser(user));
			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.OfferIssued);

			helper.GetMock<IGrantOpeningService>().Setup(x => x.Get(It.IsAny<int>()))
				.Returns(grantApplication.GrantOpening);

			// Act
			service.WithdrawOffer(grantApplication, "reason");

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.OfferWithdrawn);
		}


		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void DenyApplication_WithRecommendedForDenial_UpdatesApplicationToApplicationDenied()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Director");
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(new ApplicationUser(user));
			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.RecommendedForDenial);

			helper.GetMock<IGrantOpeningService>().Setup(x => x.Get(It.IsAny<int>()))
				.Returns(grantApplication.GrantOpening);

			// Act
			service.DenyApplication(grantApplication, "reason");

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.ApplicationDenied);
		}


		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void RecommendForApproval_WithReturnedToAssessment_UpdatesApplicationToRecommendedForApproval()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Assessor");
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(new ApplicationUser(user));
			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.ReturnedToAssessment);
			grantApplication.TrainingProviders.Add(new TrainingProvider(grantApplication) { Id = 1, TrainingProviderInventoryId = 1, TrainingProviderState = TrainingProviderStates.Complete });
			grantApplication.TrainingPrograms.Add(new TrainingProgram(grantApplication, grantApplication.TrainingProviders.First()) { Id = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(3) });

			helper.GetMock<IGrantOpeningService>().Setup(x => x.Get(It.IsAny<int>()))
				.Returns(grantApplication.GrantOpening);

			// Act
			service.RecommendForApproval(grantApplication);

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.RecommendedForApproval);
		}


		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void RecommendForDenial_WithUnderAssessment_UpdatesApplicationToRecommendedForApproval()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Assessor");
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(new ApplicationUser(user));
			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.UnderAssessment);
			grantApplication.TrainingProviders.Add(new TrainingProvider(grantApplication) { Id = 1, TrainingProviderInventoryId = 1, TrainingProviderState = TrainingProviderStates.Complete });
			grantApplication.TrainingPrograms.Add(new TrainingProgram(grantApplication, grantApplication.TrainingProviders.First()) { Id = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(3) });

			helper.GetMock<IGrantOpeningService>().Setup(x => x.Get(It.IsAny<int>()))
				.Returns(grantApplication.GrantOpening);

			// Act
			service.RecommendForDenial(grantApplication);

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.RecommendedForDenial);
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void WithdrawGrantApplication_WithUnderAssessment_UpdatesApplicationToApplicationWithdrawn()
		{
			// Arrange
			var user = EntityHelper.CreateExternalUser();
			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.UnderAssessment);
			grantApplication.TrainingProviders.Add(new TrainingProvider(grantApplication) { Id = 1, TrainingProviderInventoryId = 1, TrainingProviderState = TrainingProviderStates.Complete });
			grantApplication.TrainingPrograms.Add(new TrainingProgram(grantApplication, grantApplication.TrainingProviders.First()) { Id = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(3) });

			var helper = new ServiceHelper(typeof(GrantApplicationService), user);
			helper.MockDbSet( new[]
			{
				grantApplication
			});
			var service = helper.Create<GrantApplicationService>();

			helper.GetMock<IGrantOpeningService>().Setup(x => x.Get(It.IsAny<int>()))
				.Returns(grantApplication.GrantOpening);

			// Act
			service.Withdraw(grantApplication, "test");

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.ApplicationWithdrawn);
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void IssueOffer_WithRecommendedForApproval_UpdatesStateToOfferIssued()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Director");
			helper.GetMock<IUserManagerAdapter>().Setup(m => m.FindById(It.IsAny<string>())).Returns(new ApplicationUser(user));
			helper.MockDbSet<GrantAgreement>();
			var userStore = new Mock<IUserStore<ApplicationUser>>();
			helper.SetMock(new Mock<ApplicationUserManager>(userStore.Object));
			helper.SetMockAs<GrantProgramService, IGrantProgramService>().CallBase = true;

			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.RecommendedForApproval);
			grantApplication.FileNumber = "F1";
			grantApplication.ApplicantFirstName = "AFN";
			grantApplication.Organization = new Organization { LegalName = "LN" };
			grantApplication.OrganizationAddress = new ApplicationAddress { Country = new Country() };
			grantApplication.ApplicantPhysicalAddress = new ApplicationAddress() { AddressLine1 = "addr", City = "city", RegionId = "BC", Country = new Country("CA", "canada"), PostalCode = "abc" };
			var trainingProvider = new TrainingProvider(grantApplication) { Id = 1, TrainingProviderInventoryId = 1, TrainingProviderState = TrainingProviderStates.Complete };
			grantApplication.TrainingProviders.Add(trainingProvider);
			grantApplication.TrainingPrograms.Add(new TrainingProgram()
			{
				Id = 1,
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(3),
				TrainingProviders = new List<TrainingProvider>()
				{
					trainingProvider
				}
			});
			grantApplication.GrantOpening.GrantStream.GrantProgram.ApplicantCoverLetterTemplate = new DocumentTemplate(DocumentTypes.GrantAgreementCoverLetter, "approval", " @Model.FileNumber ");
			var service = helper.Create<GrantApplicationService>();

			// Act
			service.IssueOffer(grantApplication);

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.OfferIssued);
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void GetStateChange_WithAgreementAcceptedState_ReturnsAgreementAcceptedChange()
		{
			// Arrange
			var user = EntityHelper.CreateExternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user);
			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.Draft);
			helper.GetMock<IUserService>().Setup(m => m.GetUser(It.IsAny<Guid>())).Returns(user);
			grantApplication.StateChanges = new List<GrantApplicationStateChange>
			{
				new GrantApplicationStateChange { ToState = ApplicationStateInternal.AgreementAccepted}
			};
			grantApplication.BusinessContactRoles = new List<BusinessContactRole>
			{
				new BusinessContactRole {UserId = user.Id}
			};

			helper.MockDbSet(grantApplication).Setup(x => x.Find(It.IsAny<int>())).Returns(grantApplication);

			var service = helper.Create<GrantApplicationService>();

			// Act
			var stateChange = service.GetStateChange(1, ApplicationStateInternal.AgreementAccepted);

			// Assert
			stateChange.ToState.Should().Be(ApplicationStateInternal.AgreementAccepted);
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void GetPermittedTriggers_WithValidApplication_ReturnsTrigger()
		{
			// Arrange
			var user = EntityHelper.CreateExternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user);
			var service = helper.Create<GrantApplicationService>(user);
			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.Draft);

			// Act
			var triggers = grantApplication.ApplicationStateInternal.GetValidWorkflowTriggers();

			// Assert
			triggers.Should().Contain(ApplicationWorkflowTrigger.ViewApplication);
			triggers.Should().Contain(ApplicationWorkflowTrigger.SubmitApplication);
			triggers.Should().Contain(ApplicationWorkflowTrigger.EditTrainingCosts);
			triggers.Should().Contain(ApplicationWorkflowTrigger.EditParticipants);
			triggers.Should().HaveCount(16);
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void CloseGrantFile_WithCompletionReporting_UpdatesApplicationToClosed()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user, "Director");
			helper.MockDbSet<GrantApplication>();
			helper.MockDbSet<GrantApplicationStateChange>();
			helper.GetMock<IUserService>().Setup(m => m.GetInternalUser(It.IsAny<int>())).Returns(user);
			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.CompletionReporting);
			grantApplication.DateSubmitted = DateTime.Today;
			grantApplication.TrainingPrograms = new List<TrainingProgram> { new TrainingProgram { } };
			grantApplication.TrainingCost.TotalEstimatedCost = 1;
			grantApplication.Claims.Add(new Claim()
			{
				GrantApplication = new GrantApplication(),
				ClaimState = ClaimState.ClaimApproved
			});

			helper.GetMock<IGrantOpeningService>().Setup(x => x.Get(It.IsAny<int>()))
				.Returns(grantApplication.GrantOpening);

			// Act
			service.CloseGrantFile(grantApplication);

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.Closed);
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void SubmitCompletionReportToCloseGrantFile_WithCompletionReporting_UpdatesApplicationToClosed()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var applicationAdmin = EntityHelper.CreateExternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), applicationAdmin);

			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplication(user, ApplicationStateInternal.CompletionReporting);
			grantApplication.DateSubmitted = DateTime.Today;
			grantApplication.TrainingPrograms = new List<TrainingProgram> { new TrainingProgram { } };
			grantApplication.TrainingCost.TotalEstimatedCost = 1;
			helper.GetMock<IGrantOpeningService>().Setup(x => x.Get(It.IsAny<int>()))
				.Returns(grantApplication.GrantOpening);

			// Act
			service.SubmitCompletionReportToCloseGrantFile(grantApplication);

			// Assert
			grantApplication.ApplicationStateInternal.Should().Be(ApplicationStateInternal.Closed);
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void GetAttachments_When_GrantApplication_Is_Null_Throws_Exception()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var applicationAdmin = EntityHelper.CreateExternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), applicationAdmin);
			var service = helper.Create<GrantApplicationService>();

			// Act
			Action action = () => service.GetAttachments(1);

			// Assert
			action.Should().Throw<NoContentException>();
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void GetAttachments_When_IsApplicationAdministrator_Is_False_Throws_Exception()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var applicationAdmin = EntityHelper.CreateExternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), applicationAdmin);
			var service = helper.Create<GrantApplicationService>();
			var grantApplication = EntityHelper.CreateGrantApplication(user);

			grantApplication.BusinessContactRoles = new List<BusinessContactRole>() {
				new BusinessContactRole {UserId = 999 }
			};

			helper.MockDbSet(grantApplication).Setup(x => x.Find(It.IsAny<int>())).Returns(grantApplication);

			// Act
			Action action = () => service.GetAttachments(1);

			// Assert
			action.Should().Throw<NotAuthorizedException>();
		}

		[TestMethod, TestCategory("Grant Application"), TestCategory("Service")]
		public void GetAttachments_When_GrantApplication_Attachment_Exists_Return_One_AttachmentModel_Record()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var applicationAdmin = EntityHelper.CreateExternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), applicationAdmin);
			var service = helper.Create<GrantApplicationService>();
			var grantApplication = EntityHelper.CreateGrantApplication(user);

			grantApplication.BusinessContactRoles = new List<BusinessContactRole>() {
				new BusinessContactRole( grantApplication, new User() { Id = 1 } )
			};
			grantApplication.ApplicationStateInternal = ApplicationStateInternal.New;

			var attachment = new Attachment {Id = 1, FileName = "Avo", Description = "Unit Test", RowVersion = new byte[999] };

			grantApplication.Attachments.Add(attachment);

			helper.MockDbSet(grantApplication).Setup(x => x.Find(It.IsAny<int>())).Returns(grantApplication);
			helper.MockDbSet(attachment).Setup(x => x.Find(It.IsAny<int>())).Returns(attachment);

			// Act
			var result = service.GetAttachments(1);

			// Assert
			result.Count().Should().Be(1);

		}

		[TestMethod]
		[TestCategory("Grant Application"), TestCategory("Service")]
		public void EligibilityConfirmed_When_GrantStream_EligibilityEnabled_Is_False_Returns_True()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var applicationAdmin = EntityHelper.CreateExternalUser();
			var helper = new ServiceHelper(typeof(GrantApplication), applicationAdmin);
			var grantApplication = helper.Create<GrantApplication>();

			grantApplication.GrantOpening = new GrantOpening { GrantStream = new GrantStream { EligibilityEnabled = false } };

			// Act
			var result = GrantApplicationExtensions.EligibilityConfirmed(grantApplication);

			// Assert
			Assert.AreEqual(true, result);
		}

		[TestMethod]
		[TestCategory("Grant Application"), TestCategory("Service")]
		public void EligibilityConfirmed_When_GrantStream_EligibilityRequired_Is_True_And_GrantApplication_EligibilityConfirmed_Is_True_Returns_True()
		{
			// Arrange
			var user = EntityHelper.CreateInternalUser();
			var applicationAdmin = EntityHelper.CreateExternalUser();
			var helper = new ServiceHelper(typeof(GrantApplication), applicationAdmin);
			var grantApplication = helper.Create<GrantApplication>();

			grantApplication.GrantOpening = new GrantOpening { GrantStream = new GrantStream { EligibilityRequired = true } };
			grantApplication.EligibilityConfirmed = true;

			// Act
			var result = GrantApplicationExtensions.EligibilityConfirmed(grantApplication);

			// Assert
			Assert.AreEqual(true, result);
		}

		[TestMethod]
		[TestCategory("Grant Application"), TestCategory("Service")]
		[Ignore]
		public void RestartApplicationFromWithdrawn()
		{
			// Arrange
			AppDateTime.SetNow(DateTime.UtcNow);
			var user = EntityHelper.CreateExternalUser();
			var helper = new ServiceHelper(typeof(GrantApplicationService), user);
			var service = helper.Create<GrantApplicationService>();

			var grantApplication = EntityHelper.CreateGrantApplicationWithCosts(user, ApplicationStateInternal.Draft);

			grantApplication.TrainingCost.TrainingCostState = TrainingCostStates.Complete;
			helper.MockDbSet<GrantApplication>(grantApplication);
			helper.MockDbSet<ProgramDescription>();

			// Act
			var result = service.RestartApplicationFromWithdrawn(grantApplication.Id);
		}

		#endregion
	}
}