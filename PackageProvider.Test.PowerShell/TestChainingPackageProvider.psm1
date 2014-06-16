###
# ==++==
#
# Copyright (c) Microsoft Corporation. All rights reserved. 
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
# 
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# 
###
function Get-PackageProviderName { 
    return "TestChainingPackageProvider"
}

function Get-Feature { 
    param()
	# Metadata that describes what this provider is used for.
    write-debug "In TestChainingPackageProvider - get-feature"

	#advertise this is not for user-consumption
 	# write-Output (new-feature "automation-only" )

	#advertise which file extensions we support
	write-Output (new-feature "extensions" @("nupkg") )
}

function Get-DynamicOptions { 
    param(
        [Microsoft.OneGet.MetaProvider.PowerShell.OptionCategory] $category
    )
    write-debug "In TestChainingPackageProvider - Get-DynamicOption for category $category"

	switch( $category ) {
	    Package {
			# options when the user is trying to specify a package 
		}

		Source {
			#options when the user is trying to specify a source
		}
		Install {
			#options for installation/uninstallation 
            write-Output (New-DynamicOption $category "Destinination" File $true )
		}
	}
}

function Add-PackageSource { 
    param(
        [string] $name, 
        [string] $location, 
        [bool] $trusted
    )  
	write-debug "In TestChainingPackageProvider - Add-PackageSource"
}

function Remove-PackageSource { 
    param(
        [string] $name
    )
	write-debug "In TestChainingPackageProvider - Remove-PackageSource"
}

function Dump-object {
	param(
        [object] $obj
    )
	
	$x = ""
	foreach( $m in (Get-Member -InputObject $obj ) ) {
		$x = $x + "`r`n    $m"
	}

	write-debug "{0}" $x
}

function Find-Package { 
    param(
        [string[]] $names,
        [string] $requiredVersion,
        [string] $minimumVersion,
        [string] $maximumVersion
    )

	$providers = $request.SelectProvidersWithFeature("supports-powershell-get") 

	foreach( $pm in $providers) {
		$name = $pm.Name
		write-Debug "working with $name"
		
		$myReq  =  $request.RemoteThis;

		if($myReq -eq $null ) {
			write-Debug "IT IS NULL===================="
		}

		foreach( $pkg in $pm.FindPackages( $names, $requiredVersion, $minimumVersion, $maximumVersion, $myReq ) ) {
			Dump-object $pkg

			#Write-Output (new-SoftwareIdentity "pkgid:2"  $pkg. "1.0"  "semver"  "local"  "this is package 2" $name )
		}


	}



	write-debug "In TestChainingPackageProvider - Find-Package"
}

function Find-PackageByFile { 
    param(
        [string[]] $files
    )
    write-debug "In TestChainingPackageProvider - Find-PackageByFile"
}

function Find-PackageByUri { 
    param(
        [Uri[]] $uris
    )
	write-debug "In TestChainingPackageProvider - Find-PackageByUri"
}


function Get-InstalledPackage { 
    param(
        [string] $name
    )
    write-debug "In TestChainingPackageProvider - Get-InstalledPackage {0} {1}" $InstalledPackages.Count $name
}

function Get-PackageSource { 
    param()
	write-debug "In TestChainingPackageProvider - Find-GetPackageSources"
}

function Initialize-Provider { 
    param()
    write-debug "In TestChainingPackageProvider - Initialize-Provider"
}

function Install-Package { 
    param(
        [string] $fastPackageReference
    )
	write-debug "In TestChainingPackageProvider - Install-Package"
}

function Is-TrustedPackageSource { 
    param(
        [string] $packageSource
    )
    write-debug "In TestChainingPackageProvider - Is-TrustedPackageSource"

	return false;
}

function Is-ValidPackageSource { 
    param(
        [string] $packageSource
    )
	write-debug "In TestChainingPackageProvider - Is-ValidPackageSource"
	return false;
}

function Uninstall-Package { 
    param(
        [string] $fastPackageReference
    )
    write-debug "In TestChainingPackageProvider - Uninstall-Package"
}


function Download-Package { 
    param(
        [string] $fastPackageReference,
        [string] $location
    )
    write-debug "In TestChainingPackageProvider - Download-Package"
}

function Get-PackageDependencies { 
    param(
        [string] $fastPackageReference
    )
    write-debug "In TestChainingPackageProvider - Get-PackageDependencies"
}

function Get-PackageDetail { 
    param(
        [string] $fastPackageReference
    )
    # NOT_IMPLEMENTED_YET
}

