using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class SuccessStory : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[ForeignKey("GrantApplication")]
		public int GrantApplicationId { get; set; }

		public virtual GrantApplication GrantApplication { get; set; }

		[Required, DefaultValue(0)]
		public SuccessStoryState State { get; set; }

		public bool SuccessfulOutcome { get; set; }

		public string NoOutcomeReason { get; set; }

		public virtual ICollection<Attachment> Documents { get; set; } = new List<Attachment>();
	}
}