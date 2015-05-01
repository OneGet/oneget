# This is a test entry point script.
#
# Copyright (c) Microsoft Corporation, 2012
#
param( [switch]$IsSandboxed ) 

$origDir = (pwd)
cd $PSScriptRoot

# ensure that we're only picking up the modules that we really want to.
$env:PSModulePath = "$env:SystemRoot\System32\WindowsPowerShell\v1.0\Modules;$env:ProgramFiles\WindowsPowerShell\Modules;$PSScriptRoot\tools\;" 

if( -not $IsSandboxed  ) {
    Write-Host -fore Green "Restarting script with DNSShim and sandbox"
    cd $PSScriptRoot

    # make sure the sandbox isn't running yet.
    if( .\scripts\test-sandbox.ps1 ) {
        cd $PSScriptRoot
        .\scripts\stop-sandbox.ps1
        start-sleep 2
    }

    cd $PSScriptRoot
    .\scripts\start-sandbox.ps1 
    
   # wait for sandbox to start
    cd $PSScriptRoot
    foreach ($n in 1..10) {
        if( -not (& .\scripts\test-sandbox.ps1) ) {
        cd $PSScriptRoot
        Write-Host -fore Yellow "Waiting for sandbox..."
        start-sleep 2
        } else {
            break;
        }
    }    

    cd $PSScriptRoot
    # check to see that sandbox was able to run
    if( -not ( .\scripts\test-sandbox.ps1 )  ) {
        throw "UNABLE TO START SANDBOX"
    }
    
    # re-run the this script with the DNS shim.
    . $PSScriptRoot\tools\DnsShim.exe  /i:$PSScriptRoot\tools\hosts.txt /v powershell.exe "$($MyInvocation.MyCommand.Definition) -IsSandboxed" 

    cd $PSScriptRoot
    .\scripts\stop-sandbox.ps1 

    return 
}

#first, make sure that we've got a copy of the binaries for unit testing.
$outputPath = "$PSScriptRoot\output\debug\bin"

# make sure the output path exists before we go anywhere
if( -not (test-path $outputPath) ) {
    $null = mkdir $outputPath -ea continue
}

# put all the binaries into output path
if (test-path "c:\program files\WindowsPowerShell\Modules\PackageManagement" ) {
    copy -force "c:\program files\WindowsPowerShell\Modules\PackageManagement\*" $outputPath
} else {
    if (test-path "c:\Windows\System32\WindowsPowerShell\v1.0\Modules\PackageManagement" ) {
        copy -force "c:\Windows\System32\WindowsPowerShell\v1.0\Modules\PackageManagement\*" $outputPath
    }
}

# copy the xunit test runner
copy -force  ".\packages\xunit.runner.console.2*\tools\*" $outputPath

# build the xUnit tests
& "$env:SystemRoot\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" /p:Configuration=Debug .\unit.sln

# remove the strongname from the binaries -- xUnit is giving me nothing but pain from that
tools\snremove.exe -r "$outputPath\*PackageManagement*.dll"

# make sure chocolatey lib dir is created
mkdir c:\chocolatey\lib -ea silentlycontinue 

# remove existing nuget provider.
if (test-path $Env:ProgramFiles/OneGet/ProviderAssemblies/nuget-anycpu.exe) {
    ren $Env:ProgramFiles/OneGet/ProviderAssemblies/nuget-anycpu.exe "nuget-anycpu.$(Get-Random).deleteme"
    rm -force $Env:ProgramFiles/OneGet/ProviderAssemblies/*.deleteme -ea silentlycontinue
}

if (test-path $Env:LocalAppData/OneGet/ProviderAssemblies/nuget-anycpu.exe) {
    ren $Env:LocalAppData/OneGet/ProviderAssemblies/nuget-anycpu.exe "nuget-anycpu.$(Get-Random).deleteme"
    rm -force $Env:LocalAppData/OneGet/ProviderAssemblies/*.deleteme  -ea silentlycontinue
}

if (test-path $Env:ProgramFiles/PackageManagement/ProviderAssemblies/nuget-anycpu.exe) {
    ren $Env:ProgramFiles/PackageManagement/ProviderAssemblies/nuget-anycpu.exe "nuget-anycpu.$(Get-Random).deleteme"
    rm -force $Env:ProgramFiles/PackageManagement/ProviderAssemblies/*.deleteme -ea silentlycontinue
}

if (test-path $Env:LocalAppData/PackageManagement/ProviderAssemblies/nuget-anycpu.exe) {
    ren $Env:LocalAppData/PackageManagement/ProviderAssemblies/nuget-anycpu.exe "nuget-anycpu.$(Get-Random).deleteme"
    rm -force $Env:LocalAppData/PackageManagement/ProviderAssemblies/*.deleteme  -ea silentlycontinue
}

Import-Module DispatchLayer
Write-Host -fore White "Running xUnit test wrapper"
InvokeComponent @args -Directory $PSScriptRoot\output\debug\bin

Write-Host -fore White "Running powershell pester tests "
InvokeComponent @args -Directory $PSScriptRoot\ModuleTests\tests
