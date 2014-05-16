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

namespace Microsoft.OneGet.PackageProvider.Test.Utility {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Callback = System.Func<string, System.Collections.Generic.IEnumerable<object>, object>;

    //
    // Core and Host APIs are used to interact with the installation host
    // and any user interaction.
    // 
    #region copy core-apis

    // Core Callbacks that we'll both use internally and pass on down to providers.
    public delegate bool Warning( string message, IEnumerable<object> args = null);

    public delegate bool Error(string message, IEnumerable<object> args = null);

    public delegate bool Message(string message, IEnumerable<object> args = null);

    public delegate bool Verbose(string message, IEnumerable<object> args = null);

    public delegate bool Debug(string message, IEnumerable<object> args = null);

    public delegate bool ExceptionThrown(string exceptionType, string message, string stacktrace);

    public delegate int StartProgress(int parentActivityId, string message, IEnumerable<object> args = null);

    public delegate bool Progress(int activityId, int progress, string message, IEnumerable<object> args = null);

    public delegate bool CompleteProgress(int activityId, bool isSuccessful);

    public delegate Callback GetHostDelegate();

    /// <summary>
    ///     The provider can query to see if the operation has been cancelled.
    ///     This provides for a gentle way for the caller to notify the callee that
    ///     they don't want any more results.
    /// </summary>
    /// <returns>returns TRUE if the operation has been cancelled.</returns>
    public delegate bool IsCancelled();
    #endregion

    //
    // Core and Host APIs are used to interact with the installation host
    // and any user interaction.
    // 
    #region copy host-apis

    /// <summary>
    ///     Used by a provider to request what metadata keys were passed from the user
    /// </summary>
    /// <returns></returns>
    public delegate IEnumerable<string> GetOptionKeys(string category);

    public delegate IEnumerable<string> GetOptionValues(string category, string key);

    public delegate IEnumerable<string> PackageSources();

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
    public delegate IEnumerable<string> GetConfiguration(string path);

    public delegate bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource);

    public delegate bool ShouldProcessPackageInstall(string packageName, string version, string source);

    public delegate bool ShouldProcessPackageUninstall(string packageName, string version);

    public delegate bool ShouldContinueAfterPackageInstallFailure(string packageName, string version, string source);

    public delegate bool ShouldContinueAfterPackageUninstallFailure(string packageName, string version, string source);

    public delegate bool ShouldContinueRunningInstallScript(string packageName, string version, string source, string scriptLocation);

    public delegate bool ShouldContinueRunningUninstallScript(string packageName, string version, string source, string scriptLocation);

    public delegate bool AskPermission(string permission);

    public delegate bool WhatIf();
    #endregion

    //
    // Service APIs are used to talk to lower-level services 
    // These are only needed if you want access to the commonly 
    // provided services
    // 
    #region copy service-apis

    public delegate string GetNuGetExePath();

    public delegate string GetNuGetDllPath();

    public delegate string DownloadFile(string remoteLocation, string localLocation);

    public delegate void AddPinnedItemToTaskbar(string item);

    public delegate void RemovePinnedItemFromTaskbar(string item);

    public delegate bool CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments);

    public delegate IEnumerable<string> UnzipFileIncremental(string zipFile, string folder);

    public delegate IEnumerable<string> UnzipFile(string zipFile, string folder);

    public delegate void AddFileAssociation();

    public delegate void RemoveFileAssociation();

    public delegate void AddExplorerMenuItem();

    public delegate void RemoveExplorerMenuItem();

    public delegate bool SetEnvironmentVariable(string variable, string value, string context);

    public delegate bool RemoveEnvironmentVariable(string variable, string context);

    public delegate void AddFolderToPath();

    public delegate void RemoveFolderFromPath();

    public delegate void InstallMSI();

    public delegate void RemoveMSI();

    public delegate void StartProcess();

    public delegate void InstallVSIX();

    public delegate void UninstallVSIX();

    public delegate void InstallPowershellScript();

    public delegate void UninstallPowershellScript();

    public delegate void SearchForExecutable();

    public delegate void GetUserBinFolder();

    public delegate void GetSystemBinFolder();

    public delegate bool CopyFile(string sourcePath, string destinationPath);

    public delegate void CopyFolder();

    public delegate void Delete(string path);

    public delegate void DeleteFolder(string folder);

    public delegate void CreateFolder(string folder);

    public delegate void DeleteFile(string filename);

    public delegate void BeginTransaction();

    public delegate void AbortTransaction();

    public delegate void EndTransaction();

    public delegate void GenerateUninstallScript();

    public delegate string GetKnownFolder(string knownFolder);

    public delegate bool IsElevated();
    #endregion

    //
    // Service APIs are used to send information back from a request
    // 
    #region copy request-apis

    /// <summary>
    ///     The provider can query to see if the operation has been cancelled.
    ///     This provides for a gentle way for the caller to notify the callee that
    ///     they don't want any more results. It's essentially just !IsCancelled()
    /// </summary>
    /// <returns>returns FALSE if the operation has been cancelled.</returns>
    public delegate bool OkToContinue();

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
    public delegate bool YieldPackage(string fastPath, string name, string version, string versionScheme, string summary, string source);

    public delegate bool YieldPackageDetails(object serializablePackageDetailsObject);

    public delegate bool YieldPackageSwidtag(string fastPath, string xmlOrJsonDoc);

    /// <summary>
    ///     Used by a provider to return fields for a package source (repository)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    public delegate bool YieldSource(string name, string location, bool isTrusted);

    /// <summary>
    ///     Used by a provider to return the fields for a Metadata Definition
    ///     The cmdlets can use this to supply tab-completion for metadata to the user.
    /// </summary>
    /// <param name="category"> one of ['provider', 'source', 'package', 'install']</param>
    /// <param name="name">the provider-defined name of the option</param>
    /// <param name="expectedType"> one of ['string','int','path','switch']</param>
    /// <param name="permittedValues">either a collection of permitted values, or null for any valid value</param>
    /// <returns></returns>
    public delegate bool YieldOptionDefinition(OptionCategory category, string name, OptionType expectedType, bool isRequired, IEnumerable<string> permittedValues);
    #endregion

}