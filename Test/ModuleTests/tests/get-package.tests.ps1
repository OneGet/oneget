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
ipmo "$PSScriptRoot\utility.psm1"

$nuget = "nuget"
$source = "http://www.nuget.org/api/v2/"
$destination = "$env:tmp\GetPackageTests"

# Actual Tests:

Describe "Get-package" -Tags @('BVT', 'DRT'){
    # make sure that packagemanagement is loaded
    import-packagemanagement

    It "EXPECTED: Get-package accepts array of strings for -providername parameter" {
        $x = (get-package -providername Programs,Msi)
    }
}

Describe "Get-package with version parameter  - valid scenarios" -Tags @('BVT', 'DRT'){
    # make sure that packagemanagement is loaded
    import-packagemanagement

    It "Get-package supports -AllVersions parameter" {
        $outputWithAllVersions = (Get-Package -providername Programs,Msi -AllVersions)
        $outputWithoutAllVersions = (Get-Package -providername Programs,Msi)
        $outputWithAllVersions.count -ge $outputWithoutAllVersions.count | should be $true

    }

    It "E2E: Get-package supports -AllVersions parameter for a specific package - with multiple versions from Nuget" {
        ($foundPackages = Find-Package -Name "adept.nugetrunner" -Provider $nuget -Source $source -AllVersions)        

        foreach ($package in $foundPackages) 
        {
            ($package | Install-Package -Destination $destination -Force)
        }
		
        $installedPackages = (Get-Package -Name "adept.nugetrunner" -Provider $nuget -Destination $destination -AllVersions)
        $installedPackages.Name | should be "adept.nugetrunner"
        $installedPackages.Count -eq $foundPackages.Count | should be $true        
        
		if (Test-Path $destination\adept.nugetrunner*) {
			(Remove-Item -Recurse -Force -Path $destination\adept.nugetrunner*)
		}
    }    
}

Describe "Get-package with version parameter - Error scenarios" -Tags @('BVT', 'DRT'){
    # make sure that packagemanagement is loaded
    import-packagemanagement

    It "Get-package -AllVersions -- Cannot be used with other version parameters" {
        $Error.Clear()
        $msg = powershell 'Get-Package -AllVersions -RequiredVersion 1.0 -MinimumVersion 2.0  -warningaction:silentlycontinue -ea silentlycontinue; $ERROR[0].FullyQualifiedErrorId'
        $msg | should be "AllVersionsCannotBeUsedWithOtherVersionParameters,Microsoft.PowerShell.PackageManagement.Cmdlets.GetPackage"

    }

    It "Get-package -RequiredVersion -- Cannot be used with Min/Max version parameters" {
        $Error.Clear()
        $msg = powershell 'Get-Package -RequiredVersion 1.0 -MinimumVersion 2.0 -MaximumVersion 3.0 -warningaction:silentlycontinue -ea silentlycontinue; $ERROR[0].FullyQualifiedErrorId'
        $msg | should be "VersionRangeAndRequiredVersionCannotBeSpecifiedTogether,Microsoft.PowerShell.PackageManagement.Cmdlets.GetPackage"

    }
}