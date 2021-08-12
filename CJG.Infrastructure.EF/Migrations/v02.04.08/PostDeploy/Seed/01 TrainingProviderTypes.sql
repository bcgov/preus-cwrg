PRINT 'Start ESS [TrainingProviderTypes] Update'

PRINT 'Inserting ESS [TrainingProviderTypes]'

SET IDENTITY_INSERT [dbo].[TrainingProviderTypes] ON 

INSERT [dbo].[TrainingProviderTypes] ([Id], [IsActive], [RowSequence], [Caption], [DateAdded], [PrivateSectorValidationType], [CourseOutline], [UseForProviderTypes]) 
VALUES (16, 1, 11, N'Private training institution not certified by the Private Training Institutions Branch', GETDATE(), 2, 1, 2)

INSERT [dbo].[TrainingProviderTypes] ([Id], [IsActive], [RowSequence], [Caption], [DateAdded], [PrivateSectorValidationType], [CourseOutline], [UseForProviderTypes]) 
VALUES (17, 1, 12, N'Social service organization', GETDATE(), 2, 1, 2)

INSERT [dbo].[TrainingProviderTypes] ([Id], [IsActive], [RowSequence], [Caption], [DateAdded], [PrivateSectorValidationType], [CourseOutline], [UseForProviderTypes]) 
VALUES (18, 1, 13, N'Indigenous training and employment organization', GETDATE(), 2, 1, 2)

INSERT [dbo].[TrainingProviderTypes] ([Id], [IsActive], [RowSequence], [Caption], [DateAdded], [PrivateSectorValidationType], [CourseOutline], [UseForProviderTypes]) 
VALUES (19, 1, 14, N'Certified job coach', GETDATE(), 2, 1, 2)

INSERT [dbo].[TrainingProviderTypes] ([Id], [IsActive], [RowSequence], [Caption], [DateAdded], [PrivateSectorValidationType], [CourseOutline], [UseForProviderTypes]) 
VALUES (20, 1, 15, N'Employment or training organization', GETDATE(), 2, 1, 2)

SET IDENTITY_INSERT [dbo].[TrainingProviderTypes] OFF
PRINT 'Done ESS [TrainingProviderTypes] Update'