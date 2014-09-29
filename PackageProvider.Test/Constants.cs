// 
//  Copyright (c) Microsoft Corporation. All rights reserved. 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  

namespace Microsoft.OneGet.PackageProvider.Test {
    internal static class Constants {
        #region copy constants-implementation
public const string MSGPrefix = "MSG:";
        public const string TerminatingError = "MSG:TerminatingError";
        public const string SourceLocationNotValid = "MSG:SourceLocationNotValid_Location";
        public const string UriSchemeNotSupported = "MSG:UriSchemeNotSupported_Scheme";
        public const string UnableToResolveSource = "MSG:UnableToResolveSource_NameOrLocation";
        public const string PackageFailedInstall = "MSG:UnableToInstallPackage_package_reason";
        public const string DependencyResolutionError = "MSG:UnableToResolveDependency_dependencyPackage";
        public const string DependentPackageFailedInstall = "MSG:DependentPackageFailedInstall_dependencyPackage";
        public const string PackageProviderExists = "MSG:PackageProviderExists";
        public const string MissingRequiredParameter = "MSG:MissingRequiredParameter";

        public const string IsUpdateParameter = "IsUpdatePackageSource";

        public const string NameParameter = "Name";
        public const string LocationParameter = "Location";

        #endregion

    }

    #region copy errorcategory-implementation

    #endregion

}