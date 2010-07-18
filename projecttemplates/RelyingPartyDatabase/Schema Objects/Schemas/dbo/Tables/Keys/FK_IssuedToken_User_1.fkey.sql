ALTER TABLE [dbo].[ClientAuthorization]
    ADD CONSTRAINT [FK_IssuedToken_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([UserId]) ON DELETE CASCADE ON UPDATE CASCADE;

