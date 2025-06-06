PRINT 'Start of Course Outline update'

Update [dbo].[TrainingProviderTypes]
Set CourseOutline=1
where Caption like '%B.C. Public Post-Secondary Institution%'
and IsActive=1

PRINT 'End of Course Outline update'