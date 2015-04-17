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

[CmdletBinding(SupportsShouldProcess=$true)]
Param(
	[string]$moduleLocation = 'packagemanagement',
    [string]$tag = $null,
    [string]$name = $null,
    [string]$excludeTag = $null,
    [string]$action = 'test',
    [switch]$enableSandbox
)

$origdir = (pwd)
cd $PSScriptRoot

$allDiscoveredTests = $null
$T_total = 0
$T_failed= 0
$T_succeeded = 0

function Describe {
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [string] $Name,
        $Tags=@(),
        [Parameter(Position = 1)]
        [ValidateNotNull()]
        [ScriptBlock] $Fixture = $(Throw "No test script block is provided. (Have you put the open curly brace on the next line?)")
    )
    if( -not $allDiscoveredTests.ContainsKey( $Tags ) ) {
        $lst = New-Object "System.Collections.Generic.List``1[string]"
        $lst.Add( $name )
        $null = $allDiscoveredTests.Add( "$Tags", $lst)
    } else {
        $null = $allDiscoveredTests["$Tags"].Add( $name )
    }
}

function Get-TestsByTag{
    param(
        [string]$testPath
    )
    $allDiscoveredTests = New-Object "System.Collections.Generic.Dictionary``2[System.string,System.Collections.Generic.List``1[string]]"

    $null = Get-ChildItem $testPath -Filter "*.Tests.ps1" -Recurse |
        where { -not $_.PSIsContainer } |
        foreach {
            $testFile = $_

            try {
                $null = (&  $testFile.PSPath)
            }
            catch {
                # who cares at this point...
            }
        }
    return $allDiscoveredTests;
}

function output-counts {
    param(
        [int]$total,
        [int]$failures,
        [int]$successes
    )

    write-host "RESULTS: [" -foregroundcolor  white  -nonewline
    write-host "$successes" -foregroundcolor  green -nonewline
    write-host "/" -foregroundcolor white -nonewline

    if( $failures -gt 0 ) {
        write-host "$failures" -foregroundcolor red -nonewline
        write-host "/" -foregroundcolor white -nonewline
        write-host "$total" -foregroundcolor blue -nonewline
        write-host "]" -foregroundcolor white
        return $true
    }

    write-host "$total" -foregroundcolor green -nonewline
    write-host "]" -foregroundcolor white
    return $false
}

function process-results {
    param(
        [string]$output
    )
    # load the results from the output
    [xml]$results= Get-Content $output

    $total = $results.'test-results'.total
    $failures = $results.'test-results'.failures
    $successes = $total - $failures

    $script:T_total = $script:T_total + $total
    $script:T_failed= $script:T_failed + $failures
    $script:T_succeeded = $script:T_succeeded + $successes

    return (output-counts $total $failures $successes)
}

try {
    switch( $action ) {
        'test' {
            $failed = $false

            if( $enableSandbox ) {
                . $PSScriptRoot\scripts\start-sandbox
            }

            if( -not (test-path $PSScriptRoot\Pester\Vendor\packages) )  {
                write-error "Run run-tests -action setup first."
                return $false
            }

            if( -not (& $PSScriptRoot\scripts\test-pester.ps1) )  {
                write-error "Run run-tests -action setup first."
                return $false
            }

            # Set the environment variable to the packagemanagement module we want to test
            $env:PMModuleTest = $moduleLocation

            # adn the important parts about what we're testing
            $pester = "$PSScriptRoot\Pester\pester.psd1"
            $testPath =  "$PSScriptRoot\Tests"
            $output = "$PSScriptRoot\PackageManagement.Results.XML"
            $allTests = (Get-TestsByTag $testPath)
            $options = ""

            if( $tag ) {
                $options += " -Tag '$tag' "
            }


            if(-not $tag -or $tag -eq "pristine")   {
                # run tests tagged 'pristine' in a seperate session for each one of them
                foreach( $key in $allTests.Keys ) {
                    $keys = $key.Split(" ")

                    if( $keys -contains "pristine" )  {
                        foreach( $testName in $allTests[$key] ) {
                            if( -not $name -or $testName -match $name ) {
                                $opts = $options

                                if( $excludeTag ) {
                                    $opts += " -ExcludeTag '$excludeTag' "
                                }

                                write-host "`n=========================================================="
                                write-host -foregroundcolor yellow "Executing pristine test $testName in seperate session"

                                if( $enableSandbox ) {
                                    . $PSScriptRoot\tools\DnsShim /i:$PSScriptRoot\tools\hosts.txt /v powershell.exe "ipmo '$pester' ; Invoke-Pester -Path '$testPath' -OutputFile '$output' -OutputFormat NUnitXml -TestName '$testName' $opts"
                                } else {
                                    . powershell.exe "ipmo '$pester' ; Invoke-Pester -Path '$testPath' -OutputFile '$output' -OutputFormat NUnitXml -TestName '$testName' $opts"
                                }

                                $failed = (process-results $output) -or $failed
                            }
                        }
                    }
                }
            }

            if( $name ) {
                $options += " -TestName '$name' "
            }

            if( $excludeTag ) {
                $options += " -ExcludeTag '$excludeTag pristine' "
            } else {
                $options += " -ExcludeTag 'pristine' "
            }

            # Run using the powershell.exe so that the tests will load the PackageManagement
            # module using the version that gets specified (and not one that
            # may be in this session already)
            # . powershell.exe "ipmo '$pester' ; Invoke-Pester -Path '$testPath' -OutputFile '$output' -OutputFormat NUnitXml $options"


            if( $enableSandbox ) {
                . $PSScriptRoot\tools\DnsShim /i:$PSScriptRoot\tools\hosts.txt /v powershell.exe "ipmo '$pester' ; Invoke-Pester -Path '$testPath' -OutputFile '$output' -OutputFormat NUnitXml $options"
            } else {
                . powershell.exe "ipmo '$pester' ; Invoke-Pester -Path '$testPath' -OutputFile '$output' -OutputFormat NUnitXml $options"
            }

            $failed = (process-results $output) -or $failed

            write-host "`n`n`n=========================================================="
            write-host "Totals:   " -nonewline
            $null = output-counts $T_total $T_failed $T_succeeded
            write-host "=========================================================="
            return -not $failed
        }

        'setup' {

            #make sure that pester is configured correctly
            if( (& $PSScriptRoot\scripts\test-pester.ps1) )  {
                write-verbose "Pester appears correct"
            } else {
                cd $PSScriptRoot\Pester
                write-verbose "Running Pester Build"
                . $PSScriptRoot\Pester\build.bat package
            }
        }

        'cleanup' {
        
        }
    }
} finally {
    $env:PMModuleTest  = $ull
    cd $origdir
}
