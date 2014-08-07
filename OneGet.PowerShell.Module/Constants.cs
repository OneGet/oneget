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
    using Microsoft.OneGet.Utility.PowerShell;

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

        internal const string AssemblyProviderType = "assembly";

        // features we need to know about
        internal const string AutomationOnlyFeature = "automation-only";

        // messages 

        // Implementation Note:
        // Because OneGet allows the application layer closest to the user (host) to be in ultimate 
        // control of spcifying messages to the end user, and falls back up the chain of responsibility
        // when resolving Messages from resources, we have prefixed the constants with MSG: in order
        // to *know* when we're trying to resolve a message. 
        
        // As an optimization step, if the MSG: isn't present, then the application layer need not bother 
        // resolving the message (nor asking up the chain) since it's not a message id, but rather an
        // already resolved string.

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
        internal const string NoPackagesForProviderOrSource = "MSG:NoPackagesForProviderOrSource";

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
        internal const string OverwritingPackageSource = "MSG:OverwritingPackageSource";
        internal const string PackageSourceExists = "MSG:PackageSourceExists";

        internal const string UninstallationFailure = "MSG:UninstallationFailure";
        internal const string UninstallPackage = "MSG:UninstallPackage";
        internal const string ContinueUninstallingAfterFailing = "MSG:ContinueUninstallingAfterFailing";
        internal const string PackageUninstallFailure = "MSG:PackageUninstallFailure";
        internal const string ShouldThePackageUninstallScriptAtBeExecuted = "MSG:ShouldThePackageUninstallScriptAtBeExecuted";
        internal const string PackageContainsUninstallationScript = "MSG:PackageContainsUninstallationScript";

        internal const string PackageInstallRequiresOption = "MSG:PackageInstallRequiresOption";

        internal const string NameLocationProviderReplaceExisting = "MSG:NameLocationProviderReplaceExisting";
        internal const string NameLocationProvider = "MSG:NameLocationProvider";
        internal const string PackageFileNotRecognized = "MSG:PackageFileNotRecognized";

        internal const string BootstrapProvider = "MSG:BootstrapProvider";

        internal const string NoSourcesFoundNoCriteria = "MSG:NoSourcesFoundNoCriteria";
        internal const string NoSourcesFoundMatchingLocation = "MSG:NoSourcesFoundMatchingLocation";
        internal const string NoSourcesFoundMatchingName = "MSG:NoSourcesFoundMatchingName";

        internal const string BootstrapProviderUserRequested = "MSG:BootstrapProviderUserRequested";
        internal const string BootstrapProviderProviderRequested = "MSG:BootstrapProviderProviderRequested";
        internal const string BootstrapManualAssembly = "MSGBootstrapManualAssembly:";
        internal const string BootstrapManualInstall = "MSG:BootstrapManualInstall";
        internal const string BootstrapQuery = "MSG:BootstrapQuery";
        internal const string RegisterPackageSource = "MSG:RegisterPackageSource";
        internal const string OverwritePackageSource = "MSG:OverwritePackageSource";

        internal const string PackageTarget = "MSG:PackageTarget";
    }

    internal static class Errors {
        
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

        public static ErrorMessage NoPackagesForProviderOrSource = new ErrorMessage(Constants.NoPackagesForProviderOrSource, ErrorCategory.ObjectNotFound);

        public static ErrorMessage DisambiguateForInstall = new ErrorMessage(Constants.DisambiguateForInstall , ErrorCategory.InvalidArgument);

        public static ErrorMessage InstallationFailure = new ErrorMessage(Constants.InstallationFailure, ErrorCategory.InvalidOperation);

        public static ErrorMessage DisambiguateForUninstall = new ErrorMessage(Constants.DisambiguateForInstall, ErrorCategory.InvalidArgument);

        public static ErrorMessage UninstallationFailure = new ErrorMessage(Constants.UninstallationFailure, ErrorCategory.OperationStopped);

        public static ErrorMessage PackageInstallRequiresOption = new ErrorMessage(Constants.PackageInstallRequiresOption, ErrorCategory.InvalidArgument);

        public static ErrorMessage PackageSourceExists = new ErrorMessage(Constants.PackageSourceExists, ErrorCategory.ResourceExists);

        // ReSharper restore InconsistentNaming
    }
}