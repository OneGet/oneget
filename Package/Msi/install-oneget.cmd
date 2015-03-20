@echo off

set TARGET=%~dp0
set MODULEFOLDER=%ProgramFiles%\WindowsPowerShell\Modules

# remove old junctions
rmdir "%MODULEFOLDER%\PackageManagement"
rmdir "%MODULEFOLDER%\OneGet"
rmdir "%MODULEFOLDER%\OneGet-Edge"

# create junctions for modules (all point here)
mklink /j "%MODULEFOLDER%\PackageManagement" "%TARGET%"
mklink /j "%MODULEFOLDER%\OneGet" "%TARGET%"
mklink /j "%MODULEFOLDER%\OneGet-Edge" "%TARGET%"
