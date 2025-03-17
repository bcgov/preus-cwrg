PRINT 'Start of Grant Stream attachment update'

UPDATE GrantStreams
SET AttachmentsHeader = 'Required Documents',
	AttachmentsRequired = 1
WHERE IsActive = 1
AND GrantProgramId = 3
AND AttachmentsIsEnabled = 1

PRINT 'End of Grant Stream attachment update'