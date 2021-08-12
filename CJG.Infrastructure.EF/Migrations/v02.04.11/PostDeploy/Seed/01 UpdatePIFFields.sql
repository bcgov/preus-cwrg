PRINT 'Start Updating PIF Request'

PRINT 'Start Updating [EmploymentStatus] lookup'

-- Update EmploymentStatus to new list
--•	Unemployed
--•	Employed (including self-employed) part-time, seasonally, or casually 
--•	Precariously employed (including self-employed)
--•	Not in the labour force

UPDATE [EmploymentStatus]
SET Caption = N'Employed (including self-employed) part-time, seasonally, or casually',
	RowSequence = 2,
	DateUpdated = GETUTCDATE()
WHERE Id = 3

UPDATE [EmploymentStatus]
SET Caption = N'Precariously employed (including self-employed)',
	RowSequence = 3,
	DateUpdated = GETUTCDATE()
WHERE Id = 2

UPDATE [EmploymentStatus]
SET Caption = N'Not in the labour force',
	DateUpdated = GETUTCDATE()
WHERE Id = 4

UPDATE [EmploymentStatus]
SET IsActive = 0, 
	DateUpdated = GETUTCDATE()
WHERE Id = 5

--SET IDENTITY_INSERT [dbo].[EmploymentStatus] ON 
--INSERT INTO [EmploymentStatus] (Id, Caption, IsActive, RowSequence, DateAdded)
--VALUES (6, 'Precariously employed (including self-employed)', 1, 3, GETUTCDATE())
--SET IDENTITY_INSERT [dbo].[EmploymentStatus] OFF

PRINT 'Done Updating [EmploymentStatus] lookup'


PRINT 'Start Updating [EmploymentTypes]'

-- Permanent => FULL-TIME
-- new => PART-TIME
-- Temporary => PRECARIOUS
-- Casual => CASUAL or ON-CALL
-- Seasonal => SEASONAL
-- new => UMEMPLOYED

UPDATE [EmploymentTypes]
SET Caption = N'Full-Time',
	DateUpdated = GETUTCDATE(),
	RowSequence = 1
WHERE Id = 4

UPDATE [EmploymentTypes]
SET Caption = N'Precarious',
	DateUpdated = GETUTCDATE(),
	RowSequence = 3
WHERE Id = 2

UPDATE [EmploymentTypes]
SET Caption = N'Casual or On-Call',
	DateUpdated = GETUTCDATE(),
	RowSequence = 4
WHERE Id = 3

UPDATE [EmploymentTypes]
SET DateUpdated = GETUTCDATE(),
	RowSequence = 5
WHERE Id = 1

SET IDENTITY_INSERT [dbo].[EmploymentTypes] ON 

INSERT INTO [EmploymentTypes] (Id, Caption, IsActive, RowSequence, DateAdded)
VALUES (5, 'Part-Time', 1, 2, GETUTCDATE())

INSERT INTO [EmploymentTypes] (Id, Caption, IsActive, RowSequence, DateAdded)
VALUES (6, 'Unemployed', 1, 6, GETUTCDATE())

SET IDENTITY_INSERT [dbo].[EmploymentTypes] OFF

PRINT 'Done Updating [EmploymentTypes]'




PRINT 'Start adding new TrainingResult'

SET IDENTITY_INSERT [dbo].[TrainingResults] ON 
INSERT INTO [TrainingResults] (Id, Caption, IsActive, RowSequence, DateAdded)
VALUES (7, 'Moving from unemployment to employment', 1, 7, GETUTCDATE())
SET IDENTITY_INSERT [dbo].[TrainingResults] OFF

PRINT 'Done adding new TrainingResult'

PRINT 'Done Updating PIF Request'
