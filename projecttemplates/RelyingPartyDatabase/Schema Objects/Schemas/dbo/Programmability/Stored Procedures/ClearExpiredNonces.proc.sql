CREATE PROCEDURE dbo.ClearExpiredNonces
AS

DELETE FROM dbo.[Nonce]
WHERE [Expires] < getutcdate()
