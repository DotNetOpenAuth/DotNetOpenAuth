CREATE PROCEDURE [dbo].[AddUser]
	(
	@firstName nvarchar(50),
	@lastName nvarchar(50),
	@openid nvarchar(255),
	@role nvarchar(255)
	)
AS
	DECLARE
		@roleid int,
		@userid int

	BEGIN TRANSACTION

	INSERT INTO [dbo].[User] (FirstName, LastName) VALUES (@firstName, @lastName)
	SET @userid = (SELECT @@IDENTITY)
	
	IF (SELECT COUNT(*) FROM dbo.Role WHERE [Name] = @role) = 0
	BEGIN
		INSERT INTO dbo.Role (Name) VALUES (@role)
		SET @roleid = (SELECT @@IDENTITY)
	END
	ELSE
	BEGIN
		SET @roleid = (SELECT RoleId FROM dbo.Role WHERE [Name] = @role)
	END
	
	INSERT INTO dbo.UserRole (UserId, RoleId) VALUES (@userId, @roleid)
	
	INSERT INTO dbo.AuthenticationToken 
		(UserId, OpenIdClaimedIdentifier, OpenIdFriendlyIdentifier)
		VALUES
		(@userid, @openid, @openid)
	
	COMMIT TRANSACTION
	
	RETURN @userid
