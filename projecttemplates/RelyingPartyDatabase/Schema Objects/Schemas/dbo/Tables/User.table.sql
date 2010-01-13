CREATE TABLE [dbo].[User] (
    [UserId]               INT            IDENTITY (1, 1) NOT NULL,
    [FirstName]            NVARCHAR (50)  NULL,
    [LastName]             NVARCHAR (50)  NULL,
    [EmailAddress]         NVARCHAR (100) NULL,
    [EmailAddressVerified] BIT            NOT NULL,
    [CreatedOn]            DATETIME       NOT NULL
);

