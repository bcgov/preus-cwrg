using System.Linq;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;

namespace CJG.Web.External.Areas.Int.Models
{
    public class ParticipantInfoViewModel
	{
		public int GrantApplicationId { get; set; }
		public string FileNo { get; set; }
		public string OrganizationName { get; set; }
		public string ReportingDate { get; set; }
		public string EmployerAdministrator { get; set; }
		public string TrainingStartDate { get; set; }
		public string EmployerAdministratorEmail { get; set; }
		public int TotalParticipants { get; set; }
		public string EmployerAdministratorPhone { get; set; }

		public ParticipantContactInfoViewModel ContactInfo { get; set; }
		public ParticipantEmploymentInfoViewModel EmployerInfo { get; set; }
		public ParticipantSurveyInfoViewModel SurveyInfo { get; set; }

		public ParticipantInfoViewModel(ParticipantForm participant,
			INationalOccupationalClassificationService nationalOccupationalClassificationService,
			IUserService userService,
			ISurveyService surveyService)
		{
			GrantApplicationId = participant.GrantApplication.Id;
			FileNo = participant.GrantApplication.FileNumber;
			OrganizationName = participant.GrantApplication.OrganizationLegalName;

			var daysLate = (int)(participant.GrantApplication.StartDate - participant.DateAdded).TotalDays;
			ReportingDate = $"{participant.DateAdded.Date:yyyy-MM-dd} {((daysLate > 5) ? "" : $"(Late {daysLate} days)")}";
			participant.GrantApplication.Organization.Users.Any();

			var businessContact = participant.GrantApplication.BusinessContactRoles.FirstOrDefault(c => c.GrantApplicationId > 0);
			var employerAdministrator = userService.GetUser(businessContact.UserId);
			if (employerAdministrator != null)
			{
				EmployerAdministrator = $"{employerAdministrator.FirstName} {employerAdministrator.LastName}";
				EmployerAdministratorEmail = employerAdministrator.EmailAddress;
				EmployerAdministratorPhone = employerAdministrator.PhoneNumber + (string.IsNullOrWhiteSpace(employerAdministrator.PhoneNumber) ? "" : employerAdministrator.PhoneExtension);
			}

			TrainingStartDate = $"{participant.GrantApplication.TrainingStartDate.ToLocalTime():yyyy-MM-dd}";
			TotalParticipants = participant.GrantApplication.ParticipantForms.Count;
			ContactInfo = new ParticipantContactInfoViewModel(participant);
			EmployerInfo = new ParticipantEmploymentInfoViewModel(participant, nationalOccupationalClassificationService);
			EmployerInfo = new ParticipantEmploymentInfoViewModel(participant, nationalOccupationalClassificationService);
			SurveyInfo = new ParticipantSurveyInfoViewModel(participant, surveyService);
		}

		public ParticipantInfoViewModel()
		{
			ContactInfo = new ParticipantContactInfoViewModel();
			EmployerInfo = new ParticipantEmploymentInfoViewModel();
			SurveyInfo = new ParticipantSurveyInfoViewModel();
		}
	}
}