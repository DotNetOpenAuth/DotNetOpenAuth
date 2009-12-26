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

