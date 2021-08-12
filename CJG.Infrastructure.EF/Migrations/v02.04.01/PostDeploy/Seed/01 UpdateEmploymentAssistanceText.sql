PRINT '"Employment Assistance Services" to "Employment Support Services"'

UPDATE DocumentTemplates
SET Body = REPLACE(Body, 'Employment Assistance Services', 'Employment Support Services'),
    [DateUpdated] = GETUTCDATE()
WHERE Body LIKE '%Employment Assistance Services%'

UPDATE CompletionReportOptions 
SET Answer = REPLACE(Answer, 'employment assistance services', 'employment support services'), 
    [DateUpdated] = GETUTCDATE()
WHERE Answer LIKE '%employment assistance services%';

UPDATE ServiceCategories
SET Caption = 'Employment Support Services',
	DateUpdated = GETUTCDATE()
WHERE Caption = 'Employment Assistance Services';

UPDATE [EligibleExpenseTypes]
SET Caption = 'Employment Support Services', 
    [DateUpdated] = GETUTCDATE()
WHERE Caption = 'Employment Assistance Services'