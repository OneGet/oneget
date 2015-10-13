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

namespace Microsoft.PackageManagement.OneGetTestProvider.Sdk {
    public static partial class Constants {
        #region copy common-constants-implementation /internal/public

        public const string MinVersion = "0.0.0.1";
        public const string MSGPrefix = "MSG:";
        public static string[] FeaturePresent = new string[0];
        public static string[] Empty = new string[0];

        public static partial class Features {
            public const string AutomationOnly = "automation-only";
            public const string MagicSignatures = "magic-signatures";
            public const string SupportedExtensions = "file-extensions";
            public const string SupportedSchemes = "uri-schemes";
            public const string SupportsPowerShellModules = "supports-powershell-modules";
            public const string SupportsRegexSearch = "supports-regex-search";
            public const string SupportsSubstringSearch = "supports-substring-search";
            public const string SupportsWildcardSearch = "supports-wildcard-search";
        }

        public static partial class Messages {
            public const string CreatefolderFailed = "MSG:CreatefolderFailed";
            public const string DependencyResolutionError = "MSG:UnableToResolveDependency_dependencyPackage";
            public const string DependentPackageFailedInstall = "MSG:DependentPackageFailedInstall_dependency";
            public const string DestinationPathNotSet = "MSG:DestinationPathNotSet";
            public const string FailedProviderBootstrap = "MSG:FailedProviderBootstrap";
            public const string FileFailedVerification = "MSG:FileFailedVerification";
            public const string InvalidFilename = "MSG:InvalidFilename";
            public const string MissingRequiredParameter = "MSG:MissingRequiredParameter";
            public const string PackageFailedInstall = "MSG:UnableToInstallPackage_package_reason";
            public const string PackageSourceExists = "MSG:PackageSourceExists";
            public const string ProtocolNotSupported = "MSG:ProtocolNotSupported";
            public const string ProviderPluginLoadFailure = "MSG:ProviderPluginLoadFailure";
            public const string ProviderSwidtagUnavailable = "MSG:ProviderSwidtagUnavailable";
            public const string RemoveEnvironmentVariableRequiresElevation = "MSG:RemoveEnvironmentVariableRequiresElevation";
            public const string SchemeNotSupported = "MSG:SchemeNotSupported";
            public const string SourceLocationNotValid = "MSG:SourceLocationNotValid_Location";
            public const string UnableToCopyFileTo = "MSG:UnableToCopyFileTo";
            public const string UnableToCreateShortcutTargetDoesNotExist = "MSG:UnableToCreateShortcutTargetDoesNotExist";
            public const string UnableToDownload = "MSG:UnableToDownload";
            public const string UnableToOverwriteExistingFile = "MSG:UnableToOverwriteExistingFile";
            public const string UnableToRemoveFile = "MSG:UnableToRemoveFile";
            public const string UnableToResolvePackage = "MSG:UnableToResolvePackage";
            public const string UnableToResolveSource = "MSG:UnableToResolveSource_NameOrLocation";
            public const string UnableToUninstallPackage = "MSG:UnableToUninstallPackage";
            public const string UnknownFolderId = "MSG:UnknownFolderId";
            public const string UnknownProvider = "MSG:UnknownProvider";
            public const string UnsupportedArchive = "MSG:UnsupportedArchive";
            public const string UnsupportedProviderType = "MSG:UnsupportedProviderType";
            public const string UriSchemeNotSupported = "MSG:UriSchemeNotSupported_Scheme";
            public const string UserDeclinedUntrustedPackageInstall = "MSG:UserDeclinedUntrustedPackageInstall";
        }

        public static partial class OptionType {
            public const string String = "String";
            public const string StringArray = "StringArray";
            public const string Int = "Int";
            public const string Switch = "Switch";
            public const string Folder = "Folder";
            public const string File = "File";
            public const string Path = "Path";
            public const string Uri = "Uri";
            public const string SecureString = "SecureString";
        }

        public static partial class PackageStatus {
            public const string Available = "Available";
            public const string Dependency = "Dependency";
            public const string Installed = "Installed";
            public const string Uninstalled = "Uninstalled";
        }

        public static partial class Parameters {
            public const string IsUpdate = "IsUpdatePackageSource";
            public const string Name = "Name";
            public const string Location = "Location";
        }

        public static partial class Signatures {
            public const string Cab = "4D534346";
            public const string OleCompoundDocument = "D0CF11E0A1B11AE1";
            public const string Zip = "504b0304";
            public static string[] ZipVariants = new[] {Zip, /* should have EXEs? */};
        }

        public static partial class SwidTag {
            public const string SoftwareIdentity = "SoftwareIdentity";
        }

        #endregion
    }
}
