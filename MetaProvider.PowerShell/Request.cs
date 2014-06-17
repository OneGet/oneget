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

namespace Microsoft.OneGet.MetaProvider.PowerShell {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Security;
    using Extensions;
    using Callback = System.MarshalByRefObject;

    public abstract class Request : IDisposable {

        internal CommandInfo CommandInfo;
        private PowerShellProviderBase _provider;

        internal bool IsMethodImplemented {
            get {
                return CommandInfo != null;
            }
        }

        public string[] PackageSources {
            get {
                var ps = GetSources();
                if (ps == null) {
                    return new string[] {
                    };
                }
                return ps.ToArray();
            }
        }

        #region copy core-apis

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
        /// <param name="c"></param>
        /// <returns></returns>
        public abstract object GetPackageManagementService(Object c);

        /// <summary>
        ///     Returns the type for a Request/Callback that the OneGet Core is expecting
        ///     This is (currently) neccessary to provide an appropriately-typed version
        ///     of the Request to the core when a Plugin is calling back into the core
        ///     and has to pass a Callback.
        /// </summary>
        /// <returns></returns>
        public abstract Type GetIRequestInterface();

        public abstract bool NotifyBeforePackageInstall(string packageName, string version, string source, string destination);

        public abstract bool NotifyPackageInstalled(string packageName, string version, string source, string destination);

        public abstract bool NotifyBeforePackageUninstall(string packageName, string version, string source, string destination);

        public abstract bool NotifyPackageUninstalled(string packageName, string version, string source, string destination);
        #endregion

        #region copy host-apis

        public abstract string GetMessageString(string message);

        public abstract bool Warning(string message);

        public abstract bool Error(string message);

        public abstract bool Message(string message);

        public abstract bool Verbose(string message);

        public abstract bool Debug(string message);

        public abstract bool ExceptionThrown(string exceptionType, string message, string stacktrace);

        public abstract int StartProgress(int parentActivityId, string message);

        public abstract bool Progress(int activityId, int progress, string message);

        public abstract bool CompleteProgress(int activityId, bool isSuccessful);

        /// <summary>
        ///     Used by a provider to request what metadata keys were passed from the user
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<string> GetOptionKeys(int category);

        public abstract IEnumerable<string> GetOptionValues(int category, string key);

        public abstract IEnumerable<string> GetSources();

        public abstract string GetCredentialUsername();

        public abstract string GetCredentialPassword();

        public abstract bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource);

        public abstract bool ShouldProcessPackageInstall(string packageName, string version, string source);

        public abstract bool ShouldProcessPackageUninstall(string packageName, string version);

        public abstract bool ShouldContinueAfterPackageInstallFailure(string packageName, string version, string source);

        public abstract bool ShouldContinueAfterPackageUninstallFailure(string packageName, string version, string source);

        public abstract bool ShouldContinueRunningInstallScript(string packageName, string version, string source, string scriptLocation);

        public abstract bool ShouldContinueRunningUninstallScript(string packageName, string version, string source, string scriptLocation);

        public abstract bool AskPermission(string permission);
        #endregion

        #region copy service-apis

        public abstract void DownloadFile(Uri remoteLocation, string localFilename, Object c);

        public abstract bool IsSupportedArchive(string localFilename, Object c);

        public abstract IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, Object c);

        public abstract void AddPinnedItemToTaskbar(string item, Object c);

        public abstract void RemovePinnedItemFromTaskbar(string item, Object c);

        public abstract void CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, Object c);

        public abstract void SetEnvironmentVariable(string variable, string value, int context, Object c);

        public abstract void RemoveEnvironmentVariable(string variable, int context, Object c);

        public abstract void CopyFile(string sourcePath, string destinationPath, Object c);

        public abstract void Delete(string path, Object c);

        public abstract void DeleteFolder(string folder, Object c);

        public abstract void CreateFolder(string folder, Object c);

        public abstract void DeleteFile(string filename, Object c);

        public abstract string GetKnownFolder(string knownFolder, Object c);

        public abstract bool IsElevated(Object c);
        #endregion

        #region copy response-apis

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
        public abstract bool YieldPackage(string fastPath, string name, string version, string versionScheme, string summary, string source, string searchKey, string fullPath, string packageFileName);

        public abstract bool YieldPackageDetails(object serializablePackageDetailsObject);

        public abstract bool YieldPackageSwidtag(string fastPath, string xmlOrJsonDoc);

        /// <summary>
        ///     Used by a provider to return fields for a package source (repository)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="location"></param>
        /// <param name="isTrusted"></param>
        /// <param name="isRegistered"></param>
        /// <returns></returns>
        public abstract bool YieldPackageSource(string name, string location, bool isTrusted,bool isRegistered);

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

        public abstract bool YieldKeyValuePair(string key, string value);

        public abstract bool YieldValue(string value);
        #endregion

        #region copy Request-implementation
public bool Warning(string message, params object[] args) {
            return Warning(FormatMessageString(message,args));
        }

        public bool Error(string message, params object[] args) {
            return Error(FormatMessageString(message,args));
        }

        public bool Message(string message, params object[] args) {
            return Message(FormatMessageString(message,args));
        }

        public bool Verbose(string message, params object[] args) {
            return Verbose(FormatMessageString(message,args));
        } 

        public bool Debug(string message, params object[] args) {
            return Debug(FormatMessageString(message,args));
        }

        public int StartProgress(int parentActivityId, string message, params object[] args) {
            return StartProgress(parentActivityId, FormatMessageString(message,args));
        }

        public bool Progress(int activityId, int progress, string message, params object[] args) {
            return Progress(activityId, progress, FormatMessageString(message,args));
        }

        private static string FixMeFormat(string formatString, object[] args) {
            if (args == null || args.Length == 0 ) {
                // not really any args, and not really expectng any
                return formatString.Replace('{', '\u00ab').Replace('}', '\u00bb');
            }
            return System.Linq.Enumerable.Aggregate(args, "FIXME/Format:" + formatString.Replace('{', '\u00ab').Replace('}', '\u00bb'), (current, arg) => current + string.Format(" \u00ab{0}\u00bb", arg));
        }

        internal string FormatMessageString(string message, object[] args) {
            message = GetMessageString(message) ?? message;

            // if it doesn't look like we have the correct number of parameters
            // let's return a fixmeformat string.
            var c = System.Linq.Enumerable.Count( System.Linq.Enumerable.Where(message.ToCharArray(), each => each == '{'));
            if (c < args.Length) {
                return FixMeFormat(message, args);
            }
            return string.Format(message, args);
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
        }

        public static implicit operator MarshalByRefObject(Request req) {
            return req.RemoteThis;
        }

        internal MarshalByRefObject RemoteThis {
            get {
                return Extend();
            }
        }

        internal MarshalByRefObject Extend(params object[] objects) {
            return RequestExtensions.Extend(this, GetIRequestInterface(), objects);
        }

        #endregion

        public PSCredential Credential {
            get {
                var u = GetCredentialUsername();
                var p = GetCredentialPassword();
                if( string.IsNullOrEmpty(u) && string.IsNullOrEmpty(p) ) {
                    return null;
                }
                return new PSCredential(u, p.FromProtectedString("salt"));
            }
        }

        private Hashtable _options;
        public Hashtable Options {
            get {
                if (_options == null) {
                    _options = new Hashtable();
                    //quick and dirty, grab all four sets and merge them.
                    for (int i = 0; i < 4; i++) {
                        var keys = GetOptionKeys(i).ToArray();
                        foreach (var k in keys) {
                            var values = GetOptionValues(i, k).ToArray();
                            if (values.Length == 1) {
                                if (values[0].IsTrue()) {
                                    _options.Add(k, true);
                                } else {
                                    _options.Add(k, values[0]);
                                }
                            } else {
                                _options.Add(k, values);
                            }
                        }
                    }
                }
                return _options;
            }
        }

        public MarshalByRefObject CloneRequest(Hashtable options = null, ArrayList sources = null, PSCredential credential = null) {
            var srcs = (sources ?? new ArrayList()).ToArray().Select(each => each.ToString()).ToArray();
            var name = credential == null ? null : credential.UserName;
            var pass = credential == null ? null : credential.Password.ToProtectedString("salt");
            options = options ?? new Hashtable();

            var lst = new Dictionary<string, string[]>();
            foreach (var k in options.Keys) {
                if (k != null) {
                    var obj = options[k];

                    string[] val = null;

                    if (obj is string) {
                        val = new [] {obj as string};
                    }

                    // otherwise, try to cast it to a collection of string-like-things
                    var collection = obj as IEnumerable;
                    if (collection != null) {
                        val = collection.Cast<object>().Select(each => each.ToString()).ToArray();
                    }

                    // meh. ToString, and goodnight.
                    val = new[] { obj.ToString() };

                    lst.Add(k.ToString(),val );    
                }
            }

            return Extend(new {
                GetOptionKeys = new Func<int, IEnumerable<string>>(category => {
                    return lst.Keys.ToArray();
                }),

                GetOptionValues = new Func<int, string, IEnumerable<string>>((category, key) => {
                    if (lst.ContainsKey(key)) {
                        return lst[key];    
                    }
                    return new string[0];
                }),

                GetSources = new Func<IEnumerable<string>>(() => {
                    return srcs;
                }),

                GetCredentialUsername = new Func<string>(() => {return name;}),

                GetCredentialPassword = new Func<string>(() => {return pass;}),
            });
        }

        public object CallPowerShell(params object[] args) {
            if (IsMethodImplemented) {
                return _provider.CallPowerShell(this, args);
            }
            return null;
        }

        internal static Request New(Object c, PowerShellProviderBase provider, string methodName) {
            var req = c.As<Request>();

            req.CommandInfo = provider.GetMethod(methodName);
            if (req.CommandInfo == null) {
                req.Debug("METHOD_NOT_IMPLEMENTED", methodName);
            }
            req._provider = provider;
            return req;
        }

        private IPackageManagementService _packageManagementService;

        public IPackageManagementService PackageManagementService {
            get {
                return _packageManagementService ?? (_packageManagementService = GetPackageManagementService(RemoteThis) as IPackageManagementService);
            }
        }

        public IEnumerator<string> ProviderNames {
            get {
                return PackageManagementService.ProviderNames;
            }
        }

        public IEnumerator<object> PackageProviders {
            get {
                return PackageManagementService.PackageProviders;
            }
        }

        public IEnumerator<object> SelectProviders(string providerName) {
            return PackageManagementService.SelectProviders(providerName);
        }
        public IEnumerator<object> SelectProvidersWithFeature(string featureName) {
            return PackageManagementService.SelectProvidersWithFeature(featureName);
        }

        public IEnumerator<object> SelectProvidersWithFeature(string featureName, string value) {
            return PackageManagementService.SelectProvidersWithFeature(featureName,value);
        }
    }
}