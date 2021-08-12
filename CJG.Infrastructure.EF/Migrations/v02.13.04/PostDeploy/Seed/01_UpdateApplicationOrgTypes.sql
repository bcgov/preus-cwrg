PRINT 'Start Updating Applicant Organization Types'

SET IDENTITY_INSERT [dbo].[ApplicantOrganizationTypes] ON

INSERT INTO [dbo].[ApplicantOrganizationTypes] (Id, Caption, RowSequence, IsActive, DateAdded)
VALUES
(22, 'Major Employer in a community with a single resource economy', 0, 1, GETUTCDATE()),
(23, 'Local Governments', 0, 1, GETUTCDATE()),
(24, 'Indigenous-owned or directed non-profit social agencies', 0, 1, GETUTCDATE()),
(25, 'Province-wide Indigenous organizations', 0, 1, GETUTCDATE())

SET IDENTITY_INSERT [dbo].[ApplicantOrganizationTypes] OFF

UPDATE [dbo].[ApplicantOrganizationTypes]
SET Caption = 'Local Non-profit Service Providers',
    DateUpdated = GETUTCDATE()
WHERE Id = 3

UPDATE [dbo].[ApplicantOrganizationTypes]
SET IsActive = 0,
    DateUpdated = GETUTCDATE()
WHERE Id IN (
15	-- Agency
,2	-- Civic, Social, and Fraternal Associations
,13	-- County
,16	-- Crown Corps
,19	-- FN Health Authority
,4	-- Job Training and Vocational Rehabilitation Services
,6	-- Membership Organizations
,12	-- Municipality
,7	-- Political Organizations
,8	-- Professional Membership Organizations
,9	-- Religious Organizations
,14	-- Township
)

PRINT 'End Updating Applicant Organization Types'

