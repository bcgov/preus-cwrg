PRINT 'Start Updating Participant Employment Statuses'

UPDATE ParticipantEmploymentStatus
SET Caption = 'Employed part-time, seasonally, or casually',
DateUpdated = GETUTCDATE()
WHERE Caption = 'Underemployed (part-time, seasonal, casual)'

UPDATE ParticipantEmploymentStatus
SET Caption = 'Precariously employed',
DateUpdated = GETUTCDATE()
WHERE Caption = 'Employed and low skilled (less than high school education)'

PRINT 'Done Updating Participant Employment Statuses'
