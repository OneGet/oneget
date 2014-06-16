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
                var ps = GetSpecifiedPackageSources();
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

        public abstract IEnumerable<string> GetSpecifiedPackageSources();

        public abstract string GetCredentialUsername();

        public abstract SecureString GetCredentialPassword();

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
        public abstract bool YieldPackageSource(string name, string location, bool isTrusted, bool isRegistered);

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
            return Warning(FormatMessageString(message, args));
        }

        public bool Error(string message, params object[] args) {
            return Error(FormatMessageString(message, args));
        }

        public bool Message(string message, params object[] args) {
            return Message(FormatMessageString(message, args));
        }

        public bool Verbose(string message, params object[] args) {
            return Verbose(FormatMessageString(message, args));
        }

        public bool Debug(string message, params object[] args) {
            return Debug(FormatMessageString(message, args));
        }

        public int StartProgress(int parentActivityId, string message, params object[] args) {
            return StartProgress(parentActivityId, FormatMessageString(message, args));
        }

        public bool Progress(int activityId, int progress, string message, params object[] args) {
            return Progress(activityId, progress, FormatMessageString(message, args));
        }

        private static string FixMeFormat(string formatString, object[] args) {
            if (args == null || args.Length == 0) {
                // not really any args, and not really expectng any
                return formatString.Replace('{', '\u00ab').Replace('}', '\u00bb');
            }
            return System.Linq.Enumerable.Aggregate(args, "FIXME/Format:" + formatString.Replace('{', '\u00ab').Replace('}', '\u00bb'), (current, arg) => current + string.Format(" \u00ab{0}\u00bb", arg));
        }

        internal string FormatMessageString(string message, object[] args) {
            message = GetMessageString(message) ?? message;

            // if it doesn't look like we have the correct number of parameters
            // let's return a fixmeformat string.
            var c = System.Linq.Enumerable.Count(System.Linq.Enumerable.Where(message.ToCharArray(), each => each == '{'));
            if (c < args.Length) {
                return FixMeFormat(message, args);
            }
            return string.Format(message, args);
        }

        public void Dispose() {
        }

        public static implicit operator MarshalByRefObject(Request req) {
            return req.RemoteThis;
        }

        public MarshalByRefObject RemoteThis {
            get {
                return Extend();
            }
        }

        internal MarshalByRefObject Extend(params object[] objects) {
            return DynamicExtensions.Extend(this, GetIRequestInterface(), objects);
        }

        #endregion

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

#if NOPE
    public class oldRequest : IDisposable {
        private Callback _callback;

        //public static implicit operator Callback(Request request) {
        // return request._callback;
        //}

        internal oldRequest(Callback c) {
            _callback = c;
        }

        internal bool IsDisposed {
            get {
                return _callback == null;
            }
        }

        public void Dispose() {
            // Clearing all of these ensures that the transient APIs 
            // can't be called outside of the appropriate scope.

            _callback = null;
        }

        public Request Override(IEnumerable<string> sources, Hashtable options) {
#if NOT_RIGHT
    // create a new instance of this request,
    // but substitute the sources and options
    // for the ones that it's providing.

            var packageSources = new PackageSources(() => {
                return sources;
            });

            var getOptionKeys = new GetOptionKeys((category) => {
                return options.Keys.ToEnumerable<string>();
            });

            var getOptionValues = new GetOptionValues((category, key) => {
                foreach (var k in options.Keys) {
                    if (string.Equals(k.ToString(), key, StringComparison.OrdinalIgnoreCase)) {
                        // todo: must return collection of items.
                        return null; // options[k];
                    }
                }
                return null;
            });

            return null;
            /*
            return new Request( new Callback((fn, args) => {
                if (string.IsNullOrEmpty(fn)) {
                    var results = _callback(null, null) as IEnumerable<string>;
                    if (results == null) {
                        return null;
                    }
                    return results.Union(new[] {
                        "PackageSources",
                        "GetOptionKeys",
                        "GetOptionValues"
                    });

                }

                if (args == null) {
                    switch (fn.ToLowerInvariant()) {
                        case "packagesources":
                            return packageSources;
                        case "getoptionkeys":
                            return getOptionKeys;
                        case "getoptionvalues":
                            return getOptionValues;
                    }
                    return _callback(fn,null);
                }

                switch (fn.ToLowerInvariant()) {
                    case "packagesources":
                        return packageSources();
                    case "getoptionkeys":
                        var a = args.ToArray();
                        return getOptionKeys((string)a[0]);
                    case "getoptionvalues":
                        var b = args.ToArray();
                        return getOptionValues((string)b[0], (string)b[1]);
                }

                return _callback(fn, args);
            }));
             * */
#endif
            return null;
        }

        private void CheckDisposed() {
            if (IsDisposed) {
                throw new Exception("Invalid State--past call lifespan");
            }
        }
    }
#endif

    #region copy dynamicextension-implementation
public static class DynamicExtensions {
        private static dynamic _remoteDynamicInterface;
        private static dynamic _localDynamicInterface;

        /// <summary>
        ///  This is the Instance for DynamicInterface that we use when we're giving another AppDomain a remotable object.
        /// </summary>
        public static dynamic LocalDynamicInterface {
            get {
                return _localDynamicInterface ?? (_localDynamicInterface = Activator.CreateInstance(RemoteDynamicInterface.GetType()));
            }
        }

        /// <summary>
        /// The is the instance of the DynamicInteface service from the calling AppDomain
        /// </summary>
        public static dynamic RemoteDynamicInterface {
            get {
                return _remoteDynamicInterface;
            }
            set {
                if (_remoteDynamicInterface == null) {
                    _remoteDynamicInterface = value;
                }
            }
        }

        /// <summary>
        /// This is called to adapt an object from a foreign app domain to a known interface
        /// In this appDomain
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static T As<T>(this object instance) {
            return RemoteDynamicInterface.Create<T>(instance);
        }

        /// <summary>
        ///  This is called to adapt and extend an object that we wish to pass to a foreign app domain
        /// </summary>
        /// <param name="obj">The base object that we are passing</param>
        /// <param name="tInterface">the target interface (from the foreign appdomain)</param>
        /// <param name="objects">the overriding objects (may be anonymous objects with Delegates, or an object with methods)</param>
        /// <returns></returns>
        public static MarshalByRefObject Extend(this object obj, Type tInterface, params object[] objects) {
            return LocalDynamicInterface.Create(tInterface, objects, obj);
        }
    }

    #endregion

}