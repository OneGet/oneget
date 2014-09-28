@echo off

:checkPrivileges 
NET FILE 1>NUL 2>NUL
if '%errorlevel%' == '0' ( goto gotPrivileges 
) else ( powershell "saps -filepath %0 -verb runas" >nul 2>&1)
exit /b 

:gotPrivileges
cd /d %~dp0
.\streams.exe -d *
powershell set-executionpolicy unrestricted 
 
 