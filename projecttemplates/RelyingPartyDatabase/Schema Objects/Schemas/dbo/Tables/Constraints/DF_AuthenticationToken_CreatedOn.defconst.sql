ALTER TABLE [dbo].[AuthenticationToken]
    ADD CONSTRAINT [DF_AuthenticationToken_CreatedOn] DEFAULT (getutcdate()) FOR [CreatedOn];

