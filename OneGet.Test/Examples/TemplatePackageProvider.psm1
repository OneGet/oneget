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

#region psimplement PackageProvider-interface
<# 

#>
function Add-PackageSource { 
    param(
        [string] $name,
        [string] $location,
        [bool] $trusted,
        [Object] $requestObject
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'AddPackageSource'" );

    # expected return type : void
    #  $null;
}

<# 

#>
function Find-Package { 
    param(
        [string] $name,
        [string] $requiredVersion,
        [string] $minimumVersion,
        [string] $maximumVersion,
        [int] $id,
        [Object] $requestObject
    )

}

<# 

#>
function Find-PackageByFile { 
    param(
        [string] $file,
        [int] $id,
        [Object] $requestObject
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'FindPackageByFile'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Find-PackageByUri { 
    param(
        [Uri] $uri,
        [int] $id,
        [Object] $requestObject
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'FindPackageByUri'" );
	$request.YieldSoftwareIdentity( );
    # expected return type : bool
    # return  $null;
}

<# 

#>
function Get-InstalledPackage { 
    param(
        [string] $name,
        [Object] $requestObject
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'GetInstalledPackages'" );
    # expected return type : bool
    # return  $null;
}

<# 

#>
function Get-OptionDefinition { 
    param(
        [int] $category
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'GetDynamicOptions'" );

    # expected return type : void
    #  $null;
}

<# 
/// <summary>
            /// Returns the name of the Provider. Doesn't need callback .
            /// </summary>
            /// <returns></returns>
#>
function Get-PackageProviderName { 
    param(
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # expected return type : string
    # return  $null;
}

<# 

#>
function Get-PackageSource { 
    param(
        [Object] $c
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'TemplatePackageProvider::ResolvePackageSources'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Initialize-Provider { 
    param(
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'InitializeProvider'" );

    # expected return type : void
    #  $null;
}

<# 

#>
function Install-PackageByFastpath { 
    param(
        [string] $fastPath
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'InstallPackage'" );

    # expected return type : bool
    # return  $null;
}

<# 
// WhatIfInstallPackageBy* should be a good idea to fix -WhatIf
#>
function Install-PackageByFile { 
    param(
        [string] $filePath
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'InstallPackageByFile'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Install-PackageByUri { 
    param(
        [string] $u
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'InstallPackageByUri'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Is-TrustedPackageSource { 
    param(
        [string] $packageSource
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'IsTrustedPackageSource'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Is-ValidPackageSource { 
    param(
        [string] $packageSource
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'IsValidPackageSource'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Remove-PackageSource { 
    param(
        [string] $name,
        [Object] $requestObject
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'RemovePackageSource'" );

    # expected return type : void
    #  $null;
}

<# 

#>
function Uninstall-Package { 
    param(
        [string] $fastPath,
        [Object] $requestObject
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'UninstallPackage'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Get-Feature { 
    param(
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'GetFeatures'" );

    # expected return type : void
    #  $null;
}

<# 
// --- operations on a package ---------------------------------------------------------------------------------------------------
#>
function Download-Package { 
    param(
        [string] $fastPath,
        [string] $location,
        [Object] $requestObject
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'DownloadPackage'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Get-PackageDependencie { 
    param(
        [string] $fastPath,
        [Object] $requestObject
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'GetPackageDependencies'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Get-PackageDetail { 
    param(
        [string] $fastPath,
        [Object] $requestObject
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'GetPackageDetails'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Install-Package { 
    param(
        [string] $fastPath,
        [Object] $requestObject
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'InstallPackage'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Get-DynamicOption { 
    param(
        [int] $category
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'GetDynamicOptions'" );

    # expected return type : void
    #  $null;
}

<# 

#>
function Start-Find { 
    param(
        [Object] $requestObject
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'StartFind'" );

    # expected return type : int
    # return  $null;
}

<# 

#>
function Complete-Find { 
    param(
        [int] $id,
        [Object] $requestObject
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Calling 'CompleteFind'" );

    # expected return type : bool
    # return  $null;
}

<# 
// --- Optimization features -----------------------------------------------------------------------------------------------------
#>
function Get-IsSourceRequired {
    param(
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Resolve-PackageSource { 
    param(
        [Object] $requestObject
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # expected return type : void
    #  $null;
}

<# 

#>
function Execute-ElevatedAction { 
    param(
        [string] $payload,
        [Object] $requestObject
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # expected return type : void
    #  $null;
}

#endregion