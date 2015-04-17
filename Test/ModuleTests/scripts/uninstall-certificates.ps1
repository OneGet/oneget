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

#run this elevated
If (-Not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    $CommandLine = $MyInvocation.Line.Replace($MyInvocation.InvocationName, $MyInvocation.MyCommand.Definition)
    Start-Process -FilePath PowerShell.exe -Verb Runas -Wait -WorkingDirectory (pwd)  -ArgumentList "$CommandLine"
    cd $origdir
    return
}

# aribtrary app id
$appid = '{df8c8073-5a4b-4810-b469-5975a9c95230}'

# which servers to fake out
$Servers = @( "pmtestcert","www.google.com","google.com","go.microsoft.com","nuget.org","www.nuget.org","microsoft.com","localhost","chocolatey.org","www.chocolatey.org","oneget.org","www.oneget.org","*.com","*.org","*.net","127.0.0.1","*" )


#unbind the SSL certificates
$null = ($Servers | foreach { $v ="$_"+":443" ; netsh http delete sslcert hostnameport=$v  })
$null = (netsh http delete sslcert ipport=127.0.0.1:443)

# remove the certs from the store.
dir "Cert:\LocalMachine\my\" | Where-Object Subject -eq "CN=pmtestcert" | erase
dir "Cert:\LocalMachine\root\" | Where-Object Subject -eq "CN=pmtestcert" | erase

Write-Host "Done removing test certificates"

cd $origdir
return
