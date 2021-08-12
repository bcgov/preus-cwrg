PRINT 'Start of Update DirectorBudget FiscalYear'

  UPDATE DirectorBudgets
  SET DateUpdated = GETUTCDATE(),
	  FiscalYearId = (SELECT Id FROM FiscalYears WHERE Caption = 'FY2024/2025')

PRINT 'End of Update DirectorBudget FiscalYear'

