ALTER TABLE [dbo].[ClientAuthorization]
    ADD CONSTRAINT [FK_IssuedToken_Consumer] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[Client] ([ClientId]) ON DELETE CASCADE ON UPDATE CASCADE;

