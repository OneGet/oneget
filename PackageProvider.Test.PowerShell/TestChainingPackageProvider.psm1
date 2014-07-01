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

	# Example: Get all the parameters for another provider
	$pm = $request.SelectProvider("NuGet")
	$NuGetOptions = $pm.DynamicOptions
	foreach( $do in $NuGetOptions ) {
		$name = $do.Name
		$cat = $do.Category
		$type = $do.Type
		$req = $do.Required

		#just write them out:
		write-debug "NuGetOption: $name $cat $type $req"
	}

	switch( $category ) {
	    Package {
			# options when the user is trying to specify a package 
			write-Output (New-DynamicOption $category "SS" SecureString $false )
		}

		Source {
			#options when the user is trying to specify a source
		}
		Install {
			#options for installation/uninstallation 
            write-Output (New-DynamicOption $category "Destination" Path $true )
			
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

function new-packagereference {
	param( 
		[string] $providerName,
		[string] $packageName,
		[string] $version,
		[string] $source
	)
	return "$providerName|$packageName|$version|$source"
}

function Find-Package { 
    param(
        [string[]] $names,
        [string] $requiredVersion,
        [string] $minimumVersion,
        [string] $maximumVersion
    )
	write-debug "In TestChainingPackageProvider - Find-Package"

	# SS was asked for as a SecureString.
	$ss = $request.Options["SS"]

	# check to see if we got a value first.
	if( $ss -eq $null ) {
		write-Debug "SS == null"
	} else {
		# unsecure - transforms a securestring into a string.
		$s = [System.Runtime.InteropServices.marshal]::PtrToStringAuto([System.Runtime.InteropServices.marshal]::SecureStringToBSTR($ss))
		
		#see! it works!
		write-Debug "SS == $s"
	}

	

	# dump-object $Request
	
	$providers = $request.SelectProvidersWithFeature("supports-powershellget-modules") 

	foreach( $pm in $providers) {
	    
		$providerName= $pm.Name

		write-Debug "working with provider $name"
		
		$mySrcLocation = "https://nuget.org/api/v2"

		foreach( $pkg in $pm.FindPackages( $names, $requiredVersion, $minimumVersion, $maximumVersion, (new-request -options @{ } -sources @( $mySrcLocation ) -Credential $c) ) ) {
			$fastPackageReference = (new-packagereference $providerName $pkg.Name $pkg.Version $pkg.Source )
			Write-Output (new-SoftwareIdentity $fastPackageReference  $pkg.Name $pkg.Version  $pkg.VersionScheme $mySrcLocation $pkg.Summary $providerName $pkg.FullPath $pkg.PackagePath )
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

function  To-Array {
	param( $ienumerator ) 
	foreach( $i in $ienumerator ) {
		$i
	}
}

function Get-First {
	param( $ienumerator ) 
	foreach( $i in $ienumerator ) {
		return $i
	}
	return null
}

function Install-Package { 
    param(
        [string] $fastPackageReference
    )
	write-debug "In TestChainingPackageProvider - Install-Package"

	# take the fastPackageReference and get the package object again.
	$parts = $fastPackageReference.Split('|' )

	if( $parts.Length -eq 4 ) {
		$providerName = $parts[0]
		$packageName = $parts[1]
		$version = $parts[2]
		$source= $parts[3]

		write-debug "In TestChainingPackageProvider - Find the chained package provider"
		
		$pm = $request.SelectProvider($providerName)

		write-debug "In TestChainingPackageProvider - recreate the software identity object from the name/version/source" 

		$pkgs = $pm.FindPackages( $packageName, $version, $null, $null, (new-request -sources @( $source ) ) ) 

		#pkgs returns an IEnumerator<SoftwareIdentity>, so let's get the first element

		$p = (To-Array $pkgs)

		write-debug "In TestChainingPackageProvider - 3"

		foreach( $pkg in $p ) {
			$installed = $pm.InstallPackage( $pkg , (new-request -options @{ "Destination" = $request.Options["Destination"] } -sources @( $source ) -Credential $c) )

			write-debug "In TestChainingPackageProvider - 4"

			foreach( $pkg in $installed ) {
				write-debug "In TestChainingPackageProvider - 5"
				Write-Output (new-SoftwareIdentity $fastPackageReference $pkg.Name $pkg.Version  $pkg.VersionScheme $source $pkg.Summary $providerName $pkg.FullPath $pkg.PackagePath )
			}
		}

		write-debug "In TestChainingPackageProvider - 6"

	} else {
		$x = $parts.Length
		Write-Error "BAD PACKAGE REFERENCE $fastPackageReference`n===== $parts=====`n $x"
	}

	write-debug "In TestChainingPackageProvider - 7"

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

