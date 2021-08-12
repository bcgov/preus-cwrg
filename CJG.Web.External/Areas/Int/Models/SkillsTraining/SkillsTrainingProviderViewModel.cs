using System;
using System.Linq;
using System.Security.Principal;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Areas.Int.Models.Attachments;
using CJG.Web.External.Helpers;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.SkillsTraining
{
    public class SkillsTrainingProviderViewModel : BaseViewModel
	{
		public bool IsValidated { get; set; }
		public bool CanValidate { get; set; }
		public bool CanEdit { get; set; }
		public bool CanRemove { get; set; }
		public bool CanRecommendChangeRequest { get; set; }

		public string RowVersion { get; set; }
		public int? EligibleCostId { get; set; }

		public string Name { get; set; }
		public string ChangeRequestReason { get; set; }
		public int TrainingProgramId { get; set; }
		public int? TrainingProviderTypeId { get; set; }
		public TrainingProviderPrivateSectorValidationTypes PrivateSectorValidationType { get; set; }

		public TrainingProviderStates TrainingProviderState { get; set; }
		public int? TrainingProviderInventoryId { get; set; }

		public string ContactFirstName { get; set; }
		public string ContactLastName { get; set; }
		public string ContactEmail { get; set; }
		public string ContactPhone { get; set; }
		public string ContactPhoneAreaCode { get; set; }
		public string ContactPhoneExchange { get; set; }
		public string ContactPhoneNumber { get; set; }
		public string ContactPhoneExtension { get; set; }

		public int? TrainingAddressId { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string City { get; set; }
		public string PostalCode { get; set; }
		public string ZipCode { get; set; }
		public string RegionId { get; set; }
		public string Region { get; set; }
		public string CountryId { get; set; }

		public int? TrainingProviderAddressId { get; set; }
		public string AddressLine1TrainingProvider { get; set; }
		public string AddressLine2TrainingProvider { get; set; }
		public string CityTrainingProvider { get; set; }
		public string PostalCodeTrainingProvider { get; set; }
		public string ZipCodeTrainingProvider { get; set; }
		public string RegionIdTrainingProvider { get; set; }
		public string RegionTrainingProvider { get; set; }
		public string CountryIdTrainingProvider { get; set; }

		public bool TrainingOutsideBC { get; set; }
		public string BusinessCase { get; set; }
		public int? BusinessCaseDocumentId { get; set; }
		public AttachmentViewModel BusinessCaseDocument { get; set; }

		public int? CourseOutlineDocumentId { get; set; }
		public AttachmentViewModel CourseOutlineDocument { get; set; }

		public int? ProofOfQualificationsDocumentId { get; set; }
		public AttachmentViewModel ProofOfQualificationsDocument { get; set; }

		public int[] SelectedDeliveryMethodIds { get; set; }

		public int? ProofOfInstructorQualifications { get; set; }
		public int? CourseOutline { get; set; }

		public SkillsTrainingProviderViewModel() { }

		public SkillsTrainingProviderViewModel(TrainingProvider trainingProvider, IPrincipal user, IStaticDataService staticDataService)
		{
			if (trainingProvider == null)
				throw new ArgumentNullException(nameof(trainingProvider));

			if (user == null)
				throw new ArgumentNullException(nameof(user));

			Id = trainingProvider.Id;
			RowVersion = trainingProvider.RowVersion != null ? Convert.ToBase64String(trainingProvider.RowVersion) : null;

			IsValidated = trainingProvider.TrainingProviderInventoryId.HasValue;
			CanValidate = user.CanPerformAction(trainingProvider, ApplicationWorkflowTrigger.ValidateTrainingProvider);
			CanEdit = user.CanPerformAction(trainingProvider, ApplicationWorkflowTrigger.EditTrainingProvider);
			CanRemove = user.CanPerformAction(trainingProvider, ApplicationWorkflowTrigger.AddRemoveTrainingProvider);
			CanRecommendChangeRequest = user.CanPerformAction(trainingProvider, ApplicationWorkflowTrigger.EditTrainingProvider);

			EligibleCostId = trainingProvider.EligibleCostId;
			TrainingProviderInventoryId = trainingProvider.TrainingProviderInventoryId;
			TrainingProgramId = trainingProvider.TrainingProgram?.Id
			                    ?? throw new ArgumentException("A skills training component provider must be associated with a program.", nameof(trainingProvider));
			Name = trainingProvider.Name;
			TrainingProviderState = trainingProvider.TrainingProviderState;
			TrainingProviderTypeId = trainingProvider.TrainingProviderTypeId;
			PrivateSectorValidationType = trainingProvider.TrainingProviderType.PrivateSectorValidationType;
			ProofOfInstructorQualifications = trainingProvider.TrainingProviderType.ProofOfInstructorQualifications;
			CourseOutline = trainingProvider.TrainingProviderType.CourseOutline;

			ChangeRequestReason = trainingProvider.ChangeRequestReason;

			ContactEmail = trainingProvider.ContactEmail;
			ContactFirstName = trainingProvider.ContactFirstName;
			ContactLastName = trainingProvider.ContactLastName;
			ContactPhone = trainingProvider.ContactPhoneNumber;
			ContactPhoneAreaCode = trainingProvider.ContactPhoneNumber.GetPhoneAreaCode();
			ContactPhoneExchange = trainingProvider.ContactPhoneNumber.GetPhoneExchange();
			ContactPhoneNumber = trainingProvider.ContactPhoneNumber.GetPhoneNumber();
			ContactPhoneExtension = trainingProvider.ContactPhoneExtension;

			if (trainingProvider.TrainingAddress != null)
			{
				TrainingAddressId = trainingProvider.TrainingAddressId;
				AddressLine1 = trainingProvider.TrainingAddress.AddressLine1;
				AddressLine2 = trainingProvider.TrainingAddress.AddressLine2;
				City = trainingProvider.TrainingAddress.City;
				CountryId = trainingProvider.TrainingAddress.CountryId;
				RegionId = trainingProvider.TrainingAddress.RegionId;
				Region = staticDataService.GetRegion(CountryId, RegionId).Name;
				PostalCode = trainingProvider.TrainingAddress.PostalCode;
				ZipCode = trainingProvider.TrainingAddress.PostalCode;
			}

			if (trainingProvider.TrainingProviderAddress != null)
			{
				TrainingProviderAddressId = trainingProvider.TrainingProviderAddressId;
				AddressLine1TrainingProvider = trainingProvider.TrainingProviderAddress.AddressLine1;
				AddressLine2TrainingProvider = trainingProvider.TrainingProviderAddress.AddressLine2;
				CityTrainingProvider = trainingProvider.TrainingProviderAddress.City;
				CountryIdTrainingProvider = trainingProvider.TrainingProviderAddress.CountryId;
				RegionIdTrainingProvider = trainingProvider.TrainingProviderAddress.RegionId;
				RegionTrainingProvider = staticDataService.GetRegion(CountryIdTrainingProvider, RegionIdTrainingProvider).Name;
				PostalCodeTrainingProvider = trainingProvider.TrainingProviderAddress.PostalCode;
				ZipCodeTrainingProvider = trainingProvider.TrainingProviderAddress.PostalCode;
			}

			TrainingOutsideBC = trainingProvider.TrainingOutsideBC;
			BusinessCase = trainingProvider.BusinessCase;
			BusinessCaseDocumentId = trainingProvider.BusinessCaseDocumentId;
			BusinessCaseDocument = trainingProvider.BusinessCaseDocumentId.HasValue ? new AttachmentViewModel(trainingProvider.BusinessCaseDocument) : null;

			CourseOutlineDocumentId = trainingProvider.CourseOutlineDocumentId;
			CourseOutlineDocument = trainingProvider.CourseOutlineDocumentId.HasValue ? new AttachmentViewModel(trainingProvider.CourseOutlineDocument) : null;

			ProofOfQualificationsDocumentId = trainingProvider.ProofOfQualificationsDocumentId;
			ProofOfQualificationsDocument = trainingProvider.ProofOfQualificationsDocumentId.HasValue ? new AttachmentViewModel(trainingProvider.ProofOfQualificationsDocument) : null;
			SelectedDeliveryMethodIds = trainingProvider.TrainingProgram.DeliveryMethods.Select(dm => dm.Id).ToArray();
		}
	}
}