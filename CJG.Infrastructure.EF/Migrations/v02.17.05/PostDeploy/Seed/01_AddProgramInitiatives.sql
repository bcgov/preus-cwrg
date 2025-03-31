PRINT 'Start of Program Initiatives Addition'

SET IDENTITY_INSERT [dbo].[ProgramInitiatives] ON 

INSERT INTO [dbo].[ProgramInitiatives] ([Id], [Name], [Code], [IsActive], [RowSequence] ,[DateAdded])
VALUES
	(1, 'Community Workforce Response Grant', 'WDA', 1, 1, GETUTCDATE()),
	(2, 'Community Response', 'Provincial', 1, 2, GETUTCDATE())
GO

SET IDENTITY_INSERT [dbo].[ProgramInitiatives] OFF

PRINT 'End of Program Initiatives Addition'
