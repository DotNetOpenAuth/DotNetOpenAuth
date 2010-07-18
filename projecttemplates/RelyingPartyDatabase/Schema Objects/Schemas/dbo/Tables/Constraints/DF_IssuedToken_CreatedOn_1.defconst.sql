ALTER TABLE [dbo].[ClientAuthorization]
    ADD CONSTRAINT [DF_IssuedToken_CreatedOn] DEFAULT (getutcdate()) FOR [CreatedOn];

