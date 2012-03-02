CREATE TABLE [dbo].[Client] (
    [ClientId]         INT            IDENTITY (1, 1) NOT NULL,
    [ClientIdentifier] VARCHAR (255)  COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    [ClientSecret]     VARCHAR (255)  COLLATE SQL_Latin1_General_CP1_CS_AS NULL,
    [Callback]         VARCHAR (2048) NULL,
    [ClientType]       INT,
    [Name]             NVARCHAR (50)  NOT NULL
);





