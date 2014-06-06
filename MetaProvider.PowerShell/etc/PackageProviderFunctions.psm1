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

<#
	Overrides the default Write-Debug so that the output gets routed back thru the 
	$request.Debug() function
#>
function Write-Debug {
	param(
	[string] $text,
	[parameter(ValueFromRemainingArguments=$true,Mandatory=$false)]
	[object[]]
	 $args= @()
	)

	if( $args -eq $null ) {
		$request.Debug($text);
		return 
	}
	$request.Debug($text,$args);
}

<#
	Overrides the default Write-Verbose so that the output gets routed back thru the 
	$request.Verbose() function
#>

function Write-Verbose{
	param(
	[string] $text,
	[parameter(ValueFromRemainingArguments=$true,Mandatory=$false)]
	[object[]]
	 $args= @()
	)

	if( $args -eq $null ) {
		$request.Verbose($text);
		return 
	}
	$request.Verbose($text,$args);
}

<#
	Overrides the default Write-Warning so that the output gets routed back thru the 
	$request.Warning() function
#>

function Write-Warning{
	param(
	[string] $text,
	[parameter(ValueFromRemainingArguments=$true,Mandatory=$false)]
	[object[]]
	 $args= @()
	)

	if( $args -eq $null ) {
		$request.Warning($text);
		return 
	}
	$request.Warning($text,$args);
}

<#
	Creates a new instance of a PackageSource object
#>
function New-PackageSource { 
	param(
		[string] $name,
		[string] $location,
		[bool] $trusted,
		[System.Collections.Hashtable] $details = $null
	)
	
	return New-Object -TypeName Microsoft.OneGet.MetaProvider.PowerShell.PackageSource -ArgumentList $name,$location,$trusted,$details
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
		[System.Collections.Hashtable] $details = $null
	)
	return New-Object -TypeName Microsoft.OneGet.MetaProvider.PowerShell.SoftwareIdentity -ArgumentList $fastPackageReference, $name, $version,  $versionScheme,  $source,  $summary,  $searchKey , $details 
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

	if( $permittedValues -eq $null ) {
		return New-Object -TypeName Microsoft.OneGet.MetaProvider.PowerShell.DynamicOption -ArgumentList $category,$name,  $expectedType, $isRequired
	}
	return New-Object -TypeName Microsoft.OneGet.MetaProvider.PowerShell.DynamicOption -ArgumentList $category,$name,  $expectedType, $isRequired, $permittedValues.ToArray()
}

<#
	Creates a new instance of a Feature object
#>
function New-Feature { 
	param(
		[string] $name, 
		[System.Collections.ArrayList] $values = $null
	)

	if( $values -eq $null ) {
		return New-Object -TypeName Microsoft.OneGet.MetaProvider.PowerShell.Feature -ArgumentList $name
	}
	return New-Object -TypeName Microsoft.OneGet.MetaProvider.PowerShell.Feature -ArgumentList $name, $values.ToArray()
}
