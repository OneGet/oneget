function Find-Package { 
 [CmdletBinding()]
    param(
        [string] $name,
        [string] $requiredVersion,
        [string] $minimumVersion,
        [string] $maximumVersion
    )
	write-debug "In PSChained1Provider - Find-Package $name - $requiredVersion"

	if( -not $($request.Services) ) {
		# make sure we can see the provider services object.
		write-debug "Provider Services : NULL"
	}

	# create a request object for the chained call
	$req = New-Request -sources @( "http://nuget.org/api/v2" ) 

	if( (-not $name) -or ($name -eq "zlib"))  {
		$pkgs = $request.FindPackageByCanonicalId("nuget:zlib/1.2.8.7#http://nuget.org/api/v2", $req);
		foreach( $pkg in $pkgs ) {

			$deps = (new-Object -TypeName  System.Collections.ArrayList)
			foreach( $d in $pkg.Dependencies ) {
				write-debug "DEPENDENCY: $d"
				# add each dependency, but say it's from us.
				$deps.Add( (new-dependency "PSChained1Provider" $request.Services.ParsePackageName($d) $request.Services.ParsePackageVersion($d) "my-source" $null) )
			
			}

			$p = new-softwareidentity "zlib" $pkg.Name $pkg.Version $pkg.VersionScheme "my-source" $pkg.Summary -dependencies $deps

			Write-Output $p
		}
	}

	# return the redist package:
	if( $name -eq "zlib.redist" -and $requiredVersion -eq "1.2.8.7" ) {
		$pkgs = $request.FindPackageByCanonicalId("nuget:zlib.redist/1.2.8.7#http://nuget.org/api/v2", $req);
		foreach( $pkg in $pkgs ) {
			$deps = (new-Object -TypeName  System.Collections.ArrayList)
			foreach( $d in $pkg.Dependencies ) {
				# add each dependency, but say it's from us.
				$deps.Add( (new-dependency "PSChained1Provider" $request.Services.ParsePackageName($d) $request.Services.ParsePackageVersion($d) "my-source" $null) )
			}

			$p = new-softwareidentity "zlib.redist" $pkg.Name $pkg.Version $pkg.VersionScheme "my-source" $pkg.Summary -dependencies $deps

			Write-Output $p
		}
	}

}

function Initialize-Provider { 
    param()
    write-debug "In PSChained1Provider - Initialize-Provider"
}

function Get-PackageProviderName { 
    return "PSChained1Provider"
}
