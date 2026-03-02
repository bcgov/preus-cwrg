PRINT 'Start Inserting + Updating [CompletionReportOptions]'

-- Add new work-sharing answer
INSERT INTO CompletionReportOptions (QuestionId, Answer, Level, TriggersNextLevel, Sequence, IsActive, DisplayOther, DateAdded, NextQuestion)
VALUES (16, 'Work-sharing', 1, 0, 5, 1, 1, GETUTCDATE(), 0)

-- Move the existing non-work options forward in sequence
UPDATE CompletionReportOptions
SET [Sequence] = 6,
    DateUpdated = GETUTCDATE()
WHERE QuestionId = 16
AND Answer = 'Unemployed but looking for work'

UPDATE CompletionReportOptions
SET [Sequence] = 7,
    DateUpdated = GETUTCDATE()
WHERE QuestionId = 16
AND Answer = 'Unemployed and not looking for work'

UPDATE CompletionReportOptions
SET [Sequence] = 8,
    DateUpdated = GETUTCDATE()
WHERE QuestionId = 16
AND Answer = 'Unknown'


PRINT 'Done Inserting + Updating [CompletionReportOptions]'
