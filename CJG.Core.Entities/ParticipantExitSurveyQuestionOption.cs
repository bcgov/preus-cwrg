using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class ParticipantExitSurveyQuestionOption : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public int ParticipantExitSurveyQuestionId { get; set; }

		[ForeignKey(nameof(ParticipantExitSurveyQuestionId))]
		public virtual ParticipantExitSurveyQuestion ParticipantExitSurveyQuestion { get; set; }

		public string OptionText { get; set; }
		public string ReplacementToken { get; set; } // Token like [Training Provider] to be replaced with the actual Training Provider used
		public bool AllowOther { get; set; }
		public int Sequence { get; set; }

		[DefaultValue("true")]
		public bool IsActive { get; set; }
	}
}