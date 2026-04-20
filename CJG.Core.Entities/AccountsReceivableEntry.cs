using System;
using System.ComponentModel;
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

		[Obsolete("This may be removed in favour of the LMDA/WDA split. Keeping here for exporting/data-history.")]
		public decimal Overpayment { get; set; }

		[Description("LMDA")]
		public decimal OverpaymentLMDA { get; set; }

		[Description("WDA")]
		public decimal OverpaymentWDA { get; set; }
	}
}