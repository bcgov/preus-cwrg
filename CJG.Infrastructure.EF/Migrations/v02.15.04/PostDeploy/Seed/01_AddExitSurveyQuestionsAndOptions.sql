PRINT 'Start of Participant Exit Survey Questions + Options'

SET IDENTITY_INSERT [dbo].[ParticipantExitSurveyQuestions] ON 

INSERT [dbo].[ParticipantExitSurveyQuestions] ([Id], [IsActive], [Sequence], [QuestionType], [Question], [DateAdded]) 
VALUES 
	(1, 1, 1, 1, N'How did you hear about this training?', GETDATE()),
	(2, 1, 2, 1, N'Why did you apply to take this training?', GETDATE()),
	(3, 1, 3, 1, N'On a scale of 1-4, how did this training meet your content and delivery expectations?', GETDATE()),
	(4, 1, 4, 2, N'How did you benefit from taking the training? Select all that apply:', GETDATE()),
	(5, 1, 5, 1, N'Did you feel supported through the recruitment and training process?', GETDATE()),
	(6, 1, 6, 1, N'Do you or will you be working in the community where you received your training?', GETDATE()),
	(7, 1, 7, 3, N'What changes could be made to improve the experience? Which of the available participantsupports were the most helpful to you?', GETDATE())

INSERT [dbo].ParticipantExitSurveyQuestionOptions ([ParticipantExitSurveyQuestionId], [IsActive], [Sequence], [OptionText], [ReplacementToken], [AllowOther], [DateAdded]) 
VALUES 
	(1, 1, 1, N'Advertising/social media', NULL, 0, GETDATE()),
	(1, 1, 2, N'Through [TrainingProviderName]', N'[TrainingProviderName]', 0, GETDATE()),
	(1, 1, 3, N'Through [AgreementHolderName]', N'[AgreementHolderName]', 0, GETDATE()),
	(1, 1, 4, N'Local WorkBC office', NULL, 0, GETDATE()),
	(1, 1, 5, N'Other', NULL, 1, GETDATE()),

	(2, 1, 1, N'To gain new skills and increase employability', NULL, 0, GETDATE()),
	(2, 1, 2, N'To meet job requirements or gain certification', NULL, 0, GETDATE()),
	(2, 1, 3, N'To pursue personal development and growth', NULL, 0, GETDATE()),
	(2, 1, 4, N'To empower myself and my community through education', NULL, 0, GETDATE()),

	(3, 1, 1, N'1. Far below expectations', NULL, 0, GETDATE()),
	(3, 1, 2, N'2. Below expectations', NULL, 0, GETDATE()),
	(3, 1, 3, N'3. Met expectations', NULL, 0, GETDATE()),
	(3, 1, 4, N'4. Exceeded expectations', NULL, 0, GETDATE()),

	(4, 1, 1, N'Secured job interview', NULL, 0, GETDATE()),
	(4, 1, 2, N'Secured job offer', NULL, 0, GETDATE()),
	(4, 1, 3, N'Obtained a promotion or full-time position at current job', NULL, 0, GETDATE()),
	(4, 1, 4, N'Obtained required occupational skills or certification', NULL, 0, GETDATE()),

	(5, 1, 1, N'Yes', NULL, 0, GETDATE()),
	(5, 1, 2, N'No', NULL, 0, GETDATE()),

	(6, 1, 1, N'Yes', NULL, 0, GETDATE()),
	(6, 1, 2, N'No', NULL, 0, GETDATE()),
	(6, 1, 3, N'If no, which community will you be working in?', NULL, 1, GETDATE()),

	(7, 1, 1, N'What changes could be made to improve the experience? Which of the available participant supports were the most helpful to you?', NULL, 0, GETDATE())

SET IDENTITY_INSERT [dbo].[ParticipantExitSurveyQuestions] OFF

PRINT 'End of Participant Exit Survey Questions + Options'