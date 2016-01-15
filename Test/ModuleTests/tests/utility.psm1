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

<#
.SYNOPSIS
Removes all installed PackageManagement providers from:
    $Env:ProgramFiles/PackageManagement/ProviderAssemblies
    $Env:LocalAppData/PackageManagement/ProviderAssemblies

#>
function Remove-AllPackageManagementProviders {
	rm $Env:ProgramFiles/PackageManagement/ProviderAssemblies/*.exe
	rm $Env:LocalAppData/PackageManagement/ProviderAssemblies/*.exe
	rm $Env:ProgramFiles/PackageManagement/ProviderAssemblies/*.dll
	rm $Env:LocalAppData/PackageManagement/ProviderAssemblies/*.dll
}


<#
.SYNOPSIS
Checks to see if the PackageManagement Module is loaded.

#>
function Test-IsPackageManagementLoaded {
    if( get-module -name PackageManagement ) {
        return $true
    }

    return $false
}

<#
.SYNOPSIS
Imports the PackageManagement Module (using the environment variable to select the right one)
#>
function Import-PackageManagement {
    <#
        PackageManagement tests should have the $moduleLocation set by the calling script
        otherwise it will use the default (loading PackageManagement from the PSModulePath)
    #>

    if (-not $env:PMModuleTest ) {
        $env:PMModuleTest = "PackageManagement"
    }

    echo "Importing PackageManagement Module from $env:PMModuleTest"
    ipmo $env:PMModuleTest
    return $true
}

Export-ModuleMember *
