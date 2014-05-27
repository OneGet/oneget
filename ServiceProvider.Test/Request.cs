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

namespace Microsoft.OneGet.ServiceProvider.Test {
    using System;
    using System.Collections.Generic;

    public abstract class Request : IDisposable {
        private static dynamic _dynamicInterface;

        public void Dispose() {
        }

        #region copy core-apis

        // Core Callbacks that we'll both use internally and pass on down to providers.
        public abstract bool Warning(string message, params object[] args);

        public abstract bool Error(string message, params object[] args);

        public abstract bool Message(string message, params object[] args);

        public abstract bool Verbose(string message, params object[] args);

        public abstract bool Debug(string message, params object[] args);

        public abstract bool ExceptionThrown(string exceptionType, string message, string stacktrace);

        public abstract int StartProgress(int parentActivityId, string message, params object[] args);

        public abstract bool Progress(int activityId, int progress, string message, params object[] args);

        public abstract bool CompleteProgress(int activityId, bool isSuccessful);

        /// <summary>
        ///     The provider can query to see if the operation has been cancelled.
        ///     This provides for a gentle way for the caller to notify the callee that
        ///     they don't want any more results.
        /// </summary>
        /// <returns>returns TRUE if the operation has been cancelled.</returns>
        public abstract bool IsCancelled();

        #endregion

        #region copy host-apis

        /// <summary>
        ///     Used by a provider to request what metadata keys were passed from the user
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<string> GetOptionKeys(string category);

        public abstract IEnumerable<string> GetOptionValues(string category, string key);

        public abstract IEnumerable<string> PackageSources();

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
        public abstract IEnumerable<string> GetConfiguration(string path);

        public abstract bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource);

        public abstract bool ShouldProcessPackageInstall(string packageName, string version, string source);

        public abstract bool ShouldProcessPackageUninstall(string packageName, string version);

        public abstract bool ShouldContinueAfterPackageInstallFailure(string packageName, string version, string source);

        public abstract bool ShouldContinueAfterPackageUninstallFailure(string packageName, string version, string source);

        public abstract bool ShouldContinueRunningInstallScript(string packageName, string version, string source, string scriptLocation);

        public abstract bool ShouldContinueRunningUninstallScript(string packageName, string version, string source, string scriptLocation);

        public abstract bool AskPermission(string permission);

        public abstract bool WhatIf();

        #endregion

        #region copy service-apis

        public abstract string GetNuGetExePath(Object c);

        public abstract string GetNuGetDllPath(Object c);

        public abstract string DownloadFile(string remoteLocation, string localLocation, Object c);

        public abstract void AddPinnedItemToTaskbar(string item, Object c);

        public abstract void RemovePinnedItemFromTaskbar(string item, Object c);

        public abstract bool CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, Object c);

        public abstract IEnumerable<string> UnzipFileIncremental(string zipFile, string folder, Object c);

        public abstract IEnumerable<string> UnzipFile(string zipFile, string folder, Object c);

        public abstract void AddFileAssociation();

        public abstract void RemoveFileAssociation();

        public abstract void AddExplorerMenuItem();

        public abstract void RemoveExplorerMenuItem();

        public abstract bool SetEnvironmentVariable(string variable, string value, string context, Object c);

        public abstract bool RemoveEnvironmentVariable(string variable, string context, Object c);

        public abstract void AddFolderToPath();

        public abstract void RemoveFolderFromPath();

        public abstract void InstallMSI();

        public abstract void RemoveMSI();

        public abstract void StartProcess();

        public abstract void InstallVSIX();

        public abstract void UninstallVSIX();

        public abstract void InstallPowershellScript();

        public abstract void UninstallPowershellScript();

        public abstract void SearchForExecutable();

        public abstract void GetUserBinFolder();

        public abstract void GetSystemBinFolder();

        public abstract bool CopyFile(string sourcePath, string destinationPath, Object c);

        public abstract void CopyFolder();

        public abstract void Delete(string path, Object c);

        public abstract void DeleteFolder(string folder, Object c);

        public abstract void CreateFolder(string folder, Object c);

        public abstract void DeleteFile(string filename, Object c);

        public abstract void BeginTransaction();

        public abstract void AbortTransaction();

        public abstract void EndTransaction();

        public abstract void GenerateUninstallScript();

        public abstract string GetKnownFolder(string knownFolder, Object c);

        public abstract bool IsElevated(Object c);

        public abstract object GetPackageManagementService(Object c);

        #endregion

        #region copy request-apis

        /// <summary>
        ///     The provider can query to see if the operation has been cancelled.
        ///     This provides for a gentle way for the caller to notify the callee that
        ///     they don't want any more results. It's essentially just !IsCancelled
        /// </summary>
        /// <returns>returns FALSE if the operation has been cancelled.</returns>
        public abstract bool OkToContinue();

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
        public abstract bool YieldPackage(string fastPath, string name, string version, string versionScheme, string summary, string source);

        public abstract bool YieldPackageDetails(object serializablePackageDetailsObject);

        public abstract bool YieldPackageSwidtag(string fastPath, string xmlOrJsonDoc);

        /// <summary>
        ///     Used by a provider to return fields for a package source (repository)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public abstract bool YieldSource(string name, string location, bool isTrusted);

        /// <summary>
        ///     Used by a provider to return the fields for a Metadata Definition
        ///     The cmdlets can use this to supply tab-completion for metadata to the user.
        /// </summary>
        /// <param name="category"> one of ['provider', 'source', 'package', 'install']</param>
        /// <param name="name">the provider-defined name of the option</param>
        /// <param name="expectedType"> one of ['string','int','path','switch']</param>
        /// <param name="permittedValues">either a collection of permitted values, or null for any valid value</param>
        /// <returns></returns>
        public abstract bool YieldDynamicOption(int category, string name, int expectedType, bool isRequired, IEnumerable<string> permittedValues);

        #endregion

        public static Request New(Object c) {
            return _dynamicInterface.Create<Request>(c);
        }

        public void InitializeProvider(dynamic dynamicInterface, Object c) {
            _dynamicInterface = dynamicInterface;
        }
    }
}