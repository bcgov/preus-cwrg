PRINT 'Start update Program Initiative visibility'

UPDATE ProgramInitiatives 
SET ShowInProgramFundingReport = 1,
    DateUpdated = GETUTCDATE()
WHERE Code <> 'Provincial'

PRINT 'End update Program Initiative visibility'
