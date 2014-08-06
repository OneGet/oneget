# Release Notes


### Overall

`[P1]` error/status/warning/debug messages are not coming from resources those that do get formatted correctly, are still string literals, and those that look like constants LIKE_THIS will spit out the messages unformatted, with the message appending the parameters in `«` and  `»` characters

`[P2]` there are a couple of minor start-time or one-time performance issues that I may look into later.
​​​
- PS Module Enumeration calls the `Get-Module` cmdlet which resolves assemblies that are needed for the module (even tho​​​' the module isn't actually *loading*)

- Internally `Get-Module` appears to be implemented using dictionaries and waits till the end of the call to return the modules (instead of doing it asychronously as it's discovering them)  -- this make it so that the raw discovery takes about 2 seconds, before I can even iterate over the results.

​​​​​- ​I delay nearly every initialization and duck-typing until the first use, and when you make your first call to a provider can cause a minor delay.  This one I may investigate, as there are only a few cases I've noticed this shows up.

`[P1]` Progress messages are not currently handled, it's being overhauled to work better.

`[P2]` there are a few cases that have quirks or don't work necessarily as expected -- if they give you bumps, give me a list and I'll prioritize them soon.


### NuGet Provider:
`[P1]` currently, you must not use PackageSaveMode = nuspec -- the detection of installed dependencies currently requires the nupkg file left in the target directory. This will be fixed soon.
    
`[P1]` Many incorrect/negative cases for options/parameters are not yet checked for, stick to legal values until we get comprehensive tests and parameter checking.

`[P1]` Detection of installed packages in a given directory is pretty flimsy right now, this will be improved.
    

### Cmdlets
**Note:** During testing you're going to want to run everything with -Verbose -- there are a lot of messages that can help with debugging, and some warnings and messages only show up in -Verbose. (where/what/etc) will be revisited when we do the next cmdlet review.


### Providers:
The following working providers are included in this build:
- NuGet

**The Chocolatey is not present, it is being rebuilt to use the NuGet provider as it's base.**


## Making a PowerShell provider:

There are a few functions present for the Provider Module to use:

``` powershell 
<#
	Overrides the default Write-Debug so that the output gets routed back thru the 
	$request.Debug() function
#>
function Write-Debug {
	param(
	[string] $text,
	[parameter(ValueFromRemainingArguments=$true,Mandatory=$false)] [object[]] $args= @()
	)
}

<#
	Overrides the default Write-Verbose so that the output gets routed back thru the 
	$request.Verbose() function
#>

function Write-Verbose{
	param(
	[string] $text,
	[parameter(ValueFromRemainingArguments=$true,Mandatory=$false)] [object[]] $args= @()
	)
}

<#
	Overrides the default Write-Warning so that the output gets routed back thru the 
	$request.Warning() function
#>
function Write-Warning{
	param(
	[string] $text,
	[parameter(ValueFromRemainingArguments=$true,Mandatory=$false)] [object[]] $args= @()
	)
}

<#
	Creates a new instance of a PackageSource object
#>
function New-PackageSource { 
	param(
		[string] $name,
		[string] $location,
		[bool] $trusted,
		[bool] $registered,
		[System.Collections.Hashtable] $details = $null
	)
}

<#
	Creates a new instance of a SoftwareIdentity object
#>
function New-SoftwareIdentity { 
	param(
		[string] $fastPackageReference, 
		[string] $name, 
		[string] $version, 
		[string] $versionScheme, 
		[string] $source, 
		[string] $summary, 
		[string] $searchKey = $null, 
		[string] $fullPath = $null, 
		[string] $filename = $null, 
		[System.Collections.Hashtable] $details = $null
	)
}


<#
	Creates a new instance of a DyamicOption object
#>
function New-DynamicOption { 
	param(
		[Microsoft.OneGet.MetaProvider.PowerShell.OptionCategory] $category, 
		[string] $name, 
		[Microsoft.OneGet.MetaProvider.PowerShell.OptionType] $expectedType, 
		[bool] $isRequired, 
		[System.Collections.ArrayList] $permittedValues = $null
	)
}

<#
	Creates a new instance of a Feature object
#>
function New-Feature { 
	param(
		[string] $name, 
		[System.Collections.ArrayList] $values = $null
	)
}

<# 
	Duplicates the $request object and overrides the client-supplied data with the specified values.
#>
function New-Request {
	param(
		[System.Collections.Hashtable] $options = $null,
		[System.Collections.ArrayList] $sources = $null,
		[PSCredential] $credential = $null
	)
}

```

And the `$Request` object is exposed to the provider as well:

``` powershell
## the $request object exposes the following:

# credentials specified by the user
PSCredential Credential {get;}

# dynamic options passed by the user
Hashtable Options {get;}

# package sources selected by the user
string[] PackageSources {get;}


# only permitted user interactions
bool AskPermission(string )
bool ShouldContinueAfterPackageInstallFailure(string , string , string )
bool ShouldContinueAfterPackageUninstallFailure(string , string , string )
bool ShouldContinueRunningInstallScript(string , string , string , string )
bool ShouldContinueRunningUninstallScript(string , string , string , string )
bool ShouldContinueWithUntrustedPackageSource(string , string )
bool ShouldProcessPackageInstall(string , string , string )
bool ShouldProcessPackageUninstall(string , string )

# returns true if the user has cancelled the operation
bool IsCancelled()

# misc functions implemented for convenience. 
void AddPinnedItemToTaskbar(string , REQUEST )
void CopyFile(string , string , REQUEST )
void CreateFolder(string , REQUEST )
void CreateShortcutLink(string , string , string , string , string , REQUEST )
void Delete(string , REQUEST )
void DeleteFile(string , REQUEST )
void DeleteFolder(string , REQUEST )
void RemoveEnvironmentVariable(string , int , REQUEST )
void RemovePinnedItemFromTaskbar(string , REQUEST )
bool IsElevated(REQUEST )
string GetKnownFolder(string , REQUEST )
void SetEnvironmentVariable(string , string , int , System.Object )


# talking to the package management service (for calling other PMs)
IPackageManagementService PackageManagementService {get;}

# gets the names of the PMs
IEnumerator<String > ProviderNames {get;}

# all the loaded PMs
IEnumerator<PackageProvider>  PackageProviders {get;}

# selects just ones with a specific name
IEnumerator<PackageProvider> SelectProviders(string providerName)

# selects just ones with a specific feature name
IEnumerator<PackageProvider> SelectProvidersWithFeature(string featureName), 

# selects just ones with a specific feature name and value
IEnumerator<PackageProvider> SelectProvidersWithFeature(string featureName, string value)


# notifications called during package install/uninstall process
# not-quite-ready-for-use-yet
bool NotifyBeforePackageInstall(string , string , string , string )
bool NotifyBeforePackageUninstall(string , string , string , string )
bool NotifyPackageInstalled(string , string , string , string )
bool NotifyPackageUninstalled(string , string , string , string )


```

#### Calling methods that have a `REQUEST` parameter  

For the `REQUEST` parameter use :

``` powershell
 (new-request -options @{<#hashtable#>} -sources @(<#arraylist#>) -credential <#$credendials-object#>)
```


#### PackageProvider object (use `$request.SelectProviders<...> ` functions to get a PackageProvider

``` powershell

# Name of the 
string Name {get;}

# returns the features that the provider supports
IReadOnlyDictionary<string,List<string>> Features {get;}

# Package Source manipulation
void AddPackageSource(string name, string location, bool trusted, REQUEST)
void RemovePackageSource(string name, REQUEST)

# returns the package sources ( if REQUEST.sources contains items, it resolves and returns just those)
IEnumerator<PackageSource> ResolvePackageSources(REQUEST)

#searching for packages
IEnumerator<SoftwareIdentity> FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, REQUEST)
IEnumerator<SoftwareIdentity> FindPackages(string[] names, string requiredVersion, string minimumVersion, string maximumVersion, REQUEST)
IEnumerator<SoftwareIdentity> FindPackageByFile(string filename, int id, REQUEST)
IEnumerator<SoftwareIdentity> FindPackagesByFiles(string[] filenames, REQUEST)
IEnumerator<SoftwareIdentity> FindPackageByUri(uri uri, int id, REQUEST)
IEnumerator<SoftwareIdentity> FindPackagesByUris(uri[] uris, REQUEST)

# Download a package file
void DownloadPackage(SoftwareIdentity softwareIdentity, string destinationFilename, REQUEST)


# Get Dynamic Options from a provider
IEnumerable<DynamicOption> GetDynamicOptions(OptionCategory operation, REQUEST)

# Get Installed packages
IEnumerator<SoftwareIdentity> GetInstalledPackages(string name, REQUEST)

# Get Dependencies of a package
IEnumerator<SoftwareIdentity> GetPackageDependencies(SoftwareIdentity package, REQUEST)

# Install/Uninstall 
IEnumerator<SoftwareIdentity> InstallPackage(SoftwareIdentity softwareIdentity, REQUEST)
IEnumerator<SoftwareIdentity> UninstallPackage(SoftwareIdentity softwareIdentity, REQUEST)
    
```

#### Enums
``` c#
public enum OptionCategory {
    Package = 0,
    Provider = 1,
    Source = 2,
    Install = 3
}

public enum OptionType {
    String = 0,
    StringArray = 1,
    Int = 2,
    Switch = 3,
    Folder = 4,
    File = 5,
    Path = 6,
    Uri = 7
}
```

 

### Template for a package provider in PowerShell

Module Registration: `MyModule.PSD1`

``` powershell

	# Yada-yada-yada
    # This module should be in the PSModulePath for provider searching and loading
    #

	##
	## Must put this private data in: 
	## 
   	PrivateData = @{
		 "OneGet.Providers" = @( ".\MyModuleImplementation.psm1" )
    }

```
The actual Provider Implementation: `MyModuleImplementation.psm1`

``` powershell
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
    return "THIS_PACKAGE_PROVIDER_NAME"
}

function Get-Feature { 
    param()
	# Metadata that describes what this provider is used for.
    write-debug "In MY_PACKAGE_PROVIDER - get-feature"

	#advertise this is not for user-consumption
 	# write-Output (new-feature "automation-only" )

	#advertise which file extensions we support
	write-Output (new-feature "extensions" @("nupkg") )
}

function Get-DynamicOptions { 
    param(
        [Microsoft.OneGet.MetaProvider.PowerShell.OptionCategory] $category
    )
    write-debug "In MY_PACKAGE_PROVIDER - Get-DynamicOption for category $category"

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
	write-debug "In MY_PACKAGE_PROVIDER - Add-PackageSource"
    # return via:
    # write-output  (New-PackageSource "sourcename" "location" <istrusted> <isregistered> @{ <hashtable-of-details> })
}

function Remove-PackageSource { 
    param(
        [string] $name
    )
	write-debug "In MY_PACKAGE_PROVIDER - Remove-PackageSource"
}

function Find-Package { 
    param(
        [string[]] $names,
        [string] $requiredVersion,
        [string] $minimumVersion,
        [string] $maximumVersion
    )
	write-debug "In MY_PACKAGE_PROVIDER - Find-Package"


<# some sample code to talk to other providers	
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
#>    
}

function Find-PackageByFile { 
    param(
        [string[]] $files
    )
    write-debug "In MY_PACKAGE_PROVIDER - Find-PackageByFile"

	# return values with
	# write-output  (new-SoftwareIdentity "fastPackageReference"  "package-name" "package-version" "multipartnumeric" "source_name_or_location" "summary" "searchkey" "filename-of-the-package" "full-path-of-the-package-or-installed-location" )

}

function Find-PackageByUri { 
    param(
        [Uri[]] $uris
    )
	write-debug "In MY_PACKAGE_PROVIDER - Find-PackageByUri"

	# return values with
	# write-output  (new-SoftwareIdentity "fastPackageReference"  "package-name" "package-version" "multipartnumeric" "source_name_or_location" "summary" "searchkey" "filename-of-the-package" "full-path-of-the-package-or-installed-location" )
}


function Get-InstalledPackage { 
    param(
        [string] $name
    )
    write-debug "In MY_PACKAGE_PROVIDER - Get-InstalledPackage {0} {1}" $InstalledPackages.Count $name

	# return values with
	# write-output  (new-SoftwareIdentity "fastPackageReference"  "package-name" "package-version" "multipartnumeric" "source_name_or_location" "summary" "searchkey" "filename-of-the-package" "full-path-of-the-package-or-installed-location" )

}

function Resolve-PackageSource { 
    param()
	write-debug "In MY_PACKAGE_PROVIDER - Resolve-PackageSource"

	# get requested set from $request.Sources

	# return values with
	# write-output  (New-PackageSource "sourcename" "location" <istrusted> <isregistered> @{ <hashtable-of-details> })
}

function Initialize-Provider { 
    param()
    write-debug "In MY_PACKAGE_PROVIDER - Initialize-Provider"
}

function Install-Package { 
    param(
        [string] $fastPackageReference
    )
	write-debug "In MY_PACKAGE_PROVIDER - Install-Package"

		# return values with
	# write-output  (new-SoftwareIdentity "fastPackageReference"  "package-name" "package-version" "multipartnumeric" "source_name_or_location" "summary" "searchkey" "filename-of-the-package" "full-path-of-the-package-or-installed-location" )

}


function Uninstall-Package { 
    param(
        [string] $fastPackageReference
    )
    write-debug "In MY_PACKAGE_PROVIDER - Uninstall-Package"
	# return values with
	# write-output  (new-SoftwareIdentity "fastPackageReference"  "package-name" "package-version" "multipartnumeric" "source_name_or_location" "summary" "searchkey" "filename-of-the-package" "full-path-of-the-package-or-installed-location" )

}


function Download-Package { 
    param(
        [string] $fastPackageReference,
        [string] $location
    )
    write-debug "In MY_PACKAGE_PROVIDER - Download-Package"

}

function Get-PackageDependencies { 
    param(
        [string] $fastPackageReference
    )
    write-debug "In MY_PACKAGE_PROVIDER - Get-PackageDependencies"

	# return values with
	# write-output  (new-SoftwareIdentity "fastPackageReference"  "package-name" "package-version" "multipartnumeric" "source_name_or_location" "summary" "searchkey" "filename-of-the-package" "full-path-of-the-package-or-installed-location" )

}

function Get-PackageDetail { 
    param(
        [string] $fastPackageReference
    )
    # NOT_IMPLEMENTED_YET
}

 
```