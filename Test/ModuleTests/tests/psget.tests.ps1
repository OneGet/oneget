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


# ------------------------------------------------------------------------------
# Actual Tests:

Describe "PSGet Message Resolver" -Tags @('BVT', 'DRT'){
    # make sure that packagemanagement is loaded
    import-packagemanagement


    It "Changes message" {
        # bootstrap nuget
        
        if ($PSCulture -eq "en-US" ) { 
            get-packageprovider nuget -force
            
            $msg = powershell 'find-module -repository asdasdasd -ea silentlycontinue ; $ERROR[0].Exception.Message'
            $msg | Should match 'PSRepository' 
            $msg | Should not match 'package' 
        }
    }
}

Describe "Set-PackageSource" -Tags @('BVT', 'DRT'){
    # make sure that packagemanagement is loaded
    import-packagemanagement

	It "EXPECTED: -FAILS- To rename package source but does not remove the old source" -Skip{
        try {

            $a=(Get-PackageSource -ErrorAction Ignore -WarningAction Ignore).Location
            foreach($item in $a)
            {                
                if($item -match "https://mytestgallery.cloudapp.net/api/v2/")
                {
                    Unregister-PackageSource -Name $item
                }                
            }

            Register-PackageSource -Name internal -Location https://mytestgallery.cloudapp.net/api/v2/ -ProviderName PSModule
            $msg = powershell 'set-packagesource -name internal -newname internal2 -ea silentlycontinue ; $Error[0].FullyQualifiedErrorId'
            $msg | should match "RepositoryAlreadyRegistered,Add-PackageSource,Microsoft.PowerShell.PackageManagement.Cmdlets.SetPackageSource"
            $package = Get-PackageSource -Name internal
            $package.Name | should match 'internal'
            $package.ProviderName | should match 'PowerShellGet'
        }
        finally {
            Unregister-PackageSource -Name internal
        }
    }

    It "EXPECTED: -FAILS- when Set-PackageSource raises an error but does not remove the old source" -Skip {
        (Get-PackageSource -Name PSGallery).Count | should be 1
        $msg = powershell 'Set-PackageSource -Name PSGallery -ProviderName PowerShellGet -PackageManagementProvider PowerShellGet -ErrorAction SilentlyContinue; $Error[0].FullyQualifiedErrorId'
        $msg | should match "InvalidPackageManagementProviderValue,Add-PackageSource,Microsoft.PowerShell.PackageManagement.Cmdlets.SetPackageSource"
        (Get-PackageSource -Name PSGallery).Count | should be 1
    }
}

Describe Uninstall-Package -Tags @('BVT', 'DRT'){
	# make sure packagemanagement is loaded
	import-packagemanagement

    It "E2E: Uninstall all versions of a specific package - PowerShellGet provider" -skip {
        $packageName = "ContosoServer"
        $provider = "PowerShellGet"
        $packageSourceName = "InternalGallery"
        $internalGallerySource = "https://mytestgallery.cloudapp.net/api/v2/"

        #make sure the package repository exists
        $packageSource = Get-PackageSource -ForceBootstrap | Select-Object Location, ProviderName
    
        $found = $false
        foreach ($item in $packageSource)
        {            
            if(($item.ProviderName -eq $provider) -and ($item.Location -eq $internalGallerySource))
            {
                $found = $true
                break
            }
        }

        if(-not $found)
        {
            Register-PackageSource -Name $packageSourceName -Location $internalGallerySource -ProviderName $provider -Trusted -ForceBootstrap
        }

        # Find all versions for specified module
        ($foundPackages = Find-Package -Name $packageName -Provider $provider -Source $internalGallerySource -AllVersions)        

        # Install the found versions of the package
        foreach ($package in $foundPackages) 
        {
            ($package | Install-Package -Force)
        }

        # Uninstall all versions of the package
        Uninstall-Package -Name $packageName -Provider $provider -AllVersions		
        
        # Get-Package must not return any packages - since we just uninstalled allversions of the package
        $msg = powershell 'Get-Package -Name ContosoServer -Provider PowerShellGet -AllVersions -warningaction:silentlycontinue -ea silentlycontinue; $ERROR[0].FullyQualifiedErrorId'
        $msg | should be "NoMatchFound,Microsoft.PowerShell.PackageManagement.Cmdlets.GetPackage" 
        
        # Clean-up - Unregister the Package source
        $packageSource = Get-PackageSource -Name $packageSourceName -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
        if ($packageSource)
        {
            Unregister-PackageSource -Name $packageSourceName
        }
    }
}
