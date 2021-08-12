PRINT 'Start Fixing change request + missing script run on QA'


PRINT 'Updating missed and requested SQL changes on QA'

UPDATE [dbo].[TrainingProviderTypes]
SET [Caption] = N'Private training institution designated by the Private Training Institutions Branch',
[DateUpdated] = GETDATE()
WHERE [Id] = 13

UPDATE [dbo].[TrainingProviderTypes]
SET [Caption] = N'Private training institution certified by the Private Training Institutions Branch',
[DateUpdated] = GETDATE()
WHERE [Id] = 7



PRINT 'Re-running v02.04.01 migrations'

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


PRINT 'Done Fixing change request + missing script run on QA'
