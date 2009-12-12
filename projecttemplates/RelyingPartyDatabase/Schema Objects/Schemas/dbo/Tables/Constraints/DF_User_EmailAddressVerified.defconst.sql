ALTER TABLE [dbo].[User]
    ADD CONSTRAINT [DF_User_EmailAddressVerified] DEFAULT ((0)) FOR [EmailAddressVerified];

