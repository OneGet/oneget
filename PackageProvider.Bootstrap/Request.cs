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

namespace Microsoft.OneGet.PackageProvider.Bootstrap {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security;
    using System.Threading;
    using System.Xml;
    using System.Xml.Linq;
    using Resources;
    using Utility.Extensions;
    using Utility.Versions;
    using Utility.Xml;
    using RequestImpl = System.Object; 

    public abstract class Request : IDisposable {
        #region copy core-apis

        /* Synced/Generated code =================================================== */
        /// <summary>
        ///     The provider can query to see if the operation has been cancelled.
        ///     This provides for a gentle way for the caller to notify the callee that
        ///     they don't want any more results.
        /// </summary>
        /// <returns>returns TRUE if the operation has been cancelled.</returns>
        public abstract bool IsCancelled();

        /// <summary>
        ///     Returns a reference to the PackageManagementService API
        ///     The consumer of this function should either use this as a dynamic object
        ///     Or DuckType it to an interface that resembles IPacakgeManagementService
        /// </summary>
        /// <param name="requestImpl"></param>
        /// <returns></returns>
        public abstract object GetPackageManagementService(RequestImpl requestImpl);

        /// <summary>
        ///     Returns the interface type for a Request that the OneGet Core is expecting
        ///     This is (currently) neccessary to provide an appropriately-typed version
        ///     of the Request to the core when a Plugin is calling back into the core
        ///     and has to pass a request object.
        /// </summary>
        /// <returns></returns>
        public abstract Type GetIRequestInterface();

        /// <summary>
        /// Returns the internal version of the OneGet core.
        /// 
        /// This will usually only be updated if there is a breaking API or Interface change that might 
        /// require other code to know which version is running.
        /// </summary>
        /// <returns>Internal Version of OneGet</returns>
        public abstract int CoreVersion();

        public abstract bool NotifyBeforePackageInstall(string packageName, string version, string source, string destination);

        public abstract bool NotifyPackageInstalled(string packageName, string version, string source, string destination);

        public abstract bool NotifyBeforePackageUninstall(string packageName, string version, string source, string destination);

        public abstract bool NotifyPackageUninstalled(string packageName, string version, string source, string destination);

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

        public abstract IEnumerable<string> GetOptionValues(string category, string key);

        /// <summary>
        ///     Used by a provider to request what metadata keys were passed from the user
        ///
        /// This is API is deprecated, use the string variant instead.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<string> GetOptionKeys(int category);

        /// <summary>
        /// 
        /// 
        /// This is API is deprecated, use the string variant instead.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract IEnumerable<string> GetOptionValues(int category, string key);

        public abstract IEnumerable<string> GetSources();

        public abstract string GetCredentialUsername();

        public abstract string GetCredentialPassword();

        public abstract bool ShouldBootstrapProvider(string requestor, string providerName, string providerVersion, string providerType, string location, string destination);

        public abstract bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource);

        public abstract bool ShouldProcessPackageInstall(string packageName, string version, string source);

        public abstract bool ShouldProcessPackageUninstall(string packageName, string version);

        public abstract bool ShouldContinueAfterPackageInstallFailure(string packageName, string version, string source);

        public abstract bool ShouldContinueAfterPackageUninstallFailure(string packageName, string version, string source);

        public abstract bool ShouldContinueRunningInstallScript(string packageName, string version, string source, string scriptLocation);

        public abstract bool ShouldContinueRunningUninstallScript(string packageName, string version, string source, string scriptLocation);

        public abstract bool AskPermission(string permission);

        public abstract bool IsInteractive();

        public abstract int CallCount();
        #endregion

        #region copy service-apis

        /* Synced/Generated code =================================================== */
        public abstract void DownloadFile(Uri remoteLocation, string localFilename, RequestImpl requestImpl);

        public abstract bool IsSupportedArchive(string localFilename, RequestImpl requestImpl);

        public abstract IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, RequestImpl requestImpl);

        public abstract void AddPinnedItemToTaskbar(string item, RequestImpl requestImpl);

        public abstract void RemovePinnedItemFromTaskbar(string item, RequestImpl requestImpl);

        public abstract void CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, RequestImpl requestImpl);

        public abstract void SetEnvironmentVariable(string variable, string value, int context, RequestImpl requestImpl);

        public abstract void RemoveEnvironmentVariable(string variable, int context, RequestImpl requestImpl);

        public abstract void CopyFile(string sourcePath, string destinationPath, RequestImpl requestImpl);

        public abstract void Delete(string path, RequestImpl requestImpl);

        public abstract void DeleteFolder(string folder, RequestImpl requestImpl);

        public abstract void CreateFolder(string folder, RequestImpl requestImpl);

        public abstract void DeleteFile(string filename, RequestImpl requestImpl);

        public abstract string GetKnownFolder(string knownFolder, RequestImpl requestImpl);

        public abstract bool IsElevated(RequestImpl requestImpl);
        #endregion

        #region copy response-apis

        /* Synced/Generated code =================================================== */

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
        public abstract bool YieldPackageSource(string name, string location, bool isTrusted,bool isRegistered, bool isValidated);

        /// <summary>
        ///     Used by a provider to return the fields for a Metadata Definition
        ///     The cmdlets can use this to supply tab-completion for metadata to the user.
        /// </summary>
        /// <param name="category"> one of ['provider', 'source', 'package', 'install']</param>
        /// <param name="name">the provider-defined name of the option</param>
        /// <param name="expectedType"> one of ['string','int','path','switch']</param>
        /// <param name="isRequired">if the parameter is mandatory</param>
        /// <returns></returns>
        public abstract bool YieldDynamicOption(int category, string name, int expectedType, bool isRequired);

        public abstract bool YieldDynamicOption(string category, string name, string expectedType, bool isRequired);

        public abstract bool YieldKeyValuePair(string key, string value);

        public abstract bool YieldValue(string value);
        #endregion

        #region copy Request-implementation
/* Synced/Generated code =================================================== */

        public bool Warning(string messageText, params object[] args) {
            return Warning(FormatMessageString(messageText,args));
        }

        internal bool Error( ErrorCategory category, string targetObjectValue, string messageText, params object[] args) {
            return Error(messageText, category.ToString(), targetObjectValue, FormatMessageString(messageText, args));
        }

        internal bool ThrowError(ErrorCategory category, string targetObjectValue, string messageText, params object[] args) {
            Error(messageText, category.ToString(), targetObjectValue, FormatMessageString(messageText, args));
            throw new Exception("MSG:TerminatingError");
        }

        public bool Message(string messageText, params object[] args) {
            return Message(FormatMessageString(messageText,args));
        }

        public bool Verbose(string messageText, params object[] args) {
            return Verbose(FormatMessageString(messageText,args));
        } 

        public bool Debug(string messageText, params object[] args) {
            return Debug(FormatMessageString(messageText,args));
        }

        public int StartProgress(int parentActivityId, string messageText, params object[] args) {
            return StartProgress(parentActivityId, FormatMessageString(messageText,args));
        }

        public bool Progress(int activityId, int progressPercentage, string messageText, params object[] args) {
            return Progress(activityId, progressPercentage, FormatMessageString(messageText,args));
        }

        private static string FixMeFormat(string formatString, object[] args) {
            if (args == null || args.Length == 0 ) {
                // not really any args, and not really expectng any
                return formatString.Replace('{', '\u00ab').Replace('}', '\u00bb');
            }
            return System.Linq.Enumerable.Aggregate(args, formatString.Replace('{', '\u00ab').Replace('}', '\u00bb'), (current, arg) => current + string.Format(CultureInfo.CurrentCulture," \u00ab{0}\u00bb", arg));
        }

        internal string GetMessageStringInternal(string messageText) {
            return Messages.ResourceManager.GetString(messageText);
        }

        internal string FormatMessageString(string messageText, params object[] args) {
            if (string.IsNullOrEmpty(messageText)) {
                return string.Empty;
            }

            if (messageText.StartsWith(Constants.MSGPrefix, true, CultureInfo.CurrentCulture)) {
                // check with the caller first, then with the local resources, and fallback to using the messageText itself.
                messageText = GetMessageString(messageText.Substring(Constants.MSGPrefix.Length)) ?? GetMessageStringInternal(messageText) ?? messageText;    
            }

            // if it doesn't look like we have the correct number of parameters
            // let's return a fixmeformat string.
            var c = System.Linq.Enumerable.Count( System.Linq.Enumerable.Where(messageText.ToCharArray(), each => each == '{'));
            if (c < args.Length) {
                return FixMeFormat(messageText, args);
            }
            return string.Format(CultureInfo.CurrentCulture, messageText, args);
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
                return  GetCredentialUsername();
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing) {

        }

        public static implicit operator MarshalByRefObject(Request req) {
            return req.RemoteThis;
        }

        public static MarshalByRefObject ToMarshalByRefObject(Request request) {
            return request.RemoteThis;
        }

        internal MarshalByRefObject RemoteThis {
            get {
                return Extend();
            }
        }

        internal MarshalByRefObject Extend(params object[] objects) {
            return RequestExtensions.Extend(this, GetIRequestInterface(), objects);
        }

        internal string GetOptionValue(OptionCategory category, string name) {
            // get the value from the request
            if (CoreVersion() > 0) {
                return (GetOptionValues(category.ToString(), name) ?? Enumerable.Empty<string>()).LastOrDefault();
            }
            return (GetOptionValues((int)category, name) ?? Enumerable.Empty<string>()).LastOrDefault();
        }

        internal IEnumerable<string> GetOptionValues(OptionCategory category, string name) {
            // get the value from the request
            if (CoreVersion() > 0) {
                return (GetOptionValues(category.ToString(), name) ?? Enumerable.Empty<string>());
            }
            return (GetOptionValues((int)category, name) ?? Enumerable.Empty<string>());
        }

        public bool YieldDynamicOption(OptionCategory category, string name, OptionType expectedType, bool isRequired) {
            if (CoreVersion() > 0) {
                return YieldDynamicOption(category.ToString(), name, expectedType.ToString(), isRequired);
            }

            // Deprecated--August Preview build uses ints.
            return YieldDynamicOption((int)category, name, (int)expectedType, isRequired);
        }

        public bool YieldDynamicOption(OptionCategory category, string name, OptionType expectedType, bool isRequired, IEnumerable<string> permittedValues) {
            if (CoreVersion() > 0) {
                return YieldDynamicOption(category.ToString(), name, expectedType.ToString(), isRequired) && (permittedValues ?? Enumerable.Empty<string>()).All(each => YieldKeyValuePair(name, each));
            }
            return YieldDynamicOption((int)category, name, (int)expectedType, isRequired) && (permittedValues ?? Enumerable.Empty<string>()).All(each => YieldKeyValuePair(name, each));
        }

        #endregion

        public bool Yield(KeyValuePair<string, string[]> pair) {
            if (pair.Value.Length == 0) {
                return YieldKeyValuePair(pair.Key, null);
            }
            return pair.Value.All(each => YieldKeyValuePair(pair.Key, each));
        }

        private string GetValue(OptionCategory category, string name) {
            // get the value from the request
            return (GetOptionValues((int)category, name) ?? Enumerable.Empty<string>()).LastOrDefault();
        }

        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private IEnumerable<string> GetValues(OptionCategory category, string name) {
            // get the value from the request
            return (GetOptionValues((int)category, name) ?? Enumerable.Empty<string>());
        }

        private static XmlNamespaceManager _namespaceManager;

        internal static XmlNamespaceManager NamespaceManager {
            get {
                if (_namespaceManager == null) {
                    XmlNameTable nameTable = new NameTable();
                    _namespaceManager = new XmlNamespaceManager(nameTable);
                    _namespaceManager.AddNamespace("swid", "http://standards.iso.org/iso/19770/-2/2014/schema.xsd");
                    _namespaceManager.AddNamespace("oneget", "http://oneget.org/swidtag");
                }
                return _namespaceManager;
            }
        }

        internal DynamicElement DownloadSwidtag(IEnumerable<string> locations) {
            foreach (var location in locations) {
                if (Uri.IsWellFormedUriString(location, UriKind.Absolute)) {
                    var uri = new Uri(location);
                    var content = DownloadContent(uri);
                    XDocument document;
                    if (!String.IsNullOrEmpty(content)) {
                        try {
                            document = XDocument.Parse(content);
                            if (document.Root != null && document.Root.Name.LocalName == Constants.SoftwareIdentity) {
                                // future todo: we could do more checks here.

                                return new DynamicElement(document, NamespaceManager);
                            }
                        } catch {
                            continue;
                        }
                    }
                }
            }
            return null;
        }

        private string DownloadContent(Uri location) {
            string result = null;
            try {
                var client = new WebClient();

                // Apparently, places like Codeplex know to let this thru!
                client.Headers.Add("user-agent", "chocolatey command line");

                var done = new ManualResetEvent(false);

                client.DownloadStringCompleted += (sender, args) => {
                    if (!args.Cancelled && args.Error == null) {
                        result = args.Result;
                    }

                    done.Set();
                };
                client.DownloadProgressChanged += (sender, args) => {
                    // todo: insert progress indicator
                    // var percent = (args.BytesReceived * 100) / args.TotalBytesToReceive;
                    // Progress(c, 2, (int)percent, "Downloading {0} of {1} bytes", args.BytesReceived, args.TotalBytesToReceive);
                };
                client.DownloadStringAsync(location);
                done.WaitOne();
            } catch (Exception e){
                e.Dump();
            }
            return result;
        }

        internal DynamicElement GetProvider(DynamicElement document, string name) {
            var links = document.XPath("/swid:SoftwareIdentity/swid:Link[@rel='component' and @artifact='{0}' and @oneget:type='provider']", name.ToLowerInvariant());
            return DownloadSwidtag(links.GetAttributes("href"));
        }

        internal IEnumerable<DynamicElement> GetProviders(DynamicElement document) {
            var artifacts = document.XPath("/swid:SoftwareIdentity/swid:Link[@rel='component' and @oneget:type='provider']").GetAttributes("artifact").Distinct().ToArray();
            return artifacts.Select(each => GetProvider(document, each)).Where(each => each != null);
        }

        public bool YieldFromSwidtag(DynamicElement provider, string requiredVersion, string minimumVersion, string maximumVersion, string searchKey) {
            if (provider == null) {
                throw new ArgumentNullException("provider");
            }

            var name = provider.Attributes["name"];
            FourPartVersion version = provider.Attributes["version"];
            var versionScheme = provider.Attributes["versionScheme"];
            var packageFilename = provider.XPath("/swid:SoftwareIdentity/swid:Meta[@targetFilename]").GetAttribute("targetFilename");
            var summary = provider.XPath("/swid:SoftwareIdentity/swid:Meta[@summary]").GetAttribute("summary");

            if (AnyNullOrEmpty(name, version, versionScheme, packageFilename)) {
                Debug("Skipping yield on swid due to missing field \r\n", provider.ToString());
                return true;
            }

            if (!string.IsNullOrEmpty(requiredVersion) && version != requiredVersion) {
                return true;
            }

            if (!string.IsNullOrEmpty(minimumVersion) && version < minimumVersion) {
                return true;
            }

            if (!string.IsNullOrEmpty(maximumVersion) && version > maximumVersion) {
                return true;
            }

            if (YieldSoftwareIdentity(name, name, version, versionScheme, summary, null, searchKey, null, packageFilename)) {
                // note: temporary until we actaully support swidtags in the core.

                // yield all the meta/attributes
                if (provider.XPath("/swid:SoftwareIdentity/swid:Meta").Any(
                    meta => meta.Attributes.Any(attribute => !YieldSoftwareMetadata(name, attribute.Name.LocalName, attribute.Value)))) {
                    return false;
                }

                if (provider.XPath("/swid:SoftwareIdentity/swid:Link").Any(
                    link => !YieldLink(name, link.Attributes["href"], link.Attributes["rel"], link.Attributes["type"], link.Attributes["ownership"], link.Attributes["use"], link.Attributes["media"], link.Attributes["artifact"]))) {
                    return false;
                }

                if (provider.XPath("/swid:SoftwareIdentity/swid:Entity").Any(
                    entity => !YieldEntity(name, entity.Attributes["name"], entity.Attributes["regid"], entity.Attributes["role"], entity.Attributes["thumbprint"]))) {
                    return false;
                }

                if (!YieldSoftwareMetadata(name, "FromTrustedSource", true.ToString())) {
                    return false;
                }
            }

            return true;
        }

        internal string DestinationPath {
            get {
                return Path.GetFullPath(GetValue(OptionCategory.Install, "DestinationPath"));
            }
        }

        private static bool AnyNullOrEmpty(params string[] args) {
            return args.Any(string.IsNullOrEmpty);
        }

        public bool DownloadFileToLocation(Uri uri, string targetFile) {
            var result = false;
            var client = new WebClient();

            // Apparently, places like Codeplex know to let this thru!
            client.Headers.Add("user-agent", "chocolatey command line");

            var done = new ManualResetEvent(false);

            client.DownloadFileCompleted += (sender, args) => {
                if (args.Cancelled || args.Error != null) {
                    // failed
                    targetFile.TryHardToDelete();
                } else {
                    result = true;
                }

                done.Set();
            };
            client.DownloadProgressChanged += (sender, args) => {
                // todo: insert progress indicator
                // var percent = (args.BytesReceived * 100) / args.TotalBytesToReceive;
                // Progress(c, 2, (int)percent, "Downloading {0} of {1} bytes", args.BytesReceived, args.TotalBytesToReceive);
            };
            client.DownloadFileAsync(uri, targetFile);
            done.WaitOne();
            return result;
        }
    }

    internal static class SwidExtensions {
    }
}