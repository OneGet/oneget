//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a New-StronglyTypedCsFileForResx funciton.
//     To add or remove a member, edit your .ResX file then rerun buildCoreClr.ps1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.PowerShell.PackageManagement.Resources {
using System;
using System.Reflection;

/// <summary>
///   A strongly-typed resource class, for looking up localized strings, etc.
/// </summary>
[global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]

internal class Messages {

    private static global::System.Resources.ResourceManager resourceMan;

    private static global::System.Globalization.CultureInfo resourceCulture;

    [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    internal Messages() {
    }

    /// <summary>
    ///   Returns the cached ResourceManager instance used by this class.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
    internal static global::System.Resources.ResourceManager ResourceManager {
        get {
            if (object.ReferenceEquals(resourceMan, null)) {
                global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.PowerShell.PackageManagement.Resources.Messages", typeof(Messages).GetTypeInfo().Assembly);
                resourceMan = temp;
            }
            return resourceMan;
        }
    }

    /// <summary>
    ///   Overrides the current thread's CurrentUICulture property for all
    ///   resource lookups using this strongly typed resource class.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
    internal static global::System.Globalization.CultureInfo Culture {
        get {
            return resourceCulture;
        }
        set {
            resourceCulture = value;
        }
    }
    
    /// <summary>
    ///   Looks up a localized string similar to Do you wish to continue installing packages?
    /// </summary>
    internal static string QueryContinueInstallingAfterFailing {
        get {
            return ResourceManager.GetString("QueryContinueInstallingAfterFailing", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Do you wish to continue uninstalling packages?
    /// </summary>
    internal static string QueryContinueUninstallingAfterFailing {
        get {
            return ResourceManager.GetString("QueryContinueUninstallingAfterFailing", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to The desintation path '{0}' for package '{1}' is invalid.
    /// </summary>
    internal static string DestinationPathInvalid {
        get {
            return ResourceManager.GetString("DestinationPathInvalid", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unable to install, multiple packages matched '{0}'. {1}
    /// </summary>
    internal static string DisambiguateForInstall {
        get {
            return ResourceManager.GetString("DisambiguateForInstall", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unable to uninstall, multiple packages matched '{0}'.
    /// </summary>
    internal static string DisambiguateForUninstall {
        get {
            return ResourceManager.GetString("DisambiguateForUninstall", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to The package source '{0}' was found in multiple providers ({1}).
    /// </summary>
    internal static string SourceFoundInMultipleProviders {
        get {
            return ResourceManager.GetString("SourceFoundInMultipleProviders", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Skipping package provider provider '{0}'-- missing required option '{1}'.
    /// </summary>
    internal static string SkippedProviderMissingRequiredOption {
        get {
            return ResourceManager.GetString("SkippedProviderMissingRequiredOption", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Package '{0}' failed to install.
    /// </summary>
    internal static string InstallationFailure {
        get {
            return ResourceManager.GetString("InstallationFailure", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to The package '{0}' comes from a package source that is not marked as trusted.
    /// </summary>
    internal static string CaptionPackageNotTrusted {
        get {
            return ResourceManager.GetString("CaptionPackageNotTrusted", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to The package(s) come(s) from a package source that is not marked as trusted.
    /// </summary>
    internal static string CaptionSourceNotTrusted {
        get {
            return ResourceManager.GetString("CaptionSourceNotTrusted", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Install Package
    /// </summary>
    internal static string ActionInstallPackage {
        get {
            return ResourceManager.GetString("ActionInstallPackage", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to '{0}' matched package '{2}/{3}' from provider: '{1}', source '{4}'.
    /// </summary>
    internal static string MatchesMultiplePackages {
        get {
            return ResourceManager.GetString("MatchesMultiplePackages", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Package Source '{0}' ({1}) in provider '{2}'.
    /// </summary>
    internal static string TargetPackageSource {
        get {
            return ResourceManager.GetString("TargetPackageSource", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to No package found for '{0}'.
    /// </summary>
    internal static string NoMatchFound {
        get {
            return ResourceManager.GetString("NoMatchFound", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to No match was found for the specified search criteria and package name '{0}'. Try Get-PackageSource to see all available registered package sources.
    /// </summary>
    internal static string NoMatchFoundForCriteria {
        get {
            return ResourceManager.GetString("NoMatchFoundForCriteria", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to The package provider '{0}' did not return any packages.
    /// </summary>
    internal static string NoPackagesFoundForProvider {
        get {
            return ResourceManager.GetString("NoPackagesFoundForProvider", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Package '{0}' contains an installation script.
    /// </summary>
    internal static string CaptionPackageContainsInstallationScript {
        get {
            return ResourceManager.GetString("CaptionPackageContainsInstallationScript", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Package '{0}' contains an uninstallation script.
    /// </summary>
    internal static string CaptionPackageContainsUninstallationScript {
        get {
            return ResourceManager.GetString("CaptionPackageContainsUninstallationScript", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to File '{0}' is not recognized as a valid package.
    /// </summary>
    internal static string FileNotRecognized {
        get {
            return ResourceManager.GetString("FileNotRecognized", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Package '{0}' failed to install.
    /// </summary>
    internal static string CaptionPackageInstallFailure {
        get {
            return ResourceManager.GetString("CaptionPackageInstallFailure", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Package '{0}' failed to uninstall.
    /// </summary>
    internal static string CaptionPackageUninstallFailure {
        get {
            return ResourceManager.GetString("CaptionPackageUninstallFailure", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Should the package install script at '{0}' be processed?
    /// </summary>
    internal static string QueryShouldThePackageScriptAtBeProcessed {
        get {
            return ResourceManager.GetString("QueryShouldThePackageScriptAtBeProcessed", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Should the package uninstall script at '{0}' be processed?
    /// </summary>
    internal static string QueryShouldThePackageUninstallScriptAtBeProcessed {
        get {
            return ResourceManager.GetString("QueryShouldThePackageUninstallScriptAtBeProcessed", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unable to find package source '{0}'. Use Get-PackageSource to see all available package sources.
    /// </summary>
    internal static string SourceNotFound {
        get {
            return ResourceManager.GetString("SourceNotFound", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Are you sure you want to install software from '{1}'?
    /// </summary>
    internal static string QueryInstallUntrustedPackage {
        get {
            return ResourceManager.GetString("QueryInstallUntrustedPackage", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Package {0} failed to uninstall.
    /// </summary>
    internal static string UninstallationFailure {
        get {
            return ResourceManager.GetString("UninstallationFailure", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Uninstall Package.
    /// </summary>
    internal static string ActionUninstallPackage {
        get {
            return ResourceManager.GetString("ActionUninstallPackage", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unable to find package provider '{0}'.
    /// </summary>
    internal static string UnknownProvider {
        get {
            return ResourceManager.GetString("UnknownProvider", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to '{0}' may be manually downloaded from '{1}' and copied to '{2}'.
    /// </summary>
    internal static string BootstrapManualAssembly {
        get {
            return ResourceManager.GetString("BootstrapManualAssembly", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to {0} may be manually downloaded from {1} and installed.
    /// </summary>
    internal static string BootstrapManualInstall {
        get {
            return ResourceManager.GetString("BootstrapManualInstall", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to {0}
    ///{1}
    /// </summary>
    internal static string BootstrapProvider {
        get {
            return ResourceManager.GetString("BootstrapProvider", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to The provider '{0}' requires provider '{1} v{2}' to continue.
    /// </summary>
    internal static string BootstrapProviderProviderRequested {
        get {
            return ResourceManager.GetString("BootstrapProviderProviderRequested", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to The provider '{0} v{1}' is not installed.
    /// </summary>
    internal static string BootstrapProviderUserRequested {
        get {
            return ResourceManager.GetString("BootstrapProviderUserRequested", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Would you like PackageManagement to automatically download and install '{0}' now?
    /// </summary>
    internal static string QueryBootstrap {
        get {
            return ResourceManager.GetString("QueryBootstrap", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Package '{0}' version '{1}' from '{2}'.
    /// </summary>
    internal static string TargetPackage {
        get {
            return ResourceManager.GetString("TargetPackage", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Package '{0}' with version '{1}'.
    /// </summary>
    internal static string TargetPackageVersion {
        get {
            return ResourceManager.GetString("TargetPackageVersion", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Register Package Source.
    /// </summary>
    internal static string ActionRegisterPackageSource {
        get {
            return ResourceManager.GetString("ActionRegisterPackageSource", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Replace Package Source.
    /// </summary>
    internal static string ActionReplacePackageSource {
        get {
            return ResourceManager.GetString("ActionReplacePackageSource", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unregister Package Source.
    /// </summary>
    internal static string ActionUnregisterPackageSource {
        get {
            return ResourceManager.GetString("ActionUnregisterPackageSource", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Not Implemented.
    /// </summary>
    internal static string NotImplemmented {
        get {
            return ResourceManager.GetString("NotImplemmented", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Overwriting package source '{0}'.
    /// </summary>
    internal static string OverwritingPackageSource {
        get {
            return ResourceManager.GetString("OverwritingPackageSource", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Package '{0}' from package provider '{1}' requires the '{2}' parameter to install.
    /// </summary>
    internal static string PackageInstallRequiresOption {
        get {
            return ResourceManager.GetString("PackageInstallRequiresOption", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Package Source '{0}' exists.
    /// </summary>
    internal static string PackageSourceExists {
        get {
            return ResourceManager.GetString("PackageSourceExists", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unable to find package source for location '{0}'.
    /// </summary>
    internal static string SourceNotFoundForLocation {
        get {
            return ResourceManager.GetString("SourceNotFoundForLocation", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unable to find package source for name '{0}' location '{1}'.
    /// </summary>
    internal static string SourceNotFoundForNameAndLocation {
        get {
            return ResourceManager.GetString("SourceNotFoundForNameAndLocation", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unable to find package sources.
    /// </summary>
    internal static string SourceNotFoundNoCriteria {
        get {
            return ResourceManager.GetString("SourceNotFoundNoCriteria", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unable to find package provider for package source '{0}'.
    /// </summary>
    internal static string UnableToFindProviderForSource {
        get {
            return ResourceManager.GetString("UnableToFindProviderForSource", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Saving a package requires either a -Path or -LiteralPath parameter.
    /// </summary>
    internal static string DestinationOrLiteralPathRequired {
        get {
            return ResourceManager.GetString("DestinationOrLiteralPathRequired", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Specified ProviderName parameter matches multiple providers: {0}.
    /// </summary>
    internal static string MatchesMultipleProviders {
        get {
            return ResourceManager.GetString("MatchesMultipleProviders", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to ProviderName must be specified. Available Providers: {0}.
    /// </summary>
    internal static string ProviderNameNotSpecified {
        get {
            return ResourceManager.GetString("ProviderNameNotSpecified", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Either -Name or -Location must be specified to select a package source.
    /// </summary>
    internal static string NameOrLocationRequired {
        get {
            return ResourceManager.GetString("NameOrLocationRequired", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to No package found matching '{0}'.
    /// </summary>
    internal static string NoMatchesForWildcard {
        get {
            return ResourceManager.GetString("NoMatchesForWildcard", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to No combination of providers ({0}) match sources specified ({1}).
    /// </summary>
    internal static string NoMatchForProvidersAndSources {
        get {
            return ResourceManager.GetString("NoMatchForProvidersAndSources", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Save Package
    /// </summary>
    internal static string SavePackage {
        get {
            return ResourceManager.GetString("SavePackage", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to ??? not used ???
    /// </summary>
    internal static string ShouldContinueWithUntrustedPackageSource {
        get {
            return ResourceManager.GetString("ShouldContinueWithUntrustedPackageSource", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to User declined to install untrusted package ({0}).
    /// </summary>
    internal static string UserDeclinedUntrustedPackageInstall {
        get {
            return ResourceManager.GetString("UserDeclinedUntrustedPackageInstall", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unable to find package providers ({0}).
    /// </summary>
    internal static string UnknownProviders {
        get {
            return ResourceManager.GetString("UnknownProviders", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Package file '{0}' exists. Remove the file first or use -Force to overwrite.
    /// </summary>
    internal static string PackageFileExists {
        get {
            return ResourceManager.GetString("PackageFileExists", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unable to remove package file '{0}'. 
    /// </summary>
    internal static string UnableToOverwrite {
        get {
            return ResourceManager.GetString("UnableToOverwrite", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to The path '{0}' must refer to a single file system path.
    /// </summary>
    internal static string FilePathMustBeFileSystemPath {
        get {
            return ResourceManager.GetString("FilePathMustBeFileSystemPath", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Error running Save-Package cmdlet: {0}.
    /// </summary>
    internal static string SavePackageError {
        get {
            return ResourceManager.GetString("SavePackageError", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unable to download the list of available providers. Check your internet connection.
    /// </summary>
    internal static string ProviderSwidtagUnavailable {
        get {
            return ResourceManager.GetString("ProviderSwidtagUnavailable", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Install-Package requires parameters to select a package to install.
    /// </summary>
    internal static string MustSpecifyCriteria {
        get {
            return ResourceManager.GetString("MustSpecifyCriteria", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unable to find dependent package(s) ({0})
    /// </summary>
    internal static string UnableToFindDependencyPackage {
        get {
            return ResourceManager.GetString("UnableToFindDependencyPackage", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Network connectivity may not be available, unable to reach remote sources.
    /// </summary>
    internal static string NetworkNotAvailable {
        get {
            return ResourceManager.GetString("NetworkNotAvailable", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Installed Package '{0}' ({1} of {2}).
    /// </summary>
    internal static string InstalledPackageMultiple {
        get {
            return ResourceManager.GetString("InstalledPackageMultiple", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Installing Package '{0}' ({1} of {2}).
    /// </summary>
    internal static string InstallingPackageMultiple {
        get {
            return ResourceManager.GetString("InstallingPackageMultiple", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Installing {0} packages.
    /// </summary>
    internal static string InstallingPackagesCount {
        get {
            return ResourceManager.GetString("InstallingPackagesCount", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Skipping installed package {0} {1}.
    /// </summary>
    internal static string SkippedInstalledPackage {
        get {
            return ResourceManager.GetString("SkippedInstalledPackage", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Skipping installed package '{0}' ({1} of {2}).
    /// </summary>
    internal static string SkippedInstalledPackageMultiple {
        get {
            return ResourceManager.GetString("SkippedInstalledPackageMultiple", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to You cannot use the parameters RequiredVersion and either MinimumVersion or MaximumVersion in the same command.
    /// </summary>
    internal static string VersionRangeAndRequiredVersionCannotBeSpecifiedTogether {
        get {
            return ResourceManager.GetString("VersionRangeAndRequiredVersionCannotBeSpecifiedTogether", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to The action with the specified provider '{0}' is missing one or more required parameters: {1}.
    /// </summary>
    internal static string SpecifiedProviderMissingRequiredOption {
        get {
            return ResourceManager.GetString("SpecifiedProviderMissingRequiredOption", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Cannot find file '{0}'.
    /// </summary>
    internal static string FileNotFound {
        get {
            return ResourceManager.GetString("FileNotFound", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Multiple matches found '{0}' for the specified file '{1}'.
    /// </summary>
    internal static string MoreThanOneFileMatched {
        get {
            return ResourceManager.GetString("MoreThanOneFileMatched", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Multiple matches found '{0}' for the specified folder '{1}'.
    /// </summary>
    internal static string MoreThanOneFolderMatched {
        get {
            return ResourceManager.GetString("MoreThanOneFolderMatched", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unhandled Exception - Message:'{0}' Name:'{1}' Stack Trace:'{2}'
    /// </summary>
    internal static string UnhandledException {
        get {
            return ResourceManager.GetString("UnhandledException", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to The specified MinimumVersion '{0}' is greater than the specified MaximumVersion '{1}'
    /// </summary>
    internal static string MinimumVersionMustBeLessThanMaximumVersion {
        get {
            return ResourceManager.GetString("MinimumVersionMustBeLessThanMaximumVersion", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to The version parameter is allowed only when a single module name is specified as the value of the Name parameter, without any wildcard characters.
    /// </summary>
    internal static string MultipleNamesWithVersionNotAllowed {
        get {
            return ResourceManager.GetString("MultipleNamesWithVersionNotAllowed", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to You cannot use the RequiredVersion and either MinimumVersion or MaximumVersion in the same command. Specify only one of these parameters in your command.
    /// </summary>
    internal static string RequiredWithMaxOrMinimumVersionNotAllowed {
        get {
            return ResourceManager.GetString("RequiredWithMaxOrMinimumVersionNotAllowed", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to The Version parameter is allowed only when a single provider name is specified as the value of the Name parameter, without any wildcard characters. The file path is not allowed with version parameter.
    /// </summary>
    internal static string FullProviderFilePathVersionNotAllowed {
        get {
            return ResourceManager.GetString("FullProviderFilePathVersionNotAllowed", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unable to find package provider '{0}'. It may not be imported yet. Try 'Get-PackageProvider -ListAvailable'.
    /// </summary>
    internal static string UnknownProviderFromActivatedList {
        get {
            return ResourceManager.GetString("UnknownProviderFromActivatedList", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to You cannot use the parameter AllVersions with RequiredVersion, MinimumVersion or MaximumVersion in the same command.
    /// </summary>
    internal static string AllVersionsCannotBeUsedWithOtherVersionParameters {
        get {
            return ResourceManager.GetString("AllVersionsCannotBeUsedWithOtherVersionParameters", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to No match was found for the specified search criteria for the provider '{0}'. The package provider requires 'PackageManagement' and 'Provider' tags. Please check if the specified package has the tags.
    /// </summary>
    internal static string NoMatchFoundForProvider {
        get {
            return ResourceManager.GetString("NoMatchFoundForProvider", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Specify an exact Name and RequiredVersion parameter.
    /// </summary>
    internal static string DisambiguateForInstall_SpecifyName {
        get {
            return ResourceManager.GetString("DisambiguateForInstall_SpecifyName", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to See Get-PackageSource for all available registered sources and try again by specifying a single Source parameter.
    /// </summary>
    internal static string DisambiguateForInstall_SpecifySource {
        get {
            return ResourceManager.GetString("DisambiguateForInstall_SpecifySource", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unable to find repository with SourceLocation '{0}'. Use Get-PSRepository to see all available repositories.
    /// </summary>
    internal static string RegisterPackageSourceRequired {
        get {
            return ResourceManager.GetString("RegisterPackageSourceRequired", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to The specified version '{0}' is invalid. Error: '{1}'.
    /// </summary>
    internal static string InvalidVersion {
        get {
            return ResourceManager.GetString("InvalidVersion", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Using the provider '{0}' for searching packages.
    /// </summary>
    internal static string SelectedProviders {
        get {
            return ResourceManager.GetString("SelectedProviders", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unable to save the package '{0}'.
    /// </summary>
    internal static string ProviderFailToDownloadFile {
        get {
            return ResourceManager.GetString("ProviderFailToDownloadFile", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Folder '{0}' cannot be found
    /// </summary>
    internal static string FolderNotFound {
        get {
            return ResourceManager.GetString("FolderNotFound", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to The specified name '{0}' should not contain any wildcard characters, please correct it and try again.
    /// </summary>
    internal static string WildCardCharsAreNotSupported {
        get {
            return ResourceManager.GetString("WildCardCharsAreNotSupported", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Please specify an exact -Name and -RequiredVersion.
    /// </summary>
    internal static string SuggestRequiredVersion {
        get {
            return ResourceManager.GetString("SuggestRequiredVersion", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Please specify a single -ProviderName.
    /// </summary>
    internal static string SuggestSingleProviderName {
        get {
            return ResourceManager.GetString("SuggestSingleProviderName", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Please specify a single -Source.
    /// </summary>
    internal static string SuggestSingleSource {
        get {
            return ResourceManager.GetString("SuggestSingleSource", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Provider '{0}' does not implement '{1}'.
    /// </summary>
    internal static string MethodNotImplemented {
        get {
            return ResourceManager.GetString("MethodNotImplemented", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Administrator rights are required to install packages in '{0}'. Log on to the computer with an account that has Administrator rights, and then try again, or install in '{1}' by adding "-Scope CurrentUser" to your command. You can also try running the Windows PowerShell session with elevated rights (Run as Administrator).
    /// </summary>
    internal static string InstallRequiresCurrentUserScopeParameterForNonAdminUser {
        get {
            return ResourceManager.GetString("InstallRequiresCurrentUserScopeParameterForNonAdminUser", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to PackageManagement cannot handle '{0}' packages.The limit is 63.
    /// </summary>
    internal static string TooManyPackages {
        get {
            return ResourceManager.GetString("TooManyPackages", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to PackageManagement: A package is installed.
    /// </summary>
    internal static string PackageInstalled {
        get {
            return ResourceManager.GetString("PackageInstalled", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to PackageManagement: A package is saved.
    /// </summary>
    internal static string PackageSaved {
        get {
            return ResourceManager.GetString("PackageSaved", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to PackageManagement: A package is uninstalled.
    /// </summary>
    internal static string PackageUnInstalled {
        get {
            return ResourceManager.GetString("PackageUnInstalled", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to '{0}' to location '{1}'
    /// </summary>
    internal static string SavePackageWhatIfDescription {
        get {
            return ResourceManager.GetString("SavePackageWhatIfDescription", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Imported provider '{0}' .
    /// </summary>
    internal static string ProviderImported {
        get {
            return ResourceManager.GetString("ProviderImported", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Import-PackageProvider failed. Possibly the provider name is different from the package name '{0}'. Try Get-PackageProvider -ListAvailable to identify the associated provider name and run Import-PackageProvider".
    /// </summary>
    internal static string ProviderNameDifferentFromPackageName {
        get {
            return ResourceManager.GetString("ProviderNameDifferentFromPackageName", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Completed
    /// </summary>
    internal static string Completed {
        get {
            return ResourceManager.GetString("Completed", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Processing
    /// </summary>
    internal static string Processing {
        get {
            return ResourceManager.GetString("Processing", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to The specified name '{0}' should not be whitespaces only, please correct it and try again.
    /// </summary>
    internal static string WhitespacesAreNotSupported {
        get {
            return ResourceManager.GetString("WhitespacesAreNotSupported", resourceCulture);
        }
    }

}
}
