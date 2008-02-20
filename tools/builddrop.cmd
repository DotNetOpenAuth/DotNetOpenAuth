@echo off
SET CONFIGURATION=release
REM %~dp0 returns the directory that the script resides in
SET root=%~dp0..
SET RELEASE_DIR=%root%\drop
IF EXIST %RELEASE_DIR% GOTO ALREADYEXISTS
echo Building...
msbuild %root%\src\DotNetOpenId.sln /p:Configuration=%CONFIGURATION%
IF ERRORLEVEL 1 GOTO BUILDFAILURE

echo Copying files...
md %RELEASE_DIR%
md %RELEASE_DIR%\bin
md %RELEASE_DIR%\samples
copy %root%\bin\%CONFIGURATION%\DotNetOpenId.??? %RELEASE_DIR%\bin > nul
IF ERRORLEVEL 1 GOTO COPYFAILURE
xcopy /s /e %root%\samples %RELEASE_DIR%\samples > nul
IF ERRORLEVEL 1 GOTO COPYFAILURE

REM Do a little cleanup of files that can get caught in these directories
rd /s /q %RELEASE_DIR%\samples\consumerportal\obj %RELEASE_DIR%\samples\providerportal\obj
del %RELEASE_DIR%\samples\consumerportal\*.user %RELEASE_DIR%\samples\providerportal\*.user %RELEASE_DIR%\samples\*.sln.cache

echo Successful.  The release bits can be found in the %RELEASE_DIR% directory.

goto end

:ALREADYEXISTS
echo ERROR: The %RELEASE_DIR% directory already exists.  You should remove it first.
exit /b 1

:BUILDFAILURE
echo Release aborted due to build failure.
exit /b 2

:COPYFAILURE
echo A failure occurred while copying files to the %RELEASE_DIR% directory.
exit /b 3

:END
