CREATE TABLE [dbo].[Nonce] (
    [NonceId] INT           IDENTITY (1, 1) NOT NULL,
    [Context] VARCHAR (255) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    [Code]    VARCHAR (255) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    [Issued]  DATETIME      NOT NULL,
    [Expires] DATETIME      NOT NULL
);

