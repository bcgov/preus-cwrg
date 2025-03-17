using System.ComponentModel;

namespace CJG.Core.Entities
{
	public enum AttachmentDocumentType
	{
		[Description("Unspecified")]
		Unspecified = 0,

		// Required Document Types
		[Description("Project Description")]
		ProjectDescription = 1,

		[Description("Employer Support Form")]
		EmployerSupportForm = 2,

		[Description("ST Quote")]
		STQuote = 3,

		[Description("ESS Quote")]
		ESSQuote = 4,


		// Optional Document Types
		[Description("Instructor qualification")]
		InstructorQualification = 10,

		[Description("Certificate of Insurance")]
		CertificateOfInsurance = 11
	}
}