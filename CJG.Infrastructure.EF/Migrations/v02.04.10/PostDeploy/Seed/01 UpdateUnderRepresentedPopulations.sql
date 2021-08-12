PRINT 'Start Updating [UnderRepresentedPopulations]'

SET IDENTITY_INSERT [dbo].[UnderRepresentedPopulations] ON 

INSERT INTO [dbo].[UnderRepresentedPopulations] (Id, Caption, IsActive, RowSequence, DateAdded)
VALUES (5, N'People of Colour', 1, 2, GETUTCDATE())

SET IDENTITY_INSERT [dbo].[UnderRepresentedPopulations] OFF

UPDATE [UnderRepresentedPopulations]
SET Caption = N'Indigenous persons',
	RowSequence = 1,
	DateUpdated = GETUTCDATE()
WHERE Id = 1

UPDATE [UnderRepresentedPopulations]
SET Caption = N'Landed immigrants to Canada (within 10 years of landing)',
	RowSequence = 3,
	DateUpdated = GETUTCDATE()
WHERE Id = 2

UPDATE [UnderRepresentedPopulations]
SET RowSequence = 4,
	DateUpdated = GETUTCDATE()
WHERE Id = 3

UPDATE [UnderRepresentedPopulations]
SET	Caption = N'Youth (aged 16 to 29)',
	RowSequence = 5,
	DateUpdated = GETUTCDATE()
WHERE Id = 4

PRINT 'Done Updating [UnderRepresentedPopulations]'
