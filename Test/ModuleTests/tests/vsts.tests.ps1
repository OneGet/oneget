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
# Comment return statement below to run tests, then uncomment again when pushing to repo.

return

# Provide your own VSTS package source such as the default value below.
$VSTSsource = "https://msazure.pkgs.visualstudio.com/_packaging/MSNugetMirror/nuget/v3/index.json";
# Provide appropriate package name such as the default value below.
$packageName = "Microsoft.Kusto.Tools";
# Provide appropriate credentials for VSTS source when prompted.
# username: username
# password: personal access token
$credential = (Get-Credential);
$pkgSourceName = "MyRep";
$providerName = "NuGet";

Describe "VSTS Nuget Package Feed" {

    it "EXPECTED: Find a package from VSTS feed source" {

        $package = (find-package -name $packageName -source $VSTSsource -credential $credential) | Select-Object -First 1
        $package.Name | Should Be $packageName
        $package.Source | Should Be $VSTSsource
    }

    it "EXPECTED: Install a package from VSTS feed source" {

        $package = install-package -name $packageName -source $VSTSsource -credential $credential 
        $package.Name | Should Be $packageName
        $package.Source | Should Be $VSTSsource

        # Clean-up - uninstall the package 
        $package = Get-Package -Name $packageName -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
        if ($package)
        {
            uninstall-package -Name $packageName
        }
    }
    
    it "EXPECTED: Save a package from VSTS feed source" {

        $tempPath = [System.IO.Path]::GetFullPath($env:tmp)
        $package = save-package -name $packageName -source $VSTSsource -credential $credential -path $tempPath
        $package.Name | Should Be $packageName
        $package.Source | Should Be $VSTSsource

        $packagePath = Join-Path -path $tempPath -ChildPath $packageName
        (Get-ChildItem -Path "$packagePath*") | Should Not Be $null

        # Clean-up - delete saved papackage in temp path
        Remove-Item "$packagePath*"
    }

    it "EXPECTED: Register a VSTS feed as a package source" {
        $pkgSource = Register-PackageSource -name $pkgSourceName -location $VSTSsource -providerName $providerName -Credential $credential
        $pkgSource.Name | Should Be $pkgSourceName
        $pkgSource.ProviderName | Should Be $providerName
        $pkgSource.Location | Should Be $VSTSsource

        # Clean-up - unregister the package source
        $packageSource = Get-PackageSource -Name $pkgSourceName -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
        if ($packageSource)
        {
            $packageSource.Name | Should Be $pkgSourceName
            $packageSource.ProviderName | Should Be $providerName
            $packageSource.Location | Should Be $VSTSsource
            Unregister-PackageSource -Name $pkgSourceName
        }
	}
}

