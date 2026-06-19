using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.ParticipantFundingStreams
{
	public class ParticipantFundingStreamModel : BaseViewModel
	{
		[Required]
		public string Name { get; set; }

		public bool IsActive { get; set; }

		public int RowSequence { get; set; }

		public string RowVersion { get; set; }

		// Set externally on model 
		public bool? IsInUse { get; set; }

		public bool IsSystem { get; set; }

		public ParticipantFundingStreamModel()
		{
		}

		public ParticipantFundingStreamModel(ParticipantFundingStream fundingStream)
		{
			Id = fundingStream.Id;
			Name = fundingStream.Caption;
			IsActive = fundingStream.IsActive;
			RowSequence = fundingStream.RowSequence;
			RowVersion = Convert.ToBase64String(fundingStream.RowVersion);

			// These values have special meaning in the application, and cannot be removed or deactivated by a Director.
			var reservedFundingStreams = new List<int>
			{
				(int) Core.Entities.ParticipantFundingStreams.ComprehensiveTariffs,
				(int) Core.Entities.ParticipantFundingStreams.NotApplicable
			};

			IsSystem = reservedFundingStreams.Contains(Id);
		}

	}
}