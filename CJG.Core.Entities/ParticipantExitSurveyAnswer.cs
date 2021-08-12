using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class ParticipantExitSurveyAnswer : EntityBase, ISurveyAnswer
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public int ParticipantExitSurveyQuestionOptionId { get; set; }
		[ForeignKey(nameof(ParticipantExitSurveyQuestionOptionId))]
		public virtual ParticipantExitSurveyQuestionOption ParticipantExitSurveyQuestionOption { get; set; }

		public int ParticipantFormId { get; set; }
		[ForeignKey(nameof(ParticipantFormId))]
		public virtual ParticipantForm ParticipantForm { get; set; }

		public string OptionTextDisplayed { get; set; }
		public bool Answer { get; set; }
		public string TextAnswer { get; set; } // This holds either the 'Other' answer, the 'FreeText' answer, or the ReplacementToken Translated answer 
	}
}