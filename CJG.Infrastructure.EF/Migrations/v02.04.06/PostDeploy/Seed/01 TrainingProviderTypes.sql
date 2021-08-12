PRINT 'Start [TrainingProviderTypes] Update'

PRINT 'Deactivating [TrainingProviderTypes]'

UPDATE [dbo].[TrainingProviderTypes]
SET [IsActive] = 0,
RowSequence = 99,
[DateUpdated] = GETDATE()
WHERE [Id] IN (2, 10, 12)


PRINT 'Updating [TrainingProviderTypes]'

UPDATE [dbo].[TrainingProviderTypes] 
SET RowSequence = 3, 
[DateUpdated] = GETDATE() 
WHERE [Id] = 7

UPDATE [dbo].[TrainingProviderTypes]
SET [Caption] = N'Private training institution neither certified nor designated by the Private Training Institutions Branch',
UseForProviderTypes = 1,
RowSequence = 5
WHERE Id = 8

UPDATE [dbo].[TrainingProviderTypes]
SET [Caption] = N'Union halls and training boards',
UseForProviderTypes = 1,
IsActive = 1,
RowSequence = 7, 
CourseOutline = 1
WHERE Id = 4

UPDATE [dbo].[TrainingProviderTypes]
SET [Caption] = N'Trade or industry recognized personal safety training provider',
UseForProviderTypes = 1,
IsActive = 1,
RowSequence = 9, 
CourseOutline = 1
WHERE Id = 5

UPDATE [dbo].[TrainingProviderTypes]
SET [Caption] = N'Industry/sector association',
UseForProviderTypes = 1,
RowSequence = 6,
[DateUpdated] = GETDATE()
WHERE [Id] = 9

PRINT 'Inserting [TrainingProviderTypes]'

SET IDENTITY_INSERT [dbo].[TrainingProviderTypes] ON 

INSERT [dbo].[TrainingProviderTypes] ([Id], [IsActive], [RowSequence], [Caption], [DateAdded], [PrivateSectorValidationType], [CourseOutline], [UseForProviderTypes]) 
VALUES (13, 1, 4, N'Private training institution designated by the Private Training Institutions Branch', GETDATE(), 2, 1, 1)

INSERT [dbo].[TrainingProviderTypes] ([Id], [IsActive], [RowSequence], [Caption], [DateAdded], [PrivateSectorValidationType], [CourseOutline], [UseForProviderTypes]) 
VALUES (14, 1, 8, N'Aboriginal-controlled institute', GETDATE(), 2, 1, 1)

INSERT [dbo].[TrainingProviderTypes] ([Id], [IsActive], [RowSequence], [Caption], [DateAdded], [PrivateSectorValidationType], [CourseOutline], [UseForProviderTypes]) 
VALUES (15, 1, 10, N'Indigenous skills training provider', GETDATE(), 2, 1, 1)

SET IDENTITY_INSERT [dbo].[TrainingProviderTypes] OFF
PRINT 'Done [TrainingProviderTypes] Update'


