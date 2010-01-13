CREATE PROCEDURE dbo.ClearExpiredAssociations
AS

DELETE FROM dbo.OpenIDAssociation
WHERE [Expiration] < getutcdate()
