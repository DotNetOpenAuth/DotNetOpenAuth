CREATE TABLE [dbo].[AuthenticationToken] (
    [AuthenticationTokenId]    INT            IDENTITY (1, 1) NOT NULL,
    [UserId]                   INT            NOT NULL,
    [OpenIdClaimedIdentifier]  NVARCHAR (250) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    [OpenIdFriendlyIdentifier] NVARCHAR (250) NULL,
    [CreatedOn]                DATETIME       NOT NULL,
    [LastUsed]                 DATETIME       NOT NULL,
    [UsageCount]               INT            NOT NULL
);

