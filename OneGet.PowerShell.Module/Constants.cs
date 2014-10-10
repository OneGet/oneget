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

namespace Microsoft.PowerShell.OneGet {
    using Microsoft.OneGet.Utility.PowerShell;
    using ErrorCategory = System.Management.Automation.ErrorCategory;

    internal static class Constants {

        internal static object[] NoParameters = new object[0];

        // cmdlet naming/etc
        internal  const string PackageNoun = "Package";
        internal  const string PackageSourceNoun = "PackageSource";
        internal  const string PackageProviderNoun = "PackageProvider";
        
        internal  const string PackageBySearchSet = "PackageBySearch";
        internal const string PackageByInputObjectSet = "PackageByInputObject";
        internal const string SourceByInputObjectSet = "SourceByInputObject";
        internal const string SourceBySearchSet = "SourceBySearch";

        internal const string DestinationPathSet = "DestinationPath";
        internal const string LiteralPathSet = "LiteralPath";

        internal const string AssemblyProviderType = "assembly";

        // messages 

        // Implementation Note:
        // Because OneGet allows the application layer closest to the user (host) to be in ultimate 
        // control of spcifying messages to the end user, and falls back up the chain of responsibility
        // when resolving Messages from resources, we have prefixed the constants with MSG: in order
        // to *know* when we're trying to resolve a message. 
        
        // As an optimization step, if the MSG: isn't present, then the application layer need not bother 
        // resolving the message (nor asking up the chain) since it's not a message id, but rather an
        // already resolved string.

#if DEBUG
        internal const string NotImplemented = "MSG:NotImplemented";
#endif

        internal const string ShouldContinueWithUntrustedPackageSource = "MSG:ShouldContinueWithUntrustedPackageSource";
        internal const string DestinationPathInvalid = "MSG:DestinationPathInvalid";
        internal const string NoPackagesFoundForProvider = "MSG:NoPackagesFoundForProvider";
        internal const string DisambiguateForInstall = "MSG:DisambiguateForInstall";
        internal const string DisambiguateForUninstall = "MSG:DisambiguateForUninstall";
        internal const string UnknownProvider = "MSG:UnknownProvider";
        internal const string UnableToFindProviderForSource = "MSG:UnableToFindProviderForSource";
        internal const string SourceNotFound = "MSG:SourceNotFound";
        internal const string SourceFoundInMultipleProviders = "MSG:SourceFoundInMultipleProviders";
        internal const string SkippedProviderMissingRequiredOption = "MSG:SkippedProviderMissingRequiredOption";
        internal const string NoMatchFound = "MSG:NoMatchFound";
        internal const string MatchesMultiplePackages = "MSG:MatchesMultiplePackages";
        internal const string InstallationFailure = "MSG:InstallationFailure";
        internal const string ActionInstallPackage = "MSG:ActionInstallPackage";
        internal const string QueryInstallUntrustedPackage = "MSG:QueryInstallUntrustedPackage";
        internal const string CaptionPackageNotTrusted = "MSG:CaptionPackageNotTrusted";
        internal const string QueryContinueInstallingAfterFailing = "MSG:QueryContinueInstallingAfterFailing";
        internal const string CaptionPackageInstallFailure = "MSG:CaptionPackageInstallFailure";
        internal const string QueryShouldThePackageScriptAtBeProcessed = "MSG:QueryShouldThePackageScriptAtBeProcessed";
        internal const string CaptionPackageContainsInstallationScript = "MSG:CaptionPackageContainsInstallationScript";
        internal const string OverwritingPackageSource = "MSG:OverwritingPackageSource";
        internal const string PackageSourceExists = "MSG:PackageSourceExists";
        internal const string UninstallationFailure = "MSG:UninstallationFailure";
        internal const string ActionUninstallPackage = "MSG:ActionUninstallPackage";
        internal const string QueryContinueUninstallingAfterFailing = "MSG:QueryContinueUninstallingAfterFailing";
        internal const string CaptionPackageUninstallFailure = "MSG:CaptionPackageUninstallFailure";
        internal const string QueryShouldThePackageUninstallScriptAtBeProcessed = "MSG:QueryShouldThePackageUninstallScriptAtBeProcessed";
        internal const string CaptionPackageContainsUninstallationScript = "MSG:CaptionPackageContainsUninstallationScript";
        internal const string PackageInstallRequiresOption = "MSG:PackageInstallRequiresOption";
        internal const string TargetPackageSource = "MSG:TargetPackageSource";
        internal const string FileNotRecognized = "MSG:FileNotRecognized";
        internal const string BootstrapProvider = "MSG:BootstrapProvider";
        internal const string SourceNotFoundNoCriteria = "MSG:SourceNotFoundNoCriteria";

        internal const string ActionUnregisterPackageSource = "MSG:ActionUnregisterPackageSource";

        internal const string SourceNotFoundForLocation = "MSG:SourceNotFoundForLocation";

        internal const string BootstrapProviderUserRequested = "MSG:BootstrapProviderUserRequested";
        internal const string BootstrapProviderProviderRequested = "MSG:BootstrapProviderProviderRequested";
        internal const string BootstrapManualAssembly = "MSG:BootstrapManualAssembly";
        internal const string BootstrapManualInstall = "MSG:BootstrapManualInstall";
        internal const string QueryBootstrap = "MSG:QueryBootstrap";
        internal const string ActionRegisterPackageSource = "MSG:ActionRegisterPackageSource";
        internal const string ActionReplacePackageSource = "MSG:ActionReplacePackageSource";

        internal const string NoMatchesForWildcard = "MSG:NoMatchesForWildcard";

        internal const string MatchesMultipleProviders = "MSG:MatchesMultipleProviders";

        internal const string TargetPackage = "MSG:TargetPackage";

        internal const string NameOrLocationRequired = "MSG:NameOrLocationRequired";
        internal const string DestinationOrLiteralPathRequired = "MSG:DestinationOrLiteralPathRequired";

        internal const string SavePackage = "MSG:SavePackage";

        #region copy common-constants-implementation
        /* Synced/Generated code =================================================== */

        internal const string MinVersion = "0.0.0.1";
        internal static string[] Empty = new string[0];
        internal const string MSGPrefix = "MSG:";

        internal static class Signatures {
            internal const string Zip = "504b0304";
            internal const string Cab = "4D534346";
            internal const string OleCompoundDocument = "D0CF11E0A1B11AE1";
            internal static string[] ZipVariants = new[] { Zip, /* should have EXEs? */};

        }

        internal static class PackageStatus {
            internal const string Installed = "Installed";
            internal const string Uninstalled = "Uninstalled";
        }

        internal static class SwidTag {
            internal const string SoftwareIdentity = "SoftwareIdentity";
        }

        internal static class Features {
            internal const string AutomationOnly = "automation-only";
            internal const string SupportedExtensions = "file-extensions";
            internal const string MagicSignatures = "magic-signatures";
            internal const string SupportedSchemes = "uri-schemes";
            internal const string SupportsPowerShellModules = "supports-powershell-modules";
        }

        internal static class Parameters {
            internal const string IsUpdate = "IsUpdatePackageSource";
            internal const string Name = "Name";
            internal const string Location = "Location";
        }


        internal static class Messages {
            internal const string UnableToDownload = "MSG:UnableToDownload";
            internal const string FailedProviderBootstrap = "MSG:FailedProviderBootstrap";
            internal const string UnknownProvider = "MSG:UnknownProvider";
            internal const string UserDeclinedUntrustedPackageInstall = "MSG:UserDeclinedUntrustedPackageInstall";
            internal const string ProviderPluginLoadFailure = "MSG:ProviderPluginLoadFailure";
            internal const string ProviderSwidtagUnavailable = "MSG:ProviderSwidtagUnavailable";
            internal const string UnableToResolvePackage = "MSG:UnableToResolvePackage";
            internal const string UnsupportedProviderType = "MSG:UnsupportedProviderType";
            internal const string DestinationPathNotSet = "MSG:DestinationPathNotSet";
            internal const string InvalidFilename = "MSG:InvalidFilename";
            internal const string UnableToRemoveFile = "MSG:UnableToRemoveFile";
            internal const string FileFailedVerification = "MSG:FileFailedVerification";
            internal const string MissingRequiredParameter = "MSG:MissingRequiredParameter";
            internal const string SchemeNotSupported = "MSG:SchemeNotSupported";
            internal const string PackageProviderExists = "MSG:PackageProviderExists";
            internal const string UnableToResolveSource = "MSG:UnableToResolveSource_NameOrLocation";
            internal const string UnsupportedArchive = "MSG:UnsupportedArchive";
            internal const string CreatefolderFailed = "MSG:CreatefolderFailed";
            internal const string UnableToOverwriteExistingFile = "MSG:UnableToOverwriteExistingFile";
            internal const string UnableToCopyFileTo = "MSG:UnableToCopyFileTo";
            internal const string UnableToCreateShortcutTargetDoesNotExist = "MSG:UnableToCreateShortcutTargetDoesNotExist";
            internal const string RemoveEnvironmentVariableRequiresElevation = "MSG:RemoveEnvironmentVariableRequiresElevation";
            internal const string UnknownFolderId = "MSG:UnknownFolderId";
            internal const string ProtocolNotSupported = "MSG:ProtocolNotSupported";
            internal const string UnableToUninstallPackage = "MSG:UnableToUninstallPackage";
        }
        #endregion 

    }

    internal static class Errors {
        
        // ReSharper disable InconsistentNaming

        public static ErrorMessage UnknownProvider = new ErrorMessage(Constants.UnknownProvider, ErrorCategory.ObjectNotFound);

        public static ErrorMessage SourceNotFound = new ErrorMessage(Constants.SourceNotFound, ErrorCategory.ObjectNotFound);

        public static ErrorMessage SourceFoundInMultipleProviders = new ErrorMessage(Constants.SourceFoundInMultipleProviders, ErrorCategory.InvalidArgument);

        public static ErrorMessage NoMatchFound = new ErrorMessage(Constants.NoMatchFound , ErrorCategory.ObjectNotFound);

        public static ErrorMessage NoPackagesFoundForProvider = new ErrorMessage(Constants.NoPackagesFoundForProvider, ErrorCategory.ObjectNotFound);

        public static ErrorMessage DisambiguateForInstall = new ErrorMessage(Constants.DisambiguateForInstall , ErrorCategory.InvalidArgument);

        public static ErrorMessage InstallationFailure = new ErrorMessage(Constants.InstallationFailure, ErrorCategory.InvalidOperation);

        public static ErrorMessage DisambiguateForUninstall = new ErrorMessage(Constants.DisambiguateForUninstall , ErrorCategory.InvalidArgument);

        public static ErrorMessage UninstallationFailure = new ErrorMessage(Constants.UninstallationFailure, ErrorCategory.InvalidOperation);

        public static ErrorMessage PackageInstallRequiresOption = new ErrorMessage(Constants.PackageInstallRequiresOption, ErrorCategory.InvalidArgument);

        public static ErrorMessage PackageSourceExists = new ErrorMessage(Constants.PackageSourceExists, ErrorCategory.ResourceExists);

        public static ErrorMessage NameOrLocationRequired = new ErrorMessage(Constants.NameOrLocationRequired, ErrorCategory.InvalidArgument);

        public static ErrorMessage UnableToFindProviderForSource = new ErrorMessage(Constants.UnableToFindProviderForSource, ErrorCategory.ObjectNotFound);

        public static ErrorMessage DestinationOrLiteralPathNotSpecified = new ErrorMessage(Constants.DestinationOrLiteralPathRequired, ErrorCategory.InvalidArgument);
        public static ErrorMessage MatchesMultipleProviders= new ErrorMessage(Constants.MatchesMultipleProviders, ErrorCategory.InvalidArgument);

        // ReSharper restore InconsistentNaming
    }
}