PRINT 'Start of Participant Exit Survey corrections'

UPDATE [dbo].ParticipantExitSurveyQuestionOptions
SET [OptionText] = N'Other (please describe):'
WHERE [ParticipantExitSurveyQuestionId] = 10
AND [OptionText] = N'Other (please describe)'

UPDATE [dbo].ParticipantExitSurveyQuestionOptions
SET [OptionText] = N'To stay in my community'
WHERE [ParticipantExitSurveyQuestionId] = 11
AND [OptionText] = N'To empower myself and my community through education'

PRINT 'End of Participant Exit Survey corrections'