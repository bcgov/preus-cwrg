PRINT 'Inserting [BusinessContactRoles]'

-- Fixing issue with incorrectly assigning the same user to the Grant Application.
UPDATE ea
SET ea.[UserId] = aa.[UserId]
FROM [dbo].[BusinessContactRoles] ea
	INNER JOIN [dbo].[BusinessContactRoles] aa ON ea.[GrantApplicationId] = aa.[GrantApplicationId]
