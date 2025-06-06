PRINT 'Inserting [Users]'

-- External Users
IF (NOT EXISTS (SELECT * FROM [dbo].[Users] WHERE [BCeIDGuid] = N'6f12f706-6bbd-499f-9b02-c232873316ea'))
BEGIN
	INSERT [dbo].[Users]
		([AccountType], [BCeIDGuid], [Salutation], [FirstName], [LastName], [EmailAddress], [JobTitle], [OrganizationId], [IsOrganizationProfileAdministrator], [IsSubscriberToEmail]) VALUES
		(0, N'6f12f706-6bbd-499f-9b02-c232873316ea', N'Mrs.', N'Bonny', N'Hasting', N'bonny.hastings@gov.bc.ca', 'Business Manager', (SELECT TOP 1 Id fROM [dbo].[Organizations] WHERE [BCeIDGuid] = N'3ad433a8-614c-4484-912c-854e17ff6228'), 0, 0) -- BHastings  
END

IF (NOT EXISTS (SELECT * FROM [dbo].[Users] WHERE [BCeIDGuid] = N'394e765b-e171-4467-a41c-02d7c5e3e2b3'))
BEGIN
	INSERT [dbo].[Users]
		([AccountType], [BCeIDGuid], [Salutation], [FirstName], [LastName], [EmailAddress], [JobTitle], [OrganizationId], [IsOrganizationProfileAdministrator], [IsSubscriberToEmail]) VALUES
		(0, N'394e765b-e171-4467-a41c-02d7c5e3e2b3', N'Mrs.', N'Kate', N'Moyer', N'kate.moyer@gov.bc.ca', 'Business Manager', (SELECT TOP 1 Id fROM [dbo].[Organizations] WHERE [BCeIDGuid] = N'3ad433a8-614c-4484-912c-854e17ff6228'), 0, 0) -- KMOYERCJF 
END

