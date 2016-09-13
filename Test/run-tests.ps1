#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#  Licensed under the Apache License, Version 2.0 (the "License");
#  you may not use this file except in compliance with the License.
#  You may obtain a copy of the License at
#  http://www.apache.org/licenses/LICENSE-2.0
#
#  Unless required by applicable law or agreed to in writing, software
#  distributed under the License is distributed on an "AS IS" BASIS,
#  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
#  See the License for the specific language governing permissions and
#  limitations under the License.
#

#Instructions to run the tests
#1. git clone the oneget
#2. cd to c:\localOneGetFolder\oneget\Test
#3. run-tests.ps1


#Step 1 - test setup
$TestHome = $PSScriptRoot
$TestBin = "$($TestHome)\..\src\out\PackageManagement\"
$PowerShellGetPath = "$($TestHome)\..\src\Modules\PowerShellGet\"
$PowerShellGetVersion = "1.0.0.1"
$PackageManagementVersion = "1.0.0.1"


#Import-Module "$($TestBin)\PackageManagement.psd1"

$ProgramProviderInstalledPath = "$Env:ProgramFiles\PackageManagement\ProviderAssemblies"

$LocalAppData = [Environment]::GetFolderPath("LocalApplicationData")
$UserProviderInstalledPath = "$($LocalAppData)\PackageManagement\ProviderAssemblies"

$mydocument = [Environment]::GetFolderPath("MyDocuments")
$UserModulePath = "$($mydocument)\WindowsPowerShell\Modules"

$ProgramModulePath = "$Env:ProgramFiles\WindowsPowerShell\Modules"


$testframework = [System.Environment]::GetEnvironmentVariable("test_framework")

if ($testframework -eq "coreclr")
{
    # install powershell core if test framework is coreclr
    Install-PackageProvider PSL -Force; $powershellCore = Install-Package PowerShell -Force -Provider PSL

    $powershellVersion = $powershellCore.Version
    # copy the bits into powershell core
    $powershellFolder = "$Env:ProgramFiles\PowerShell\$powershellVersion\"
    Copy-Item "$TestBin\netstandard1.6\*.dll" $powershellFolder -Force -Verbose
    Copy-Item "$TestBin\*.psd1" "$powershellFolder\PackageManagement" -force -Verbose
    Copy-Item "$TestBin\*.psm1" "$powershellFolder\PackageManagement" -force -Verbose
    Copy-Item "$TestBin\*.ps1xml" "$powershellFolder\PackageManagement" -force -Verbose
}

$packagemanagementfolder = "$ProgramModulePath\PackageManagement\$PackageManagementVersion"
$powershellGetfolder = "$ProgramModulePath\PowerShellGet\$PowerShellGetVersion"
#$packagemanagementfolder = "$ProgramModulePath\PackageManagement"
#$powershellGetfolder = "$ProgramModulePath\PowerShellGet"

if ($testframework -eq "fullclr")
{    
    if(-not (Test-Path $packagemanagementfolder)){
        New-Item -Path $packagemanagementfolder -ItemType Directory -Force  
        Write-Host "Created  $packagemanagementfolder"
    } else{
        Get-ChildItem -Path $packagemanagementfolder | %{ren "$packagemanagementfolder\$_" "$packagemanagementfolder\$_.deleteMe"}
        Get-ChildItem -Path $packagemanagementfolder  -Recurse |  Remove-Item -force -Recurse
    }
}

if(-not (Test-Path $powershellGetfolder)){
    New-Item -Path $powershellGetfolder -ItemType Directory -Force  
    Write-Host "Created  $powershellGetfolder"
} else{
    Get-ChildItem -Path $powershellGetfolder | %{ren "$powershellGetfolder\$_" "$powershellGetfolder\$_.deleteMe"}
    Get-ChildItem -Path $powershellGetfolder  -Recurse |  Remove-Item -force -Recurse
}

if ($testframework -eq "fullclr")
{
    Copy-Item "$PowerShellGetPath\*" $powershellGetfolder -force -verbose
    Copy-Item "$TestBin\net451\*.dll" $packagemanagementfolder -force -Verbose
    Copy-Item "$TestBin\*.psd1" $packagemanagementfolder -force -Verbose
    Copy-Item "$TestBin\*.psm1" $packagemanagementfolder -force -Verbose
    Copy-Item "$TestBin\*.ps1" $packagemanagementfolder -force -Verbose
    Copy-Item "$TestBin\*.ps1xml" $packagemanagementfolder -force -Verbose
}

if(-not (Test-Path $ProgramProviderInstalledPath)){
    New-Item -Path $ProgramProviderInstalledPath -ItemType Directory -Force  
    Write-Host "Created  $ProgramProviderInstalledPath"
} else{
    Get-ChildItem -Path $ProgramProviderInstalledPath -Recurse | Remove-Item -force -Recurse -ea silentlycontinue
}

if(-not (Test-Path $UserProviderInstalledPath)) {
    New-Item -Path $UserProviderInstalledPath -ItemType Directory -Force  
    Write-Host "Created  $ProgramProviderInstalledPath"

} else{
    Get-ChildItem -Path $UserProviderInstalledPath -Recurse | Remove-Item -force -Recurse -ea silentlycontinue
}

if(-not (Test-Path $UserModulePath)) {
    New-Item -Path $UserModulePath -ItemType Directory -Force  
    Write-Host "Created  $UserModulePath"

} else{
    Get-ChildItem -Path $UserModulePath -Recurse | Remove-Item -force -Recurse -ea silentlycontinue
}


# remove the strongname from the binaries

reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PackageManagement.ArchiverProviders,31bf3856ad364e35"  /f
reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PackageManagement.CoreProviders,31bf3856ad364e35"  /f
reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PackageManagement,31bf3856ad364e35"  /f
reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PackageManagement.MetaProvider.PowerShell,31bf3856ad364e35"  /f
reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PackageManagement.MsiProvider,31bf3856ad364e35"  /f
reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PackageManagement.MsuProvider,31bf3856ad364e35"  /f
reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PackageManagement.NuGetProvider,*"  /f
reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PackageManagement.Test,31bf3856ad364e35"  /f
reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PowerShell.PackageManagement,31bf3856ad364e35"  /f
reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PackageManagement.OneGetTestProvider,31bf3856ad364e35"  /f


reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PackageManagement.ArchiverProviders,31bf3856ad364e35"  /f
reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PackageManagement.CoreProviders,31bf3856ad364e35"  /f
reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PackageManagement,31bf3856ad364e35"  /f
reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PackageManagement.MetaProvider.PowerShell,31bf3856ad364e35"  /f
reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PackageManagement.MsiProvider,31bf3856ad364e35"  /f
reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PackageManagement.MsuProvider,31bf3856ad364e35"  /f
reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PackageManagement.NuGetProvider,*"  /f
reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PackageManagement.Test,31bf3856ad364e35"  /f
reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PowerShell.PackageManagement,31bf3856ad364e35"  /f
reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PackageManagement.OneGetTestProvider,31bf3856ad364e35"  /f

Restart-Service msiserver

if (Test-Path "$env:temp\PackageManagementDependencies") {
    Remove-Item -Recurse -Force "$env:temp\PackageManagementDependencies"
}

Copy-Item "$($TestHome)\Unit\Providers\Dependencies" "$env:tmp\PackageManagementDependencies" -Recurse -Force


# remove existing nuget provider.
if (test-path $Env:ProgramFiles/PackageManagement/ProviderAssemblies/nuget-anycpu.exe) {
    ren $Env:ProgramFiles/PackageManagement/ProviderAssemblies/nuget-anycpu.exe "nuget-anycpu.$(Get-Random).deleteme"
    rm -force $Env:ProgramFiles/PackageManagement/ProviderAssemblies/*.deleteme -ea silentlycontinue
}

if (test-path $Env:LocalAppData/PackageManagement/ProviderAssemblies/nuget-anycpu.exe) {
    ren $Env:LocalAppData/PackageManagement/ProviderAssemblies/nuget-anycpu.exe "nuget-anycpu.$(Get-Random).deleteme"
    rm -force $Env:LocalAppData/PackageManagement/ProviderAssemblies/*.deleteme  -ea silentlycontinue
}

if (test-path $Env:ProgramFiles/PackageManagement/ProviderAssemblies/nuget-anycpu.exe) {
    ren $Env:ProgramFiles/PackageManagement/ProviderAssemblies/nuget-anycpu.exe "nuget-anycpu.$(Get-Random).deleteme"
    rm -force $Env:ProgramFiles/PackageManagement/ProviderAssemblies/*.deleteme -ea silentlycontinue
}

if (test-path $Env:LocalAppData/PackageManagement/ProviderAssemblies/nuget-anycpu.exe) {
    ren $Env:LocalAppData/PackageManagement/ProviderAssemblies/nuget-anycpu.exe "nuget-anycpu.$(Get-Random).deleteme"
    rm -force $Env:LocalAppData/PackageManagement/ProviderAssemblies/*.deleteme  -ea silentlycontinue
}

if (test-path $Env:AppData/NuGet/nuget.config) {
    copy -force $Env:AppData/NuGet/nuget.config $Env:AppData/NuGet/nuget.config.original -ea silentlycontinue
    rm -force $Env:AppData/NuGet/nuget.config  -ea silentlycontinue
}




#Copy-Item  "$($Testbin)\Microsoft.PackageManagement.OneGetTestProvider.dll" "$($ProgramProviderInstalledPath)\" -force 
#Copy-Item  "$($Testbin)\Microsoft.PackageManagement.OneGetTestProvider.dll" "$($UserProviderInstalledPath)\" -force

Copy-Item  "$($TestHome)\Unit\Providers\PSChained1Provider.psm1" "$($ProgramModulePath)\" -force
Copy-Item  "$($TestHome)\Unit\Providers\PSChained1Provider.psm1" "$($UserModulePath)\" -force
Copy-Item  "$($TestHome)\Unit\Providers\PSOneGetTestProvider" "$($ProgramModulePath)\"  -Recurse -force


#Step 2 - run tests
Write-Host -fore White "Running powershell pester tests "

if ($testframework -eq "fullclr")
{
    Invoke-Pester -Path "$($TestHome)\ModuleTests\tests"
}

Write-Host -fore White "Finished tests"

#Step3 - cleanup
if (test-path $Env:AppData/NuGet/nuget.config.original ) {
    copy -force $Env:AppData/NuGet/nuget.config.original $Env:AppData/NuGet/nuget.config -ea silentlycontinue
    rm -force $Env:AppData/NuGet/nuget.config.original  -ea silentlycontinue
}

cd $TestHome