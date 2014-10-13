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

namespace Microsoft.OneGet.Implementation {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security;
    using Api;
    using Utility.Extensions;
    using IRequestObject = System.Object;

    public abstract class Request : IRequest, IDisposable {
        #region copy core-apis

        public abstract bool IsCanceled {get;}

        /* Synced/Generated code =================================================== */

        /// <summary>
        ///     Returns the interface type for a Request that the OneGet Core is expecting
        ///     This is (currently) neccessary to provide an appropriately-typed version
        ///     of the Request to the core when a Plugin is calling back into the core
        ///     and has to pass a request object.
        /// </summary>
        /// <returns></returns>
        public abstract Type GetIRequestInterface();

        /// <summary>
        ///     Returns the internal version of the OneGet core.
        ///     This will usually only be updated if there is a breaking API or Interface change that might
        ///     require other code to know which version is running.
        /// </summary>
        /// <returns>Internal Version of OneGet</returns>
        public abstract int CoreVersion();

        public abstract bool NotifyBeforePackageInstall(string packageName, string version, string source, string destination);

        public abstract bool NotifyPackageInstalled(string packageName, string version, string source, string destination);

        public abstract bool NotifyBeforePackageUninstall(string packageName, string version, string source, string destination);

        public abstract bool NotifyPackageUninstalled(string packageName, string version, string source, string destination);

        public abstract IEnumerable<string> ProviderNames {get;}

        public abstract IEnumerable<PackageProvider> PackageProviders {get;}

        public abstract IEnumerable<PackageProvider> SelectProvidersWithFeature(string featureName);

        public abstract IEnumerable<PackageProvider> SelectProvidersWithFeature(string featureName, string value);

        public abstract IEnumerable<PackageProvider> SelectProviders(string providerName, IRequestObject requestObject);

        public abstract bool RequirePackageProvider(string requestor, string packageProviderName, string minimumVersion, IRequestObject requestObject);

        public abstract string GetCanonicalPackageId(string providerName, string packageName, string version);

        public abstract string ParseProviderName(string canonicalPackageId);

        public abstract string ParsePackageName(string canonicalPackageId);

        public abstract string ParsePackageVersion(string canonicalPackageId);

        #endregion

        #region copy host-apis

        /* Synced/Generated code =================================================== */
        public abstract string GetMessageString(string messageText);

        public abstract bool Warning(string messageText);

        public abstract bool Error(string id, string category, string targetObjectValue, string messageText);

        public abstract bool Message(string messageText);

        public abstract bool Verbose(string messageText);

        public abstract bool Debug(string messageText);

        public abstract int StartProgress(int parentActivityId, string messageText);

        public abstract bool Progress(int activityId, int progressPercentage, string messageText);

        public abstract bool CompleteProgress(int activityId, bool isSuccessful);

        /// <summary>
        ///     Used by a provider to request what metadata keys were passed from the user
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<string> GetOptionKeys();

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract IEnumerable<string> GetOptionValues(string key);

        public abstract IEnumerable<string> GetSources();

        public abstract string GetCredentialUsername();

        public abstract string GetCredentialPassword();

        public abstract bool ShouldBootstrapProvider(string requestor, string providerName, string providerVersion, string providerType, string location, string destination);

        public abstract bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource);

        public abstract bool AskPermission(string permission);

        public abstract bool IsInteractive();

        public abstract int CallCount();

        #endregion

        #region copy response-apis

        /* Synced/Generated code =================================================== */

        /// <summary>
        ///     Used by a provider to return fields for a SoftwareIdentity.
        /// </summary>
        /// <param name="fastPath"></param>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="versionScheme"></param>
        /// <param name="summary"></param>
        /// <param name="source"></param>
        /// <param name="searchKey"></param>
        /// <param name="fullPath"></param>
        /// <param name="packageFileName"></param>
        /// <returns></returns>
        public abstract bool YieldSoftwareIdentity(string fastPath, string name, string version, string versionScheme, string summary, string source, string searchKey, string fullPath, string packageFileName);

        public abstract bool YieldSoftwareMetadata(string parentFastPath, string name, string value);

        public abstract bool YieldEntity(string parentFastPath, string name, string regid, string role, string thumbprint);

        public abstract bool YieldLink(string parentFastPath, string referenceUri, string relationship, string mediaType, string ownership, string use, string appliesToMedia, string artifact);

#if M2
        public abstract bool YieldSwidtag(string fastPath, string xmlOrJsonDoc);

        public abstract bool YieldMetadata(string fieldId, string @namespace, string name, string value);

        #endif

        /// <summary>
        ///     Used by a provider to return fields for a package source (repository)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="location"></param>
        /// <param name="isTrusted"></param>
        /// <param name="isRegistered"></param>
        /// <param name="isValidated"></param>
        /// <returns></returns>
        public abstract bool YieldPackageSource(string name, string location, bool isTrusted, bool isRegistered, bool isValidated);

        /// <summary>
        ///     Used by a provider to return the fields for a Metadata Definition
        ///     The cmdlets can use this to supply tab-completion for metadata to the user.
        /// </summary>
        /// <param name="name">the provider-defined name of the option</param>
        /// <param name="expectedType"> one of ['string','int','path','switch']</param>
        /// <param name="isRequired">if the parameter is mandatory</param>
        /// <returns></returns>
        public abstract bool YieldDynamicOption(string name, string expectedType, bool isRequired);

        public abstract bool YieldKeyValuePair(string key, string value);

        public abstract bool YieldValue(string value);

        #endregion

        #region copy service-apis

        /* Synced/Generated code =================================================== */
        public abstract bool IsElevated {get;}

        public abstract void DownloadFile(Uri remoteLocation, string localFilename, IRequestObject requestObject);

        public abstract bool IsSupportedArchive(string localFilename, IRequestObject requestObject);

        public abstract IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, IRequestObject requestObject);

        public abstract void AddPinnedItemToTaskbar(string item, IRequestObject requestObject);

        public abstract void RemovePinnedItemFromTaskbar(string item, IRequestObject requestObject);

        public abstract void CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, IRequestObject requestObject);

        public abstract void SetEnvironmentVariable(string variable, string value, string context, IRequestObject requestObject);

        public abstract void RemoveEnvironmentVariable(string variable, string context, IRequestObject requestObject);

        public abstract void CopyFile(string sourcePath, string destinationPath, IRequestObject requestObject);

        public abstract void Delete(string path, IRequestObject requestObject);

        public abstract void DeleteFolder(string folder, IRequestObject requestObject);

        public abstract void CreateFolder(string folder, IRequestObject requestObject);

        public abstract void DeleteFile(string filename, IRequestObject requestObject);

        public abstract string GetKnownFolder(string knownFolder, IRequestObject requestObject);

        public abstract string CanonicalizePath(string text, string currentDirectory);

        public abstract bool FileExists(string path);

        public abstract bool DirectoryExists(string path);

        public abstract bool Install(string fileName, string additionalArgs, IRequestObject requestObject);

        public abstract bool IsSignedAndTrusted(string filename, IRequestObject requestObject);

        public abstract bool ExecuteElevatedAction(string provider, string payload, IRequestObject requestObject);

        #endregion

        #region declare Request-implementation

        public virtual void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing) {
        }

        public bool Yield(KeyValuePair<string, string[]> pair) {
            if (pair.Value.Length == 0) {
                return YieldKeyValuePair(pair.Key, null);
            }
            return pair.Value.All(each => YieldKeyValuePair(pair.Key, each));
        }

        public bool Error(ErrorCategory category, string targetObjectValue, string messageText, params object[] args) {
            return Error(messageText, category.ToString(), targetObjectValue, FormatMessageString(messageText, args));
        }

        public bool Warning(string messageText, params object[] args) {
            return Warning(FormatMessageString(messageText, args));
        }

        public bool Message(string messageText, params object[] args) {
            return Message(FormatMessageString(messageText, args));
        }

        public bool Verbose(string messageText, params object[] args) {
            return Verbose(FormatMessageString(messageText, args));
        }

        public bool Debug(string messageText, params object[] args) {
            return Debug(FormatMessageString(messageText, args));
        }

        public int StartProgress(int parentActivityId, string messageText, params object[] args) {
            return StartProgress(parentActivityId, FormatMessageString(messageText, args));
        }

        public bool Progress(int activityId, int progressPercentage, string messageText, params object[] args) {
            return Progress(activityId, progressPercentage, FormatMessageString(messageText, args));
        }

        public string GetOptionValue(string name) {
            // get the value from the request
            return (GetOptionValues(name) ?? Enumerable.Empty<string>()).LastOrDefault();
        }

        private static string FixMeFormat(string formatString, object[] args) {
            if (args == null || args.Length == 0) {
                // not really any args, and not really expectng any
                return formatString.Replace('{', '\u00ab').Replace('}', '\u00bb');
            }
            return args.Aggregate(formatString.Replace('{', '\u00ab').Replace('}', '\u00bb'), (current, arg) => current + string.Format(CultureInfo.CurrentCulture, " \u00ab{0}\u00bb", arg));
        }

        public SecureString Password {
            get {
                var p = GetCredentialPassword();
                if (p == null) {
                    return null;
                }
                return p.FromProtectedString("salt");
            }
        }

        public string Username {
            get {
                return GetCredentialUsername();
            }
        }

        #endregion

        private string FormatMessageString(string messageText, params object[] args) {
            if (string.IsNullOrEmpty(messageText)) {
                return string.Empty;
            }

            if (messageText.StartsWith(Constants.MSGPrefix, true, CultureInfo.CurrentCulture)) {
                // check with the caller first, then with the local resources, and fallback to using the messageText itself.
                messageText = GetMessageString(messageText.Substring(Constants.MSGPrefix.Length)) ?? messageText;
            }

            // if it doesn't look like we have the correct number of parameters
            // let's return a fixmeformat string.
            var c = messageText.ToCharArray().Where(each => each == '{').Count();
            if (c < args.Length) {
                return FixMeFormat(messageText, args);
            }
            return string.Format(CultureInfo.CurrentCulture, messageText, args);
        }
    }
}