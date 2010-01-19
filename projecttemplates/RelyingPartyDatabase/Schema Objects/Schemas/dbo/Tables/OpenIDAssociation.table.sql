CREATE TABLE [dbo].[OpenIDAssociation] (
    [AssociationId]        INT           IDENTITY (1, 1) NOT NULL,
    [DistinguishingFactor] VARCHAR (255) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    [AssociationHandle]    VARCHAR (255) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    [Expiration]           DATETIME      NOT NULL,
    [PrivateData]          BINARY (64)   NOT NULL,
    [PrivateDataLength]    INT           NOT NULL
);

