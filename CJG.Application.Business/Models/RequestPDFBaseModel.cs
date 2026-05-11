using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CJG.Core.Entities;

namespace CJG.Application.Business.Models
{
	public abstract class RequestPDFBaseModel
	{
		public string RecipientName { get; set; }

		public string InvoiceAmount { get; set; }

		public string ContractNumber { get; set; }

		public string ClientNumb { get; set; }
		public string RESP { get; set; }
		public string ServiceLine { get; set; }
		public string STOB { get; set; }
		public string ProjectCode { get; set; }

		public bool IsAccrual { get; set; }

		public bool Duplicate { get; set; }

		public List<PaymentRequestAccountCodeModel> AccountCodes { get; set; } = new List<PaymentRequestAccountCodeModel>();

		public RequestPDFBaseModel(PaymentRequest paymentRequest, bool duplicate)
		{
			if (paymentRequest == null)
				throw new ArgumentNullException(nameof(paymentRequest));

			InvoiceAmount = Math.Abs(paymentRequest.PaymentAmount).ToString("C2", new CultureInfo("en-US", false));
			ContractNumber = paymentRequest.GrantApplication.FileNumber;
			Duplicate = duplicate;
			IsAccrual = paymentRequest.PaymentType == PaymentTypes.Accrual;
			ClientNumb = paymentRequest.GLClientNumber;
			RESP = paymentRequest.GLRESP;
			ServiceLine = paymentRequest.GLServiceLine;
			STOB = paymentRequest.GLSTOB;
			ProjectCode = paymentRequest.GLProjectCode;

			// If we have no account codes, serve out the default stored value
			if (!paymentRequest.PaymentRequestAccountCodes.Any())
			{
				AccountCodes.Add(new PaymentRequestAccountCodeModel
				{
					ClientNumb = paymentRequest.GLClientNumber,
					RESP = paymentRequest.GLRESP,
					ServiceLine = paymentRequest.GLServiceLine,
					STOB = paymentRequest.GLSTOB,
					ProjectCode = paymentRequest.GLProjectCode
				});
			}
			else
			{
				foreach (var accountCode in paymentRequest.PaymentRequestAccountCodes)
				{
					AccountCodes.Add(new PaymentRequestAccountCodeModel
					{
						ClientNumb = accountCode.GLClientNumber,
						RESP = accountCode.GLRESP,
						ServiceLine = accountCode.GLServiceLine,
						STOB = accountCode.GLSTOB,
						ProjectCode = accountCode.GLProjectCode
					});
				}
			}
		}
	}
}
