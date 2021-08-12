using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class ParticipantWithdrawalSurveyQuestionOption : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public int ParticipantWithdrawalSurveyQuestionId { get; set; }

		[ForeignKey(nameof(ParticipantWithdrawalSurveyQuestionId))]
		public virtual ParticipantWithdrawalSurveyQuestion ParticipantWithdrawalSurveyQuestion { get; set; }

		public string OptionText { get; set; }
		public string ReplacementToken { get; set; } // Token like [Training Provider] to be replaced with the actual Training Provider used
		public bool AllowOther { get; set; }
		public int Sequence { get; set; }

		[DefaultValue("true")]
		public bool IsActive { get; set; }
	}
}