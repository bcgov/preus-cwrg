PRINT 'Start of Course Outline update'

UPDATE [dbo].[TrainingProviderTypes]
SET CourseOutline = 1,
    DateUpdated = GETUTCDATE()
WHERE Caption LIKE '%B.C. Public Post-Secondary Institution%'
AND IsActive = 1

PRINT 'End of Course Outline update'