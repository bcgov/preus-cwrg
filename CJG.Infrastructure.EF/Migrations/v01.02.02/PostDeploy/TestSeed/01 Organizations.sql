-- Check if the Organization exists, if it doesn't add it.
IF (NOT EXISTS (SELECT TOP 1 Id fROM [dbo].[Organizations] WHERE [BCeIDGuid] = N'3ad433a8-614c-4484-912c-854e17ff6228'))
BEGIN
	PRINT 'Inserting [Organizations]'
	INSERT [dbo].[Organizations]
	 ([BceIdGuid], [LegalName], [EmployerTypeCode]) VALUES
	 (N'3ad433a8-614c-4484-912c-854e17ff6228', N'CJF BC SPsT', 0)
END