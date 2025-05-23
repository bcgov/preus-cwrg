﻿using CJG.Core.Entities;
using System;
using System.Collections.Generic;

namespace CJG.Application.Business.Models
{
    public class EligibleCostModel
    {
        #region Properties
        public int Id { get; set; }
        public EligibleExpenseTypeModel EligibleExpenseType { get; set; }
        public int EstimatedParticipants { get; set; }
        public decimal EstimatedParticipantCost { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal EstimatedEmployerContribution { get; set; }
        public decimal EstimatedReimbursement { get; set; }

        public int AgreedMaxParticipants { get; set; }
        public decimal AgreedMaxParticipantCost { get; set; }
        public decimal AgreedCost { get; set; }
        public decimal AgreedEmployerContribution { get; set; }
        public decimal AgreedMaxReimbursement { get; set; }
        public ServiceTypes? ServiceType { get; set; }
        public bool ShowBreakdowns { get; set; }

        public bool AddedByAssessor { get; set; }


        public List<EligibleCostBreakdownModel> Breakdowns { get; set; } = new List<EligibleCostBreakdownModel>();
        #endregion

        #region Constructors
        public EligibleCostModel()
        {

        }

        public EligibleCostModel(EligibleCost eligibleCost)
        {
            if (eligibleCost == null)
                throw new ArgumentNullException(nameof(eligibleCost));

            this.Id = eligibleCost.Id;
            this.EstimatedCost = eligibleCost.EstimatedCost;
            this.EstimatedParticipantCost = eligibleCost.EstimatedParticipantCost;
            this.EstimatedEmployerContribution = eligibleCost.EstimatedEmployerContribution;
            this.EstimatedParticipants = eligibleCost.EstimatedParticipants;
            this.EstimatedReimbursement = eligibleCost.EstimatedReimbursement;

            this.AgreedMaxParticipants = eligibleCost.AgreedMaxParticipants;
            this.AgreedMaxParticipantCost = eligibleCost.AgreedMaxParticipantCost;
            this.AgreedCost = eligibleCost.AgreedMaxCost;
            this.AgreedEmployerContribution = eligibleCost.AgreedEmployerContribution;
            this.AgreedMaxReimbursement = eligibleCost.AgreedMaxReimbursement;

            this.EligibleExpenseType = new EligibleExpenseTypeModel(eligibleCost.EligibleExpenseType);
            this.ShowBreakdowns = !(eligibleCost.EligibleExpenseType.ServiceCategory?.ServiceTypeId.In(ServiceTypes.EmploymentServicesAndSupports, ServiceTypes.Administration) ?? false);
            this.ServiceType = eligibleCost.EligibleExpenseType.ServiceCategory?.ServiceTypeId;
            this.AddedByAssessor = eligibleCost.AddedByAssessor;

            foreach (var item in eligibleCost.Breakdowns)
            {
                Breakdowns.Add(new EligibleCostBreakdownModel(item));
            }

        }

        public EligibleCostModel(EligibleExpenseTypeModel eligibleExpenseType)
        {
            this.EligibleExpenseType = eligibleExpenseType;
        }
        #endregion
    }
}
