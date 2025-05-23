using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Core.Interfaces;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Models.Shared;
using System;
using System.Linq;
using System.Security.Principal;
using DeliveryPartnerService = CJG.Core.Entities.DeliveryPartnerService;

namespace CJG.Web.External.Areas.Int.Models.Applications
{
	public class ApplicationPreviewModel : BaseViewModel
	{
		public int GrantProgramId { get; set; }
		public ApplicationStateInternal ApplicationStateInternal { get; set; }

		public struct TestEntities
		{
			public GrantApplication GrantApplication;
			public GrantOpening GrantOpening;
			public GrantProgram GrantProgram;
			public FiscalYear FiscalYear;
			public User Applicant;
			public ParticipantForm Participant;
			public Organization Organization;
			public TrainingProgram TrainingProgram;
			public TrainingProvider TrainingProvider;
		}

		public ApplicationPreviewModel() { }

		public TestEntities GenerateTestEntities(IPrincipal _user, IUserService userService, IGrantProgramService grantProgramService, IFiscalYearService fiscalYearService, IStaticDataService staticDataService)
		{
			var now = AppDateTime.UtcNow;
			var user = userService.GetInternalUser(_user.GetUserId().Value);
			var grantProgram = grantProgramService.Get(this.GrantProgramId);
			var fiscalYear = fiscalYearService.GetFiscalYear(now);
			var applicantOrganization = new Organization(new OrganizationType("Business"), Guid.NewGuid(), "Organization Name", new LegalStructure("Legal Structure"), 2000, 50, 25, 5000, 5)
			{
				HeadOfficeAddress = new Address("1234 Street", "", "Vancouver", "V9V9V9", new Region("BC", "British Columbia", new Country("CA", "Canada")))
			};

			var physicalAddress = new Address("4321 Ave", "", "Victoria", "V3V3V3", new Region("BC", "British Columbia", new Country("CA", "Canada")));

			var applicant = new User(Guid.NewGuid(), user.FirstName, user.LastName, user.Email, applicantOrganization, physicalAddress);

			var grantOpening = new GrantOpening(new GrantStream("Test Grant Stream", "Test Objective", grantProgram), new TrainingPeriod(fiscalYear, "Test Training Period", now, now.AddMonths(6), now, now), 1000);
			var applicationType = new ApplicationType("Test Application Type");

			var grantApplication = new GrantApplication(grantOpening, applicant, applicationType)
			{
				Id = 1,
				ApplicationStateInternal = this.ApplicationStateInternal,
				ApplicationStateExternal = this.ApplicationStateInternal.GetExternalState(),
				InvitationKey = ApplicationStateInternal != ApplicationStateInternal.Draft ? Guid.NewGuid() : Guid.Empty,
				FileNumber = $"{fiscalYear.EndDate:yy}{++fiscalYear.NextAgreementNumber:d5}",
				StartDate = now,
				EndDate = now.AddDays(10),
				MaxReimbursementAmt = 10000,
				ReimbursementRate = 2d / 3d,
				PrioritySector = new PrioritySector("Priority Sector"),
				UsedDeliveryPartner = true,
				DeliveryPartner = new DeliveryPartner(grantProgram, "Delivery Partner"),
				DeliveryPartnerServices = new[] { new DeliveryPartnerService(grantProgram, "Delivery Partner Service") }
			};

			grantApplication.TrainingCost = new TrainingCost(grantApplication, 10);

			var trainingProgram = new TrainingProgram(grantApplication)
			{
				Id = 1,
				CourseTitle = "Test Training Program",
				StartDate = now,
				EndDate = now.AddDays(10),
				DeliveryMethods = new[] { new DeliveryMethod("Delivery Method") },
				InDemandOccupation = new InDemandOccupation("In Demand Occupation"),
				SkillFocus = new SkillsFocus("Skill Focus"),
				SkillLevel = new SkillLevel("Skill Level"),
				TotalTrainingHours = 40,
				TrainingLevel = new TrainingLevel("Training Level"),
				TrainingProgramState = TrainingProgramStates.Complete
			};

			var trainingProvider = new TrainingProvider(trainingProgram)
			{
				Id = 1,
				Name = "Training Provider Name",
				ContactEmail = "provider@test.com",
				ContactFirstName = "John",
				ContactLastName = "Doe",
				ContactPhoneNumber = "(123) 123-1232",
				TrainingAddress = new ApplicationAddress(applicant.MailingAddress),
				TrainingProviderState = TrainingProviderStates.Complete,
				TrainingProviderType = new TrainingProviderType("Training Provider Type")
			};

			var provider = new TrainingProvider(grantApplication)
			{
				Id = 2,
				Name = "Service Provider Name",
				ContactEmail = "provider@test.com",
				ContactFirstName = "John",
				ContactLastName = "Doe",
				ContactPhoneNumber = "(123) 123-1232",
				TrainingAddress = new ApplicationAddress(applicant.MailingAddress),
				TrainingProviderState = TrainingProviderStates.Complete,
				TrainingProviderType = new TrainingProviderType("Training Provider Type")
			};

			var skillsTrainingServiceCategory = new ServiceCategory("Skills Training", ServiceTypes.SkillsTraining, true, false, 0, 0, 1, 5) { Id = 1 };
			var skillsTrainingExpenseType = new EligibleExpenseType(skillsTrainingServiceCategory, ExpenseTypes.NotParticipantLimited);
			var skillsTrainingCost = new EligibleCost(grantApplication, skillsTrainingExpenseType, 3333.33m, 10);
			skillsTrainingCost.Breakdowns.Add(new EligibleCostBreakdown(skillsTrainingCost, new EligibleExpenseBreakdown(new ServiceLine("Service Line", "Service Breakdowns", 1) { ServiceCategory = skillsTrainingServiceCategory }, skillsTrainingExpenseType), 3333.33m));
			trainingProgram.EligibleCostBreakdown = skillsTrainingCost.Breakdowns.First();

			EligibleCost essCost = null;
			EligibleCost adminCost = null;

			var serviceTypes = staticDataService.GetServiceTypes();
			//try to get values from the database
			if (serviceTypes != null)
			{
				var employmentServicesAndSupports = serviceTypes.FirstOrDefault(f => f.Id == ServiceTypes.EmploymentServicesAndSupports);
				if (employmentServicesAndSupports != null)
				{
					var employmentSupportServices = employmentServicesAndSupports.ServiceCategories.FirstOrDefault(f => f.ServiceTypeId == ServiceTypes.EmploymentServicesAndSupports);
					if (employmentSupportServices != null)
					{
						var newExpenseType = new EligibleExpenseType(employmentSupportServices, ExpenseTypes.NotParticipantLimited);
						essCost = new EligibleCost(grantApplication, newExpenseType, 5555.55m, 10);
					}
				}

				var admin = serviceTypes.FirstOrDefault(f => f.Id == ServiceTypes.Administration);
				if (admin != null)
				{
					var adminFees = admin.ServiceCategories.FirstOrDefault(f => f.ServiceTypeId == ServiceTypes.Administration);
					if (adminFees != null)
					{
						var newExpenseType = new EligibleExpenseType(adminFees, ExpenseTypes.NotParticipantLimited);
						adminCost = new EligibleCost(grantApplication, newExpenseType, 560.55m, 2);
					}
				}
			}

			//if we fail to get values from the database then use static values
			if (essCost == null)
				essCost = new EligibleCost(grantApplication, new EligibleExpenseType(new ServiceCategory("Employment Support Services", ServiceTypes.EmploymentServicesAndSupports, true, false, 0, 0, 1, 5), ExpenseTypes.NotParticipantLimited), 5555.55m, 10);

			if (adminCost == null)
				adminCost = new EligibleCost(grantApplication, new EligibleExpenseType(new ServiceCategory("Administration", ServiceTypes.Administration, true, false, 0, 0, 1, 5), ExpenseTypes.NotParticipantLimited), 560.55m, 2);

			var pssCost = new EligibleCost(grantApplication, new EligibleExpenseType(new ServiceCategory("Participant Financial Supports", ServiceTypes.EmploymentServicesAndSupports, true, false, 0, 0, 1, 5), ExpenseTypes.NotParticipantLimited), 560.55m, 2);

			provider.EligibleCost = essCost;

			grantApplication.TrainingCost.EligibleCosts.Add(skillsTrainingCost);
			grantApplication.TrainingCost.EligibleCosts.Add(essCost);
			grantApplication.TrainingCost.EligibleCosts.Add(pssCost);
			grantApplication.TrainingCost.EligibleCosts.Add(adminCost);
			grantApplication.TrainingCost.RecalculateEstimatedCosts();

			switch (ApplicationStateInternal)
			{
				case ApplicationStateInternal.Draft:
				case ApplicationStateInternal.New:
				case ApplicationStateInternal.Unfunded:
				case ApplicationStateInternal.ApplicationWithdrawn:
				case ApplicationStateInternal.ApplicationDenied:
					break;

				case ApplicationStateInternal.PendingAssessment:
				case ApplicationStateInternal.UnderAssessment:
				case ApplicationStateInternal.ReturnedToAssessment:
				case ApplicationStateInternal.RecommendedForApproval:
				case ApplicationStateInternal.RecommendedForDenial:
					AddAssessment(grantApplication);
					break;

				case ApplicationStateInternal.CancelledByMinistry:
				case ApplicationStateInternal.CancelledByAgreementHolder:
				case ApplicationStateInternal.OfferWithdrawn:
				case ApplicationStateInternal.OfferIssued:
				case ApplicationStateInternal.AgreementAccepted:
				case ApplicationStateInternal.AgreementRejected:
				case ApplicationStateInternal.ChangeRequestDenied:
					AddAgreement(grantApplication);
					break;

				case ApplicationStateInternal.ChangeRequest:
				case ApplicationStateInternal.ChangeForApproval:
				case ApplicationStateInternal.ChangeForDenial:
				case ApplicationStateInternal.ChangeReturned:
					AddParticipants(grantApplication);

					new TrainingProvider(trainingProvider)
					{
						Id = 3,
						Name = "Change Training Provider Name",
						ContactEmail = "change@test.com",
						ContactFirstName = "John",
						ContactLastName = "Doe",
						ContactPhoneNumber = "(123) 123-1232",
						TrainingAddress = new ApplicationAddress(applicant.MailingAddress),
						TrainingProviderType = new TrainingProviderType("Training Provider Type")
					};

					break;

				case ApplicationStateInternal.NewClaim:
				case ApplicationStateInternal.ClaimAssessEligibility:
				case ApplicationStateInternal.ClaimAssessReimbursement:
				case ApplicationStateInternal.ClaimReturnedToApplicant:
				case ApplicationStateInternal.ClaimDenied:
					AddParticipants(grantApplication);
					AddClaim(grantApplication);
					break;

				case ApplicationStateInternal.ClaimApproved:
					AddParticipants(grantApplication);
					AddClaim(grantApplication);
					break;

				case ApplicationStateInternal.CompletionReporting:
				case ApplicationStateInternal.Closed:
					AddParticipants(grantApplication);
					AddPaymentRequest(grantApplication);
					break;
			}

			return new TestEntities
			{
				GrantApplication = grantApplication,
				GrantOpening = grantOpening,
				GrantProgram = grantProgram,
				FiscalYear = fiscalYear,
				Applicant = applicant,
				Participant = grantApplication.ParticipantForms.FirstOrDefault(),
				Organization = applicantOrganization,
				TrainingProgram = trainingProgram,
				TrainingProvider = trainingProvider
			};
		}

		private static void AddAssessment(GrantApplication grantApplication)
		{
			grantApplication.TrainingCost.CopyEstimatedIntoAgreed();
			grantApplication.TrainingCost.RecalculateAgreedCosts();
		}

		private static void AddAgreement(GrantApplication grantApplication)
		{
			AddAssessment(grantApplication);
			grantApplication.GrantAgreement = new GrantAgreement(grantApplication)
			{
				DateAccepted = grantApplication.StartDate,
				StartDate = grantApplication.StartDate,
				EndDate = grantApplication.EndDate,
				CompletionReportingDueDate = grantApplication.StartDate.AddDays(5),
				ReimbursementClaimDueDate = grantApplication.StartDate.AddDays(6)
			};
		}

		private static void AddParticipants(GrantApplication grantApplication)
		{
			AddAgreement(grantApplication);

			grantApplication.ParticipantForms.Add(new ParticipantForm(grantApplication, Guid.NewGuid()) { FirstName = "John P.", LastName = "Doe", EmailAddress = "john.doe@participant.com" });
			grantApplication.ParticipantForms.Add(new ParticipantForm(grantApplication, Guid.NewGuid()) { FirstName = "Jane P.", LastName = "Doe", EmailAddress = "jane.doe@participant.com" });
		}

		private static Claim AddClaim(GrantApplication grantApplication, ClaimState claimState = ClaimState.Unassessed)
		{
			var claim = new Claim(1, 1, grantApplication, grantApplication.GrantOpening.GrantStream.ProgramConfiguration.ClaimTypeId)
			{
				ClaimState = grantApplication.ApplicationStateInternal == ApplicationStateInternal.ClaimApproved ? ClaimState.ClaimApproved : grantApplication.ApplicationStateInternal == ApplicationStateInternal.ClaimDenied ? ClaimState.ClaimDenied : claimState,
				ParticipantForms = grantApplication.ParticipantForms
			};

			grantApplication.TrainingCost.EligibleCosts.ForEach(ec =>
			{
				var cost = new ClaimEligibleCost(claim, ec)
				{
					ClaimCost = ec.AgreedMaxCost,
					ClaimParticipants = ec.AgreedMaxParticipants,
					AssessedCost = ec.AgreedMaxCost,
					AssessedParticipants = ec.AgreedMaxParticipants
				};
				claim.EligibleCosts.Add(cost);
			});

			claim.RecalculateClaimedCosts();
			claim.RecalculateAssessedCosts();

			grantApplication.Claims.Add(claim);
			return claim;
		}

		private static void AddPaymentRequest(GrantApplication grantApplication)
		{
			var claim = AddClaim(grantApplication, ClaimState.ClaimApproved);

			claim.PaymentRequests.Add(new PaymentRequest(grantApplication, new PaymentRequestBatch(grantApplication.GrantOpening.GrantStream.GrantProgram, "batch", PaymentBatchTypes.PaymentRequest, new InternalUser()), claim));
		}
	}
}
