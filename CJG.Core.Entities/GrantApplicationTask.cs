using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class GrantApplicationTask : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[ForeignKey("GrantApplication")]
		public int GrantApplicationId { get; set; }
		public virtual GrantApplication GrantApplication { get; set; }

		[ForeignKey("ChecklistItem")]
		public int ChecklistItemId { get; set; }
		public virtual ChecklistItem ChecklistItem { get; set; }

		// Is this task/item checked off
		public bool IsChecked { get; set; }
	}
}