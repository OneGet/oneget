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
    using System.Management.Automation;
    using Microsoft.OneGet;

    internal static class Constants {

        internal static object[] NoParameters = new object[0];

        // cmdlet naming/etc
        internal  const string PackageNoun = "Package";
        internal  const string PackageSourceNoun = "PackageSource";
        internal  const string PackageProviderNoun = "PackageProvider";
        
        internal  const string PackageBySearchSet = "PackageBySearch";
        internal  const string PackageByObjectSet = "PackageByObject";
        internal  const string SourceByObjectSet = "SourceByObject";
        internal  const string ProviderByObjectSet = "ProviderByObject";
        internal  const string ProviderByNameSet = "ProviderByName";
        internal  const string OverwriteExistingSourceSet = "OverwriteExistingSource";

        // features we need to know about
        internal const string AutomationOnlyFeature = "automation-only";

        // messages 
        internal const string ShouldProcessPackageInstall = "MSG:ShouldProcessPackageInstall";
        internal const string ShouldProcessPackageUninstall = "MSG:ShouldProcessPackageUnInstall";
        internal const string ShouldContinueAfterPackageInstallFailure = "MSG:ShouldContinueAfterPackageInstallFailure";
        internal const string ShouldContinueAfterPackageUnInstallFailure = "MSG:ShouldContinueAfterPackageUnInstallFailure";
        internal const string ShouldContinueRunningInstallScript = "MSG:ShouldContinueRunningInstallScript";
        internal const string ShouldContinueRunningUninstallScript = "MSG:ShouldContinueRunningUninstallScript";
        internal const string AskPermission = "MSG:AskPermission";
        internal const string ShouldContinueWithUntrustedPackageSource = "MSG:ShouldContinueWithUntrustedPackageSource";
        internal const string SaveToPathNotValid = "MSG:DestinationPathInvalid_path";
        internal const string NoPackagesFoundForProvider = "MSG:NoPackagesFoundForProvider_providerName";

        internal const string DisambiguateForInstall = "MSG:DisambiguateForInstall";
        internal const string NoProviderSelected = "MSG:NoProviderSelected";
        internal const string RequiredValuePackageName = "MSG:RequiredValuePackageName";
        internal const string GetPackageNotFound = "MSG:GetPackageNotFound";
        internal const string DisambiguateForUninstall = "MSG:DisambiguateForUninstall";
        internal const string UnknownProvider = "MSG:UnknownProvider";
        internal const string NullOrEmptyPackageSource = "MSG:NullOrEmptyPackageSource";
        internal const string UnableToResolvePackageProvider = "MSG:UnableToResolvePackageProvider";
        internal const string ProviderNotFound = "MSG:ProviderNotFound";
        internal const string SourceNotFound = "MSG:SourceNotFound";
        internal const string DisambiguateSourceVsProvider = "MSG:DisambiguateSourceFoundInMultipleProivders";
        internal const string ExcludedProviderDueToMissingRequiredOption = "MSG:ExcludedProviderDueToMissingRequiredOption";
        internal const string NoMatchForPackageName = "MSG:NoMatchForPackageName";

        internal const string ProviderReturnedNoPackageSourcesNameLocation = "MSG:ProviderReturnedNoPackageSourcesNameLocation";
        internal const string ProviderReturnedNoPackageSourcesName = "MSG:ProviderReturnedNoPackageSourcesName";
        internal const string ProviderReturnedNoPackageSourcesLocation = "MSG:ProviderReturnedNoPackageSourcesLocation";
        internal const string ProviderReturnedNoPackageSources = "MSG:ProviderReturnedNoPackageSources";
        internal const string NoPackagesFoundNoPackageNamesCriteriaListed = "MSG:NoPackagesFoundNoPackageNamesCriteriaListed";
        internal const string NoPackageFound = "MSG:NoPackageFound";
        internal const string MatchesMultiplePackages = "MSG:MatchesMultiplePackages";

        internal const string InstallationFailure = "MSG:InstallationFailure";
        internal const string InstallPackage = "MSG:InstallPackage";
        internal const string ThisPackageSourceIsNotMarkedAsSafe = "MSG:ThisPackageSourceIsNotMarkedAsSafe";
        internal const string InstallingPackageFromUntrustedSource = "MSG:InstallingPackageFromUntrustedSource";
        internal const string ContinueInstallingAfterFailing = "MSG:ContinueInstallingAfterFailing";
        internal const string PackageInstallFailure = "MSG:PackageInstallFailure";
        internal const string ShouldThePackageScriptAtBeExecuted = "MSG:ShouldThePackageScriptAtBeExecuted";
        internal const string PackageContainsInstallationScript = "MSG:PackageContainsInstallationScript";


        internal const string UninstallationFailure = "MSG:UninstallationFailure";
        internal const string UninstallPackage = "MSG:UninstallPackage";
        internal const string ContinueUninstallingAfterFailing = "MSG:ContinueUninstallingAfterFailing";
        internal const string PackageUninstallFailure = "MSG:PackageUninstallFailure";
        internal const string ShouldThePackageUninstallScriptAtBeExecuted = "MSG:ShouldThePackageUninstallScriptAtBeExecuted";
        internal const string PackageContainsUninstallationScript = "MSG:PackageContainsUninstallationScript";


        internal const string NameLocationProviderReplaceExisting = "MSG:NameLocationProviderReplaceExisting";
        internal const string NameLocationProvider = "MSG:NameLocationProvider";
        internal const string PackageFileNotRecognized = "MSG:PackageFileNotRecognized";
    }

    internal static class Messages {
        
        // ReSharper disable InconsistentNaming
        public static ErrorMessage NoProviderSelected = new ErrorMessage(Constants.NoProviderSelected, ErrorCategory.InvalidArgument);

        public static ErrorMessage UnknownProvider = new ErrorMessage(Constants.UnknownProvider, ErrorCategory.InvalidArgument);

        public static ErrorMessage ExcludedProviderDueToMissingRequiredOption = new ErrorMessage(Constants.ExcludedProviderDueToMissingRequiredOption, ErrorCategory.InvalidArgument);

        public static ErrorMessage RequiredValuePackageName = new ErrorMessage(Constants.RequiredValuePackageName, ErrorCategory.InvalidArgument);

        public static ErrorMessage NullOrEmptyPackageSource = new ErrorMessage(Constants.NullOrEmptyPackageSource, ErrorCategory.InvalidArgument);

        public static ErrorMessage UnableToResolvePackageProvider = new ErrorMessage(Constants.UnableToResolvePackageProvider, ErrorCategory.ObjectNotFound);

        public static ErrorMessage SourceNotFound = new ErrorMessage(Constants.SourceNotFound, ErrorCategory.ObjectNotFound);

        public static ErrorMessage DisambiguateSourceVsProvider = new ErrorMessage(Constants.DisambiguateSourceVsProvider, ErrorCategory.InvalidArgument);

        public static ErrorMessage GetPackageNotFound = new ErrorMessage(Constants.GetPackageNotFound, ErrorCategory.ObjectNotFound);

        public static ErrorMessage NoMatchForPackageName = new ErrorMessage(Constants.NoMatchForPackageName , ErrorCategory.ObjectNotFound);

        public static ErrorMessage DisambiguateForInstall = new ErrorMessage(Constants.DisambiguateForInstall , ErrorCategory.InvalidArgument);

        public static ErrorMessage InstallationFailure = new ErrorMessage(Constants.InstallationFailure, ErrorCategory.InvalidOperation);

        public static ErrorMessage DisambiguateForUninstall = new ErrorMessage(Constants.DisambiguateForInstall, ErrorCategory.InvalidArgument);

        public static ErrorMessage UninstallationFailure = new ErrorMessage(Constants.UninstallationFailure, ErrorCategory.OperationStopped);

        // ReSharper restore InconsistentNaming
    }
}