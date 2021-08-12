using System;
using CJG.Core.Entities;

namespace CJG.Web.External.Areas.Ext.Models.ChangeRequest
{
    public class ChangeRequestSkillTrainingViewModel
    {
        public int TrainingProgramId { get; set; }

        public string CourseTitle { get; set; }
        public string TrainingProviderName { get; set; }
        public int TrainingProviderId { get; set; }

        public string RequestTrainingProviderName { get; set; }
        public int RequestTrainingProviderId { get; set; }

        public ChangeRequestProgramTrainingDateViewModel ProgramTrainingDate { get; set; }

        public ChangeRequestSkillTrainingViewModel()
        {

        }
        public ChangeRequestSkillTrainingViewModel(TrainingProgram trainingProgram)
        {
            if (trainingProgram == null)
	            throw new ArgumentNullException(nameof(trainingProgram));

            TrainingProgramId = trainingProgram.Id;
            CourseTitle = trainingProgram.CourseTitle;

            TrainingProviderName = trainingProgram.TrainingProvider.Name;
            TrainingProviderId = trainingProgram.TrainingProvider.Id;

            var requestTrainingProvider = trainingProgram.RequestedTrainingProvider;
            if (requestTrainingProvider != null)
            {
                RequestTrainingProviderName = requestTrainingProvider.Name;
                RequestTrainingProviderId = requestTrainingProvider.Id;
            }

            ProgramTrainingDate = new ChangeRequestProgramTrainingDateViewModel(trainingProgram);
        }
    }
}