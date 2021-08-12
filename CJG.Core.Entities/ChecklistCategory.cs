using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class ChecklistCategory : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		/// <summary>
		/// get/set - Foreign key to the parent grant program.
		/// </summary>
		[Index("IX_GrantStreamChecklistCategories", 1)]
		public int GrantStreamId { get; set; }

		[ForeignKey(nameof(GrantStreamId))]
		public virtual GrantStream GrantStream { get; set; }

		[Required, MaxLength(250)]
		public string Caption { get; set; }

		[Required]
		public int RowSequence { get; set; }

		[Required]
		public bool IsActive { get; set; }

		public virtual ICollection<ChecklistItem> Items { get; set; } = new List<ChecklistItem>();
	}
}