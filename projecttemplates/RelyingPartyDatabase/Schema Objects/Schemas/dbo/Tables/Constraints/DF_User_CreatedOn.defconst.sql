ALTER TABLE [dbo].[User]
    ADD CONSTRAINT [DF_User_CreatedOn] DEFAULT (getutcdate()) FOR [CreatedOn];

