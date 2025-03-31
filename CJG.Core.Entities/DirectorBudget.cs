using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class DirectorBudget : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public int FiscalYearId { get; set; }

		[ForeignKey(nameof(FiscalYearId))]
		public virtual FiscalYear FiscalYear { get; set; }

		[Obsolete("This field is no longer used. Remove.")]
		public BudgetEntryType BudgetEntryType { get; set; }

		[Obsolete("Maybe?")]
		public string StreamFilter { get; set; }

		public int? ProgramInitiativeId { get; set; }
		[ForeignKey(nameof(ProgramInitiativeId))]
		public virtual ProgramInitiative ProgramInitiative { get; set; }

		/// <summary>
		/// The 'Opening' budget on each Director Budget Grouping
		/// </summary>
		public decimal? Budget { get; set; }

		[Obsolete("This field is no longer used. Remove.")]
		public decimal? ForecastBudget { get; set; }

		public virtual ICollection<DirectorBudgetEntry> BudgetEntries { get; set; } = new List<DirectorBudgetEntry>();
	}

	public enum BudgetEntryType
	{
		CoreStream = 1,
		NonCore = 2
	}
}