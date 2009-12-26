ALTER TABLE [dbo].[AuthenticationToken]
    ADD CONSTRAINT [FK_AuthenticationToken_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([UserId]) ON DELETE CASCADE ON UPDATE CASCADE;

