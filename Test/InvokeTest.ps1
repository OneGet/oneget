# This is a test entry point script.
#
# Copyright (c) Microsoft Corporation, 2012
#
param( [switch]$IsSandboxed ) 

$origDir = (pwd)
cd $PSScriptRoot

if (test-path $Env:ProgramFiles/OneGet/ProviderAssemblies/nuget-anycpu.exe) {
    ren $Env:ProgramFiles/OneGet/ProviderAssemblies/nuget-anycpu.exe "nuget-anycpu.$(Get-Random).old"
    rm -force $Env:ProgramFiles/OneGet/ProviderAssemblies/*.old -ea silentlycontinue
}

if (test-path $Env:LocalAppData/OneGet/ProviderAssemblies/nuget-anycpu.exe) {
    ren $Env:LocalAppData/OneGet/ProviderAssemblies/nuget-anycpu.exe "nuget-anycpu.$(Get-Random).old"
    rm -force $Env:LocalAppData/OneGet/ProviderAssemblies/*.old  -ea silentlycontinue
}

# ensure that we're only picking up the modules that we really want to.
$env:PSModulePath = "$env:SystemRoot\System32\WindowsPowerShell\v1.0\Modules;$PSScriptRoot\tools\;" 

if( -not $IsSandboxed  ) {
    Write-Host -fore Green "Restarting script with DNSShim and sandbox"
    cd $PSScriptRoot

    # make sure the sandbox isn't running yet.
    if( .\scripts\test-sandbox.ps1 ) {
        cd $PSScriptRoot
        .\scripts\stop-sandbox.ps1
    }

    cd $PSScriptRoot
    .\scripts\start-sandbox.ps1 
    
    # wait for a moment for it to start.
    start-sleep 5
    
    # re-run the this script with the DNS shim.
    . $PSScriptRoot\tools\DnsShim.exe  /i:$PSScriptRoot\tools\hosts.txt /v powershell.exe "$($MyInvocation.MyCommand.Definition) -IsSandboxed" 

    
    cd $PSScriptRoot
    .\scripts\stop-sandbox.ps1 

    return 
}

Import-Module DispatchLayer
Write-Host -fore White "Running xUnit test wrapper"
InvokeComponent @args -Directory $PSScriptRoot\output\debug\bin

Write-Host -fore White "Running powershell pester tests "
InvokeComponent @args -Directory $PSScriptRoot\ModuleTests\tests
