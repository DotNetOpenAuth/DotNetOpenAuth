ALTER DATABASE [$(DatabaseName)]
    ADD FILE (NAME = [$(Path1)$(DatabaseName).mdf], FILENAME = '$(Path1)$(DatabaseName).mdf', MAXSIZE = UNLIMITED, FILEGROWTH = 1024 KB) TO FILEGROUP [PRIMARY];

