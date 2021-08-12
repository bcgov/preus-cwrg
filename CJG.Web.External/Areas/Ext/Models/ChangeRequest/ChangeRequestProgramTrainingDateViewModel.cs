using System;
using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Ext.Models.ChangeRequest
{
	public class ChangeRequestProgramTrainingDateViewModel : BaseViewModel
	{
		public string RowVersion { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public ChangeRequestProgramTrainingDateViewModel()
		{

		}

		public ChangeRequestProgramTrainingDateViewModel(TrainingProgram trainingProgram)
		{
			if (trainingProgram == null)
				throw new ArgumentNullException(nameof(trainingProgram));

			Id = trainingProgram.Id;
			StartDate = trainingProgram.StartDate.ToLocalTime();
			EndDate = trainingProgram.EndDate.ToLocalTime();
			RowVersion = Convert.ToBase64String(trainingProgram.RowVersion);
		}
	}
}