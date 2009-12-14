ALTER TABLE [dbo].[IssuedToken]
    ADD CONSTRAINT [DF_IssuedToken_CreatedOn] DEFAULT (getutcdate()) FOR [CreatedOn];

