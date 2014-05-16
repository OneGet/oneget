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

namespace Microsoft.OneGet.MetaProvider.PowerShell.Utility {
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     This generated class can be copied to any project that implements a OneGet provider
    ///     This gives type-safe access to the callbacks and APIs without having to take a direct
    ///     dependency on the OneGet core Assemblies.
    /// </summary>
    internal static class CallbackExtensions {
        /// <summary>
        ///     This transforms a generic delegate into a type-specific delegate so that you can
        ///     call the target delegate with the appropriate signature.
        /// </summary>
        /// <typeparam name="TDelegate"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static TDelegate CastDelegate<TDelegate>(this Delegate src) where TDelegate : class {
            return (TDelegate)(object)Delegate.CreateDelegate(typeof(TDelegate), src.Target, src.Method, true); // throw on fail
        }

        /// <summary>
        ///     This calls the supplied delegate with the name of the callback that we're actaully looking for
        ///     and then casts the resulting delegate back to the type that we're expecting.
        /// </summary>
        /// <typeparam name="TDelegate"></typeparam>
        /// <param name="rootCallback"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static TDelegate Resolve<TDelegate>(this Func<string, IEnumerable<object>, object> rootCallback, params object[] args) where TDelegate : class {
            var delegateType = typeof(TDelegate);
            if (delegateType.BaseType != typeof(MulticastDelegate)) {
                throw new Exception("Generic Type Incorrect");
            }
            // calling with null args set returns the delegate instead of calling the delegate.
            // return CastDelegate<TDelegate>(CastDelegate<Func<string, IEnumerable<object>, Delegate>>(rootCallback)(delegateType.Name, null));
            // var m = rootCallback(delegateType.Name, null);
            var m = (Delegate)rootCallback(delegateType.Name, null);
            return m == null ? null : CastDelegate<TDelegate>(m);
        }

#if OLD_WAY
        #region generate-resolved service-apis

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static string GetNuGetExePath (this Callback c   ) {
            return (c.Resolve<GetNuGetExePath>() ?? (()=>default(string) ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static string GetNuGetDllPath (this Callback c   ) {
            return (c.Resolve<GetNuGetDllPath>() ?? (()=>default(string) ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static string DownloadFile (this Callback c , string remoteLocation, string localLocation ) {
            return (c.Resolve<DownloadFile>() ?? ((premoteLocation,plocalLocation)=>default(string) ) )(remoteLocation,localLocation);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void AddPinnedItemToTaskbar (this Callback c , string item ) {
            (c.Resolve<AddPinnedItemToTaskbar>() ?? ((pitem)=>{ } ) )(item);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void RemovePinnedItemFromTaskbar (this Callback c , string item ) {
            (c.Resolve<RemovePinnedItemFromTaskbar>() ?? ((pitem)=>{ } ) )(item);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool CreateShortcutLink (this Callback c , string linkPath, string targetPath, string description, string workingDirectory, string arguments ) {
            return (c.Resolve<CreateShortcutLink>() ?? ((plinkPath,ptargetPath,pdescription,pworkingDirectory,parguments)=>default(bool) ) )(linkPath,targetPath,description,workingDirectory,arguments);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static IEnumerable<string> UnzipFileIncremental (this Callback c , string zipFile, string folder ) {
            return (c.Resolve<UnzipFileIncremental>() ?? ((pzipFile,pfolder)=>default(IEnumerable<string>) ) )(zipFile,folder);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static IEnumerable<string> UnzipFile (this Callback c , string zipFile, string folder ) {
            return (c.Resolve<UnzipFile>() ?? ((pzipFile,pfolder)=>default(IEnumerable<string>) ) )(zipFile,folder);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void AddFileAssociation (this Callback c   ) {
            (c.Resolve<AddFileAssociation>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void RemoveFileAssociation (this Callback c   ) {
            (c.Resolve<RemoveFileAssociation>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void AddExplorerMenuItem (this Callback c   ) {
            (c.Resolve<AddExplorerMenuItem>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void RemoveExplorerMenuItem (this Callback c   ) {
            (c.Resolve<RemoveExplorerMenuItem>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool SetEnvironmentVariable (this Callback c , string variable, string value, string context ) {
            return (c.Resolve<SetEnvironmentVariable>() ?? ((pvariable,pvalue,pcontext)=>default(bool) ) )(variable,value,context);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool RemoveEnvironmentVariable (this Callback c , string variable, string context ) {
            return (c.Resolve<RemoveEnvironmentVariable>() ?? ((pvariable,pcontext)=>default(bool) ) )(variable,context);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void AddFolderToPath (this Callback c   ) {
            (c.Resolve<AddFolderToPath>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void RemoveFolderFromPath (this Callback c   ) {
            (c.Resolve<RemoveFolderFromPath>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void InstallMSI (this Callback c   ) {
            (c.Resolve<InstallMSI>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void RemoveMSI (this Callback c   ) {
            (c.Resolve<RemoveMSI>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void StartProcess (this Callback c   ) {
            (c.Resolve<StartProcess>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void InstallVSIX (this Callback c   ) {
            (c.Resolve<InstallVSIX>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void UninstallVSIX (this Callback c   ) {
            (c.Resolve<UninstallVSIX>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void InstallPowershellScript (this Callback c   ) {
            (c.Resolve<InstallPowershellScript>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void UninstallPowershellScript (this Callback c   ) {
            (c.Resolve<UninstallPowershellScript>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void SearchForExecutable (this Callback c   ) {
            (c.Resolve<SearchForExecutable>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void GetUserBinFolder (this Callback c   ) {
            (c.Resolve<GetUserBinFolder>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void GetSystemBinFolder (this Callback c   ) {
            (c.Resolve<GetSystemBinFolder>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool CopyFile (this Callback c , string sourcePath, string destinationPath ) {
            return (c.Resolve<CopyFile>() ?? ((psourcePath,pdestinationPath)=>default(bool) ) )(sourcePath,destinationPath);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void CopyFolder (this Callback c   ) {
            (c.Resolve<CopyFolder>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void Delete (this Callback c , string path ) {
            (c.Resolve<Delete>() ?? ((ppath)=>{ } ) )(path);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void DeleteFolder (this Callback c , string folder ) {
            (c.Resolve<DeleteFolder>() ?? ((pfolder)=>{ } ) )(folder);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void CreateFolder (this Callback c , string folder ) {
            (c.Resolve<CreateFolder>() ?? ((pfolder)=>{ } ) )(folder);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void DeleteFile (this Callback c , string filename ) {
            (c.Resolve<DeleteFile>() ?? ((pfilename)=>{ } ) )(filename);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void BeginTransaction (this Callback c   ) {
            (c.Resolve<BeginTransaction>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void AbortTransaction (this Callback c   ) {
            (c.Resolve<AbortTransaction>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void EndTransaction (this Callback c   ) {
            (c.Resolve<EndTransaction>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static void GenerateUninstallScript (this Callback c   ) {
            (c.Resolve<GenerateUninstallScript>() ?? (()=>{ } ) )();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static string GetKnownFolder (this Callback c , string knownFolder ) {
            return (c.Resolve<GetKnownFolder>() ?? ((pknownFolder)=>default(string) ) )(knownFolder);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool IsElevated (this Callback c   ) {
            return (c.Resolve<IsElevated>() ?? (()=>default(bool) ) )();
        }

        #endregion

        #region generate-resolved core-apis

        // Core Callbacks that we'll both use internally and pass on down to providers.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool Warning (this Callback c ,  string message, IEnumerable<object> args = null ) {
            return (c.Resolve<Warning>() ?? ((pmessage,pargs)=>default(bool) ) )(message,args);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool Error (this Callback c , string message, IEnumerable<object> args = null ) {
            return (c.Resolve<Error>() ?? ((pmessage,pargs)=>default(bool) ) )(message,args);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool Message (this Callback c , string message, IEnumerable<object> args = null ) {
            return (c.Resolve<Message>() ?? ((pmessage,pargs)=>default(bool) ) )(message,args);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool Verbose (this Callback c , string message, IEnumerable<object> args = null ) {
            return (c.Resolve<Verbose>() ?? ((pmessage,pargs)=>default(bool) ) )(message,args);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool Debug (this Callback c , string message, IEnumerable<object> args = null ) {
            return (c.Resolve<Debug>() ?? ((pmessage,pargs)=>default(bool) ) )(message,args);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool ExceptionThrown (this Callback c , string exceptionType, string message, string stacktrace ) {
            return (c.Resolve<ExceptionThrown>() ?? ((pexceptionType,pmessage,pstacktrace)=>default(bool) ) )(exceptionType,message,stacktrace);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static int StartProgress (this Callback c , int parentActivityId, string message, IEnumerable<object> args = null ) {
            return (c.Resolve<StartProgress>() ?? ((pparentActivityId,pmessage,pargs)=>default(int) ) )(parentActivityId,message,args);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool Progress (this Callback c , int activityId, int progress, string message, IEnumerable<object> args = null ) {
            return (c.Resolve<Progress>() ?? ((pactivityId,pprogress,pmessage,pargs)=>default(bool) ) )(activityId,progress,message,args);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool CompleteProgress (this Callback c , int activityId, bool isSuccessful ) {
            return (c.Resolve<CompleteProgress>() ?? ((pactivityId,pisSuccessful)=>default(bool) ) )(activityId,isSuccessful);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static Callback GetHostDelegate (this Callback c   ) {
            return (c.Resolve<GetHostDelegate>() ?? (()=>default(Callback) ) )();
        }

        /// <summary>
    ///     The provider can query to see if the operation has been cancelled.
    ///     This provides for a gentle way for the caller to notify the callee that
    ///     they don't want any more results.
    /// </summary>
    /// <returns>returns TRUE if the operation has been cancelled.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool IsCancelled (this Callback c   ) {
            return (c.Resolve<IsCancelled>() ?? (()=>default(bool) ) )();
        }

        #endregion

        #region generate-resolved request-apis

        /// <summary>
    ///     The provider can query to see if the operation has been cancelled.
    ///     This provides for a gentle way for the caller to notify the callee that
    ///     they don't want any more results. It's essentially just !IsCancelled()
    /// </summary>
    /// <returns>returns FALSE if the operation has been cancelled.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool OkToContinue (this Callback c   ) {
            return (c.Resolve<OkToContinue>() ?? (()=>default(bool) ) )();
        }

        /// <summary>
    ///     Used by a provider to return fields for a SoftwareIdentity.
    /// </summary>
    /// <param name="fastPath"></param>
    /// <param name="name"></param>
    /// <param name="version"></param>
    /// <param name="versionScheme"></param>
    /// <param name="summary"></param>
    /// <param name="source"></param>
    /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool YieldPackage (this Callback c , string fastPath, string name, string version, string versionScheme, string summary, string source ) {
            return (c.Resolve<YieldPackage>() ?? ((pfastPath,pname,pversion,pversionScheme,psummary,psource)=>default(bool) ) )(fastPath,name,version,versionScheme,summary,source);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool YieldPackageDetails (this Callback c , object serializablePackageDetailsObject ) {
            return (c.Resolve<YieldPackageDetails>() ?? ((pserializablePackageDetailsObject)=>default(bool) ) )(serializablePackageDetailsObject);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool YieldPackageSwidtag (this Callback c , string fastPath, string xmlOrJsonDoc ) {
            return (c.Resolve<YieldPackageSwidtag>() ?? ((pfastPath,pxmlOrJsonDoc)=>default(bool) ) )(fastPath,xmlOrJsonDoc);
        }

        /// <summary>
    ///     Used by a provider to return fields for a package source (repository)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="location"></param>
    /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool YieldSource (this Callback c , string name, string location, bool isTrusted ) {
            return (c.Resolve<YieldSource>() ?? ((pname,plocation,pisTrusted)=>default(bool) ) )(name,location,isTrusted);
        }

        /// <summary>
    ///     Used by a provider to return the fields for a Metadata Definition
    ///     The cmdlets can use this to supply tab-completion for metadata to the user.
    /// </summary>
    /// <param name="category"> one of ['provider', 'source', 'package', 'install']</param>
    /// <param name="name">the provider-defined name of the option</param>
    /// <param name="expectedType"> one of ['string','int','path','switch']</param>
    /// <param name="permittedValues">either a collection of permitted values, or null for any valid value</param>
    /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool YieldOptionDefinition (this Callback c , OptionCategory category, string name, OptionType expectedType, bool isRequired, IEnumerable<string> permittedValues ) {
            return (c.Resolve<YieldOptionDefinition>() ?? ((pcategory,pname,pexpectedType,pisRequired,ppermittedValues)=>default(bool) ) )(category,name,expectedType,isRequired,permittedValues);
        }

        #endregion

        #region generate-resolved host-apis

        /// <summary>
    ///     Used by a provider to request what metadata keys were passed from the user
    /// </summary>
    /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static IEnumerable<string> GetOptionKeys (this Callback c , string category ) {
            return (c.Resolve<GetOptionKeys>() ?? ((pcategory)=>default(IEnumerable<string>) ) )(category);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static IEnumerable<string> GetOptionValues (this Callback c , string category, string key ) {
            return (c.Resolve<GetOptionValues>() ?? ((pcategory,pkey)=>default(IEnumerable<string>) ) )(category,key);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static IEnumerable<string> PackageSources (this Callback c   ) {
            return (c.Resolve<PackageSources>() ?? (()=>default(IEnumerable<string>) ) )();
        }

        /// <summary>
    ///     Returns a string collection of values from a specified path in a hierarchal
    ///     configuration hashtable.
    /// </summary>
    /// <param name="path">
    ///     Path to the configuration key. Nodes are traversed by specifying a '/' character:
    ///     Example: "Providers/Module" ""
    /// </param>
    /// <returns>
    ///     A collection of string values from the configuration.
    ///     Returns an empty collection if no data is found for that path
    /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static IEnumerable<string> GetConfiguration (this Callback c , string path ) {
            return (c.Resolve<GetConfiguration>() ?? ((ppath)=>default(IEnumerable<string>) ) )(path);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool ShouldContinueWithUntrustedPackageSource (this Callback c , string package, string packageSource ) {
            return (c.Resolve<ShouldContinueWithUntrustedPackageSource>() ?? ((ppackage,ppackageSource)=>default(bool) ) )(package,packageSource);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool ShouldProcessPackageInstall (this Callback c , string packageName, string version, string source ) {
            return (c.Resolve<ShouldProcessPackageInstall>() ?? ((ppackageName,pversion,psource)=>default(bool) ) )(packageName,version,source);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool ShouldProcessPackageUninstall (this Callback c , string packageName, string version ) {
            return (c.Resolve<ShouldProcessPackageUninstall>() ?? ((ppackageName,pversion)=>default(bool) ) )(packageName,version);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool ShouldContinueAfterPackageInstallFailure (this Callback c , string packageName, string version, string source ) {
            return (c.Resolve<ShouldContinueAfterPackageInstallFailure>() ?? ((ppackageName,pversion,psource)=>default(bool) ) )(packageName,version,source);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool ShouldContinueAfterPackageUninstallFailure (this Callback c , string packageName, string version, string source ) {
            return (c.Resolve<ShouldContinueAfterPackageUninstallFailure>() ?? ((ppackageName,pversion,psource)=>default(bool) ) )(packageName,version,source);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool ShouldContinueRunningInstallScript (this Callback c , string packageName, string version, string source, string scriptLocation ) {
            return (c.Resolve<ShouldContinueRunningInstallScript>() ?? ((ppackageName,pversion,psource,pscriptLocation)=>default(bool) ) )(packageName,version,source,scriptLocation);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool ShouldContinueRunningUninstallScript (this Callback c , string packageName, string version, string source, string scriptLocation ) {
            return (c.Resolve<ShouldContinueRunningUninstallScript>() ?? ((ppackageName,pversion,psource,pscriptLocation)=>default(bool) ) )(packageName,version,source,scriptLocation);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool AskPermission (this Callback c , string permission ) {
            return (c.Resolve<AskPermission>() ?? ((ppermission)=>default(bool) ) )(permission);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Generated Code")]
        public static bool WhatIf (this Callback c   ) {
            return (c.Resolve<WhatIf>() ?? (()=>default(bool) ) )();
        }

        #endregion

#endif 
    }
}