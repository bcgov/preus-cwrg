PRINT 'Inserting [Users]'
SET IDENTITY_INSERT [dbo].[Users] ON
-- External Users
INSERT [dbo].[Users]
 ([Id], [AccountType], [BCeIDGuid], [Salutation], [FirstName], [LastName], [EmailAddress], [JobTitle], [MailingAddressId],
  [PhysicalAddressId], [OrganizationId], [IsOrganizationProfileAdministrator], [IsSubscriberToEmail], [PhoneNumber]) VALUES
 (01, 0, N'b0d099e1-fdfd-4d5f-9286-9e228f948903', N'Mr.', N'CJG01', N'Test01', N'Raman.Samra@avocette.com', 'Program Director', 1, 1, 1, 1, 0, N'(604) 662-8407')
,(02, 0, N'221371bb-030c-4163-8c58-15768336da55', N'Mr.', N'CJG02', N'Test02', N'Raman.Samra@avocette.com', 'Program Director', 1, 1, 1, 0, 0, N'(604) 662-8407')
,(03, 0, N'305494E4-8680-4880-9C6C-F743B04FC8F6', N'Mr.', N'CJG03', N'Test03', N'ian@ccal.ca',        'Program Director', 4, 4, 4, 1, 0, N'(604) 395-6000')
,(04, 0, N'729E2A4D-FB99-4D4D-B34E-A08097E37486', N'Mr.', N'CJG04', N'Test04', N'ian@ccal.ca',        'Program Director', 4, 4, 4, 0, 0, N'(604) 395-6000')
,(05, 0, N'BE03CBFA-A00F-478B-894E-61841FF70EFB', N'Mr.', N'CJG05', N'Test05', N'ian@ccal.ca',        'Program Director', 4, 4, 4, 0, 0, N'(604) 395-6000')
SET IDENTITY_INSERT [dbo].[Users] OFF