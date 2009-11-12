/****** Object:  Table [dbo].[Consumer]    Script Date: 11/11/2009 17:04:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Consumer](
	[ConsumerId] [int] IDENTITY(1,1) NOT NULL,
	[ConsumerKey] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
	[ConsumerSecret] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[X509Certificate] [image] NULL,
	[Callback] [nvarchar](2048) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[VerificationCodeFormat] [int] NOT NULL,
	[VerificationCodeLength] [int] NOT NULL,
	[Name] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_Consumer] PRIMARY KEY CLUSTERED 
(
	[ConsumerId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Consumer] ON [dbo].[Consumer] 
(
	[ConsumerKey] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User]    Script Date: 11/11/2009 17:04:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[LastName] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[EmailAddress] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[EmailAddressVerified] [bit] NOT NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Role]    Script Date: 11/11/2009 17:04:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Role](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IssuedToken]    Script Date: 11/11/2009 17:04:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[IssuedToken](
	[TokenId] [int] IDENTITY(1,1) NOT NULL,
	[ConsumerId] [int] NOT NULL,
	[UserId] [int] NULL,
	[Token] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
	[TokenSecret] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[Callback] [nvarchar](2048) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[VerificationCode] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ConsumerVersion] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ExpirationDate] [datetime] NULL,
	[IsAccessToken] [bit] NOT NULL,
	[Scope] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_IssuedToken] PRIMARY KEY CLUSTERED 
(
	[TokenId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_IssuedToken] ON [dbo].[IssuedToken] 
(
	[Token] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserRole]    Script Date: 11/11/2009 17:04:47 ******/
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
/****** Object:  Table [dbo].[AuthenticationToken]    Script Date: 11/11/2009 17:04:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AuthenticationToken](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[OpenIdClaimedIdentifier] [nvarchar](250) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
	[OpenIdFriendlyIdentifier] [nvarchar](250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_AuthenticationToken] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[AddUser]    Script Date: 11/11/2009 17:05:07 ******/
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
/****** Object:  Default [DF_User_EmailAddressVerified]    Script Date: 11/11/2009 17:04:47 ******/
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_EmailAddressVerified]  DEFAULT ((0)) FOR [EmailAddressVerified]
GO
/****** Object:  Default [DF_IssuedToken_CreatedOn]    Script Date: 11/11/2009 17:04:47 ******/
ALTER TABLE [dbo].[IssuedToken] ADD  CONSTRAINT [DF_IssuedToken_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
/****** Object:  Default [DF_IssuedToken_IsAccessToken]    Script Date: 11/11/2009 17:04:47 ******/
ALTER TABLE [dbo].[IssuedToken] ADD  CONSTRAINT [DF_IssuedToken_IsAccessToken]  DEFAULT ((0)) FOR [IsAccessToken]
GO
/****** Object:  ForeignKey [FK_IssuedToken_Consumer]    Script Date: 11/11/2009 17:04:47 ******/
ALTER TABLE [dbo].[IssuedToken]  WITH CHECK ADD  CONSTRAINT [FK_IssuedToken_Consumer] FOREIGN KEY([ConsumerId])
REFERENCES [dbo].[Consumer] ([ConsumerId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[IssuedToken] CHECK CONSTRAINT [FK_IssuedToken_Consumer]
GO
/****** Object:  ForeignKey [FK_IssuedToken_User]    Script Date: 11/11/2009 17:04:47 ******/
ALTER TABLE [dbo].[IssuedToken]  WITH CHECK ADD  CONSTRAINT [FK_IssuedToken_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[IssuedToken] CHECK CONSTRAINT [FK_IssuedToken_User]
GO
/****** Object:  ForeignKey [FK_UserRole_Role]    Script Date: 11/11/2009 17:04:47 ******/
ALTER TABLE [dbo].[UserRole]  WITH CHECK ADD  CONSTRAINT [FK_UserRole_Role] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Role] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserRole] CHECK CONSTRAINT [FK_UserRole_Role]
GO
/****** Object:  ForeignKey [FK_UserRole_User]    Script Date: 11/11/2009 17:04:47 ******/
ALTER TABLE [dbo].[UserRole]  WITH CHECK ADD  CONSTRAINT [FK_UserRole_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserRole] CHECK CONSTRAINT [FK_UserRole_User]
GO
/****** Object:  ForeignKey [FK_AuthenticationToken_User]    Script Date: 11/11/2009 17:04:47 ******/
ALTER TABLE [dbo].[AuthenticationToken]  WITH CHECK ADD  CONSTRAINT [FK_AuthenticationToken_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AuthenticationToken] CHECK CONSTRAINT [FK_AuthenticationToken_User]
GO
