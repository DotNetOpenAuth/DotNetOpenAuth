CREATE TABLE [dbo].[CryptoKey] (
    [CryptoKeyId] INT              IDENTITY (1, 1) NOT NULL,
    [Bucket]      VARCHAR (255)    COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    [Handle]      VARCHAR (255)    COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    [Expiration]  DATETIME         NOT NULL,
    [Secret]      VARBINARY (4096) NOT NULL
);



