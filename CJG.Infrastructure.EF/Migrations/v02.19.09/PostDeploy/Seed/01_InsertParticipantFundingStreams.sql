PRINT 'Start Inserting [ParticipantFundingStreams]'
SET IDENTITY_INSERT [dbo].[ParticipantFundingStreams] ON 

INSERT [dbo].[ParticipantFundingStreams] ([Id], [IsActive], [RowSequence], [Caption], [DateAdded]) 
VALUES 
(1, 1, 1, N'Steel Tariffs', GETUTCDATE()),
(2, 1, 2, N'Softwood Lumber Tariffs', GETUTCDATE()),
(3, 1, 3, N'Comprehensive Tariffs', GETUTCDATE()),
(8, 1, 4, N'Not Applicable', GETUTCDATE())

SET IDENTITY_INSERT [dbo].[ParticipantFundingStreams] OFF
PRINT 'Done Inserting [ParticipantFundingStreams]'
