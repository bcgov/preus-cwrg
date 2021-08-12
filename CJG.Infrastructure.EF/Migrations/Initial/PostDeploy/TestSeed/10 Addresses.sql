PRINT 'Inserting [Addresses]'
SET IDENTITY_INSERT [dbo].[Addresses] ON
INSERT [dbo].[Addresses]
 ([Id], [AddressLine1], [AddressLine2], [City], [PostalCode], [RegionId], [CountryId]) VALUES
 (1, N'730 View St.',      N'Suite 320',         N'Victoria',        N'V8W 3Y7', N'BC', N'CA')
,(2, N'333 Seymour St.',   N'Suite 1000',        N'Vancouver',       N'V6B 5A6', N'BC', N'CA')
,(3, N'1205 Broad Street', N'',                  N'Victoria',        N'V8W 2A4', N'BC', N'CA')
,(4, N'202 - 610 Street',  N'Royal City Center', N'New Westminster', N'V3L 3C2', N'BC', N'CA')
SET IDENTITY_INSERT [dbo].[Addresses] OFF