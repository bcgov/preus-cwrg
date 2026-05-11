PRINT 'Start adding Unassociated Account Codes'

-- These account codes are not associated to a specific Grant Program. They use used by the Claim Tariff codes.
INSERT INTO AccountCodes (GLClientNumber, GLRESP, GLServiceLine, GLSTOBNormal, GLSTOBAccrual, GLProjectCode, DateAdded)
VALUES 
('019', '11651', '11855', '8001', '8001', '111TRSW', GETUTCDATE()),
('019', '11651', '11855', '8001', '8001', '111TRST', GETUTCDATE()),
('019', '11651', '11855', '8001', '8001', '111TRCO', GETUTCDATE())

PRINT 'Done adding Unassociated Account Codes'

