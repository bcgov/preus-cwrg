using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class ChecklistItem : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[ForeignKey("ChecklistCategory")]
		public int ChecklistCategoryId { get; set; }

		public virtual ChecklistCategory ChecklistCategory { get; set; }

		[Required, MaxLength(250)]
		public string Caption { get; set; }

		[Required]
		public int RowSequence { get; set; }

		[Required]
		public bool IsActive { get; set; }
	}
}