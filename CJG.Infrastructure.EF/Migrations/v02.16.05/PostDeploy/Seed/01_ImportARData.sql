PRINT 'Start of AR Data Import'

PRINT '-- Inserting AR Parent records from Claim AR data'
INSERT INTO AccountsReceivables (GrantApplicationId, PaidDate, DateAdded)
SELECT c.GrantApplicationId, MIN(cec.AccountsReceivablePaidDate) AS PaidDate, GETDATE() AS DateAdded
FROM ClaimEligibleCosts cec
INNER JOIN Claims c ON c.Id = cec.ClaimId AND c.ClaimVersion = cec.ClaimVersion
INNER JOIN EligibleExpenseTypes eet ON eet.Id = cec.EligibleExpenseTypeId
WHERE AccountsReceivableOverpayment != 0
GROUP BY GrantApplicationId

PRINT '-- Inserting AR Child records from Claim AR data'
INSERT INTO AccountsReceivableEntries (AccountsReceivableId, ServiceCategoryId, Overpayment, DateAdded)
SELECT (SELECT ar.Id FROM AccountsReceivables ar WHERE ar.GrantApplicationId = c.GrantApplicationId) AS GrantApplicationId, eet.ServiceCategoryId AS ServiceCategoryId, cec.AccountsReceivableOverpayment AS Overpayment, cec.AccountsReceivablePaidDate AS PaidDate
FROM ClaimEligibleCosts cec
INNER JOIN Claims c ON c.Id = cec.ClaimId AND c.ClaimVersion = cec.ClaimVersion
INNER JOIN EligibleExpenseTypes eet ON eet.Id = cec.EligibleExpenseTypeId
WHERE AccountsReceivableOverpayment != 0


PRINT 'End of AR Data Import'
