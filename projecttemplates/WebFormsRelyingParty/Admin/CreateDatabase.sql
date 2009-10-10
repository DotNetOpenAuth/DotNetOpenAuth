/****** Object:  Table [dbo].[User]    Script Date: 10/08/2009 18:10:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[EmailAddress] [nvarchar](100) NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Role]    Script Date: 10/08/2009 18:10:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Role](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserRole]    Script Date: 10/08/2009 18:10:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserRole](
	[UserId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
 CONSTRAINT [PK_UserRole] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AuthenticationToken]    Script Date: 10/08/2009 18:10:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AuthenticationToken](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[OpenIdClaimedIdentifier] [nvarchar](250) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL, -- very important that claimed_id comparisons be case sensitive
	[OpenIdFriendlyIdentifier] [nvarchar](250) NULL,
 CONSTRAINT [PK_AuthenticationToken] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[AddUser]    Script Date: 10/08/2009 18:10:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
		SET @roleid = (SELECT Id FROM dbo.Role WHERE [Name] = @role)
	END
	
	INSERT INTO dbo.UserRole (UserId, RoleId) VALUES (@userId, @roleid)
	
	INSERT INTO dbo.AuthenticationToken 
		(UserId, OpenIdClaimedIdentifier, OpenIdFriendlyIdentifier)
		VALUES
		(@userid, @openid, @openid)
	
	COMMIT TRANSACTION
	
	RETURN @userid
GO
/****** Object:  ForeignKey [FK_UserRole_Role]    Script Date: 10/08/2009 18:10:17 ******/
ALTER TABLE [dbo].[UserRole]  WITH CHECK ADD  CONSTRAINT [FK_UserRole_Role] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Role] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserRole] CHECK CONSTRAINT [FK_UserRole_Role]
GO
/****** Object:  ForeignKey [FK_UserRole_User]    Script Date: 10/08/2009 18:10:17 ******/
ALTER TABLE [dbo].[UserRole]  WITH CHECK ADD  CONSTRAINT [FK_UserRole_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserRole] CHECK CONSTRAINT [FK_UserRole_User]
GO
/****** Object:  ForeignKey [FK_AuthenticationToken_User]    Script Date: 10/08/2009 18:10:17 ******/
ALTER TABLE [dbo].[AuthenticationToken]  WITH CHECK ADD  CONSTRAINT [FK_AuthenticationToken_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AuthenticationToken] CHECK CONSTRAINT [FK_AuthenticationToken_User]
GO
