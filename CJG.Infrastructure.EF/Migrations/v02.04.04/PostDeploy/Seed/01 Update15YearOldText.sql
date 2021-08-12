PRINT 'Update "15 years old" text to "16 years old"'

UPDATE DocumentTemplates
SET Body = REPLACE(Body, 'be at least 15 years old', 'be at least 16 years old'),
    [DateUpdated] = GETUTCDATE()
WHERE Body LIKE '%be at least 15 years old%'

UPDATE UnderRepresentedPopulations
SET Caption = 'Youth (aged 16 to 29)',
[DateUpdated] = GETUTCDATE()
WHERE Caption = 'Youth (aged 15 to 29)'

UPDATE VulnerableGroups
SET Caption = 'Youth at risk including youth in care or former youth in care (aged 16 to 29)',
[DateUpdated] = GETUTCDATE()
WHERE Caption = 'Youth at risk including youth in care or former youth in care (aged 15 to 29)'
