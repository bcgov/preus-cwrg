PRINT 'Start Updating Training Provider Types'

SET IDENTITY_INSERT [dbo].[TrainingProviderTypes] ON

INSERT INTO [dbo].[TrainingProviderTypes]
           (Id
           ,[Caption]
           ,[IsActive]
           ,[RowSequence]
           ,[DateAdded]
           ,[PrivateSectorValidationType]
           ,[ProofOfInstructorQualifications]
           ,[CourseOutline]
           ,[UseForProviderTypes])
     VALUES
        (21, 'ICBC Designated Driving School', 1, 16, GETUTCDATE(), 2, 0, 1, 1)

SET IDENTITY_INSERT [dbo].[TrainingProviderTypes] OFF

PRINT 'End Updating Training Provider Types'

