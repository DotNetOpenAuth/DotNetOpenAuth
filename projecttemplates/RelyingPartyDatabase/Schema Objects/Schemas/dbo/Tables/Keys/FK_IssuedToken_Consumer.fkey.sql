ALTER TABLE [dbo].[IssuedToken]
    ADD CONSTRAINT [FK_IssuedToken_Consumer] FOREIGN KEY ([ConsumerId]) REFERENCES [dbo].[Consumer] ([ConsumerId]) ON DELETE CASCADE ON UPDATE CASCADE;

