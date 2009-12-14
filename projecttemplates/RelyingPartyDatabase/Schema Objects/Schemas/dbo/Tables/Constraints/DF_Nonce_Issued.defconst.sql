ALTER TABLE [dbo].[Nonce]
    ADD CONSTRAINT [DF_Nonce_Issued] DEFAULT (getutcdate()) FOR [Issued];

