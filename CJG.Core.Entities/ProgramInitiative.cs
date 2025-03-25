using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAnnotationsExtensions;

namespace CJG.Core.Entities
{
	public class ProgramInitiative : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[MaxLength(255)]
		public string Name { get; set; }

		[MaxLength(50)]
		public string Code { get; set; }

		[DefaultValue(true), Index("IX_Active")]
		public bool IsActive { get; set; }

		[Min(0)]
		public int RowSequence { get; set; }
	}
}