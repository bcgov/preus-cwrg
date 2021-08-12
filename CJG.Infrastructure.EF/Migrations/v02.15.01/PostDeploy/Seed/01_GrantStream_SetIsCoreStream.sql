PRINT 'Start of Update IsCoreStream for existing GrantStreams'

UPDATE [dbo].[GrantStreams]
SET IsCoreStream = 1,
	DateUpdated = GETUTCDATE()
WHERE [Name] <> 'Community Response'


PRINT 'End of Update IsCoreStream for existing GrantStreams'

