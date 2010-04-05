ALTER TABLE [dbo].[AuthenticationToken]
    ADD CONSTRAINT [DF_AuthenticationToken_UsageCount] DEFAULT ((0)) FOR [UsageCount];

