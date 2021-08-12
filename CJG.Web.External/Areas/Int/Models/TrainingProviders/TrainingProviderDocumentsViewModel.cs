using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using CJG.Core.Entities;
using CJG.Web.External.Areas.Int.Models.Attachments;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.TrainingProviders
{
	public class TrainingProviderDocumentsViewModel : BaseViewModel
	{
		public string RowVersion { get; set; }
		public int MaximumDocuments { get; set; } = 50;
		public string AllowedFileTypes { get; set; }
		public string PermittedAttachmentTypes { get; set; }
		public IEnumerable<AttachmentViewModel> Documents { get; set; }

		public TrainingProviderDocumentsViewModel()
		{
		}

		public TrainingProviderDocumentsViewModel(TrainingProviderInventory trainingProviderInventory)
		{
			if (trainingProviderInventory == null)
				throw new ArgumentNullException(nameof(trainingProviderInventory));

			Id = trainingProviderInventory.Id;
			RowVersion = Convert.ToBase64String(trainingProviderInventory.RowVersion);

			AllowedFileTypes = GetAllowedFileTypes();
			MaximumDocuments = 50;
			PermittedAttachmentTypes = ConfigurationManager.AppSettings["HistoryPermittedAttachmentTypes"];

			Documents = trainingProviderInventory.Documents
				.Select(a => new AttachmentViewModel(a));
		}

		private static string GetAllowedFileTypes()
		{
			var allowedFileTypes = ConfigurationManager.AppSettings["HistoryPermittedAttachmentTypes"];

			if (string.IsNullOrWhiteSpace(allowedFileTypes))
				return string.Empty;

			var types = allowedFileTypes.ToUpper().Replace(".", string.Empty).Split('|');
			return string.Join(", ", types);
		}
	}
}