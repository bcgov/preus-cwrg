using System;
using System.Collections.Generic;
using CJG.Core.Entities;

namespace CJG.Application.Business.Models
{
	public class AccountsReceivableBreakdown
	{
		public int FiscalYearId { get; set; }
		public string FiscalYear { get; set; }

		[Obsolete("REFACTOR: Remove in favour of the Breakdowns collection.")]
		public int CoreApplicationNumber { get; set; }
		[Obsolete("REFACTOR: Remove in favour of the Breakdowns collection.")]
		public int CRSApplicationNumber { get; set; }

		[Obsolete("REFACTOR: Remove in favour of the Breakdowns collection.")]
		public decimal CoreApplicationTotal { get; set; }
		[Obsolete("REFACTOR: Remove in favour of the Breakdowns collection.")]
		public decimal CRSApplicationTotal { get; set; }

		public Dictionary<ProgramInitiative, AccountsReceivableInitiativeData> Breakdowns { get; set; } = new Dictionary<ProgramInitiative, AccountsReceivableInitiativeData>();
	}
}