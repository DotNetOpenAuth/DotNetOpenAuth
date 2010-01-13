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

