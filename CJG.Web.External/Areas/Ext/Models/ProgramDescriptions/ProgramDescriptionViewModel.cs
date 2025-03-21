﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using CJG.Application.Services;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Helpers;
using CJG.Web.External.Helpers.Validation;
using CJG.Web.External.Models.Shared;
using DataAnnotationsExtensions;

namespace CJG.Web.External.Areas.Ext.Models.ProgramDescriptions
{
    public class ProgramDescriptionViewModel : BaseViewModel
	{
		public string RowVersion { get; set; }

		[MaxLength(300, ErrorMessage = "Project Description cannot be longer than 300 characters.")]
		[Required(ErrorMessage = "Project Description is required.")]
		public string Description { get; set; }

		//[Required(ErrorMessage = "Number of community representatives and employer letters submitted in support of this project is required.")]
		[Obsolete("The SupportingEmployers field has been deprecated")]
		public int? SupportingEmployers { get; set; }

		[Required(ErrorMessage = "Number of Participants must be greater than or equal to 1.")]
		[Max(1500, ErrorMessage = "Number of Participants must be less than or equal to 1500.")]
		[Min(1, ErrorMessage = "Number of Participants must be greater than or equal to 1.")]
		public int? NumberOfParticipants { get; set; }

		[Required(ErrorMessage = "Applicant Type is required.")]
		public int? ApplicantOrganizationTypeId { get; set; }

		[Required(ErrorMessage = "Other Applicant Type information is required.")]
		public string ApplicantOrganizationTypeInfo { get; set; }

		[CustomValidation(typeof(ProgramDescriptionViewModelValidation), "ValidateNAICS")]
		public int? Naics1Id { get; set; }

		[CustomValidation(typeof(ProgramDescriptionViewModelValidation), "ValidateNAICS")]
		public int? Naics2Id { get; set; }

		[CustomValidation(typeof(ProgramDescriptionViewModelValidation), "ValidateNAICS")]
		public int? Naics3Id { get; set; }

		public int? Naics4Id { get; set; }

		public int? Naics5Id { get; set; }

		[CustomValidation(typeof(ProgramDescriptionViewModelValidation), "ValidateNOC")]
		public int? Noc1Id { get; set; }

		[CustomValidation(typeof(ProgramDescriptionViewModelValidation), "ValidateNOC")]
		public int? Noc2Id { get; set; }

		[CustomValidation(typeof(ProgramDescriptionViewModelValidation), "ValidateNOC")]
		public int? Noc3Id { get; set; }

		[CustomValidation(typeof(ProgramDescriptionViewModelValidation), "ValidateNOC")]
		public int? Noc4Id { get; set; }

		[CustomValidation(typeof(ProgramDescriptionViewModelValidation), "ValidateNOC")]
		public int? Noc5Id { get; set; }

		public IEnumerable<int> SelectedVulnerableGroupIds { get; set; }

		public IEnumerable<int> SelectedUnderRepresentedPopulationIds { get; set; }

		[RequiredEnumerable(ErrorMessage = "Community Names are required.")]
		[CustomValidation(typeof(ProgramDescriptionViewModelValidation), "ValidateCommunitiesForApplicant")]
		public IEnumerable<int> SelectedCommunityIds { get; set; }

		[RequiredEnumerable(ErrorMessage = "You must select at least one participant employment status.")]
		[CustomValidation(typeof(ProgramDescriptionViewModelValidation), "ValidateParticipantEmploymentStatuses")]
		public IEnumerable<int> SelectedParticipantEmploymentStatusIds { get; set; }

		public ProgramDescriptionViewModel()
		{ }

		public ProgramDescriptionViewModel(ProgramDescription programDescription,
			INaIndustryClassificationSystemService naIndustryClassificationSystemService,
			INationalOccupationalClassificationService nationalOccupationalClassificationService)
		{
			if (programDescription == null)
				throw new ArgumentNullException(nameof(programDescription));
			if (naIndustryClassificationSystemService == null)
				throw new ArgumentNullException(nameof(naIndustryClassificationSystemService));
			if (nationalOccupationalClassificationService == null)
				throw new ArgumentNullException(nameof(nationalOccupationalClassificationService));

			Utilities.MapProperties(programDescription, this);
			Id = programDescription.GrantApplicationId;
			RowVersion = programDescription.RowVersion != null ? Convert.ToBase64String(programDescription.RowVersion) : null;
			NumberOfParticipants = programDescription.RowVersion == null ? (int?)null : programDescription.GrantApplication.TrainingCost.EstimatedParticipants;
			SupportingEmployers = programDescription.RowVersion == null ? (int?)null : programDescription.SupportingEmployers;
			SelectedUnderRepresentedPopulationIds = programDescription.UnderRepresentedPopulations.Select(o => o.Id).ToArray();
			SelectedVulnerableGroupIds = programDescription.VulnerableGroups.Select(o => o.Id).ToArray();
			SelectedCommunityIds = programDescription.Communities.Select(c => c.Id).ToArray();
			SelectedParticipantEmploymentStatusIds = programDescription.ParticipantEmploymentStatuses.Select(o => o.Id).ToArray();

			MapNAICsAndNOCs(programDescription, naIndustryClassificationSystemService, nationalOccupationalClassificationService);
		}

		private void MapNAICsAndNOCs(ProgramDescription programDescription,
			INaIndustryClassificationSystemService naIndustryClassificationSystemService,
			INationalOccupationalClassificationService nationalOccupationalClassificationService)
		{
			var naics = programDescription.TargetNAICS;
			var noc = programDescription.NationalOccupationalClassification;

			var naicsIds = new List<int>();
			var nocIds = new List<int>();

			//New NAICS codes to be used from the cutoffdate
			DateTime cutOffDate = Convert.ToDateTime(ConfigurationManager.AppSettings["CutOffDate"]);
			DateTime currentDate = DateTime.Now;
			int naicsVersion = 2012; //old naics codes
			int rootParentId = 0;

			//find the NAICS id for root level for NAICS 2017
			if (currentDate >= cutOffDate)
			{
				naicsVersion = 2017; //new naics codes

				rootParentId = naIndustryClassificationSystemService.GetRootNaicsID(naicsVersion);
			}

			while ((naics?.Id ?? rootParentId) > rootParentId)
			{
				naicsIds.Add(naics.Id);
				naics = naIndustryClassificationSystemService.GetNaIndustryClassificationSystem(naics.ParentId);
			}

			for (int i = naicsIds.Count - 1; i >= 0; i--)
			{
				var property = GetType().GetProperty($"Naics{naicsIds.Count - i}Id");
				property?.SetValue(this, naicsIds[i]);
			}

			while ((noc?.Level ?? 0) > 0)
			{
				nocIds.Add(noc.Id);
				noc = nationalOccupationalClassificationService.GetNationalOccupationalClassification(noc.ParentId);
			}

			for (int i = nocIds.Count - 1; i >= 0; i--)
			{
				var property = GetType().GetProperty($"Noc{nocIds.Count - i}Id");
				property?.SetValue(this, nocIds[i]);
			}
		}

		/// <summary>
		/// Copy the view model information into the specified program description.
		/// </summary>
		/// <param name="programDescription"></param>
		/// <param name="vulnerableGroupService"></param>
		/// <param name="underRepresentedPopulationService"></param>
		/// <param name="communityService"></param>
		public void MapToEntity(
			ProgramDescription programDescription,
			IVulnerableGroupService vulnerableGroupService,
			IUnderRepresentedPopulationService underRepresentedPopulationService,
			ICommunityService communityService,
			IParticipantEmploymentStatusService partEmploymentStatusService)
		{
			if (programDescription == null) throw new ArgumentException(nameof(programDescription));
			if (vulnerableGroupService == null) throw new ArgumentNullException(nameof(vulnerableGroupService));
			if (underRepresentedPopulationService == null) throw new ArgumentNullException(nameof(underRepresentedPopulationService));
			if (communityService == null) throw new ArgumentNullException(nameof(communityService));
			if (partEmploymentStatusService == null) throw new ArgumentNullException(nameof(partEmploymentStatusService));

			Utilities.MapProperties(this, programDescription);

			programDescription.VulnerableGroups.Clear();
			programDescription.UnderRepresentedPopulations.Clear();
			programDescription.Communities.Clear();
			programDescription.ParticipantEmploymentStatuses.Clear();

			if (SelectedVulnerableGroupIds != null)
			{
				var vulnerableGroups = vulnerableGroupService.GetVulnerableGroups(SelectedVulnerableGroupIds.ToArray());
				foreach (var vulnerableGroup in vulnerableGroups)
					programDescription.VulnerableGroups.Add(vulnerableGroup);
			}
			if (SelectedUnderRepresentedPopulationIds != null)
			{
				var underRepresentedPopulations = underRepresentedPopulationService.GetUnderRepresentedPopulations(SelectedUnderRepresentedPopulationIds.ToArray());
				foreach (var underRepresentedPopulation in underRepresentedPopulations)
					programDescription.UnderRepresentedPopulations.Add(underRepresentedPopulation);
			}
			if (SelectedCommunityIds != null)
			{
				var communities = communityService.GetCommunities(SelectedCommunityIds.ToArray());
				foreach (var community in communities)
					programDescription.Communities.Add(community);
			}

			if (SelectedParticipantEmploymentStatusIds != null)
			{
				var selectedStatuses = partEmploymentStatusService.GetParticipantEmploymentStatuses(SelectedParticipantEmploymentStatusIds.ToArray());
				foreach (var status in selectedStatuses)
					programDescription.ParticipantEmploymentStatuses.Add(status);
			}
			programDescription.TargetNAICSId = Naics5Id ?? Naics4Id ?? Naics3Id ?? Naics2Id ?? Naics1Id;
			programDescription.TargetNOCId = Noc5Id ?? Noc4Id ?? Noc3Id ?? Noc2Id ?? Noc1Id;
		}
	}
}
