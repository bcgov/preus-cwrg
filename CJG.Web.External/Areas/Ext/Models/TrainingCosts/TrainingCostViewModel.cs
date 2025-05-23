﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Ext.Models.TrainingCosts
{
	public class TrainingCostViewModel : BaseViewModel
	{
		public int GrantApplicationId { get; set; }
		public string RowVersion { get; set; }

		public bool IsEditable { get; set; }

		public int GrantProgramId { get; set; }
		public ProgramTypes ProgramType { get; set; }
		public int? AgreedParticipants { get; set; }

		[Required(ErrorMessage = "You must enter the number of participants.")]
		public int? EstimatedParticipants { get; set; }
		public decimal MaxReimbursementAmt { get; set; }
		public double ReimbursementRate { get; set; }
		public decimal TotalEstimatedCost { get; set; }
		public decimal TotalEmployer { get; set; }
		public decimal TotalRequest { get; set; }

		public decimal TotalAgreedCost { get; set; }
		public decimal TotalAgreedEmployer { get; set; }
		public decimal TotalAgreedReimbursement { get; set; }
		public IEnumerable<EligibleCostViewModel> EligibleCosts { get; set; } = new List<EligibleCostViewModel>();

		public decimal ESSAgreedAverage { get; set; }
		public decimal ESSEstimatedAverage { get; set; }

		public bool AllExpenseTypeAllowMultiple
		{
			get
			{
				return EligibleCosts.All(t => t.EligibleExpenseType.AllowMultiple && t.EligibleExpenseType.IsActive);
			}
		}

		public bool ShouldDisplayEmployerContribution { get; set; }
		public bool ShouldDisplayESSSummary { get; set; }
		public string UserGuidanceCostEstimates { get; set; }


		public TrainingCostViewModel() { }

		public TrainingCostViewModel(GrantApplication grantApplication, IPrincipal user, IGrantStreamService grantStreamService)
		{
			if (grantApplication == null)
				throw new ArgumentNullException(nameof(grantApplication));

			if (grantStreamService == null)
				throw new ArgumentNullException(nameof(grantStreamService));

			var autoIncludeEligibleExpenseTypes = grantStreamService
				.GetAutoIncludeActiveEligibleExpenseTypes(grantApplication.GrantOpening.GrantStreamId)
				.ToList();

			GrantApplicationId = grantApplication.Id;
			RowVersion = Convert.ToBase64String(grantApplication.TrainingCost.RowVersion);
			IsEditable = user.CanPerformAction(grantApplication, ApplicationWorkflowTrigger.EditTrainingCosts);

			var grantStream = grantApplication.GrantOpening.GrantStream;

			GrantProgramId = grantStream.GrantProgramId;
			ProgramType = grantStream.GrantProgram.ProgramTypeId;
			MaxReimbursementAmt = grantApplication.MaxReimbursementAmt;
			ReimbursementRate = grantApplication.ReimbursementRate;
			AgreedParticipants = grantApplication.TrainingCost.AgreedParticipants == 0 ? null : (int?)grantApplication.TrainingCost.AgreedParticipants;
			EstimatedParticipants = grantApplication.TrainingCost.EstimatedParticipants == 0 ? null : (int?)grantApplication.TrainingCost.EstimatedParticipants;

			var eligibleCosts = !grantApplication.HasOfferBeenIssued()
				? grantApplication.TrainingCost.EligibleCosts
					.Where(ec => !ec.AddedByAssessor)
					.Select(ec => new EligibleCostViewModel(ec))
					.ToList()
				: grantApplication.TrainingCost.EligibleCosts
					.Select(ec => new EligibleCostViewModel(ec))
					.ToList();

			if (eligibleCosts.Count != autoIncludeEligibleExpenseTypes.Count)
			{
				eligibleCosts.AddRange(autoIncludeEligibleExpenseTypes
					.Where(t => !eligibleCosts.Select(e => e.EligibleExpenseType.Id).Contains(t.Id))
					.Select(eet => new EligibleCostViewModel(eet)
					{
						EstimatedParticipants = EstimatedParticipants ?? 0
					})
					.ToList());
			}

			EligibleCosts = eligibleCosts
				.OrderBy(t => t.EligibleExpenseType.RowSequence)
				.ThenBy(ec => ec.EligibleExpenseType.Caption)
				.ToList();

			if (grantApplication.GetProgramType() != ProgramTypes.EmployerGrant)
			{
				TotalEmployer = EligibleCosts.Sum(x => x.EstimatedEmployerContribution);
				TotalEstimatedCost = EligibleCosts.Sum(x => x.EstimatedCost);
				TotalRequest = EligibleCosts.Sum(x => x.EstimatedReimbursement);
			}

			ShouldDisplayEmployerContribution = grantApplication.ReimbursementRate != 1;
			ShouldDisplayESSSummary = grantStream.GrantProgram.ProgramTypeId == ProgramTypes.WDAService;

			UserGuidanceCostEstimates = grantStream.ProgramConfiguration?.GrantPrograms.Count == 0
				? grantStream.ProgramConfiguration.UserGuidanceCostEstimates
				: grantStream.GrantProgram.ProgramConfiguration.UserGuidanceCostEstimates;
		}

		public GrantApplication UpdateTrainingCosts(IGrantApplicationService grantApplicationService)
		{
			if (grantApplicationService == null)
				throw new ArgumentNullException(nameof(grantApplicationService));

			var grantApplication = grantApplicationService.Get(GrantApplicationId);

			var trainingCost = grantApplication.TrainingCost;
			trainingCost.RowVersion = Convert.FromBase64String(RowVersion);
			trainingCost.EstimatedParticipants = EstimatedParticipants.Value;

			// Remove any eligible cost that exists in the datasource but not in the updated training cost.
			var currentCostIds = EligibleCosts.Select(x => x.Id).ToArray();
			var removeEligibleCosts = trainingCost.EligibleCosts.Where(ec => !currentCostIds.Contains(ec.Id)).ToArray();
			var currentBreakdownIds = EligibleCosts.SelectMany(t => t.Breakdowns).Select(t => t.Id);
			var removeEligibleCostBreakdownIds = trainingCost.EligibleCosts.SelectMany(t => t.Breakdowns).Where(t => !currentBreakdownIds.Contains(t.Id)).Select(b => b.Id).Distinct().ToArray();

			// Remove eligible costs.
			foreach (var remove in removeEligibleCosts)
			{
				trainingCost.EligibleCosts.Remove(remove);
			}

			// Update eligible costs.
			foreach (var cost in EligibleCosts)
			{
				var expenseType = grantApplicationService.Get<EligibleExpenseType>(cost.EligibleExpenseType.Id);
				var eligibleCost = cost.Id == 0 ? new EligibleCost(grantApplication, expenseType, cost.EstimatedCost, cost.EstimatedParticipants) : grantApplicationService.Get<EligibleCost>(cost.Id);
				eligibleCost.EligibleExpenseTypeId = cost.EligibleExpenseType.Id;
				eligibleCost.EligibleExpenseType = expenseType;

				switch (cost.EligibleExpenseType.ExpenseTypeId)
				{
					case (ExpenseTypes.ParticipantLimited):
					case (ExpenseTypes.NotParticipantLimited):
					case (ExpenseTypes.AutoLimitEstimatedCosts):
						eligibleCost.EstimatedParticipants = EstimatedParticipants.Value;
						break;
					default:
						eligibleCost.EstimatedParticipants = cost.EstimatedParticipants;
						break;
				}
				eligibleCost.EstimatedParticipantCost = cost.EstimatedParticipantCost;
				eligibleCost.EstimatedCost = cost.EstimatedCost;

				foreach (var breakdown in cost.Breakdowns)
				{
					var breakdownEntity = grantApplicationService.Get<EligibleCostBreakdown>(breakdown.Id);
					breakdownEntity.EstimatedCost = breakdown.EstimatedCost;
				}

				eligibleCost.EligibleExpenseTypeId = cost.EligibleExpenseType.Id;

				// Remove breakdowns.
				foreach (var breakdown in eligibleCost.Breakdowns.Where(b => removeEligibleCostBreakdownIds.Contains(b.Id)).ToArray())
				{
					eligibleCost.Breakdowns.Remove(breakdown);
				}

				eligibleCost.RecalculateEstimatedCost();
				eligibleCost.RecalculateAgreedCosts();

				if (cost.Id == 0)
				{
					trainingCost.EligibleCosts.Add(eligibleCost);
				}
			}

			trainingCost.RecalculateEstimatedCosts();
			trainingCost.RecalculateAgreedCosts();

			grantApplicationService.UpdateTrainingCosts(grantApplication);

			return grantApplication;
		}
	}
}