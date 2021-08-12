PRINT 'Start Inserting Default Director Budgets'

SET IDENTITY_INSERT [dbo].[DirectorBudgets] ON

INSERT INTO [DirectorBudgets] (Id, BudgetEntryType, StreamFilter, DateAdded) VALUES
(1, 1, NULL, GETUTCDATE()), 
(2, 2, 'Community Response', GETUTCDATE())

SET IDENTITY_INSERT [dbo].[DirectorBudgets] OFF

PRINT 'End Inserting Default Director Budgets'
