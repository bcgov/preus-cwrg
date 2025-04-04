﻿PRINT 'INSERT [GrantPrograms]'

SET IDENTITY_INSERT [dbo].[GrantPrograms] ON 

INSERT [dbo].[GrantPrograms] (
	[Id]
	, [AccountCodeId]
	, [ProgramConfigurationId]
	, [ProgramTypeId]
	, [State]
	, [ProgramCode]
	, [Name]
	, [Message]
	, [ShowMessage]
	, [EligibilityDescription]
	, [CanReportParticipants]
	, [CanReportSponsors]
	, [ApplicantDeclarationTemplateId]
	, [ApplicantCoverLetterTemplateId]
	, [ApplicantScheduleATemplateId]
	, [ApplicantScheduleBTemplateId]
	, [ParticipantConsentTemplateId]
	, [ExpenseAuthorityId]
	, [RequestedBy]
	, [ProgramPhone]
	, [DocumentPrefix]
	, [BatchRequestDescription]
	, [DateAdded]
	, [DateUpdated]
) 
VALUES (
	3 -- [Id]
	, 3 -- [AccountCodeId]
	, 2 -- [ProgramConfigurationId]
	, 2 -- [ProgramTypeId]
	, 0 -- [State]
	, N'CWRG' -- [ProgramCode]
	, N'Community Workforce Response Grant' -- [Name]
	, N'(optional)' -- [Message]
	, 0 -- [ShowMessage]
	, N'SAM TO PROVIDE' -- [EligibilityDescription]
	, 1 -- [CanReportParticipants]
	, 1 -- [CanReportSponsors]
	, 4 -- [ApplicantDeclarationTemplateId]
	, 1 -- [ApplicantCoverLetterTemplateId]
	, 2 -- [ApplicantScheduleATemplateId]
	, 3 -- [ApplicantScheduleBTemplateId]
	, 5 -- [ParticipantConsentTemplateId]
	, null -- [ExpenseAuthorityId]
	, N'Labour Market and Skills Training Unit, Program Design and Delivery Branch, Workforce Innovation and Division Responsible for Skills Training, Ministry of Advanced Education, Skills and Training' -- [RequestedBy]
	, N'(250) 387-4428' -- [ProgramPhone]
	, N'CWR' -- [DocumentPrefix]
	, N'Payment on receipt of claim for eligible training expenses under the Workforce Development Agreement, as per Agreement.' -- [BatchRequestDescription]
	, GETUTCDATE() -- [DateAdded]
	, NULL -- [DateUpdated]
)

SET IDENTITY_INSERT [dbo].[GrantPrograms] OFF