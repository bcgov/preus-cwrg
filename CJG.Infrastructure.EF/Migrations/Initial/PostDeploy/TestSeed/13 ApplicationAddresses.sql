PRINT 'Inserting [ApplicationAddresses]'
SET IDENTITY_INSERT [dbo].[ApplicationAddresses] ON
INSERT [dbo].[ApplicationAddresses]
 ([Id], [AddressLine1], [AddressLine2], [City], [PostalCode], [RegionId], [CountryId]) VALUES
 (1, N'730 View St.',    N'Suite 320',  N'Victoria',  N'V8W 3Y7', N'BC', N'CA')
,(2, N'730 View St.',    N'Suite 320',  N'Victoria',  N'V8W 3Y7', N'BC', N'CA')
,(3, N'730 View St.',    N'Suite 320',  N'Victoria',  N'V8W 3Y7', N'BC', N'CA')
,(4, N'730 View St.',    N'Suite 320',  N'Victoria',  N'V8W 3Y7', N'BC', N'CA')
,(5, N'333 Seymour St.', N'Suite 1000', N'Vancouver', N'V6B 5A6', N'BC', N'CA')
,(6, N'333 Seymour St.', N'Suite 1000', N'Vancouver', N'V6B 5A6', N'BC', N'CA')
SET IDENTITY_INSERT [dbo].[ApplicationAddresses] OFF