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

param(
    [ValidateSet("coreclr", "fullclr")]
    [string]$testframework = "fullclr"
)


# Step 0 -- remove the strongname from the binaries
#region
try
{
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
}
catch{}
#endregion

#Step 1 - test setup
$TestHome = $PSScriptRoot
$TestBin = "$($TestHome)\..\src\out\PackageManagement\"
$PowerShellGetPath = "$($TestHome)\..\src\Modules\PowerShellGet\"
$PowerShellGetVersion = "1.1.0"
$PackageManagementVersion = "1.1.0"

$ProgramProviderInstalledPath = "$Env:ProgramFiles\PackageManagement\ProviderAssemblies"
$LocalAppData = $env:LocalAppdata
$UserProviderInstalledPath = "$($LocalAppData)\PackageManagement\ProviderAssemblies"
$ProgramModulePath = "$Env:ProgramFiles\WindowsPowerShell\Modules"


$testframeworkVariable = $null
# For appveyor runs
try
{
    $testframeworkVariable = [System.Environment]::GetEnvironmentVariable("test_framework")
}
catch {}

if ($testframeworkVariable)
{
    $testframework = $testframeworkVariable
}

Write-host "testframework =  $testframework"


if ($testframework -eq "fullclr")
{
    $mydocument = Microsoft.PowerShell.Management\Join-Path -Path $HOME -ChildPath 'Documents\PowerShell'
}
else
{
    $mydocument = Microsoft.PowerShell.Management\Join-Path -Path $HOME -ChildPath ".local/share/powershell"
}

$UserModulePath = "$($mydocument)\WindowsPowerShell\Modules"
$packagemanagementfolder = "$ProgramModulePath\PackageManagement\$PackageManagementVersion"
$powershellGetfolder = "$ProgramModulePath\PowerShellGet\$PowerShellGetVersion"

# Setting up Packagemanagement and PowerShellGet folders
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


# Copying files to Packagemanagement and PowerShellGet folders
if ($testframework -eq "fullclr")
{
    Copy-Item "$PowerShellGetPath\*" $powershellGetfolder -force -verbose
    Copy-Item "$TestBin\net451\*.dll" $packagemanagementfolder -force -Verbose
    Copy-Item "$TestBin\*.psd1" $packagemanagementfolder -force -Verbose
    Copy-Item "$TestBin\*.psm1" $packagemanagementfolder -force -Verbose
    Copy-Item "$TestBin\*.ps1" $packagemanagementfolder -force -Verbose
    Copy-Item "$TestBin\*.ps1xml" $packagemanagementfolder -force -Verbose
}

# Setting up provider path
if(-not (Test-Path $ProgramProviderInstalledPath)){
    New-Item -Path $ProgramProviderInstalledPath -ItemType Directory -Force  
    Write-Host "Created  $ProgramProviderInstalledPath"
} else{
    Get-ChildItem -Path $ProgramProviderInstalledPath -Recurse | Remove-Item -force -Recurse -ea silentlycontinue
}

if(-not (Test-Path $UserProviderInstalledPath)) {
    New-Item -Path $UserProviderInstalledPath -ItemType Directory -Force  
    Write-Host "Created  $UserProviderInstalledPath"

} else{
    Get-ChildItem -Path $UserProviderInstalledPath -Recurse | Remove-Item -force -Recurse -ea silentlycontinue
}

if(-not (Test-Path $UserModulePath)) {
    New-Item -Path $UserModulePath -ItemType Directory -Force  
    Write-Host "Created  $UserModulePath"

} else{
    Get-ChildItem -Path $UserModulePath -Recurse | Remove-Item -force -Recurse -ea silentlycontinue
}


if ($testframework -eq "coreclr")
{
    # install powershell core if test framework is coreclr 
    Install-PackageProvider PSL -Force; 
    $powershellCore = (Get-Package -provider PSL -name PowerShell)
    if (-not $powershellCore)
    {   
        $powershellCore = Install-Package PowerShell -Provider PSL -Force
    }

    $powershellVersion = $powershellCore.Version
    Write-host ("PowerShell Version '{0}'" -f $powershellVersion)

    $powershellFolder = "$Env:ProgramFiles\PowerShell\$powershellVersion\"
    Write-host ("PowerShell Folder '{0}'" -f $powershellFolder)

    $OneGetPath = "$powershellFolder\Modules\PackageManagement"
    Write-host ("OneGet Folder '{0}'" -f $OneGetPath)

    if(-not (Test-Path -Path $OneGetPath))
    {
        New-Item -Path $OneGetPath -ItemType Directory -Force -Verbose
    }

    # copy OneGet module files
    Copy-Item "$TestBin\*.psd1" $OneGetPath -force -Verbose
    Copy-Item "$TestBin\*.psm1" $OneGetPath -force -Verbose
    Copy-Item "$TestBin\*.ps1xml" $OneGetPath -force -Verbose

     # copy the OneGet bits into powershell core
    Copy-Item "$TestBin\netstandard1.6\*.dll" $OneGetPath -Force -Verbose

    # copy test modules
    Copy-Item  "$($TestHome)\Unit\Providers\PSChained1Provider.psm1" "$($powershellFolder)\Modules" -force -verbose
    Copy-Item  "$($TestHome)\Unit\Providers\PSOneGetTestProvider" "$($powershellFolder)\Modules"  -Recurse -force -verbose
}



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

if ($testframework -eq "fullclr")
{
    Copy-Item  "$($TestHome)\Unit\Providers\PSChained1Provider.psm1" "$($ProgramModulePath)\" -force -verbose
    Copy-Item  "$($TestHome)\Unit\Providers\PSChained1Provider.psm1" "$($UserModulePath)\" -force -verbose
    Copy-Item  "$($TestHome)\Unit\Providers\PSOneGetTestProvider" "$($ProgramModulePath)\"  -Recurse -force -verbose
}

#Step 2 - run tests
Write-Host -fore White "Running powershell pester tests "

if ($testframework -eq "fullclr")
{
    Write-Host "FullClr: Calling Invoke-Pester $($TestHome)\ModuleTests\tests" 
    $pm =Get-Module -Name PackageManagement

    if($pm)
    {
        Write-Warning ("PackageManagement is loaded already from '{0}'" -f $pm.ModuleBase)
    }
      
    $command = "Invoke-Pester $($TestHome)\ModuleTests\tests"
      
    powershell -command $command
}

if ($testframework -eq "coreclr")
{
    $command = "Set-ExecutionPolicy -Scope Process Unrestricted -force;"
    $pesterFolder = "$powerShellFolder\Modules\Pester"

    $command += "Import-Module '$pesterFolder';"

    $command += "Invoke-Pester $($TestHome)\ModuleTests\tests"

    Write-Host "CoreCLR: Calling $command"

    & "$powershellFolder\powershell" -command $command
}

Write-Host -fore White "Finished tests"

#Step3 - cleanup
if (test-path $Env:AppData/NuGet/nuget.config.original ) {
    copy -force $Env:AppData/NuGet/nuget.config.original $Env:AppData/NuGet/nuget.config -ea silentlycontinue
    rm -force $Env:AppData/NuGet/nuget.config.original  -ea silentlycontinue
}

cd $TestHome