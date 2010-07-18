CREATE TABLE [dbo].[ClientAuthorization] (
    [AuthorizationId] INT            IDENTITY (1, 1) NOT NULL,
    [ClientId]        INT            NOT NULL,
    [UserId]          INT            NOT NULL,
    [CreatedOn]       DATETIME       NOT NULL,
    [ExpirationDate]  DATETIME       NULL,
    [Scope]           VARCHAR (2048) NULL
);

