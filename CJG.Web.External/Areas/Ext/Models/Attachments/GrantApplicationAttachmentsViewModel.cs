﻿using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Ext.Models.Attachments
{
    public class GrantApplicationAttachmentsViewModel : BaseViewModel
	{
		public string RowVersion { get; set; }
		public string AttachmentsHeader { get; set; }
		public bool AttachmentsIsEnabled { get; set; }
		public bool AttachmentsRequired { get; set; }
		public int AttachmentsMaximum { get; set; }
		public string AttachmentsUserGuidance { get; set; }
		public bool? NotRequestingESS { get; set; }
		public IEnumerable<AttachmentViewModel> Attachments { get; set; }

		public GrantApplicationAttachmentsViewModel() { }

		public GrantApplicationAttachmentsViewModel(GrantApplication grantApplication)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			Id = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.RowVersion);
			AttachmentsHeader = grantApplication.GrantOpening.GrantStream.AttachmentsHeader;
			AttachmentsIsEnabled = grantApplication.GrantOpening.GrantStream.AttachmentsIsEnabled;
			AttachmentsRequired = grantApplication.GrantOpening.GrantStream.AttachmentsRequired;
			AttachmentsMaximum = grantApplication.GrantOpening.GrantStream.AttachmentsMaximum;
			AttachmentsUserGuidance = grantApplication.GrantOpening.GrantStream.AttachmentsUserGuidance;
			NotRequestingESS = grantApplication.NotRequestingESS;
			Attachments = grantApplication.Attachments
				.Where(a => a.AttachmentType == AttachmentType.Attachment)
				.Select(a => new AttachmentViewModel(a));
		}
	}
}
