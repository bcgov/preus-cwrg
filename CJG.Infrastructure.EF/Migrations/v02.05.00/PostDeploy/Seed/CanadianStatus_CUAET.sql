PRINT 'Inserting new CUAET value to [CanadianStatus]'
SET IDENTITY_INSERT [dbo].[CanadianStatus] ON 
INSERT [dbo].[CanadianStatus]
 ([Id], [IsActive], [RowSequence], [Caption], [DateAdded]) VALUES
 (5, 1, 4, N'Temporary resident under the Canada-Ukraine Authorization for Emergency Travel (CUAET) measures', GETUTCDATE())
SET IDENTITY_INSERT [dbo].[CanadianStatus] OFF

PRINT 'Reordering [CanadianStatus] row sequence for "None of the above"'
UPDATE [dbo].[CanadianStatus]
SET RowSequence = 5,
	DateUpdated = GETUTCDATE()
WHERE Id = 4