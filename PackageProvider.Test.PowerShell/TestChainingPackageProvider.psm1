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
	write-debug "In TestChainingPackageProvider - Find-Package"

	dump-object $Request
	
	$providers = $request.SelectProvidersWithFeature("supports-powershellget-modules") 

	foreach( $pm in $providers) {
	    
		dump-object $pm 

		$name = $pm.Name
		write-Debug "working with $name"
		
		$mySrcLocation = "https://nuget.org/api/v2"

		foreach( $pkg in $pm.FindPackages( $names, $requiredVersion, $minimumVersion, $maximumVersion, (new-request -options @{ } -sources @( $mySrcLocation ) -Credential $c) ) ) {
			$fastPackageReference = $pkg.Name+$mySrcLocation
			Write-Output (new-SoftwareIdentity $fastPackageReference  $pkg.Name $pkg.Version  $pkg.VersionScheme $mySrcLocation $pkg.Summary $name $pkg.FullPath $pkg.PackagePath )
		}
	}
}

function Find-PackageByFile { 
    param(
        [string[]] $files
    )
    write-debug "In TestChainingPackageProvider - Find-PackageByFile"

	# return values with
	# write-output  (new-SoftwareIdentity "fastPackageReference"  "package-name" "package-version" "multipartnumeric" "source_name_or_location" "summary" "searchkey" "filename-of-the-package" "full-path-of-the-package-or-installed-location" )

}

function Find-PackageByUri { 
    param(
        [Uri[]] $uris
    )
	write-debug "In TestChainingPackageProvider - Find-PackageByUri"

	# return values with
	# write-output  (new-SoftwareIdentity "fastPackageReference"  "package-name" "package-version" "multipartnumeric" "source_name_or_location" "summary" "searchkey" "filename-of-the-package" "full-path-of-the-package-or-installed-location" )
}


function Get-InstalledPackage { 
    param(
        [string] $name
    )
    write-debug "In TestChainingPackageProvider - Get-InstalledPackage {0} {1}" $InstalledPackages.Count $name

	# return values with
	# write-output  (new-SoftwareIdentity "fastPackageReference"  "package-name" "package-version" "multipartnumeric" "source_name_or_location" "summary" "searchkey" "filename-of-the-package" "full-path-of-the-package-or-installed-location" )

}

function Resolve-PackageSource { 
    param()
	write-debug "In TestChainingPackageProvider - Resolve-PackageSource"

	# get requested set from $request.Sources

	# return values with
	# write-output  (New-PackageSource "sourcename" "location" <istrusted> <isregistered> @{ <hashtable-of-details> })
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

		# return values with
	# write-output  (new-SoftwareIdentity "fastPackageReference"  "package-name" "package-version" "multipartnumeric" "source_name_or_location" "summary" "searchkey" "filename-of-the-package" "full-path-of-the-package-or-installed-location" )

}


function Uninstall-Package { 
    param(
        [string] $fastPackageReference
    )
    write-debug "In TestChainingPackageProvider - Uninstall-Package"
	# return values with
	# write-output  (new-SoftwareIdentity "fastPackageReference"  "package-name" "package-version" "multipartnumeric" "source_name_or_location" "summary" "searchkey" "filename-of-the-package" "full-path-of-the-package-or-installed-location" )

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

	# return values with
	# write-output  (new-SoftwareIdentity "fastPackageReference"  "package-name" "package-version" "multipartnumeric" "source_name_or_location" "summary" "searchkey" "filename-of-the-package" "full-path-of-the-package-or-installed-location" )

}

function Get-PackageDetail { 
    param(
        [string] $fastPackageReference
    )
    # NOT_IMPLEMENTED_YET
}

