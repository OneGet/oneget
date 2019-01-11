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
[CmdletBinding()]
param(
    [ValidateSet("coreclr", "fullclr")]
    [string]$testframework = "fullclr",
    [ValidateSet("v2", "v3", "all")]
    [string]$nugetApiVersion = "v2"
)

Import-Module "$PSScriptRoot\TestUtility.psm1" -Force

# Add TLS12 for GitHub downloads, in case PSCore needs to be downloaded
[Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12

#region Step 0 -- remove the strongname from the binaries
# Get the current OS
try
{
    $script:IsLinux = (Get-Variable -Name IsLinux -ErrorAction Ignore) -and $IsLinux
    $script:IsMacOS = (Get-Variable -Name IsMacOS -ErrorAction Ignore) -and $IsMacOS
    $script:IsCoreCLR = (Get-Variable -Name IsCoreCLR -ErrorAction Ignore) -and $IsCoreCLR
    $script:IsWindows = $true

    $runtimeInfo = ($null -ne ('System.Runtime.InteropServices.RuntimeInformation' -as [Type]))
    if($runtimeInfo)
    {
        $Runtime = [System.Runtime.InteropServices.RuntimeInformation]
        $OSPlatform = [System.Runtime.InteropServices.OSPlatform]
       
        $script:IsWindows = $Runtime::IsOSPlatform($OSPlatform::Windows)
    }
}catch{ } # on linux error from PowerShell: "Cannot overwrite variable IsLinux because it is read-only". Tracking PowerShellCore issue#2609.


if($script:IsWindows)
{
  try
  {
      reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PackageManagement.ArchiverProviders,31bf3856ad364e35"  /f
      reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PackageManagement.CoreProviders,31bf3856ad364e35"  /f
      reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PackageManagement,31bf3856ad364e35"  /f
      reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PackageManagement.MetaProvider.PowerShell,31bf3856ad364e35"  /f
      reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PackageManagement.MsiProvider,31bf3856ad364e35"  /f
      reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PackageManagement.MsuProvider,31bf3856ad364e35"  /f
      reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PackageManagement.NuGetProvider,31bf3856ad364e35"  /f
      reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PackageManagement.Test,31bf3856ad364e35"  /f
      reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PowerShell.PackageManagement,31bf3856ad364e35"  /f
      reg ADD "HKLM\Software\Microsoft\StrongName\Verification\Microsoft.PackageManagement.OneGetTestProvider,31bf3856ad364e35"  /f
  
  
      reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PackageManagement.ArchiverProviders,31bf3856ad364e35"  /f
      reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PackageManagement.CoreProviders,31bf3856ad364e35"  /f
      reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PackageManagement,31bf3856ad364e35"  /f
      reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PackageManagement.MetaProvider.PowerShell,31bf3856ad364e35"  /f
      reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PackageManagement.MsiProvider,31bf3856ad364e35"  /f
      reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PackageManagement.MsuProvider,31bf3856ad364e35"  /f
      reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PackageManagement.NuGetProvider,31bf3856ad364e35"  /f
      reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PackageManagement.Test,31bf3856ad364e35"  /f
      reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PowerShell.PackageManagement,31bf3856ad364e35"  /f
      reg ADD "HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\Microsoft.PackageManagement.OneGetTestProvider,31bf3856ad364e35"  /f
  }
  catch{}
}
#endregion

#region Step 1 - test setup
$TestHome = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($PSScriptRoot)
$TestBin = "$($TestHome)\..\src\out\PackageManagement\"
$CoreCLRTestHome = "$($TestHome)\..\Test"

# Get OneGet version
$packageManagementManifest = Test-ModuleManifest "$TestBin\PackageManagement.psd1"
$PackageManagementVersion = $packageManagementManifest.Version.ToString()

$testframeworkVariable = $null
$totalTestFailures = 0
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

$allNugetApiVersions = @()
if ($nugetApiVersion -eq 'all') {
	$allNugetApiVersions = @('v2','v3')
} else {
	$allNugetApiVersions += $nugetApiVersion
}

if ($testframework -eq "fullclr")
{
    $mydocument = Microsoft.PowerShell.Management\Join-Path -Path $HOME -ChildPath 'Documents\PowerShell'
}
else
{
    $mydocument = Microsoft.PowerShell.Management\Join-Path -Path $HOME -ChildPath ".local/share/powershell"
}


# Setting up Packagemanagement
if ($testframework -eq "fullclr")
{    
    $ProgramProviderInstalledPath = "$Env:ProgramFiles\PackageManagement\ProviderAssemblies"
    $LocalAppData = $env:LocalAppdata
    $UserProviderInstalledPath = "$($LocalAppData)\PackageManagement\ProviderAssemblies"
    $ProgramModulePath = "$Env:ProgramFiles\WindowsPowerShell\Modules"

    $UserModulePath = "$($mydocument)\WindowsPowerShell\Modules"
    $packagemanagementfolder = "$ProgramModulePath\PackageManagement\$PackageManagementVersion"

    if(-not (Test-Path $packagemanagementfolder)){
        New-Item -Path $packagemanagementfolder -ItemType Directory -Force  
        Write-Host "Created  $packagemanagementfolder"
    } else{
        Get-ChildItem -Path $packagemanagementfolder | %{ren "$packagemanagementfolder\$_" "$packagemanagementfolder\$_.deleteMe" -ErrorAction SilentlyContinue}
        Get-ChildItem -Path $packagemanagementfolder  -Recurse |  Remove-Item -force -Recurse -ErrorAction SilentlyContinue
    }

    # Copying files to Packagemanagement
    Copy-Item "$TestBin\fullclr\*.dll" $packagemanagementfolder -force -Verbose
    Copy-Item "$TestBin\*.psd1" $packagemanagementfolder -force -Verbose
    Copy-Item "$TestBin\*.psm1" $packagemanagementfolder -force -Verbose
    Copy-Item "$TestBin\*.ps1" $packagemanagementfolder -force -Verbose
    Copy-Item "$TestBin\*.ps1xml" $packagemanagementfolder -force -Verbose
    New-DirectoryIfNotExist (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources")
    Copy-Item "$TestBin\DSCResources\*.psm1" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources")
    Copy-Item "$TestBin\DSCResources\*.psd1" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources")
    New-DirectoryIfNotExist (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagement")
    Copy-Item "$TestBin\DSCResources\MSFT_PackageManagement\*.psm1" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagement")
    Copy-Item "$TestBin\DSCResources\MSFT_PackageManagement\*.psd1" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagement")
    Copy-Item "$TestBin\DSCResources\MSFT_PackageManagement\*.mof" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagement")
    Copy-Item "$TestBin\DSCResources\MSFT_PackageManagement\*.mfl" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagement")
    New-DirectoryIfNotExist (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagementSource")
    Copy-Item "$TestBin\DSCResources\MSFT_PackageManagementSource\*.psm1" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagementSource")
    Copy-Item "$TestBin\DSCResources\MSFT_PackageManagementSource\*.psd1" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagementSource")
    Copy-Item "$TestBin\DSCResources\MSFT_PackageManagementSource\*.mof" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagementSource")
    Copy-Item "$TestBin\DSCResources\MSFT_PackageManagementSource\*.mfl" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagementSource")
    New-DirectoryIfNotExist (Join-Path -Path $packagemanagementfolder -ChildPath "Examples")
    Copy-Item "$TestHome\Examples\*.ps1" (Join-Path -Path $packagemanagementfolder -ChildPath "Examples")

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

    
    # Clean up
    if (Test-Path "$env:temp\PackageManagementDependencies") {
        Remove-Item -Recurse -Force "$env:temp\PackageManagementDependencies"
    }

    Copy-Item "$($TestHome)\Unit\Providers\Dependencies" "$env:tmp\PackageManagementDependencies" -Recurse -Force


    if (test-path $Env:AppData/NuGet/nuget.config) {
        copy -force $Env:AppData/NuGet/nuget.config $Env:AppData/NuGet/nuget.config.original -ea silentlycontinue
        rm -force $Env:AppData/NuGet/nuget.config  -ea silentlycontinue
    }

    # Copy test dependencies

    Copy-Item  "$($TestHome)\Unit\Providers\PSChained1Provider.psm1" "$($ProgramModulePath)\" -force -verbose
    Copy-Item  "$($TestHome)\Unit\Providers\PSChained1Provider.psm1" "$($UserModulePath)\" -force -verbose
    Copy-Item  "$($TestHome)\Unit\Providers\PSOneGetTestProvider" "$($ProgramModulePath)\"  -Recurse -force -verbose
}

$powershellCoreFilePath = 'powershell'
if ($testframework -eq "coreclr")
{
    # install powershell core if test framework is coreclr 

    If($script:IsWindows)
    {
        if($env:APPVEYOR_SCHEDULED_BUILD -eq 'True')
        {
            # for the daily run, we need to install PowerShellCore from github.com/powershell/powershell appveryor artifacts

            $powershellMSIPackage = Get-PowerShellCoreBuild -Verbose

            Write-Verbose $powershellMSIPackage
            $powershellCore = Install-Package  $powershellMSIPackage -provider msi -verbose -force
        }
        else
        {
            Install-PackageProvider PSL -Force -verbose
            if ([Environment]::OSVersion.Version.Major -eq 6) {
                Write-Verbose "Assuming OS is Win 8.1 (includes Win Server 2012 R2)"
                $pslLocation = Join-Path -Path $PSScriptRoot -ChildPath "PSL\win81\PSL.json"
            } else {
                Write-Verbose "Assuming OS is Win 10"
                $pslLocation = Join-Path -Path $PSScriptRoot -ChildPath "PSL\win10\PSL.json"
            }

            $powershellCore = (Get-Package -provider msi -name PowerShell-6.1* -ErrorAction SilentlyContinue | Sort-Object -Property Version -Descending | Select-Object -First 1)
            if ($powershellCore)
            {
                Write-Warning ("PowerShell already installed" -f $powershellCore.Name)
            }
            else
            {
                $powershellCore = Install-PowerShellCore -PSLLocation $pslLocation
            }
        }

        $powershellVersion = $powershellCore.Version
        $powershellFolder = "$Env:ProgramFiles\PowerShell\6"
        if ((-not (Test-Path -Path (Join-Path -Path $powershellFolder -ChildPath 'powershell.exe'))) -and
                -not (Test-Path -Path (Join-Path -Path $powershellFolder -ChildPath 'pwsh.exe')) -and 
                (-not (Test-Path -Path (Join-Path -Path $powershellFolder -ChildPath 'powershell'))) -and
                -not (Test-Path -Path (Join-Path -Path $powershellFolder -ChildPath 'pwsh'))) {
            # Everything after "PowerShell-"
            $powershellVersion = $powershellCore.Name.Substring(11)
            $powershellFolder = "$Env:ProgramFiles\PowerShell\$powershellVersion"
        }

        Write-host ("PowerShell Version '{0}'" -f $powershellVersion)
        Write-host ("PowerShell Folder '{0}'" -f $powershellFolder)
    }
    else
    {
        # set up powershellFolder On Linux
        $powershellFolder = (Get-Module -Name Microsoft.PowerShell.Utility).ModuleBase
    }

    if ((-not (Test-Path -Path (Join-Path -Path $powershellFolder -ChildPath "$powershellCoreFilePath.exe"))) -and
         -not (Test-Path -Path (Join-Path -Path $powershellFolder -ChildPath $powershellCoreFilePath))) {
        $powershellCoreFilePath = "pwsh"
        if ((-not (Test-Path -Path (Join-Path -Path $powershellFolder -ChildPath "$powershellCoreFilePath.exe"))) -and
             -not (Test-Path -Path (Join-Path -Path $powershellFolder -ChildPath $powershellCoreFilePath))) {
            throw "Couldn't find PowerShell Core exe path in folder: $powershellFolder"
        }
    }
    
    & pwsh -command 'Install-Module Pester -Scope CurrentUser -Force'

    # Delete installed PackageManagement  
    $packagemanagementfolder = "$powershellFolder\Modules\PackageManagement\"
    Write-Verbose -Verbose ("PackageManagement Folder '{0}'" -f $packagemanagementfolder)
    Remove-Item $packagemanagementfolder -Recurse
    New-Item -Path $packagemanagementfolder -ItemType Directory -Force -Verbose

    # copy OneGet module files
    Copy-Item "$TestBin\*.psd1" $packagemanagementfolder -force -Verbose
    Copy-Item "$TestBin\*.psm1" $packagemanagementfolder -force -Verbose
    Copy-Item "$TestBin\*.ps1xml" $packagemanagementfolder -force -Verbose
    New-DirectoryIfNotExist (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources")
    Copy-Item "$TestBin\DSCResources\*.psm1" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources")
    Copy-Item "$TestBin\DSCResources\*.psd1" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources")
    New-DirectoryIfNotExist (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagement")
    Copy-Item "$TestBin\DSCResources\MSFT_PackageManagement\*.psm1" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagement")
    Copy-Item "$TestBin\DSCResources\MSFT_PackageManagement\*.psd1" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagement")
    Copy-Item "$TestBin\DSCResources\MSFT_PackageManagement\*.mof" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagement")
    Copy-Item "$TestBin\DSCResources\MSFT_PackageManagement\*.mfl" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagement")
    New-DirectoryIfNotExist (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagementSource")
    Copy-Item "$TestBin\DSCResources\MSFT_PackageManagementSource\*.psm1" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagementSource")
    Copy-Item "$TestBin\DSCResources\MSFT_PackageManagementSource\*.psd1" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagementSource")
    Copy-Item "$TestBin\DSCResources\MSFT_PackageManagementSource\*.mof" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagementSource")
    Copy-Item "$TestBin\DSCResources\MSFT_PackageManagementSource\*.mfl" (Join-Path -Path $packagemanagementfolder -ChildPath "DSCResources\MSFT_PackageManagementSource")
    New-DirectoryIfNotExist (Join-Path -Path $packagemanagementfolder -ChildPath "Examples")
    Copy-Item "$CoreCLRTestHome\Examples\*.ps1" (Join-Path -Path $packagemanagementfolder -ChildPath "Examples")
    Copy-Item "$TestBin\coreclr" $packagemanagementfolder -Recurse
    
    # copy test modules
    Copy-Item  "$($TestHome)\Unit\Providers\PSChained1Provider.psm1" "$($powershellFolder)\Modules" -force -verbose
    Copy-Item  "$($TestHome)\Unit\Providers\PSOneGetTestProvider" "$($powershellFolder)\Modules"  -Recurse -force -verbose
}

#endregion

foreach ($currentNugetApiVersion in $allNugetApiVersions) {
    Write-host ("testframework={0}, IsCoreCLR={1}, IsLinux={2}, IsMacOS={3}, IsWindows={4}, NugetApiVersion={5}" -f $testframework, $script:IsCoreCLR, $script:IsLinux, $script:IsMacOS, $script:IsWindows, $currentNugetApiVersion)
    if ($currentNugetApiVersion -eq 'v2') {
        $nugetApiUrl = "https://nuget.org/api/v2/"
        $nugetApiUrlAlternate = "https://nuget.org/api/v2"
    } else {
        $nugetApiUrl = "https://api.nuget.org/v3/index.json"
        $nugetApiUrlAlternate = "https://api.nuget.org/v3/index.json"
    }

    Write-Verbose -Message "Using NuGet API URL: $nugetApiUrl"
    Write-Verbose -Message "Using NuGet API URL: $nugetApiUrlAlternate"

    # Set up test repositories for DSC tests when on Windows (DSC tests)
    if ($script:IsWindows) {
        # Run these in another context because SNV was just setup
        $localRepoCommand = "Import-Module `"$PSScriptRoot\TestUtility.psm1`" -Force"
        $localRepoCommand += ";Setup-TestRepositoryPathVars -RepositoryRootDirectory `"$PSScriptRoot\DSCTests`""
        $localRepoCommand += ";New-TestRepositoryModules -RepositoryRootDirectory `"$PSScriptRoot\DSCTests`""
        powershell -command "& {$localRepoCommand}"
    }

    #Step 2 - run tests
    Write-Host "Running powershell pester tests "

    if ($testframework -eq "fullclr")
    {
        try
        {
            $env:NUGET_API_URL = $nugetApiUrl
            $env:NUGET_API_URL_ALTERNATE = $nugetApiUrlAlternate
            $env:NUGET_API_VERSION = $currentNugetApiVersion
            Write-Host "FullClr: Calling Invoke-Pester $($TestHome)\ModuleTests\tests" 
            $pm =Get-Module -Name PackageManagement

            if($pm)
            {
                Write-Warning ("PackageManagement is loaded already from '{0}'" -f $pm.ModuleBase)
            }

            $testResultsFile="$($TestHome)\ModuleTests\tests\testresult.xml"
            $command = "Invoke-Pester $($TestHome)\ModuleTests\tests -OutputFile $testResultsFile -OutputFormat NUnitXml -EnableExit"

            Powershell -command "& {get-packageprovider -verbose; $command}"

            $x = [xml](Get-Content -raw $testResultsFile)
            $numTestFailures = [int]$x.'test-results'.failures
            if ($numTestFailures -gt 0)
            {
                $totalTestFailures = $numTestFailures
                throw "$($x.'test-results'.failures) tests failed"
            }

            $testResultsFile="$($TestHome)\DSCTests\tests\testresult.xml"
            $command = "Invoke-Pester $($TestHome)\DSCTests\tests -OutputFile $testResultsFile -OutputFormat NUnitXml -EnableExit"

            Powershell -command "& {get-packageprovider -verbose; $command}"

            $x = [xml](Get-Content -raw $testResultsFile)
            $numTestFailures = [int]$x.'test-results'.failures
            if ($numTestFailures -gt 0)
            {
                $totalTestFailures = $numTestFailures
                throw "$($x.'test-results'.failures) tests failed"
            }
        } catch {}
        finally
        {
            Remove-Item Env:\NUGET_API_URL
            Remove-Item Env:\NUGET_API_URL_ALTERNATE
            Remove-Item Env:\NUGET_API_VERSION
        }
    }

    if ($testframework -eq "coreclr")
    {
        $pesterFolder = "$powershellFolder\Modules\Pester"
        $command ="`$env:NUGET_API_URL = '$nugetApiUrl';`$env:NUGET_API_URL_ALTERNATE = `'$nugetApiUrlAlternate`';`$env:NUGET_API_VERSION = `'$currentNugetApiVersion`';"
        $testResultsFile="$($TestHome)\ModuleTests\tests\testresult.xml"
        if($script:IsWindows)
        {
            $command += "Set-ExecutionPolicy -Scope Process Unrestricted -force;"
        }

        $command += "Invoke-Pester $($TestHome)\ModuleTests\tests -OutputFile $testResultsFile -OutputFormat NUnitXml -EnableExit"

        Write-Host "CoreCLR: Calling $powershellFolder\$powershellCoreFilePath -command  $command"

        if($script:IsWindows)
        {
            & "$powershellFolder\$powershellCoreFilePath" -command "& {get-packageprovider -verbose; $command}"
        }
        else
        {
            & "$powershellCoreFilePath" -command "& {get-packageprovider -verbose; $command}"
        }

        $x = [xml](Get-Content -raw $testResultsFile)
        $numTestFailures = [int]$x.'test-results'.failures
        if ($numTestFailures -gt 0)
        {
            $totalTestFailures = $numTestFailures
            throw "$($x.'test-results'.failures) tests failed"
        }
        <#  Disable DSC tests for Linux for now. #>
If($script:IsWindows)
{
            $command ="`$env:NUGET_API_URL = '$nugetApiUrl';`$env:NUGET_API_URL_ALTERNATE = '$nugetApiUrlAlternate';`$env:NUGET_API_VERSION = '$currentNugetApiVersion';"
            $testResultsFile="$($TestHome)\DSCTests\tests\testresult.xml"
            $command += "Invoke-Pester $($TestHome)\DSCTests\tests -OutputFile $testResultsFile -OutputFormat NUnitXml -EnableExit"

            Write-Host "CoreCLR: Calling $powershellFolder\$powershellCoreFilePath -command  $command"

            if($script:IsWindows)
            {
                & "$powershellFolder\$powershellCoreFilePath" -command "& {get-packageprovider -verbose; $command}"
            }
            else
            {
                & "$powershellCoreFilePath" -command "& {get-packageprovider -verbose; $command}"
            }

            $x = [xml](Get-Content -raw $testResultsFile)
            $numTestFailures = [int]$x.'test-results'.failures
            if ($numTestFailures -gt 0)
            {
                $totalTestFailures = $numTestFailures
                throw "$($x.'test-results'.failures) tests failed"
            }
        }
    }
}

if ($totalTestFailures -ne 0)
{
    $host.SetShouldExit($totalTestFailures) 
}

Write-Host -fore White "Finished tests"

#Step3 - cleanup
if (test-path $Env:AppData/NuGet/nuget.config.original ) {
    copy -force $Env:AppData/NuGet/nuget.config.original $Env:AppData/NuGet/nuget.config -ea silentlycontinue
    rm -force $Env:AppData/NuGet/nuget.config.original  -ea silentlycontinue
}

cd $TestHome