﻿using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class ParticipantExitSurveyQuestion : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public SurveyQuestionType QuestionType { get; set; }

		public string Question { get; set; }
		public int Sequence { get; set; }

		[DefaultValue("true")]
		public bool IsActive { get; set; }

		public virtual ICollection<ParticipantExitSurveyQuestionOption> Options { get; } = new List<ParticipantExitSurveyQuestionOption>();
	}
}