PRINT 'Start of Participant Withdrawal Survey Questions + Options'

UPDATE [dbo].[ParticipantWithdrawalSurveyQuestions]
SET IsActive = 0

UPDATE [dbo].ParticipantWithdrawalSurveyQuestionOptions
SET IsActive = 0

SET IDENTITY_INSERT [dbo].[ParticipantWithdrawalSurveyQuestions] ON 

INSERT [dbo].[ParticipantWithdrawalSurveyQuestions] ([Id], [IsActive], [Sequence], [QuestionType], [Question], [DateAdded]) 
VALUES 
	(10, 1, 1, 1, N'Why did you leave training early?', GETDATE()),
	(11, 1, 2, 1, N'How did you hear about this training?', GETDATE()),
	(12, 1, 3, 1, N'What supports would have helped you to remain in training?', GETDATE()),
	(13, 1, 4, 3, N'Are there any additional comments you wish to add? Please do not include any personal information about you or others.', GETDATE())

INSERT [dbo].ParticipantWithdrawalSurveyQuestionOptions ([ParticipantWithdrawalSurveyQuestionId], [IsActive], [Sequence], [OptionText], [ReplacementToken], [AllowOther], [DateAdded]) 
VALUES 
	(10, 1, 1, N'Personal/Medical reasons', NULL, 0, GETDATE()),
	(10, 1, 2, N'Lost interest', NULL, 0, GETDATE()),
	(10, 1, 3, N'Could not afford to stay in training', NULL, 0, GETDATE()),
	(10, 1, 4, N'Level of training and its instruction did not meet expectations', NULL, 0, GETDATE()),
	(10, 1, 5, N'Other (please describe)', NULL, 1, GETDATE()),
	(10, 1, 6, N'Prefer not to answer', NULL, 0, GETDATE()),

	(11, 1, 1, N'Advertising/Social Media', NULL, 0, GETDATE()),
	(11, 1, 2, N'Through [TrainingProviderName]', N'[TrainingProviderName]', 0, GETDATE()),
	(11, 1, 3, N'Through [AgreementHolderName]', N'[AgreementHolderName]', 0, GETDATE()),
	(11, 1, 4, N'Local WorkBC office', NULL, 0, GETDATE()),
	(11, 1, 5, N'Other (please describe)', NULL, 1, GETDATE()),
	(11, 1, 6, N'Prefer not to answer', NULL, 0, GETDATE()),

	(12, 1, 1, N'More/better financial support', NULL, 0, GETDATE()),
	(12, 1, 2, N'Frequent check-ins by [AgreementHolderName]', N'[AgreementHolderName]', 0, GETDATE()),
	(12, 1, 3, N'Mental health support', NULL, 0, GETDATE()),
	(12, 1, 4, N'Career counselling', NULL, 0, GETDATE()),
	(12, 1, 5, N'Other (please describe)', NULL, 1, GETDATE()),
	(12, 1, 6, N'Prefer not to answer', NULL, 0, GETDATE()),

	(13, 1, 1, N'Are there any additional comments you wish to add? Please do not include any personal information about you or others.', NULL, 0, GETDATE())

SET IDENTITY_INSERT [dbo].[ParticipantWithdrawalSurveyQuestions] OFF

PRINT 'End of Participant Withdrawal Survey Questions + Options'