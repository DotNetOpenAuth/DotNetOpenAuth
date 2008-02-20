@echo off
REM %~dp0 returns the directory that the script resides in
pushd %~dp0..
IF EXIST bin rd /s /q bin
IF EXIST drop rd /s /q drop

IF EXIST samples\consumerportal\obj rd /s /q samples\consumerportal\obj
IF EXIST samples\consumerportal\bin rd /s /q samples\consumerportal\bin
IF EXIST samples\providerportal\obj rd /s /q samples\providerportal\obj
IF EXIST samples\providerportal\bin rd /s /q samples\providerportal\bin
IF EXIST src\dotnetopenid\obj       rd /s /q src\dotnetopenid\obj
IF EXIST src\dotnetopenid.test\obj  rd /s /q src\dotnetopenid.test\obj

popd