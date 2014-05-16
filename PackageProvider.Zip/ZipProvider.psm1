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
        [bool] $trusted
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'AddPackageSource'" );

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
        [string] $maximumVersion
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'FindPackage'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Find-PackageByFile { 
    param(
        [string] $file
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'FindPackageByFile'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Find-PackageByUri { 
    param(
        [Uri] $uri
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'FindPackageByUri'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Get-InstalledPackage { 
    param(
        [string] $name
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'GetInstalledPackages'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Get-MetadataDefinition { 
    param(
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'GetMetadataDefinitions'" );

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
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'GetPackageSources'" );

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
    $request.Debug("Information","Calling 'InitializeProvider'" );

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
    $request.Debug("Information","Calling 'InstallPackage'" );

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
    $request.Debug("Information","Calling 'InstallPackageByFile'" );

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
    $request.Debug("Information","Calling 'InstallPackageByUri'" );

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
    $request.Debug("Information","Calling 'IsTrustedPackageSource'" );

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
    $request.Debug("Information","Calling 'IsValidPackageSource'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Remove-PackageSource { 
    param(
        [string] $name
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'RemovePackageSource'" );

    # expected return type : void
    #  $null;
}

<# 

#>
function Uninstall-Package { 
    param(
        [string] $fastPath
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'UninstallPackage'" );

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
    $request.Debug("Information","Calling 'GetOptionDefinitions'" );

    # expected return type : void
    #  $null;
}

<# 

#>
function Get-Feature {
    param(
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'GetFeatures'" );

    # expected return type : void
    #  $null;
}

<# 
// --- Optimization features -----------------------------------------------------------------------------------------------------
#>
function Get-MagicSignature {
    param(
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # expected return type : IEnumerable<string>
    # return  $null;
}

<# 

#>
function Get-Scheme {
    param(
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # expected return type : IEnumerable<string>
    # return  $null;
}

<# 

#>
function Get-FileExtension {
    param(
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # expected return type : IEnumerable<string>
    # return  $null;
}

<# 

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
// --- operations on a package ---------------------------------------------------------------------------------------------------
#>
function Download-Package {
    param(
        [string] $fastPath,
        [string] $location
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'DownloadPackage'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Get-PackageDependencie {
    param(
        [string] $fastPath
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'GetPackageDependencies'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Get-PackageDetail {
    param(
        [string] $fastPath
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'GetPackageDetails'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Install-Package {
    param(
        [string] $fastPath
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'InstallPackage'" );

    # expected return type : bool
    # return  $null;
}

#endregion