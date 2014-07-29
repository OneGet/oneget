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


$InstalledPackages = (new-Object System.Collections.ArrayList)
$InstalledPackages.Add( ( new-SoftwareIdentity  "pkgid:X"  "DefaultInstalledX" "1.0"  "semver"  "local"  "this is package X" $null ) )
$InstalledPackages.Add( ( new-SoftwareIdentity  "pkgid:Y"  "DefaultInstalledY" "1.0"  "semver"  "local"  "this is package Y" $null ) )
$InstalledPackages.Add( ( new-SoftwareIdentity  "pkgid:Z"  "DefaultInstalledZ" "1.0"  "semver"  "local"  "this is package Z" $null ) )

$RegisteredPackageSources = (new-Object System.Collections.ArrayList)
$RegisteredPackageSources.Add( 	(New-PackageSource "source1" "http://nowhere.com/test" $true $true $false @{ "color" = "green" }) )
$RegisteredPackageSources.Add( 	(New-PackageSource "source2" "http://nowhere.com/test/untrusted" $false $true $false))
$RegisteredPackageSources.Add( 	(New-PackageSource "source3" "http://nowhere.com/test2" $true $true $false))

<# 

#>
function Add-PackageSource { 
    param(
        [string] $name, 
        [string] $location, 
        [bool] $trusted
    )  
	write-debug "In TestPackageProvider - Add-PackageSource"

	# remove any existing object first.
	for($i=$RegisteredPackageSources.Count; $i -gt 0; $i--) {
		$src = $RegisteredPackageSources[$i-1]
		if( $src.Name -eq $name )  {
			$RegisteredPackageSources.Remove( $src )
		}
	}

	# create a new one
	$src = (new-PackageSource $name $location $trusted $true $false)

	#add it to our stored list.
	$RegisteredPackageSources.Add( $src )

	# return the source to the caller.
	write-Output $src 
}

<# 

#>
function Remove-PackageSource { 
    param(
        [string] $name
    )
	write-debug "In TestPackageProvider - Remove-PackageSource"

    for($i=$RegisteredPackageSources.Count; $i -gt 0; $i--) {
		$src = $RegisteredPackageSources[$i-1]
		if( $src.Name -eq $name  -or $src.Location -eq $name )  {
			write-debug "Removing {0} {1}" $src.Name $src.Location
			$RegisteredPackageSources.Remove( $src )
			write-Output $src
		}
	}
}


<# 

#>  
function Find-Package { 
    param(
        [string[]] $names,
        [string] $requiredVersion,
        [string] $minimumVersion,
        [string] $maximumVersion
    )

	write-debug "In TestPackageProvider - Find-Package"
	foreach( $name in $names ) {
			#$fastPackageReference, $name, $version,  $versionScheme,  $source,  $summary,  $searchKey = $null, $details 
		if( $name -eq "single" ) {
			Write-Output (new-SoftwareIdentity "pkgid:1"  "PackageOne" "1.0"  "semver"  "local"  "this is package 1" $name )
			continue;
		}
		if( $name -eq "multiple" ) {
			Write-Output (new-SoftwareIdentity "pkgid:2"  "PackageTwo" "1.0"  "semver"  "local"  "this is package 2" $name )
			Write-Output (new-SoftwareIdentity "pkgid:3"  "PackageThree" "1.0"  "semver"  "local"  "this is package 3" $name )
			Write-Output (new-SoftwareIdentity "pkgid:4"  "PackageFour" "1.0"  "semver"  "local"  "this is package 4" $name )
			continue;
		} 
		Write-Output (new-SoftwareIdentity "pkgid:$name"  "Package$name" "1.0"  "semver"  "local"  "this is package $name" $name )
	}
}

<#  

#>
function Find-PackageByFile { 
    param(
        [string[]] $files
    )
    write-debug "In TestPackageProvider - Find-PackageByFile"
	$i = 0
	foreach( $file in $files ) {
		$i++;
		Write-Output (new-SoftwareIdentity "pkgid:$i"  "Package$i" "1.0"  "semver"  "local"  "this is package $i" $file )
	}
}

<# 

#>
function Find-PackageByUri { 
    param(
        [Uri[]] $uris
    )
	write-debug "In TestPackageProvider - Find-PackageByUri"
	$i = 0
	foreach( $uri in $uris ) {
		$i++;
		Write-Output (new-SoftwareIdentity "pkgid:$i"  "PackageByUri$i" "1.0"  "semver"  "local"  "this is package byuri $i" $uri )
	}
}


<# 

#>
function Get-InstalledPackage { 
    param(
        [string] $name
    )
    write-debug "In TestPackageProvider - Get-InstalledPackage {0} {1}" $InstalledPackages.Count $name


	# all packages
	if( $name -eq $null -or $name -eq "" ) {
		foreach( $pkg in $InstalledPackages ) {
			# Write-Debug "Returning installed package {0}" $pkg.Name
			Write-output $pkg
		}
	}

	else {
		# a specific packaeg
		foreach( $pkg in $InstalledPackages ) {
			if( $pkg.Name -eq $name ) {
				Write-output $pkg
			}
		}
	}
}

<# 
	Returns the name of the Provider. Doesn't need callback .
#>
function Get-PackageProviderName { 
    param()
    return "TestPSProvider"
}

<# 

#>
function Resolve-PackageSource { 
    param()
   
	write-debug "In TestPackageProvider - Resolve-PackageSources"
    
	$srcs = $request.GetSources();

	if( $srcs -eq $null -or $srcs.Length -eq 0 ) {
		# if there is nothing passed in, 
		# just return all the known package sources
		foreach( $src in $RegisteredPackageSources )  {
			write-debug "Writing out a package source {0}" $src.Name
			Write-Output $src
		}
		return;
	}
	
	foreach ($each in $srcs) {
		$found = $false
		# otherwise, for each item, check
			# if we have a source by that name or location, return it.
			foreach( $src in $RegisteredPackageSources ) {
				if( $each -eq $src.Name ){
					Write-Output $src
					$found = $true
					break;
				}
			}

			if( $found ) {
				continue;
			}


			# or, is that string a valid source, return it as an 'untrusted' source
			if( $each.ToLower().StartsWith("http://") -and $each.IndexOf("test") -gt -1 ) {
				Write-Output (New-PackageSource $each $each $false $false $false)
				continue;
			}

			# if it's not a valid source location send a warning back.
			# $request.Warning(" '{0}' does not represent a valid source", $each);
			write-warning " '$each' does not represent a valid source"

	}

	
	write-debug "Done In TestPackageProvider - Get-PackageSources"
}

<# 

#>
function Initialize-Provider { 
    param()
    write-debug "In TestPackageProvider - Initialize-Provider"
}

<# 

#>
function Install-Package { 
    param(
        [string] $fastPackageReference
    )
	write-debug "In TestPackageProvider - Install-Package"
	$pkg = (new-SoftwareIdentity $fastPackageReference   "installedby$fastPackageReference" "1.0"  "semver"  "local"  "this is package $fastPackageReference" ) 
	$InstalledPackages.Add( $pkg)
	write-Output $pkg
}

<# 

#>
function Is-TrustedPackageSource { 
    param(
        [string] $packageSource
    )
    write-debug "In TestPackageProvider - Is-TrustedPackageSource"

	return false;
}

<# 

#>
function Is-ValidPackageSource { 
    param(
        [string] $packageSource
    )
	write-debug "In TestPackageProvider - Is-ValidPackageSource"
	return false;
}

<# 

#>
function Uninstall-Package { 
    param(
        [string] $fastPackageReference
    )
    write-debug "In TestPackageProvider - Uninstall-Package"
	    for($i=$InstalledPackages.Count; $i -gt 0; $i--) {
		$pkg = $InstalledPackages[$i-1]
		if( $pkg.FastPackageReference -eq $fastPackageReference  )  {
			write-debug "Removing Pkg {0} " pkg.Name 
			$InstalledPackages.Remove( $pkg )
			write-Output $pkg
		}
	}
}

<# 

#>
function Get-Feature { 
    param()
	# Metadata that describes what this provider is used for.
    write-debug "In TestPackageProvider - get-feature"

	#advertise this is not for user-consumption
	write-Output (new-feature "automation-only" )

	#advertise which file extensions we support
	write-Output (new-feature "extensions" @("testpkg","testpkg2") )
}

<# 
// --- operations on a package ---------------------------------------------------------------------------------------------------
#>
function Download-Package { 
    param(
        [string] $fastPackageReference,
        [string] $location
    )
    write-debug "In TestPackageProvider - Download-Package"
}

<# 

#>
function Get-PackageDependencies { 
    param(
        [string] $fastPackageReference
    )
    write-debug "In TestPackageProvider - Get-PackageDependencies"
}

<# 

#>
function Get-PackageDetail { 
    param(
        [string] $fastPackageReference
    )
    # NOT_IMPLEMENTED_YET
}

<# 

#>
function Get-DynamicOptions { 
    param(
        [Microsoft.OneGet.MetaProvider.PowerShell.OptionCategory] $category
    )
    write-debug "In TestPackageProvider - Get-DynamicOption for category $category"

	switch( $category ) {
	    Package {
			# options when the user is trying to specify a package 
			write-Output (New-DynamicOption $category "hint" String $false )
			write-Output (New-DynamicOption $category  "color" String $false @("red","green","blue"))
			write-Output (New-DynamicOption $category  "flavor" String $false @("chocolate","vanilla","peach"))
		}

		Source {
			#options when the user is trying to specify a source
		}
		Install {
			#options for installation/uninstallation 
			#get-package  -destination .\ 
		}
	}
}