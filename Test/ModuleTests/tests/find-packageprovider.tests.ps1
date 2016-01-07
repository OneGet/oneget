﻿#
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
# ------------------ PackageManagement Test  -----------------------------------
ipmo "$PSScriptRoot\utility.psm1"
$InternalGallery = "https://www.PowerShellGallery.com/api/v2/"

# ------------------------------------------------------------------------------
# Actual Tests:

Describe "find-packageprovider" -Tags @('BVT', 'DRT'){
    # make sure that packagemanagement is loaded
    import-packagemanagement

    #make sure the package repository exists
    $a=Get-PackageSource  -force| select Location, ProviderName
    
    $found = $false
    foreach ($item in $a)
    {       
        #name contains "." foo.bar for example for the registered sources internally
        if(($item.ProviderName -eq "PSModule") -and ($item.Location -eq $InternalGallery))
        {
            $found = $true
            break
        }
    }

    if(-not $found)
    {
        Register-PackageSource -Name 'OneGetTestSource' -Location $InternalGallery -ProviderName 'PSModule' -ForceBootstrap
    }

    It "find-packageprovider without any parameters, Expect succeed" {
        $a = (Find-PackageProvider -force).name 
        $a -contains "TSDProvider" | should be $true
        $a -contains "nuget" | should be $true
    }
    
    It "find-packageprovider -name, Expect succeed" {
        $a = (Find-PackageProvider -name nuget).name 
        $a -contains "GistProvider" | should be $false
        $a -contains "nuget" | should be $true
    }
            
    It "find-packageprovider -name with wildcards, Expect succeed" {
        $a = (Find-PackageProvider -name gist*).name 
        $a -contains "GistProvider" | should be $true
        $a -contains "nuget" | should be $false
    }

    It "find-packageprovider -name with wildcards, Expect succeed" {
        $a = (Find-PackageProvider -name nu*).name 
        $a -contains "GistProvider" | should be $false
        $a -contains "nuget" | should be $true
    }

    It "find-packageprovider -name array, Expect succeed" {
        $names=@("gistprovider", "TSD*")

        $a = (Find-PackageProvider -name $names).name 
        $a -contains "GistProvider" | should be $true
        $a -contains "TSDProvider" | should be $true
    }  
    
    It "find-packageprovider -allversions, Expect succeed" {
        $a = (Find-PackageProvider -allversions)
         
        $a.Name -contains "TSDProvider" | should be $true
        $a.Name -contains "nuget" | should be $true
        $a.Count -gt 1 | should be $true
    }

    It "find-packageprovider -name -allversions, Expect succeed" {
        $a = (Find-PackageProvider -name TSDProvider -AllVersions).name 
        $a -contains "TSDProvider" | should be $true

        $b = (Find-PackageProvider -name TSDP* -AllVersions).name 
        $b.Count -ge $a.Count | should be $true
    }      


    It "EXPECTED: success 'find-packageprovider nuget -allVersions'" {
        $a = find-packageprovider -name nuget -allVersions  
        $a.Count -ge 5| should be $true

        $b = find-packageprovider -allVersions 
        $b.Count -gt $a.Count| should be $true
    }

    It "find-packageprovider -Source, Expect succeed" {
        $a = (Find-PackageProvider -source $InternalGallery).name 
        $a -contains "TSDProvider" | should be $true
        $a -contains "nuget" | should be $false
    }

    It "find-packageprovider -Source -Name, Expect succeed" {
        $a = (Find-PackageProvider -name gistprovider -source $InternalGallery).name 
        $a -contains "gistprovider" | should be $true
        $a -contains "nuget" | should be $false
    }
    It "find-packageprovider -Name with dependencies, Expect succeed" -Skip {
        # gistprovider 1.5 depends on tsdprovider 0.2
        $a = (Find-PackageProvider -name gistprovider -RequiredVersion 1.5 -source $InternalGallery -IncludeDependencies) 
        $a.Name -contains "gistprovider" | should be $true
        $a.Name -contains "tsdprovider" | should be $true
    }
    
   It "find-install-packageprovider with PowerShell provider, Expect succeed" {
        $provider= find-packageprovider -name TSDProvider -MinimumVersion 0.1 -MaximumVersion 0.2  -Source $InternalGallery 
        $provider | ?{ $_.Version -eq "0.2" } | should not BeNullOrEmpty

        $a=install-packageprovider -name TSDProvider -MinimumVersion 0.1  -MaximumVersion 0.2 -Force -Source $InternalGallery 
        $a.Name | should match "TSDProvider"
        $a.Version | should match "0.2"
    }

   It "find-install-packageprovider nuget, Expect succeed" {
        $provider= find-packageprovider -name nuget -MinimumVersion 2.8.5.1 -MaximumVersion 2.8.5.123

        $provider | ?{ $_.Version -eq "2.8.5.122" } | should not BeNullOrEmpty
        $provider.Count -eq 1 | should be $true

        $a=install-packageprovider -name nuget -MinimumVersion 2.8.5.1 -MaximumVersion 2.8.5.123 -Force
        $a.Name | should match "nuget"
        $a.Version | should match "2.8.5.122"

        $b= Get-PackageProvider -ListAvailable
        $b | ?{ $_.Version -eq "2.8.5.122" } | should not BeNullOrEmpty
    }
   
 }
    

Describe "Find-PackageProvider with Versions" -Tags @('BVT', 'DRT') {
    # make sure that packagemanagement is loaded
    import-packagemanagement
    <# Nuget
    2.8.5.127
    2.8.5.122
    2.8.5.120
    2.8.5.101
    2.8.5.24#>

    It "EXPECTED: success 'Find a provider  -requiredVersion 3.5'" {
        (find-packageprovider -name Nuget -requiredVersion 2.8.5.122).Version.ToString() | should match "2.8.5.122"
    }
 

    It "EXPECTED: success 'find a provider with MinimumVersion and MaximumVersion'" {
        (find-packageprovider -name nuget -MinimumVersion 2.8.5.105 -MaximumVersion 2.8.5.123).Version.ToString() | should match "2.8.5.122"
    }
    
    It "EXPECTED: success 'find a provider with MaximumVersion'" {
        (find-packageprovider -name nuget -MaximumVersion 2.8.5.122).Version -contains "2.8.5.122" | should be $true
    }
}    



Describe "find-packageprovider Error Cases" -Tags @('BVT', 'DRT') {
    # make sure that packagemanagement is loaded
    import-packagemanagement

  AfterAll {
        $x =Get-PackageSource -Name OneGetTestSource -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
        if($x)
        {
            Unregister-PackageSource -Name OneGetTestSource
        }
  }

   It "EXPECTED:  returns an error when inputing a bad version format" {
        $Error.Clear()
        $msg =  powershell 'find-packageprovider -name Gistprovider -RequiredVersion BOGUSVERSION  -warningaction:silentlycontinue -ea silentlycontinue; $ERROR[0].FullyQualifiedErrorId'
        $msg | should be "InvalidVersion,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackageProvider"
    }

    
   It "EXPECTED:  returns an error when asking for a provider that does not exist" {
        $Error.Clear()
        $msg =  powershell 'find-packageprovider -name NOT_EXISTS  -warningaction:silentlycontinue -ea silentlycontinue; $ERROR[0].FullyQualifiedErrorId'
        $msg | should be "NoMatchFoundForCriteria,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackageProvider"
    }
 

   It "EXPECTED:  returns an error when asking for a provider with RequiredVersoin and MinimumVersion" {
        $Error.Clear()
        $msg = powershell 'find-packageprovider -name NOT_EXISTS -RequiredVersion 1.0 -MinimumVersion 2.0  -warningaction:silentlycontinue -ea silentlycontinue; $ERROR[0].FullyQualifiedErrorId'
        $msg | should be "VersionRangeAndRequiredVersionCannotBeSpecifiedTogether,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackageProvider"
    }

   It "EXPECTED:  returns an error when asking for a provider with RequiredVersoin and MaximumVersion" {
        $Error.Clear()
        $msg = powershell 'find-packageprovider -name NOT_EXISTS -RequiredVersion 1.0 -MaximumVersion 2.0  -warningaction:silentlycontinue -ea silentlycontinue;$ERROR[0].FullyQualifiedErrorId'
        $msg | should be "VersionRangeAndRequiredVersionCannotBeSpecifiedTogether,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackageProvider"
    }

   It "EXPECTED:  returns an error when asking for a provider with a MinimumVersion greater than MaximumVersion" {
        $Error.Clear()
        $msg = powershell 'find-packageprovider -name nuget -MaximumVersion 1.0 -MinimumVersion 2.0 -warningaction:silentlycontinue -ea silentlycontinue;$ERROR[0].FullyQualifiedErrorId'
        $msg | should be "NoMatchFoundForCriteria,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackageProvider"
    }

   It "EXPECTED:  returns an error when asking for a provider with MinimumVersion that does not exist" {
        $Error.Clear()
        $msg = powershell 'find-packageprovider -name gistprovider -MinimumVersion 20.2 -warningaction:silentlycontinue -ea silentlycontinue;$ERROR[0].FullyQualifiedErrorId'
        $msg | should be "NoMatchFoundForCriteria,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackageProvider"
    }

   It "EXPECTED:  returns an error when asking for a provider with MaximumVersion that does not exist" {
        $Error.Clear()
        $msg = powershell 'find-packageprovider -name gistprovider -MaximumVersion 0.1 -warningaction:silentlycontinue -ea silentlycontinue;$ERROR[0].FullyQualifiedErrorId'
        $msg | should be "NoMatchFoundForCriteria,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackageProvider"
    }

   It "EXPECTED:  returns an error when asking for a provider that has name with wildcard and version" {
        $Error.Clear()
        $msg = powershell 'find-packageprovider -name "AnyName*" -RequiredVersion 4.5 -warningaction:silentlycontinue -ea silentlycontinue;$ERROR[0].FullyQualifiedErrorId'
        $msg | should be "MultipleNamesWithVersionNotAllowed,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackageProvider"
    }  
     
   It "EXPECTED:  returns an error when asking for a provider that has name with wildcard and version" {
        $Error.Clear()
        $msg = powershell 'find-packageprovider -name "AnyName" -RequiredVersion 4.5 -allVersions -warningaction:silentlycontinue -ea silentlycontinue;$ERROR[0].FullyQualifiedErrorId'
        $msg | should be "AllVersionsCannotBeUsedWithOtherVersionParameters,Microsoft.PowerShell.PackageManagement.Cmdlets.FindPackageProvider"
   }      
}


