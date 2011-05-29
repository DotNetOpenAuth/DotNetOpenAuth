CREATE PROCEDURE dbo.ClearExpiredCryptoKeys
AS

DELETE FROM dbo.CryptoKey
WHERE [Expiration] < getutcdate()
