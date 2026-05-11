using System;
using CJG.Core.Entities;

namespace CJG.Application.Business.Models
{
	public class PaymentRequestPDFModel : RequestPDFBaseModel
	{
		public string Recipient { get; set; }
		public string RecipientAddress { get; set; }

		public string InvoiceDate { get; set; }
		public string InvoiceNumber { get; set; }

		public string Receiver { get; set; }

		public string PaymentType { get; set; }

		public string BusinessNumber { get; set; }
		public bool BusinessNumberVerified { get; set; }

		public PaymentRequestPDFModel(PaymentRequest paymentRequest, bool duplicate) : base(paymentRequest, duplicate)
		{
			if (paymentRequest == null)
				throw new ArgumentNullException(nameof(paymentRequest));

			var application = paymentRequest.GrantApplication;
			var address = application.ApplicantMailingAddressId == null ? application.OrganizationAddress : application.ApplicantMailingAddress;

			Recipient = application.OrganizationLegalName;
			RecipientName = $"{application.ApplicantFirstName} {application.ApplicantLastName}";
			RecipientAddress = $"{address.AddressLine1 + (string.IsNullOrEmpty(address.AddressLine2) ? "" : " " + address.AddressLine2)}<br />{address.City}, {address.RegionId} {address.PostalCode}";

			InvoiceDate = paymentRequest.Claim.DateSubmitted?.ToString("yyyy-MM-dd");
			InvoiceNumber = paymentRequest.DocumentNumber;

			Receiver = $"{paymentRequest.Claim.Assessor.FirstName} {paymentRequest.Claim.Assessor.LastName}";

			BusinessNumber = application.Organization.BusinessNumber;
			BusinessNumberVerified = application.Organization.BusinessNumberVerified == true;

			PaymentType = paymentRequest.PaymentType.ToString();
		}
	}
}
