PRINT 'Start disabling CUAET value in [CanadianStatus]'

UPDATE [dbo].[CanadianStatus]
SET IsActive = 0,
	DateUpdated = GETUTCDATE()
WHERE Id = 5
AND Caption LIKE '%CUAET%'

PRINT 'Finished disabling CUAET value in [CanadianStatus]'
