@echo off
REM %~dp0 returns the directory that the script resides in
pushd %~dp0..
IF EXIST bin rd /s /q bin
IF EXIST drop rd /s /q drop
popd