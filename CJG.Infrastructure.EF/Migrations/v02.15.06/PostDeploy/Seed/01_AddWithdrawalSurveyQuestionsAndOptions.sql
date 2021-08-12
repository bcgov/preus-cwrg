PRINT 'Start of Participant Withdrawal Survey Questions + Options'

SET IDENTITY_INSERT [dbo].[ParticipantWithdrawalSurveyQuestions] ON 

INSERT [dbo].[ParticipantWithdrawalSurveyQuestions] ([Id], [IsActive], [Sequence], [QuestionType], [Question], [DateAdded]) 
VALUES 
	(1, 1, 1, 1, N'What factors or circumstances influenced your decision to leave training early?', GETDATE()),
	(2, 1, 2, 1, N'How did you hear about this training?', GETDATE()),
	(3, 1, 3, 1, N'Did you feel supported through the recruitment and training process?', GETDATE()),
	(4, 1, 4, 3, N'What changes could be made to improve the experience? Which of the available participantsupports were the most helpful to you?', GETDATE())

INSERT [dbo].ParticipantWithdrawalSurveyQuestionOptions ([ParticipantWithdrawalSurveyQuestionId], [IsActive], [Sequence], [OptionText], [ReplacementToken], [AllowOther], [DateAdded]) 
VALUES 
	(1, 1, 1, N'Personal/Health reasons', NULL, 0, GETDATE()),
	(1, 1, 2, N'Lost interest', NULL, 0, GETDATE()),
	(1, 1, 3, N'Could not afford to stay in training', NULL, 0, GETDATE()),
	(1, 1, 4, N'Training was not what I expected', NULL, 0, GETDATE()),
	(1, 1, 5, N'Other', NULL, 1, GETDATE()),

	(2, 1, 1, N'Advertising/social media', NULL, 0, GETDATE()),
	(2, 1, 2, N'Through [TrainingProviderName]', N'[TrainingProviderName]', 0, GETDATE()),
	(2, 1, 3, N'Through [AgreementHolderName]', N'[AgreementHolderName]', 0, GETDATE()),
	(2, 1, 4, N'Local WorkBC office', NULL, 0, GETDATE()),
	(2, 1, 5, N'Other', NULL, 1, GETDATE()),

	(3, 1, 1, N'Yes', NULL, 0, GETDATE()),
	(3, 1, 2, N'No', NULL, 0, GETDATE()),
	
	(4, 1, 1, N'What changes could be made to improve the experience and to encourage participants to complete the entire training program?', NULL, 0, GETDATE())

SET IDENTITY_INSERT [dbo].[ParticipantWithdrawalSurveyQuestions] OFF

PRINT 'End of Participant Withdrawal Survey Questions + Options'