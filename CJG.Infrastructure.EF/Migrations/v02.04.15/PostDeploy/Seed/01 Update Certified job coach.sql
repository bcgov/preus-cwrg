PRINT 'Start disabling Training Provider Type ''Certified job coach'''


/*
	Disable training provider type 'Certified job coach'
*/

UPDATE [dbo].[TrainingProviderTypes]
SET [IsActive] = 0,
[DateUpdated] = GETDATE()
WHERE [Id] = 19


PRINT 'Done disabling Training Provider Type ''Certified job coach'''