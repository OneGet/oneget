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


$origdir = (pwd)

cd $PSScriptRoot

# where stuff is
$root = resolve-path "$PSScriptRoot\.."

# check for installed certificates first.
if( (dir "Cert:\LocalMachine\my\" | Where-Object Subject -eq "CN=pmtestcert" ).Length -eq 0 )  {
    write-warning "pmtestcert does not appear to be installed"
    cd $origdir
    return;
}

# Make sure the sandbox can run
if (get-command remove-iissite -ea silentlycontinue) { 

    if (get-iissite -Name "Default Web Site" ) { 
        remove-iissite -Name "Default Web Site" -Confirm:$false
        
        if (get-iissite -Name "Default Web Site" ) { 
            throw "UNABLE TO REMOVE DEFAULT WEBSITE"
        }
    }
}

if( (.\test-sandbox.ps1) ) {
    write-warning "Sandbox is already running"
    cd $origdir
    return;
}

Write-Host Starting sandbox webserver...

#run this elevated
If (-Not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    $CommandLine = $MyInvocation.Line.Replace($MyInvocation.InvocationName, $MyInvocation.MyCommand.Definition)
    Start-Process -FilePath PowerShell.exe -Verb Runas -WorkingDirectory (pwd)  -ArgumentList "$CommandLine"
    cd $origdir
    return
}

# just start the sandbox server then.
start-process powershell .\webserver.ps1
