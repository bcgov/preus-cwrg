PRINT 'Start of Participant Exit Survey Questions + Options'

UPDATE [dbo].[ParticipantExitSurveyQuestions]
SET IsActive = 0

UPDATE [dbo].[ParticipantExitSurveyQuestionOptions]
SET IsActive = 0

SET IDENTITY_INSERT [dbo].[ParticipantExitSurveyQuestions] ON 

INSERT [dbo].[ParticipantExitSurveyQuestions] ([Id], [IsActive], [Sequence], [QuestionType], [Question], [DateAdded]) 
VALUES 
	(10, 1, 10, 2, N'How did you hear about this training?', GETDATE()),
	(11, 1, 12, 2, N'Why did you apply to take this skills training?', GETDATE()),
	(12, 1, 12, 1, N'How did this training meet your content and delivery expectations?', GETDATE()),
	(13, 1, 13, 2, N'How did you benefit from taking the training?', GETDATE()),
	(14, 1, 14, 3, N'Are there any additional comments you wish to make? Please do not include any personal information about you or others.', GETDATE())



UPDATE [dbo].ParticipantExitSurveyQuestionOptions
SET [OptionText] = N'Other (please describe):'
WHERE [ParticipantExitSurveyQuestionId] = 10
AND [OptionText] = N'Other (please describe)'

UPDATE [dbo].ParticipantExitSurveyQuestionOptions
SET [OptionText] = N'To stay in my community'
WHERE [ParticipantExitSurveyQuestionId] = 11
AND [OptionText] = N'To empower myself and my community through education'



INSERT [dbo].ParticipantExitSurveyQuestionOptions ([ParticipantExitSurveyQuestionId], [IsActive], [Sequence], [OptionText], [ReplacementToken], [AllowOther], [DateAdded]) 
VALUES 
	(10, 1, 1, N'Advertising/social media', NULL, 0, GETDATE()),
	(10, 1, 2, N'Through [TrainingProviderName]', N'[TrainingProviderName]', 0, GETDATE()),
	(10, 1, 3, N'Through [AgreementHolderName]', N'[AgreementHolderName]', 0, GETDATE()),
	(10, 1, 4, N'Local WorkBC office', NULL, 0, GETDATE()),
	(10, 1, 5, N'Other (please describe)', NULL, 1, GETDATE()),
	(10, 1, 6, N'Prefer not to answer', NULL, 0, GETDATE()),

	(11, 1, 1, N'To upskill to get a better job in my current field', NULL, 0, GETDATE()),
	(11, 1, 2, N'To reskill to get a job in a new field', NULL, 0, GETDATE()),
	(11, 1, 3, N'To move from part-time to full-time', NULL, 0, GETDATE()),
	(11, 1, 4, N'To empower myself and my community through education', NULL, 0, GETDATE()),
	(11, 1, 5, N'Other (please describe):', NULL, 1, GETDATE()),
	(11, 1, 6, N'Prefer not to answer', NULL, 0, GETDATE()),

	(12, 1, 1, N'Did not meet expectations', NULL, 0, GETDATE()),
	(12, 1, 2, N'Did not have any expectations', NULL, 0, GETDATE()),
	(12, 1, 3, N'Met expectations', NULL, 0, GETDATE()),
	(12, 1, 4, N'Exceeded expectations', NULL, 0, GETDATE()),
	(12, 1, 5, N'Prefer not to answer', NULL, 0, GETDATE()),

	(13, 1, 1, N'Secured job interview', NULL, 0, GETDATE()),
	(13, 1, 2, N'Reskilled and obtained a job in a new field specific to the training I received', NULL, 0, GETDATE()),
	(13, 1, 3, N'Upskilled and obtained a better job in my current field', NULL, 0, GETDATE()),
	(13, 1, 4, N'Secured a part-time job offer', NULL, 0, GETDATE()),
	(13, 1, 5, N'Secured a full-time job offer', NULL, 0, GETDATE()),
	(13, 1, 6, N'Able to remain in my community', NULL, 0, GETDATE()),
	(13, 1, 7, N'Other (please describe):', NULL, 1, GETDATE()),
	(13, 1, 8, N'Prefer not to answer', NULL, 0, GETDATE()),

	(14, 1, 1, N'Are there any additional comments you wish to make? Please do not include any personal information about you or others.', NULL, 0, GETDATE())

SET IDENTITY_INSERT [dbo].[ParticipantExitSurveyQuestions] OFF

PRINT 'End of Participant Exit Survey Questions + Options'