﻿using CJG.Application.Services;
using CJG.Core.Entities;

namespace CJG.Web.External.Areas.Int.Models.WorkQueue
{
    public class WorkQueueFilterAttributeViewModel
	{
		public int Id { get; set; }
		public string Key { get; set; }
		public string Value { get; set; }
		public FilterOperator Operator { get; set; }

		public WorkQueueFilterAttributeViewModel() { }

		public WorkQueueFilterAttributeViewModel(InternalUserFilterAttribute attribute)
		{
			Utilities.MapProperties(attribute, this);
		}
	}
}