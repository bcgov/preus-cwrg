PRINT 'Start Updating CWRG PIF Lookup data'

-- Change title of Precariously Employed
UPDATE EmploymentStatus
SET Caption = 'Employed Precariously (unstable)',
    DateUpdated = GETUTCDATE()
WHERE Id = 2

-- Deactivate Precarious and Unemployed
UPDATE EmploymentTypes
SET IsActive = 0,
    DateUpdated = GETUTCDATE()
WHERE Id IN (2, 6)

PRINT 'Done Updating CWRG PIF Lookup data'
