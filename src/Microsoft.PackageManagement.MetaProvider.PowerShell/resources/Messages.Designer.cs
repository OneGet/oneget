//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a New-StronglyTypedCsFileForResx funciton.
//     To add or remove a member, edit your .ResX file then rerun buildCoreClr.ps1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.PackageManagement.MetaProvider.PowerShell.Internal.Resources {
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
                global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.PackageManagement.MetaProvider.PowerShell.Internal.Resources.Messages", typeof(Messages).GetTypeInfo().Assembly);
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
    ///   Looks up a localized string similar to Fail to import the provider '{0}' because the provider with the same name also exists from path '{1}'.
    /// </summary>
    internal static string DuplicateProviderName {
        get {
            return ResourceManager.GetString("DuplicateProviderName", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Import-PackageProvider '{0}' failed.
    /// </summary>
    internal static string FailedToImportProvider {
        get {
            return ResourceManager.GetString("FailedToImportProvider", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to File '{0}' not found.
    /// </summary>
    internal static string FileNotFound {
        get {
            return ResourceManager.GetString("FileNotFound", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Import-Module failed: '{0}'.
    /// </summary>
    internal static string ImportModuleFailed {
        get {
            return ResourceManager.GetString("ImportModuleFailed", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to The module '{0}' is already loaded.
    /// </summary>
    internal static string ModuleAlreadyLoaded {
        get {
            return ResourceManager.GetString("ModuleAlreadyLoaded", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Cannot find provider '{0}' under the specified path.
    /// </summary>
    internal static string ModuleNotFound {
        get {
            return ResourceManager.GetString("ModuleNotFound", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to 'PackageManagementProviders' key is not found in the .psd1 file under '{0}'.
    /// </summary>
    internal static string PackageManagementProvidersNotFound {
        get {
            return ResourceManager.GetString("PackageManagementProvidersNotFound", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to PowerShell Script '{0}' Function '{1}' returns null.
    /// </summary>
    internal static string PowershellScriptFunctionReturnsNull {
        get {
            return ResourceManager.GetString("PowershellScriptFunctionReturnsNull", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unable to find provider '{0}'. Please check if the ProviderName is set to '{1}' in '{2}'. 
    /// </summary>
    internal static string ProvideNameMismatch {
        get {
            return ResourceManager.GetString("ProvideNameMismatch", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Name of the provider '{0}' is null, empty or not implemented by the provider.
    /// </summary>
    internal static string ProviderNameIsNullOrEmpty {
        get {
            return ResourceManager.GetString("ProviderNameIsNullOrEmpty", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Root module {0} does not match the packagemanagementprovider name {1}.
    /// </summary>
    internal static string RootModuleAndPackageManagementProviderNameNotMatch {
        get {
            return ResourceManager.GetString("RootModuleAndPackageManagementProviderNameNotMatch", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Script failure at : {0}
    /// </summary>
    internal static string ScriptStackTrace {
        get {
            return ResourceManager.GetString("ScriptStackTrace", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Loaded PowerShell package provider: '[{0}]'.
    /// </summary>
    internal static string SuccessfullyLoadedModule {
        get {
            return ResourceManager.GetString("SuccessfullyLoadedModule", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unable to find package provider '{0}' under the specified path.
    /// </summary>
    internal static string UnableToFindModuleProvider {
        get {
            return ResourceManager.GetString("UnableToFindModuleProvider", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unable to find 'PackageProviderFunctions.psm1' at '{0}'
    /// </summary>
    internal static string UnableToFindPowerShellFunctionsFile {
        get {
            return ResourceManager.GetString("UnableToFindPowerShellFunctionsFile", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Cannot load module '{0}' because no valid provider found under the specified path.
    /// </summary>
    internal static string UnableToLoadModule {
        get {
            return ResourceManager.GetString("UnableToLoadModule", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to Cannot find base Powershell module folder.
    /// </summary>
    internal static string CantFindBasePowerShellModuleFolder {
        get {
            return ResourceManager.GetString("CantFindBasePowerShellModuleFolder", resourceCulture);
        }
    }

}
}
