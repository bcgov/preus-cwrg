using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class AccountsReceivableEntry : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[ForeignKey("AccountsReceivable")]
		public int AccountsReceivableId { get; set; }

		public virtual AccountsReceivable AccountsReceivable { get; set; }

		[ForeignKey("ServiceCategory")]
		public int ServiceCategoryId { get; set; }
		public virtual ServiceCategory ServiceCategory { get; set; }

		public decimal Overpayment { get; set; }
	}
}