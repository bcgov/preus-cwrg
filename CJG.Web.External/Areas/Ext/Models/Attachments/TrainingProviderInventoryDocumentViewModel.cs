using System;
using CJG.Application.Business.Models;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Ext.Models.Attachments
{
    public class TrainingProviderInventoryDocumentViewModel : BaseViewModel
	{
		public int TrainingProviderInventoryId { get; set; }
		public string Guid { get; set; }
		public AttachmentModel Attachment { get; set; } = new AttachmentModel();

		public TrainingProviderInventoryDocumentViewModel()
		{
			Guid = System.Guid.NewGuid().ToString();
		}

		public TrainingProviderInventoryDocumentViewModel(TrainingProviderInventory trainingProviderInventory, Attachment attachment) : this(trainingProviderInventory)
		{
			if (attachment == null)
				throw new ArgumentNullException(nameof(attachment));

			Attachment.Id = attachment.Id;
			Attachment.Name = attachment.FileName;
			Attachment.Description = attachment.Description;
			Attachment.RowVersion = attachment.RowVersion != null ? Convert.ToBase64String(attachment.RowVersion) : null;
		}

		public TrainingProviderInventoryDocumentViewModel(TrainingProviderInventory trainingProviderInventory)
		{
			if (trainingProviderInventory == null)
				throw new ArgumentNullException(nameof(trainingProviderInventory));

			TrainingProviderInventoryId = trainingProviderInventory.Id;
		}
	}
}