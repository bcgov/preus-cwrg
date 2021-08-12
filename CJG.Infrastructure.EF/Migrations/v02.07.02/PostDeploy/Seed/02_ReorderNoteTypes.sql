PRINT 'Start CWRG Note Types Reorder'

UPDATE [dbo].[NoteTypes] SET RowSequence = 1, DateUpdated = GETUTCDATE() WHERE [Id] = 21
UPDATE [dbo].[NoteTypes] SET RowSequence = 2, DateUpdated = GETUTCDATE() WHERE [Id] = 7
UPDATE [dbo].[NoteTypes] SET RowSequence = 3, DateUpdated = GETUTCDATE() WHERE [Id] = 19
UPDATE [dbo].[NoteTypes] SET RowSequence = 4, DateUpdated = GETUTCDATE() WHERE [Id] = 13
UPDATE [dbo].[NoteTypes] SET RowSequence = 5, DateUpdated = GETUTCDATE() WHERE [Id] = 12
UPDATE [dbo].[NoteTypes] SET RowSequence = 6, DateUpdated = GETUTCDATE() WHERE [Id] = 20
UPDATE [dbo].[NoteTypes] SET RowSequence = 7, DateUpdated = GETUTCDATE() WHERE [Id] = 14
UPDATE [dbo].[NoteTypes] SET RowSequence = 8, DateUpdated = GETUTCDATE() WHERE [Id] = 8

UPDATE [dbo].[NoteTypes] SET RowSequence = 10, DateUpdated = GETUTCDATE() WHERE [Id] = 15
UPDATE [dbo].[NoteTypes] SET RowSequence = 11, DateUpdated = GETUTCDATE() WHERE [Id] = 16
UPDATE [dbo].[NoteTypes] SET RowSequence = 12, DateUpdated = GETUTCDATE() WHERE [Id] = 17
UPDATE [dbo].[NoteTypes] SET RowSequence = 13, DateUpdated = GETUTCDATE() WHERE [Id] = 11
UPDATE [dbo].[NoteTypes] SET RowSequence = 14, DateUpdated = GETUTCDATE() WHERE [Id] = 18

PRINT 'Done CWRG Note Types Order'
