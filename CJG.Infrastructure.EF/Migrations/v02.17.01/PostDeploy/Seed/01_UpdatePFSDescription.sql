PRINT 'Start of update PFS Service Description'

UPDATE ServiceCategories
SET Description = 'Participant Financial Supports (PFS) are designed to remove barriers to the participant’s success in the program.',
	DateUpdated = GETUTCDATE()
WHERE Caption = 'Participant Financial Supports'

PRINT 'End of update PFS Service Description'