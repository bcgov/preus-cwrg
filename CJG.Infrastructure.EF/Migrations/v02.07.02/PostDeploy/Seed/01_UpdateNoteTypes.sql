PRINT 'Start Updating CWRG Note Types'

-- 1. change “Note to Assessor” to “Note to Program Manager” while maintaining original “Notes to Assessor.”
UPDATE [dbo].[NoteTypes] SET
	[Caption] = N'PR', 
	[Description] = N'Note to Program Manager',
	DateUpdated = GETUTCDATE()
WHERE [Id] = 12

-- 3. change “Note to QA to “Note to Section 25. “ Note to QA” currently contains Section 25 requests - suggested that “Note to QA” be renamed “Note to Section 25” so that current notes will be reassigned to this category and therefore note transfer does not have to be manually completed.
UPDATE [dbo].[NoteTypes] SET
	[Caption] = N'S25', 
	[Description] = N'Note to Section 25',
	DateUpdated = GETUTCDATE()
WHERE [Id] = 8

-- 2. add “Note to Evaluation”
-- 4. add new “Note to QA”
-- 5. add “Note to Communication (Marketing & News Releases)”
SET IDENTITY_INSERT [dbo].[NoteTypes] ON 

INSERT [dbo].[NoteTypes] 
 ([Id], [IsSystem], [IsActive], [RowSequence], [Caption], [Description], [DateAdded]) 
VALUES
 (19, 0, 1, 6, N'NE', N'Note to Evaluation', GETUTCDATE()),
 (20, 0, 1, 7, N'QA', N'Note to QA', GETUTCDATE()),
 (21, 0, 1, 8, N'MN', N'Note to Communication (Marketing & News Releases)', GETUTCDATE())

SET IDENTITY_INSERT [dbo].[NoteTypes] OFF

PRINT 'Done Updating CWRG Note Types'
