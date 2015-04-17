@echo off

set TARGET=%~dp0
set MODULEFOLDER=%ProgramFiles%\WindowsPowerShell\Modules

:remove old junctions
rmdir "%MODULEFOLDER%\PackageManagement" > nul
rmdir "%MODULEFOLDER%\OneGet" > nul
rmdir "%MODULEFOLDER%\OneGet-Edge" > nul
rmdir "%MODULEFOLDER%\PowerShellGet" > nul


: create junctions for modules (all point here)
mklink /j "%MODULEFOLDER%\PackageManagement" "%TARGET%"
mklink /j "%MODULEFOLDER%\OneGet" "%TARGET%"
mklink /j "%MODULEFOLDER%\OneGet-Edge" "%TARGET%"

: and do powershellGet too...
mklink /j "%MODULEFOLDER%\PowerShellGet" "%TARGET%\PowerShellGet" > nul

: check if OneGet-Edge is in the psmodulepath
pushd c:\
powershell ipmo oneget-edge -ea silentlycontinue & if errorlevel 1 (
  for /F "tokens=* delims=⌂ eol=~" %%f IN ('powershell -noprofile echo $env:psmodulepath') do setx.exe PSMODULEPATH "%%f;%PROGRAMW6432%\WindowsPowerShell\Modules" -m
)

