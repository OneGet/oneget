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

#region psimplement ServicesProvider-interface
<# 
/// <summary>
            /// Returns the name of the Provider. Doesn't need callback .
            /// </summary>
            /// <returns></returns>
#>
function Get-ServicesProviderName { 
    param(
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # expected return type : string
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
function Supported-DownloadScheme { 
    param(
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'SupportedDownloadSchemes'" );

    # expected return type : IEnumerable<string>
    # return  $null;
}

<# 

#>
function Download-File { 
    param(
        [Uri] $remoteLocation,
        [string] $localFilename
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'DownloadFile'" );

    # expected return type : void
    #  $null;
}

<# 

#>
function Supported-ArchiveExtension { 
    param(
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'SupportedArchiveExtensions'" );

    # expected return type : IEnumerable<string>
    # return  $null;
}

<# 

#>
function Is-SupportedArchive { 
    param(
        [string] $localFilename
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'IsSupportedArchive'" );

    # expected return type : bool
    # return  $null;
}

<# 

#>
function Unpack-Archive { 
    param(
        [string] $localFilename,
        [string] $destinationFolder
    )
    # TODO: Fill in implementation
    # Delete this method if you do not need to implement it

    # use the request object to interact with the OneGet core:
    $request.Debug("Information","Calling 'UnpackArchive'" );

    # expected return type : void
    #  $null;
}

#endregion