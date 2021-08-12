PRINT 'Start Updating "Program" to "Project"'

PRINT 'Updating ServiceLine'

UPDATE ServiceLines
SET [Description] = REPLACE(Description, ' program ', ' project '),
	DateUpdated = GETUTCDATE()
WHERE [Description] LIKE '% program % '


PRINT 'Updating Completion Report Questions'

UPDATE CompletionReportQuestions
SET Question = REPLACE(Question, ' program', ' project'),
	DefaultText = REPLACE(DefaultText, ' Program', ' Project'),
	[DateUpdated] = GETUTCDATE()
WHERE Question LIKE '% program%'

-- Replace any eagerly replaced references to the entire CWRG Project Name
UPDATE CompletionReportQuestions 
SET Question = REPLACE(Question, 'Community Workforce Response Grant Project', 'Community Workforce Response Grant Program'),
	[DateUpdated] = GETUTCDATE()
WHERE Question LIKE '%Community Workforce Response Grant Project%'

PRINT 'Done Updating "Program" to "Project"'