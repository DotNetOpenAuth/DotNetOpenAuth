/*
Deployment script for RelyingPartyDatabase
*/

GO
SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, NUMERIC_ROUNDABORT, QUOTED_IDENTIFIER OFF;


GO
/*
:setvar Path1 "WEBROOT\App_Data\"
:setvar DatabaseName "RelyingPartyDatabase"
:setvar DefaultDataPath ""
*/

GO
USE [master]

GO
IF (DB_ID(N'$(DatabaseName)') IS NOT NULL
    AND DATABASEPROPERTYEX(N'$(DatabaseName)','Status') <> N'ONLINE')
BEGIN
    RAISERROR(N'The state of the target database, %s, is not set to ONLINE. To deploy to this database, its state must be set to ONLINE.', 16, 127,N'$(DatabaseName)') WITH NOWAIT
    RETURN
END

GO
IF (DB_ID(N'$(DatabaseName)') IS NOT NULL) 
BEGIN
    ALTER DATABASE [$(DatabaseName)]
    SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [$(DatabaseName)];
END

GO
PRINT N'Creating $(DatabaseName)...'
GO
CREATE DATABASE [$(DatabaseName)]
    ON 
    PRIMARY(NAME = [$(Path1)$(DatabaseName).mdf], FILENAME = '$(Path1)$(DatabaseName).mdf', MAXSIZE = UNLIMITED, FILEGROWTH = 1024 KB)
    LOG ON (NAME = [$(DatabaseName)_log], FILENAME = '$(Path1)$(DatabaseName)_log.LDF', MAXSIZE = 2097152 MB, FILEGROWTH = 10 %) COLLATE SQL_Latin1_General_CP1_CI_AS
GO
EXECUTE sp_dbcmptlevel [$(DatabaseName)], 90;


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'$(DatabaseName)')
    BEGIN
        ALTER DATABASE [$(DatabaseName)]
            SET ANSI_NULLS OFF,
                ANSI_PADDING OFF,
                ANSI_WARNINGS OFF,
                ARITHABORT OFF,
                CONCAT_NULL_YIELDS_NULL OFF,
                NUMERIC_ROUNDABORT OFF,
                QUOTED_IDENTIFIER OFF,
                ANSI_NULL_DEFAULT OFF,
                CURSOR_DEFAULT GLOBAL,
                RECOVERY SIMPLE,
                CURSOR_CLOSE_ON_COMMIT OFF,
                AUTO_CREATE_STATISTICS ON,
                AUTO_SHRINK OFF,
                AUTO_UPDATE_STATISTICS ON,
                RECURSIVE_TRIGGERS OFF 
            WITH ROLLBACK IMMEDIATE;
        ALTER DATABASE [$(DatabaseName)]
            SET AUTO_CLOSE ON 
            WITH ROLLBACK IMMEDIATE;
    END


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'$(DatabaseName)')
    BEGIN
        ALTER DATABASE [$(DatabaseName)]
            SET ALLOW_SNAPSHOT_ISOLATION OFF;
    END


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'$(DatabaseName)')
    BEGIN
        ALTER DATABASE [$(DatabaseName)]
            SET READ_COMMITTED_SNAPSHOT OFF;
    END


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'$(DatabaseName)')
    BEGIN
        ALTER DATABASE [$(DatabaseName)]
            SET AUTO_UPDATE_STATISTICS_ASYNC OFF,
                PAGE_VERIFY CHECKSUM,
                DATE_CORRELATION_OPTIMIZATION OFF,
                DISABLE_BROKER,
                PARAMETERIZATION SIMPLE 
            WITH ROLLBACK IMMEDIATE;
    END


GO
IF IS_SRVROLEMEMBER(N'sysadmin') = 1
    BEGIN
        IF EXISTS (SELECT 1
                   FROM   [master].[dbo].[sysdatabases]
                   WHERE  [name] = N'$(DatabaseName)')
            BEGIN
                EXECUTE sp_executesql N'ALTER DATABASE [$(DatabaseName)]
    SET TRUSTWORTHY OFF,
        DB_CHAINING OFF 
    WITH ROLLBACK IMMEDIATE';
            END
    END
ELSE
    BEGIN
        PRINT N'The database settings for DB_CHAINING or TRUSTWORTHY cannot be modified. You must be a SysAdmin to apply these settings.';
    END


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'$(DatabaseName)')
    BEGIN
        ALTER DATABASE [$(DatabaseName)]
            SET HONOR_BROKER_PRIORITY OFF 
            WITH ROLLBACK IMMEDIATE;
    END


GO
USE [$(DatabaseName)]

GO
IF fulltextserviceproperty(N'IsFulltextInstalled') = 1
    EXECUTE sp_fulltext_database 'enable';


GO

GO
/*
 Pre-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be executed before the build script.	
 Use SQLCMD syntax to include a file in the pre-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the pre-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

GO

GO
PRINT N'Creating dbo.AuthenticationToken...';


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
CREATE TABLE [dbo].[AuthenticationToken] (
    [AuthenticationTokenId]    INT            IDENTITY (1, 1) NOT NULL,
    [UserId]                   INT            NOT NULL,
    [OpenIdClaimedIdentifier]  NVARCHAR (250) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    [OpenIdFriendlyIdentifier] NVARCHAR (250) NULL,
    [CreatedOn]                DATETIME       NOT NULL,
    [LastUsed]                 DATETIME       NOT NULL,
    [UsageCount]               INT            NOT NULL
);


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER OFF;


GO
PRINT N'Creating dbo.Consumer...';


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
CREATE TABLE [dbo].[Consumer] (
    [ConsumerId]             INT             IDENTITY (1, 1) NOT NULL,
    [ConsumerKey]            NVARCHAR (255)  COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    [ConsumerSecret]         NVARCHAR (255)  COLLATE SQL_Latin1_General_CP1_CS_AS NULL,
    [X509Certificate]        IMAGE           NULL,
    [Callback]               NVARCHAR (2048) NULL,
    [VerificationCodeFormat] INT             NOT NULL,
    [VerificationCodeLength] INT             NOT NULL,
    [Name]                   NVARCHAR (50)   NULL
);


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER OFF;


GO
PRINT N'Creating dbo.IssuedToken...';


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
CREATE TABLE [dbo].[IssuedToken] (
    [IssuedTokenId]    INT             IDENTITY (1, 1) NOT NULL,
    [ConsumerId]       INT             NOT NULL,
    [UserId]           INT             NULL,
    [Token]            NVARCHAR (255)  COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    [TokenSecret]      NVARCHAR (255)  COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    [CreatedOn]        DATETIME        NOT NULL,
    [Callback]         NVARCHAR (2048) NULL,
    [VerificationCode] NVARCHAR (255)  COLLATE SQL_Latin1_General_CP1_CS_AS NULL,
    [ConsumerVersion]  VARCHAR (10)    NULL,
    [ExpirationDate]   DATETIME        NULL,
    [IsAccessToken]    BIT             NOT NULL,
    [Scope]            NVARCHAR (255)  NULL
);


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER OFF;


GO
PRINT N'Creating dbo.Nonce...';


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
CREATE TABLE [dbo].[Nonce] (
    [NonceId] INT           IDENTITY (1, 1) NOT NULL,
    [Context] VARCHAR (255) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    [Code]    VARCHAR (255) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    [Issued]  DATETIME      NOT NULL,
    [Expires] DATETIME      NOT NULL
);


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER OFF;


GO
PRINT N'Creating dbo.OpenIDAssociation...';


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
CREATE TABLE [dbo].[OpenIDAssociation] (
    [AssociationId]        INT           IDENTITY (1, 1) NOT NULL,
    [DistinguishingFactor] VARCHAR (255) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    [AssociationHandle]    VARCHAR (255) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    [Expiration]           DATETIME      NOT NULL,
    [PrivateData]          BINARY (64)   NOT NULL,
    [PrivateDataLength]    INT           NOT NULL
);


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER OFF;


GO
PRINT N'Creating dbo.Role...';


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
CREATE TABLE [dbo].[Role] (
    [RoleId] INT           IDENTITY (1, 1) NOT NULL,
    [Name]   NVARCHAR (50) NOT NULL
);


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER OFF;


GO
PRINT N'Creating dbo.User...';


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
CREATE TABLE [dbo].[User] (
    [UserId]               INT            IDENTITY (1, 1) NOT NULL,
    [FirstName]            NVARCHAR (50)  NULL,
    [LastName]             NVARCHAR (50)  NULL,
    [EmailAddress]         NVARCHAR (100) NULL,
    [EmailAddressVerified] BIT            NOT NULL,
    [CreatedOn]            DATETIME       NOT NULL
);


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER OFF;


GO
PRINT N'Creating dbo.UserRole...';


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
CREATE TABLE [dbo].[UserRole] (
    [UserId] INT NOT NULL,
    [RoleId] INT NOT NULL
);


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER OFF;


GO
PRINT N'Creating dbo.Consumer.IX_Consumer...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Consumer]
    ON [dbo].[Consumer]([ConsumerKey] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating dbo.IssuedToken.IX_IssuedToken...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_IssuedToken]
    ON [dbo].[IssuedToken]([Token] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating dbo.Nonce.IX_Nonce_Code...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Nonce_Code]
    ON [dbo].[Nonce]([Context] ASC, [Code] ASC, [Issued] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating dbo.Nonce.IX_Nonce_Expires...';


GO
CREATE NONCLUSTERED INDEX [IX_Nonce_Expires]
    ON [dbo].[Nonce]([Expires] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating dbo.OpenIDAssociation.IX_OpenIDAssociations...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_OpenIDAssociations]
    ON [dbo].[OpenIDAssociation]([DistinguishingFactor] ASC, [AssociationHandle] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0);


GO
PRINT N'Creating dbo.DF_AuthenticationToken_CreatedOn...';


GO
ALTER TABLE [dbo].[AuthenticationToken]
    ADD CONSTRAINT [DF_AuthenticationToken_CreatedOn] DEFAULT (getutcdate()) FOR [CreatedOn];


GO
PRINT N'Creating dbo.DF_AuthenticationToken_LastUsed...';


GO
ALTER TABLE [dbo].[AuthenticationToken]
    ADD CONSTRAINT [DF_AuthenticationToken_LastUsed] DEFAULT (getutcdate()) FOR [LastUsed];


GO
PRINT N'Creating dbo.DF_AuthenticationToken_UsageCount...';


GO
ALTER TABLE [dbo].[AuthenticationToken]
    ADD CONSTRAINT [DF_AuthenticationToken_UsageCount] DEFAULT ((0)) FOR [UsageCount];


GO
PRINT N'Creating dbo.DF_IssuedToken_CreatedOn...';


GO
ALTER TABLE [dbo].[IssuedToken]
    ADD CONSTRAINT [DF_IssuedToken_CreatedOn] DEFAULT (getutcdate()) FOR [CreatedOn];


GO
PRINT N'Creating dbo.DF_IssuedToken_IsAccessToken...';


GO
ALTER TABLE [dbo].[IssuedToken]
    ADD CONSTRAINT [DF_IssuedToken_IsAccessToken] DEFAULT ((0)) FOR [IsAccessToken];


GO
PRINT N'Creating dbo.DF_Nonce_Issued...';


GO
ALTER TABLE [dbo].[Nonce]
    ADD CONSTRAINT [DF_Nonce_Issued] DEFAULT (getutcdate()) FOR [Issued];


GO
PRINT N'Creating dbo.DF_User_CreatedOn...';


GO
ALTER TABLE [dbo].[User]
    ADD CONSTRAINT [DF_User_CreatedOn] DEFAULT (getutcdate()) FOR [CreatedOn];


GO
PRINT N'Creating dbo.DF_User_EmailAddressVerified...';


GO
ALTER TABLE [dbo].[User]
    ADD CONSTRAINT [DF_User_EmailAddressVerified] DEFAULT ((0)) FOR [EmailAddressVerified];


GO
PRINT N'Creating dbo.PK_AuthenticationToken...';


GO
ALTER TABLE [dbo].[AuthenticationToken]
    ADD CONSTRAINT [PK_AuthenticationToken] PRIMARY KEY CLUSTERED ([AuthenticationTokenId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);


GO
PRINT N'Creating dbo.PK_Consumer...';


GO
ALTER TABLE [dbo].[Consumer]
    ADD CONSTRAINT [PK_Consumer] PRIMARY KEY CLUSTERED ([ConsumerId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);


GO
PRINT N'Creating dbo.PK_IssuedToken...';


GO
ALTER TABLE [dbo].[IssuedToken]
    ADD CONSTRAINT [PK_IssuedToken] PRIMARY KEY CLUSTERED ([IssuedTokenId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);


GO
PRINT N'Creating dbo.PK_Nonce...';


GO
ALTER TABLE [dbo].[Nonce]
    ADD CONSTRAINT [PK_Nonce] PRIMARY KEY CLUSTERED ([NonceId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);


GO
PRINT N'Creating dbo.PK_OpenIDAssociations...';


GO
ALTER TABLE [dbo].[OpenIDAssociation]
    ADD CONSTRAINT [PK_OpenIDAssociations] PRIMARY KEY CLUSTERED ([AssociationId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);


GO
PRINT N'Creating dbo.PK_Role...';


GO
ALTER TABLE [dbo].[Role]
    ADD CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED ([RoleId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);


GO
PRINT N'Creating dbo.PK_User...';


GO
ALTER TABLE [dbo].[User]
    ADD CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([UserId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);


GO
PRINT N'Creating dbo.PK_UserRole...';


GO
ALTER TABLE [dbo].[UserRole]
    ADD CONSTRAINT [PK_UserRole] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);


GO
PRINT N'Creating dbo.FK_AuthenticationToken_User...';


GO
ALTER TABLE [dbo].[AuthenticationToken]
    ADD CONSTRAINT [FK_AuthenticationToken_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([UserId]) ON DELETE CASCADE ON UPDATE CASCADE;


GO
PRINT N'Creating dbo.FK_IssuedToken_Consumer...';


GO
ALTER TABLE [dbo].[IssuedToken]
    ADD CONSTRAINT [FK_IssuedToken_Consumer] FOREIGN KEY ([ConsumerId]) REFERENCES [dbo].[Consumer] ([ConsumerId]) ON DELETE CASCADE ON UPDATE CASCADE;


GO
PRINT N'Creating dbo.FK_IssuedToken_User...';


GO
ALTER TABLE [dbo].[IssuedToken]
    ADD CONSTRAINT [FK_IssuedToken_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([UserId]) ON DELETE CASCADE ON UPDATE CASCADE;


GO
PRINT N'Creating dbo.FK_UserRole_Role...';


GO
ALTER TABLE [dbo].[UserRole]
    ADD CONSTRAINT [FK_UserRole_Role] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Role] ([RoleId]) ON DELETE CASCADE ON UPDATE CASCADE;


GO
PRINT N'Creating dbo.FK_UserRole_User...';


GO
ALTER TABLE [dbo].[UserRole]
    ADD CONSTRAINT [FK_UserRole_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([UserId]) ON DELETE CASCADE ON UPDATE CASCADE;


GO
PRINT N'Creating dbo.AddUser...';


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
CREATE PROCEDURE [dbo].[AddUser]
@firstName NVARCHAR (50), @lastName NVARCHAR (50), @openid NVARCHAR (255), @role NVARCHAR (255)
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


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER OFF;


GO
PRINT N'Creating dbo.ClearExpiredAssociations...';


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
CREATE PROCEDURE [dbo].[ClearExpiredAssociations]

AS
DELETE FROM dbo.OpenIDAssociation
WHERE [Expiration] < getutcdate()


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER OFF;


GO
PRINT N'Creating dbo.ClearExpiredNonces...';


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
CREATE PROCEDURE [dbo].[ClearExpiredNonces]

AS
DELETE FROM dbo.[Nonce]
WHERE [Expires] < getutcdate()


GO
SET ANSI_NULLS, QUOTED_IDENTIFIER OFF;


GO
PRINT N'Creating AutoCreatedLocal...';


GO
CREATE ROUTE [AutoCreatedLocal]
    AUTHORIZATION [dbo]
    WITH ADDRESS = N'LOCAL';


GO

GO
/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

GO

GO

GO
ALTER DATABASE [$(DatabaseName)]
    SET MULTI_USER 
    WITH ROLLBACK IMMEDIATE;


GO
