ALTER DATABASE [$(DatabaseName)]
    ADD FILE (NAME = [$(Path1)Database.mdf], FILENAME = '$(Path1)Database.mdf', MAXSIZE = UNLIMITED, FILEGROWTH = 1024 KB) TO FILEGROUP [PRIMARY];

