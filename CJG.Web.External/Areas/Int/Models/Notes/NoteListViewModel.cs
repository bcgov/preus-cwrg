using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Security.Principal;
using System.Web;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.Notes
{
	public class NoteListViewModel: BaseViewModel
	{
		public string RowVersion { get; set; }
		public IEnumerable<NoteViewModel> Notes { get; set; }

		public string PermittedAttachmentTypes { get; set; }

		public int MaxUploadSize { get; set; }

		public NoteListViewModel() { }

		public NoteListViewModel(GrantApplication grantApplication, IPrincipal user)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			Id = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);
			Notes = grantApplication.Notes
				.OrderByDescending(n => n.DateAdded)
				.Select(n => new NoteViewModel(n, user, false, true))
				.ToArray();
			MaxUploadSize = int.Parse(ConfigurationManager.AppSettings["MaxUploadSizeInBytes"]);
			var permittedAttachmentTypes = ConfigurationManager.AppSettings["NotesPermittedAttachmentTypes"];
			PermittedAttachmentTypes = permittedAttachmentTypes;
		}

		public NoteListViewModel(GrantApplication grantApplication, GrantApplicationEvaluation grantApplicationEvaluation, IPrincipal user)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			Id = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);
			Notes = grantApplication.EvaluationNotes
				.OrderByDescending(n => n.DateAdded)
				.Select(n => new NoteViewModel(n, user, false, true))
				.ToArray();
			MaxUploadSize = int.Parse(ConfigurationManager.AppSettings["MaxUploadSizeInBytes"]);
			var permittedAttachmentTypes = ConfigurationManager.AppSettings["NotesPermittedAttachmentTypes"];
			PermittedAttachmentTypes = permittedAttachmentTypes;
		}
	}
}