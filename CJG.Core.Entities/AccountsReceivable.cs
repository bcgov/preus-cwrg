using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class AccountsReceivable : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[ForeignKey("GrantApplication")]
		public int GrantApplicationId { get; set; }
		public virtual GrantApplication GrantApplication { get; set; }

		public DateTime PaidDate { get; set; }

		public virtual ICollection<AccountsReceivableEntry> AccountsReceivableEntries { get; set; } = new List<AccountsReceivableEntry>();
	}
}