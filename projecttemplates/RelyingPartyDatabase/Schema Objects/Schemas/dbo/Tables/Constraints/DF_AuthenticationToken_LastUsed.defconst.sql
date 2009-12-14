ALTER TABLE [dbo].[AuthenticationToken]
    ADD CONSTRAINT [DF_AuthenticationToken_LastUsed] DEFAULT (getutcdate()) FOR [LastUsed];

