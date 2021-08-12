using System;
using CJG.Application.Business.Models;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Ext.Models.Attachments
{
	public class OrganizationDocumentViewModel : BaseViewModel
	{
		public int OrganizationId { get; set; }
		public string Guid { get; set; }
		public AttachmentModel Attachment { get; set; } = new AttachmentModel();

		public OrganizationDocumentViewModel()
		{
			Guid = System.Guid.NewGuid().ToString();
		}

		public OrganizationDocumentViewModel(Organization organization, Attachment attachment) : this(organization)
		{
			if (attachment == null)
				throw new ArgumentNullException(nameof(attachment));

			Attachment.Id = attachment.Id;
			Attachment.Name = attachment.FileName;
			Attachment.Description = attachment.Description;
			Attachment.RowVersion = attachment.RowVersion != null ? Convert.ToBase64String(attachment.RowVersion) : null;
		}

		public OrganizationDocumentViewModel(Organization organization)
		{
			if (organization == null)
				throw new ArgumentNullException(nameof(organization));

			OrganizationId = organization.Id;
		}
	}
}