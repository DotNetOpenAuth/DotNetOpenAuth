ALTER DATABASE [$(DatabaseName)]
    ADD FILE (NAME = [C:\Users\andarno\git\dotnetopenid\projecttemplates\WebFormsRelyingParty\App_Data\Database.mdf], FILENAME = '$(Path2)Database.mdf', MAXSIZE = UNLIMITED, FILEGROWTH = 1024 KB) TO FILEGROUP [PRIMARY];

