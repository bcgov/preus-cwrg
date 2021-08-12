using System.ComponentModel;

namespace CJG.Core.Entities
{
	public enum AttachmentType
	{
		// Regular Attachments
		[Description("Applicant Attachment")]
		Attachment = 0,

		[Description("Ministry Attachment")]
		Document = 1,


		// Success Story Attachments
		[Description("Consent Form")]
		SuccessStoryConsentForm = 10,

		[Description("Testimonial")]
		SuccessStoryTestimonial = 11,
	}
}