ALTER DATABASE [$(DatabaseName)]
    ADD LOG FILE (NAME = [Database_log], FILENAME = '$(Path1)Database_log.LDF', MAXSIZE = 2097152 MB, FILEGROWTH = 10 %);

