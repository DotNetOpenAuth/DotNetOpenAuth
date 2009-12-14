ALTER TABLE [dbo].[IssuedToken]
    ADD CONSTRAINT [DF_IssuedToken_IsAccessToken] DEFAULT ((0)) FOR [IsAccessToken];

