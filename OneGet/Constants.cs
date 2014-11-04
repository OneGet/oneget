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

namespace Microsoft.OneGet {
    using System;

    internal static class Constants {
        internal const int CoreVersion = 1;
        internal const int TimeoutUnspecified = 0;
        internal const int TimeoutHostNotAvailable = -1;

        internal const int HostNotAvailableTimeout = 5*60; // time when host not around
        internal const int HostNotAvailableResponsiveness = 5; // five seconds

        internal const int TimeoutAfterCancel = 60;
        internal const int ResponsivenessAfterCancel = 1;
        internal static TimeSpan DefaultCallTimeout = TimeSpan.FromMinutes(60);
        // TODO: Setting responsiveness to 15 minutes until we know that 
        // we're handling it all right.
        internal static TimeSpan DefaultResponsiveness = TimeSpan.FromSeconds(15 * 60); 

        internal static TimeSpan Zero = new TimeSpan(0);

        #region declare common-constants-implementation

        /* Synced/Generated code =================================================== */

        internal const string MinVersion = "0.0.0.1";
        internal const string MSGPrefix = "MSG:";
        internal static string[] Empty = new string[0];

        internal static class Features {
            internal const string AutomationOnly = "automation-only";
            internal const string MagicSignatures = "magic-signatures";
            internal const string SupportedExtensions = "file-extensions";
            internal const string SupportedSchemes = "uri-schemes";
            internal const string SupportsPowerShellModules = "supports-powershell-modules";
            internal const string SupportsRegexSearch = "supports-regex-search";
            internal const string SupportsSubstringSearch = "supports-substring-search";
            internal const string SupportsWildcardSearch = "supports-wildcard-search";
        }

        internal static class Messages {
            internal const string CreatefolderFailed = "MSG:CreatefolderFailed";
            internal const string DependencyResolutionError = "MSG:UnableToResolveDependency_dependencyPackage";
            internal const string DependentPackageFailedInstall = "MSG:DependentPackageFailedInstall_dependency";
            internal const string DestinationPathNotSet = "MSG:DestinationPathNotSet";
            internal const string FailedProviderBootstrap = "MSG:FailedProviderBootstrap";
            internal const string FileFailedVerification = "MSG:FileFailedVerification";
            internal const string InvalidFilename = "MSG:InvalidFilename";
            internal const string MissingRequiredParameter = "MSG:MissingRequiredParameter";
            internal const string PackageFailedInstall = "MSG:UnableToInstallPackage_package_reason";
            internal const string PackageSourceExists = "MSG:PackageSourceExists";
            internal const string ProtocolNotSupported = "MSG:ProtocolNotSupported";
            internal const string ProviderPluginLoadFailure = "MSG:ProviderPluginLoadFailure";
            internal const string ProviderSwidtagUnavailable = "MSG:ProviderSwidtagUnavailable";
            internal const string RemoveEnvironmentVariableRequiresElevation = "MSG:RemoveEnvironmentVariableRequiresElevation";
            internal const string SchemeNotSupported = "MSG:SchemeNotSupported";
            internal const string SourceLocationNotValid = "MSG:SourceLocationNotValid_Location";
            internal const string UnableToCopyFileTo = "MSG:UnableToCopyFileTo";
            internal const string UnableToCreateShortcutTargetDoesNotExist = "MSG:UnableToCreateShortcutTargetDoesNotExist";
            internal const string UnableToDownload = "MSG:UnableToDownload";
            internal const string UnableToOverwriteExistingFile = "MSG:UnableToOverwriteExistingFile";
            internal const string UnableToRemoveFile = "MSG:UnableToRemoveFile";
            internal const string UnableToResolvePackage = "MSG:UnableToResolvePackage";
            internal const string UnableToResolveSource = "MSG:UnableToResolveSource_NameOrLocation";
            internal const string UnableToUninstallPackage = "MSG:UnableToUninstallPackage";
            internal const string UnknownFolderId = "MSG:UnknownFolderId";
            internal const string UnknownProvider = "MSG:UnknownProvider";
            internal const string UnsupportedArchive = "MSG:UnsupportedArchive";
            internal const string UnsupportedProviderType = "MSG:UnsupportedProviderType";
            internal const string UriSchemeNotSupported = "MSG:UriSchemeNotSupported_Scheme";
            internal const string UserDeclinedUntrustedPackageInstall = "MSG:UserDeclinedUntrustedPackageInstall";
        }

        internal static class PackageStatus {
            internal const string Available = "Available";
            internal const string Dependency = "Dependency";
            internal const string Installed = "Installed";
            internal const string Uninstalled = "Uninstalled";
        }

        internal static class Parameters {
            internal const string IsUpdate = "IsUpdatePackageSource";
            internal const string Name = "Name";
            internal const string Location = "Location";
        }

        internal static class Signatures {
            internal const string Cab = "4D534346";
            internal const string OleCompoundDocument = "D0CF11E0A1B11AE1";
            internal const string Zip = "504b0304";
            internal static string[] ZipVariants = new[] {Zip, /* should have EXEs? */};
        }

        internal static class SwidTag {
            internal const string SoftwareIdentity = "SoftwareIdentity";
        }

        #endregion
    }
}