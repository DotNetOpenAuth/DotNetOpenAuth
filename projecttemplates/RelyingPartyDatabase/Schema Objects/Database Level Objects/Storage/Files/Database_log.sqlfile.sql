ALTER DATABASE [$(DatabaseName)]
    ADD LOG FILE (NAME = [$(DatabaseName)_log], FILENAME = '$(Path1)$(DatabaseName)_log.LDF', MAXSIZE = 2097152 MB, FILEGROWTH = 10 %);

