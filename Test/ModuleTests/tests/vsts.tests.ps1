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
# ------------------ PackageManagement Test  ----------------------------------------------

# ------------------------------------------------------------------------------
# Actual Tests:

# This file contains tests for using find-package and install-package with
# VSTS NuGet package feed.  
# Provide a VSTS NuGet package source such as:
# "https://msazure.pkgs.visualstudio.com/_packaging/MSNugetMirror/nuget/v3/index.json"
# Provide approprite package name such as:
# "Microsoft.Kusto.Tools"
# Provide appropriate credentials (username and password) for VSTS source when prompted.
# Comment return statement below to run tests, then uncomment again when pushing to repo.

return

try {
    $WindowsPowerShell = $PSHOME.Trim('\').EndsWith('\WindowsPowerShell\v1.0', [System.StringComparison]::OrdinalIgnoreCase)    
    $Runtime = [System.Runtime.InteropServices.RuntimeInformation]
    $OSPlatform = [System.Runtime.InteropServices.OSPlatform]

    $IsCoreCLR = $true
    $IsLinux = $Runtime::IsOSPlatform($OSPlatform::Linux)
    $IsOSX = $Runtime::IsOSPlatform($OSPlatform::OSX)
    $IsWindows = $Runtime::IsOSPlatform($OSPlatform::Windows)
} catch {
    # If these are already set, then they're read-only and we're done
    try {
        $IsCoreCLR = $false
        $IsLinux = $false
        $IsOSX = $false
        $IsWindows = $true
        $WindowsPowerShell = $true
    }
    catch { }
}

$VSTSsource = "";
$packageName = "";
$credential = (Get-Credential);
$pkgSourceName = "MyRep";
$providerName = "NuGet";

Describe "VSTS Nuget Package Feed" {

    it "EXPECTED: Find a package from VSTS feed source" {

        $packages = find-package -name $packageName -source $VSTSsource -credential $credential 
        $ERROR[0].FullyQualifiedErrorId | Should Not Be "No match was found for the specified search criteria and package name"
        $packages.Name | Should Be $packageName
        $packages.Source | Should Be $VSTSsource
    }

    it "EXPECTED: Install a package from VSTS feed source" {

        $packages = install-package -name $packageName -source $VSTSsource -credential $credential 
        $ERROR[0].FullyQualifiedErrorId | Should Not Be "No match was found for the specified search criteria and package name"
        $packages.Name | Should Be $packageName
        $packages.Source | Should Be $VSTSsource

        # Clean-up - uninstall the package 
        $package = Get-Package -Name $packageName -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
        if ($package)
        {
            uninstall-package -Name $packageName
        }
    }
    
    it "EXPECTED: Save a package from VSTS feed source" {

        $packages = save-package -name $packageName -source $VSTSsource -credential $credential -path . #create temp path
        $packages.Name | Should Be $packageName
        $packages.Source | Should Be $VSTSsource
    }

    it "EXPECTED: Register a package source from VSTS feed" {
        $pkgSource = Register-PackageSource -name $pkgSourceName -location $VSTSsource -providerName $providerName -Credential $credential
        $pkgSource.Name | Should Be $pkgSourceName
        $pkgSource.ProviderName | Should Be $providerName
        $pkgSource.Location | Should Be $VSTSsource

        # Clean-up - unregister the package source
        $packageSource = Get-PackageSource -Name $pkgSourceName -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
        if ($packageSource)
        {
            Unregister-PackageSource -Name $pkgSourceName
        }
	}
}

