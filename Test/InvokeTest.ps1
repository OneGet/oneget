# This is a test entry point script.
#
# Copyright (c) Microsoft Corporation, 2012
#
param( [switch]$IsSandboxed ) 

$origDir = (pwd)
cd $PSScriptRoot

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
InvokeComponent @args -Directory $PSScriptRoot\output\debug\bin
InvokeComponent @args -Directory $PSScriptRoot\bvt\cmdlet-testsuite\tests

