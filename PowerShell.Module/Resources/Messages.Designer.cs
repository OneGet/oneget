﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.PowerShell.OneGet.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.PowerShell.OneGet.Resources.Messages", typeof(Messages).Assembly);
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
        ///   Looks up a localized string similar to Install Package.
        /// </summary>
        internal static string ActionInstallPackage {
            get {
                return ResourceManager.GetString("ActionInstallPackage", resourceCulture);
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
        ///   Looks up a localized string similar to Uninstall Package.
        /// </summary>
        internal static string ActionUninstallPackage {
            get {
                return ResourceManager.GetString("ActionUninstallPackage", resourceCulture);
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
        ///   Looks up a localized string similar to &apos;{0}&apos; may be manually downloaded from &apos;{1}&apos; and copied to &apos;{2}&apos;..
        /// </summary>
        internal static string BootstrapManualAssembly {
            get {
                return ResourceManager.GetString("BootstrapManualAssembly", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} may be manually downloaded from {1} and installed..
        /// </summary>
        internal static string BootstrapManualInstall {
            get {
                return ResourceManager.GetString("BootstrapManualInstall", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0}
        ///{1}.
        /// </summary>
        internal static string BootstrapProvider {
            get {
                return ResourceManager.GetString("BootstrapProvider", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The provider &apos;{0}&apos; requires provider &apos;{1} v{2}&apos; to continue..
        /// </summary>
        internal static string BootstrapProviderProviderRequested {
            get {
                return ResourceManager.GetString("BootstrapProviderProviderRequested", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The provider &apos;{0} v{1}&apos; is not installed..
        /// </summary>
        internal static string BootstrapProviderUserRequested {
            get {
                return ResourceManager.GetString("BootstrapProviderUserRequested", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Package &apos;{0}&apos; contains an installation script.
        /// </summary>
        internal static string CaptionPackageContainsInstallationScript {
            get {
                return ResourceManager.GetString("CaptionPackageContainsInstallationScript", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Package &apos;{0}&apos; contains an uninstallation script.
        /// </summary>
        internal static string CaptionPackageContainsUninstallationScript {
            get {
                return ResourceManager.GetString("CaptionPackageContainsUninstallationScript", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Package &apos;{0}&apos; failed to install..
        /// </summary>
        internal static string CaptionPackageInstallFailure {
            get {
                return ResourceManager.GetString("CaptionPackageInstallFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The package &apos;{0}&apos; comes from a package source that is not marked as trusted..
        /// </summary>
        internal static string CaptionPackageNotTrusted {
            get {
                return ResourceManager.GetString("CaptionPackageNotTrusted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Package &apos;{0}&apos; failed to uninstall..
        /// </summary>
        internal static string CaptionPackageUninstallFailure {
            get {
                return ResourceManager.GetString("CaptionPackageUninstallFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Saving a packge requires either a -DestinationPath or -LiteralPath parameter.
        /// </summary>
        internal static string DestinationOrLiteralPathRequired {
            get {
                return ResourceManager.GetString("DestinationOrLiteralPathRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The desintation path  &apos;{0}&apos; for package &apos;{1}&apos; is invalid..
        /// </summary>
        internal static string DestinationPathInvalid {
            get {
                return ResourceManager.GetString("DestinationPathInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to install, multiple packages matched &apos;{0}&apos;. {1}.
        /// </summary>
        internal static string DisambiguateForInstall {
            get {
                return ResourceManager.GetString("DisambiguateForInstall", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to uninstall, multiple packages matched &apos;{0}&apos;..
        /// </summary>
        internal static string DisambiguateForUninstall {
            get {
                return ResourceManager.GetString("DisambiguateForUninstall", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to File &apos;{0}&apos; is not recognized as a valid package..
        /// </summary>
        internal static string FileNotRecognized {
            get {
                return ResourceManager.GetString("FileNotRecognized", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Package &apos;{0}&apos; failed to install..
        /// </summary>
        internal static string InstallationFailure {
            get {
                return ResourceManager.GetString("InstallationFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;{0}&apos; matched package &apos;{2}/{3}&apos; from provider: &apos;{1}&apos;, source &apos;{4}&apos;.
        /// </summary>
        internal static string MatchesMultiplePackages {
            get {
                return ResourceManager.GetString("MatchesMultiplePackages", resourceCulture);
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
        ///   Looks up a localized string similar to Either -Name or -Location must be specified to select a package source..
        /// </summary>
        internal static string NameOrLocationRequired {
            get {
                return ResourceManager.GetString("NameOrLocationRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No package found matching &apos;{0}&apos; ..
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
        ///   Looks up a localized string similar to No package found for  &apos;{0}&apos;..
        /// </summary>
        internal static string NoMatchFound {
            get {
                return ResourceManager.GetString("NoMatchFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The package provider &apos;{0}&apos; did not return any packages..
        /// </summary>
        internal static string NoPackagesFoundForProvider {
            get {
                return ResourceManager.GetString("NoPackagesFoundForProvider", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Not Implemented..
        /// </summary>
        internal static string NotImplemmented {
            get {
                return ResourceManager.GetString("NotImplemmented", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Overwriting package source &apos;{0}&apos;..
        /// </summary>
        internal static string OverwritingPackageSource {
            get {
                return ResourceManager.GetString("OverwritingPackageSource", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Package file &apos;{0}&apos; exists. Remove the file first or use -Force to overwrite.
        /// </summary>
        internal static string PackageFileExists {
            get {
                return ResourceManager.GetString("PackageFileExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Package &apos;{0}&apos; from package provider &apos;{1}&apos; requires the &apos;{2}&apos; parameter to install..
        /// </summary>
        internal static string PackageInstallRequiresOption {
            get {
                return ResourceManager.GetString("PackageInstallRequiresOption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Package Source &apos;{0}&apos; exists..
        /// </summary>
        internal static string PackageSourceExists {
            get {
                return ResourceManager.GetString("PackageSourceExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Would you like OneGet to automatically download and install &apos;{0}&apos; now?.
        /// </summary>
        internal static string QueryBootstrap {
            get {
                return ResourceManager.GetString("QueryBootstrap", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do you wish to continue installing packages?.
        /// </summary>
        internal static string QueryContinueInstallingAfterFailing {
            get {
                return ResourceManager.GetString("QueryContinueInstallingAfterFailing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do you wish to continue uninstalling packages?.
        /// </summary>
        internal static string QueryContinueUninstallingAfterFailing {
            get {
                return ResourceManager.GetString("QueryContinueUninstallingAfterFailing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Are you sure you want to install software from &apos;{1}&apos;?.
        /// </summary>
        internal static string QueryInstallUntrustedPackage {
            get {
                return ResourceManager.GetString("QueryInstallUntrustedPackage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Should the package install script at &apos;{0}&apos; be processed?.
        /// </summary>
        internal static string QueryShouldThePackageScriptAtBeProcessed {
            get {
                return ResourceManager.GetString("QueryShouldThePackageScriptAtBeProcessed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Should the package uninstall script at &apos;{0}&apos; be processed?.
        /// </summary>
        internal static string QueryShouldThePackageUninstallScriptAtBeProcessed {
            get {
                return ResourceManager.GetString("QueryShouldThePackageUninstallScriptAtBeProcessed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Save Package.
        /// </summary>
        internal static string SavePackage {
            get {
                return ResourceManager.GetString("SavePackage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ??? not used ???.
        /// </summary>
        internal static string ShouldContinueWithUntrustedPackageSource {
            get {
                return ResourceManager.GetString("ShouldContinueWithUntrustedPackageSource", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Skipping package provider provider &apos;{0}&apos;-- missing required option &apos;{1}&apos;.
        /// </summary>
        internal static string SkippedProviderMissingRequiredOption {
            get {
                return ResourceManager.GetString("SkippedProviderMissingRequiredOption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The package source &apos;{0}&apos; was found in multiple providers ({1})..
        /// </summary>
        internal static string SourceFoundInMultipleProviders {
            get {
                return ResourceManager.GetString("SourceFoundInMultipleProviders", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find package source &apos;{0}&apos;..
        /// </summary>
        internal static string SourceNotFound {
            get {
                return ResourceManager.GetString("SourceNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find package source for location &apos;{0}&apos;..
        /// </summary>
        internal static string SourceNotFoundForLocation {
            get {
                return ResourceManager.GetString("SourceNotFoundForLocation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find package sources..
        /// </summary>
        internal static string SourceNotFoundNoCriteria {
            get {
                return ResourceManager.GetString("SourceNotFoundNoCriteria", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Package &apos;{0}&apos; v&apos;{1}&apos; from &apos;{2}&apos;.
        /// </summary>
        internal static string TargetPackage {
            get {
                return ResourceManager.GetString("TargetPackage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Package Source &apos;{0}&apos; ({1}) in provider &apos;{2}&apos;.
        /// </summary>
        internal static string TargetPackageSource {
            get {
                return ResourceManager.GetString("TargetPackageSource", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find package provider for package source &apos;{0}&apos;..
        /// </summary>
        internal static string UnableToFindProviderForSource {
            get {
                return ResourceManager.GetString("UnableToFindProviderForSource", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to remove package file &apos;{0}&apos;. .
        /// </summary>
        internal static string UnableToOverwrite {
            get {
                return ResourceManager.GetString("UnableToOverwrite", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Package {0} failed to uninstall..
        /// </summary>
        internal static string UninstallationFailure {
            get {
                return ResourceManager.GetString("UninstallationFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find package provider &apos;{0}&apos;..
        /// </summary>
        internal static string UnknownProvider {
            get {
                return ResourceManager.GetString("UnknownProvider", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find package providers ({0}..
        /// </summary>
        internal static string UnknownProviders {
            get {
                return ResourceManager.GetString("UnknownProviders", resourceCulture);
            }
        }
    }
}
