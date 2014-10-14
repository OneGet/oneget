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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Ipc;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api;
    using Utility.Async;
    using Utility.Collections;
    using Utility.Extensions;
    using Utility.Platform;
    using Utility.Plugin;
    using IRequestObject = System.Object;

    public abstract class RequestObject : AsyncAction, IRequest {
        private static readonly Regex _canonicalPackageRegex = new Regex("(.*?):(.*?)/(.*)");
        private static int _c;

        protected readonly ProviderBase Provider;
        protected Action<RequestObject> _action;

        private IHostApi _hostApi;
        protected Task _invocationTask;

        internal RequestObject(ProviderBase provider, IHostApi hostApi, Action<RequestObject> action) {
            // construct request object
            _hostApi = hostApi;
            Provider = provider;
            _action = action;
        }

        internal RequestObject(ProviderBase provider, IHostApi hostApi) {
            // construct request object
            _hostApi = hostApi;
            Provider = provider;
        }

        private bool CanCallHost {
            get {
                if (IsCompleted || IsAborted) {
                    return false;
                }
                Activity();
                return _hostApi != null;
            }
        }

        #region HostApi Wrapper

        public bool IsElevated {
            get {
                Activity();
                return AdminPrivilege.IsElevated;
            }
        }

        public string GetMessageString(string messageText) {
            if (CanCallHost) {
                return _hostApi.GetMessageString(messageText);
            }
            return null;
        }

        public bool Warning(string messageText) {
            if (CanCallHost) {
                return _hostApi.Warning(messageText);
            }
            return true;
        }

        public bool Error(string id, string category, string targetObjectValue, string messageText) {
            if (CanCallHost) {
                return _hostApi.Error(id, category, targetObjectValue, messageText);
            }
            return true;
        }

        public bool Message(string messageText) {
            if (CanCallHost) {
                return _hostApi.Message(messageText);
            }
            return true;
        }

        public bool Verbose(string messageText) {
            if (CanCallHost) {
                return _hostApi.Verbose(messageText);
            }
            return true;
        }

        public bool Debug(string messageText) {
            if (CanCallHost) {
                return _hostApi.Debug(messageText);
            }
            return true;
        }

        public int StartProgress(int parentActivityId, string messageText) {
            if (CanCallHost) {
                return _hostApi.StartProgress(parentActivityId, messageText);
            }
            return 0;
        }

        public bool Progress(int activityId, int progressPercentage, string messageText) {
            if (CanCallHost) {
                return _hostApi.Progress(activityId, progressPercentage, messageText);
            }
            return true;
        }

        public bool CompleteProgress(int activityId, bool isSuccessful) {
            if (CanCallHost) {
                return _hostApi.CompleteProgress(activityId, isSuccessful);
            }
            return true;
        }

        public IEnumerable<string> GetOptionKeys() {
            if (CanCallHost) {
                return _hostApi.GetOptionKeys();
            }
            return new string[0];
        }

        public IEnumerable<string> GetOptionValues(string key) {
            if (CanCallHost) {
                return _hostApi.GetOptionValues(key);
            }
            return new string[0];
        }

        public IEnumerable<string> GetSources() {
            if (CanCallHost) {
                return _hostApi.GetSources();
            }
            return new string[0];
        }

        public string GetCredentialUsername() {
            if (CanCallHost) {
                return _hostApi.GetCredentialUsername();
            }
            return null;
        }

        public string GetCredentialPassword() {
            if (CanCallHost) {
                return _hostApi.GetCredentialPassword();
            }
            return null;
        }

        public bool ShouldBootstrapProvider(string requestor, string providerName, string providerVersion, string providerType, string location, string destination) {
            if (CanCallHost) {
                return _hostApi.ShouldBootstrapProvider(requestor, providerName, providerVersion, providerType, location, destination);
            }
            return false;
        }

        public bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource) {
            if (CanCallHost) {
                return _hostApi.ShouldContinueWithUntrustedPackageSource(package, packageSource);
            }
            return false;
        }

        public bool AskPermission(string permission) {
            if (CanCallHost) {
                return _hostApi.AskPermission(permission);
            }
            return false;
        }

        public bool IsInteractive() {
            if (CanCallHost) {
                return _hostApi.IsInteractive();
            }
            return false;
        }

        public int CallCount() {
            if (CanCallHost) {
                return _hostApi.CallCount();
            }
            return 0;
        }

        #endregion

        #region CoreApi implementation

        public Type GetIRequestInterface() {
            Activity();
            return typeof (IRequest);
        }

        public int CoreVersion() {
            Activity();
            return Constants.CoreVersion;
        }

        public bool NotifyBeforePackageInstall(string packageName, string version, string source, string destination) {
            Activity();
            return true;
        }

        public bool NotifyPackageInstalled(string packageName, string version, string source, string destination) {
            Activity();
            return true;
        }

        public bool NotifyBeforePackageUninstall(string packageName, string version, string source, string destination) {
            Activity();
            return true;
        }

        public bool NotifyPackageUninstalled(string packageName, string version, string source, string destination) {
            Activity();
            return true;
        }

        public IEnumerable<string> ProviderNames {
            get {
                Activity();
                return PackageManager._instance.ProviderNames;
            }
        }

        public IEnumerable<PackageProvider> PackageProviders {
            get {
                Activity();
                return PackageManager._instance.PackageProviders;
            }
        }

        public IEnumerable<PackageProvider> SelectProvidersWithFeature(string featureName) {
            Activity();
            return PackageManager._instance.SelectProvidersWithFeature(featureName);
        }

        public IEnumerable<PackageProvider> SelectProvidersWithFeature(string featureName, string value) {
            Activity();
            return PackageManager._instance.SelectProvidersWithFeature(featureName, value);
        }

        public IEnumerable<PackageProvider> SelectProviders(string providerName, IRequestObject requestObject) {
            Activity();
            return PackageManager._instance.SelectProviders(providerName, requestObject);
        }

        public bool RequirePackageProvider(string requestor, string packageProviderName, string minimumVersion, IRequestObject requestObject) {
            Activity();
            return PackageManager._instance.RequirePackageProvider(requestor, packageProviderName, minimumVersion, requestObject);
        }

        public string GetCanonicalPackageId(string providerName, string packageName, string version) {
            Activity();
            return "{0}:{1}/{2}".format(providerName, packageName, version);
        }

        public string ParseProviderName(string canonicalPackageId) {
            Activity();
            return _canonicalPackageRegex.Match(canonicalPackageId).Groups[1].Value;
        }

        public string ParsePackageName(string canonicalPackageId) {
            Activity();
            return _canonicalPackageRegex.Match(canonicalPackageId).Groups[2].Value;
        }

        public string ParsePackageVersion(string canonicalPackageId) {
            Activity();
            return _canonicalPackageRegex.Match(canonicalPackageId).Groups[3].Value;
        }

        #endregion

        #region response api implementation

        public virtual bool YieldSoftwareIdentity(string fastPath, string name, string version, string versionScheme, string summary, string source, string searchKey, string fullPath, string packageFileName) {
            Console.WriteLine("SHOULD NOT GET HERE [YieldSoftwareIdentity] ================================================");
            // todo: give an actual error here
            return true; // cancel
        }

        public virtual bool YieldSoftwareMetadata(string parentFastPath, string name, string value) {
            Console.WriteLine("SHOULD NOT GET HERE [YieldSoftwareIdentity] ================================================");
            // todo: give an actual error here
            return true; // cancel
        }

        public virtual bool YieldEntity(string parentFastPath, string name, string regid, string role, string thumbprint) {
            Console.WriteLine("SHOULD NOT GET HERE [YieldSoftwareIdentity] ================================================");
            // todo: give an actual error here
            return true; // cancel
        }

        public virtual bool YieldLink(string parentFastPath, string referenceUri, string relationship, string mediaType, string ownership, string use, string appliesToMedia, string artifact) {
            Console.WriteLine("SHOULD NOT GET HERE [YieldSoftwareIdentity] ================================================");
            // todo: give an actual error here
            return true; // cancel
        }

        public virtual bool YieldPackageSource(string name, string location, bool isTrusted, bool isRegistered, bool isValidated) {
            Console.WriteLine("SHOULD NOT GET HERE [YieldSoftwareIdentity] ================================================");
            // todo: give an actual error here
            return true; // cancel
        }

        public virtual bool YieldDynamicOption(string name, string expectedType, bool isRequired) {
            Console.WriteLine("SHOULD NOT GET HERE [YieldSoftwareIdentity] ================================================");
            // todo: give an actual error here
            return true; // cancel
        }

        public virtual bool YieldKeyValuePair(string key, string value) {
            Console.WriteLine("SHOULD NOT GET HERE [YieldSoftwareIdentity] ================================================");
            // todo: give an actual error here
            return true; // cancel
        }

        public virtual bool YieldValue(string value) {
            Console.WriteLine("SHOULD NOT GET HERE [YieldSoftwareIdentity] ================================================");
            // todo: give an actual error here
            return true; // cancel
        }

        #endregion

        #region IProviderServices implementation 

        public bool IsSupportedArchive(string localFilename, IRequestObject requestObject) {
            Activity();
            return PackageManager._instance.Archivers.Values.Any(archiver => archiver.IsSupportedFile(localFilename));
        }

        public void DownloadFile(Uri remoteLocation, string localFilename, IRequestObject requestObject) {
            Activity();

            // check the Uri type, see if we have anyone who can handle that 
            // if so, call that provider's download file
            if (remoteLocation == null) {
                throw new ArgumentNullException("remoteLocation");
            }
            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }

            using (var request = requestObject.As<Request>()) {
                foreach (var downloader in PackageManager._instance.Downloaders.Values) {
                    if (downloader.SupportedUriSchemes.Contains(remoteLocation.Scheme, StringComparer.OrdinalIgnoreCase)) {
                        downloader.DownloadFile(remoteLocation, localFilename, request);
                        return;
                    }
                }
                request.Error(ErrorCategory.NotImplemented, Constants.Messages.ProtocolNotSupported, remoteLocation.Scheme, Constants.Messages.ProtocolNotSupported, remoteLocation.Scheme);
            }
        }

        public IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, IRequestObject requestObject) {
            Activity();

            // check who supports the archive type
            // and call that provider.
            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }

            using (var request = requestObject.As<Request>()) {
                foreach (var archiver in PackageManager._instance.Archivers.Values) {
                    if (archiver.IsSupportedFile(localFilename)) {
                        return archiver.UnpackArchive(localFilename, destinationFolder, request).ByRefEnumerable();
                    }
                }
                request.Error(ErrorCategory.NotImplemented, localFilename, Constants.Messages.UnsupportedArchive);
            }
            return Enumerable.Empty<string>();
        }

        public void AddPinnedItemToTaskbar(string item, IRequestObject requestObject) {
            Activity();

            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }
            using (var request = requestObject.As<Request>()) {
                request.Debug("Calling 'ProviderService::AddPinnedItemToTaskbar'");
                ShellApplication.Pin(item);
            }
        }

        public void RemovePinnedItemFromTaskbar(string item, IRequestObject requestObject) {
            Activity();

            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }
            using (var request = requestObject.As<Request>()) {
                request.Debug("Calling 'ProviderService::RemovePinnedItemFromTaskbar'");
                ShellApplication.Unpin(item);
            }
        }

        public void CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, IRequestObject requestObject) {
            Activity();

            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }

            using (var request = requestObject.As<Request>()) {
                request.Debug("Calling 'ProviderService::CreateShortcutLink'");

                if (File.Exists(linkPath)) {
                    request.Verbose("Creating Shortcut '{0}' => '{1}'", linkPath, targetPath);
                    ShellLink.CreateShortcut(linkPath, targetPath, description, workingDirectory, arguments);
                }
                request.Error(ErrorCategory.InvalidData, targetPath, Constants.Messages.UnableToCreateShortcutTargetDoesNotExist, targetPath);
            }
        }

        public void SetEnvironmentVariable(string variable, string value, string context, IRequestObject requestObject) {
            Activity();

            if (context == null) {
                throw new ArgumentNullException("context");
            }

            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }

            using (var request = requestObject.As<Request>()) {
                request.Debug("Calling 'ProviderService::SetEnvironmentVariable'");
                if (string.IsNullOrEmpty(value)) {
                    RemoveEnvironmentVariable(variable, context, requestObject);
                }

                switch (context.ToLowerInvariant()) {
                    case "system":
                        if (!IsElevated) {
                            request.Warning("SetEnvironmentVariable Failed - Admin Elevation required to set variable '{0}' in machine context", variable);
                            return;
                        }
                        request.Verbose("SetEnvironmentVariable (machine) '{0}' = '{1}'", variable, value);
                        Environment.SetEnvironmentVariable(variable, value, EnvironmentVariableTarget.Machine);
                        break;

                    default:
                        request.Verbose("SetEnvironmentVariable (user) '{0}' = '{1}'", variable, value);
                        Environment.SetEnvironmentVariable(variable, value, EnvironmentVariableTarget.User);
                        break;
                }
                Environment.SetEnvironmentVariable(variable, null, EnvironmentVariableTarget.Process);
            }
        }

        public void RemoveEnvironmentVariable(string variable, string context, IRequestObject requestObject) {
            Activity();

            if (context == null) {
                throw new ArgumentNullException("context");
            }

            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }

            using (var request = requestObject.As<Request>()) {
                request.Debug("Calling 'ProviderService::RemoveEnvironmentVariable'");

                if (string.IsNullOrEmpty(variable)) {
                    return;
                }
                switch (context.ToLowerInvariant()) {
                    case "user":
                        request.Verbose("RemoveEnvironmentVariable (user)--'{0}'", variable);
                        Environment.SetEnvironmentVariable(variable, null, EnvironmentVariableTarget.User);
                        break;

                    case "system":
                        if (!IsElevated) {
                            request.Warning(Constants.Messages.RemoveEnvironmentVariableRequiresElevation, variable);
                            return;
                        }
                        request.Verbose("RemoveEnvironmentVariable (machine)", "'{0}'", variable);
                        Environment.SetEnvironmentVariable(variable, null, EnvironmentVariableTarget.Machine);
                        break;

                    default:
                        request.Verbose("RemoveEnvironmentVariable (all) '{0}'", variable);
                        Environment.SetEnvironmentVariable(variable, null, EnvironmentVariableTarget.User);
                        Environment.SetEnvironmentVariable(variable, null, EnvironmentVariableTarget.Machine);
                        break;
                }
                Environment.SetEnvironmentVariable(variable, null, EnvironmentVariableTarget.Process);
            }
        }

        public void CopyFile(string sourcePath, string destinationPath, IRequestObject requestObject) {
            Activity();

            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }

            using (var request = requestObject.As<Request>()) {
                request.Debug("Calling 'ProviderService::CopyFile'");
                if (sourcePath == null) {
                    throw new ArgumentNullException("sourcePath");
                }
                if (destinationPath == null) {
                    throw new ArgumentNullException("destinationPath");
                }
                if (File.Exists(destinationPath)) {
                    destinationPath.TryHardToDelete();
                    if (File.Exists(destinationPath)) {
                        request.Error(ErrorCategory.OpenError, destinationPath, Constants.Messages.UnableToOverwriteExistingFile, destinationPath);
                    }
                }
                File.Copy(sourcePath, destinationPath);
                if (!File.Exists(destinationPath)) {
                    request.Error(ErrorCategory.InvalidResult, destinationPath, Constants.Messages.UnableToCopyFileTo, destinationPath);
                }
            }
        }

        public void Delete(string path, IRequestObject requestObject) {
            Activity();

            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }

            using (var request = requestObject.As<Request>()) {
                request.Debug("Calling 'ProviderService::Delete'");
                if (string.IsNullOrEmpty(path)) {
                    return;
                }

                path.TryHardToDelete();
            }
        }

        public void DeleteFolder(string folder, IRequestObject requestObject) {
            Activity();

            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }

            using (var request = requestObject.As<Request>()) {
                request.Debug("Calling 'ProviderService::DeleteFolder' {0}".format(folder));
                if (string.IsNullOrEmpty(folder)) {
                    return;
                }
                if (Directory.Exists(folder)) {
                    folder.TryHardToDelete();
                }
            }
        }

        public void CreateFolder(string folder, IRequestObject requestObject) {
            Activity();

            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }

            using (var request = requestObject.As<Request>()) {
                request.Debug("Calling 'ProviderService::CreateFolder'");

                if (!Directory.Exists(folder)) {
                    try {
                        Directory.CreateDirectory(folder);
                        request.Verbose("CreateFolder Success {0}", folder);
                        return;
                    } catch (Exception e) {
                        request.Error(ErrorCategory.InvalidResult, folder, Constants.Messages.CreatefolderFailed, folder, e.Message);
                        return;
                    }
                }
                request.Verbose("CreateFolder -- Already Exists {0}", folder);
            }
        }

        public void DeleteFile(string filename, IRequestObject requestObject) {
            Activity();

            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }

            using (var request = requestObject.As<Request>()) {
                request.Debug("Calling 'ProviderService::DeleteFile'");
                if (string.IsNullOrEmpty(filename)) {
                    return;
                }

                if (File.Exists(filename)) {
                    filename.TryHardToDelete();
                }
            }
        }

        public string GetKnownFolder(string knownFolder, IRequestObject requestObject) {
            Activity();

            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }

            using (var request = requestObject.As<Request>()) {
                request.Debug("Calling 'ProviderService::GetKnownFolder'");
                if (!string.IsNullOrEmpty(knownFolder)) {
                    if (knownFolder.Equals("tmp", StringComparison.OrdinalIgnoreCase) || knownFolder.Equals("temp", StringComparison.OrdinalIgnoreCase)) {
                        return FilesystemExtensions.TempPath;
                    }
                    KnownFolder folder;
                    if (Enum.TryParse(knownFolder, true, out folder)) {
                        return KnownFolders.GetFolderPath(folder);
                    }
                }

                request.Error(ErrorCategory.InvalidArgument, knownFolder, Constants.Messages.UnknownFolderId, knownFolder);
            }
            return null;
        }

        public string CanonicalizePath(string path, string currentDirectory) {
            Activity();

            if (string.IsNullOrEmpty(path)) {
                return null;
            }

            return path.CanonicalizePath(!string.IsNullOrEmpty(currentDirectory));
        }

        public bool FileExists(string path) {
            Activity();

            if (string.IsNullOrEmpty(path)) {
                return false;
            }

            return path.FileExists();
        }

        public bool DirectoryExists(string path) {
            Activity();
            if (string.IsNullOrEmpty(path)) {
                return false;
            }
            return path.DirectoryExists();
        }

        public bool Install(string fileName, string additionalArgs, IRequestObject requestObject) {
            Activity();

            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }

            if (string.IsNullOrEmpty(fileName)) {
                return false;
            }

            using (var request = requestObject.As<Request>()) {
                // high-level api for simply installing a file 
                // returns false if unsuccessful.
                foreach (var provider in PackageManager._instance.PackageProviders) {
                    var packages = provider.FindPackageByFile(fileName, 0, request).ToArray();
                    if (packages.Length > 0) {
                        // found a provider that can handle this package.
                        // install with this provider
                        // ToDo: @FutureGarrett -- we need to be able to handle priorities and who wins...
                        foreach (var package in packages) {
                            foreach (var installedPackage in provider.InstallPackage(package, request.Extend<IRequest>(new {
                                GetOptionValues = new Func<string, IEnumerable<string>>(key => {
                                    if (key.EqualsIgnoreCase("additionalArguments")) {
                                        return new[] {additionalArgs};
                                    }
                                    return request.GetOptionValues(key);
                                })
                            }))) {
                                request.Debug("Installed internal package {0}", installedPackage.Name);
                            }
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsSignedAndTrusted(string filename, IRequestObject requestObject) {
            Activity();

            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }

            if (string.IsNullOrEmpty(filename) || !FileExists(filename)) {
                return false;
            }

            using (var request = requestObject.As<Request>()) {
                request.Debug("Calling 'ProviderService::IsSignedAndTrusted, '{0}'", filename);

                var wtd = new WinTrustData(filename);
                var result = NativeMethods.WinVerifyTrust(new IntPtr(-1), new Guid("{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}"), wtd);
                return result == WinVerifyTrustResult.Success;
            }
        }

        public bool ExecuteElevatedAction(string provider, string payload, IRequestObject requestObject) {
            Activity();

            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
                    
            }

            // launches a new elevated host that 
            // talks back to this (unelevated) host for everything in HostApi
            // everything else should be handled in the new process.
            var guid = Guid.NewGuid();

            var properties = new Hashtable();
            properties.Add("portName", "OneGet_" + guid);
            properties.Add("authorizedGroup", "Administrators");
            properties.Add("secure", "true");
            properties.Add("exclusiveAddressUse", "true");
            properties.Add("strictBinding", "false");
            properties.Add("name", "OneGetHost");

            // set up the server IPC channel
            var serverChannel = new IpcServerChannel(properties, new BinaryServerFormatterSinkProvider(properties, null));

            ChannelServices.RegisterChannel(serverChannel, true);

            var instance = new RemotableHostApi(requestObject.As<IHostApi>());

            var objRef = RemotingServices.Marshal(instance, "Host", typeof (IHostApi));
            var remoteUris = serverChannel.GetUrlsForUri("Host");
            var uri = remoteUris[0];
            // Create the client elevated
            try {
                var process = AsyncProcess.Start(new ProcessStartInfo {
                    FileName = Assembly.GetExecutingAssembly().Location,
                    Arguments = "{0} {1} {2}".format( uri, provider, (string.IsNullOrWhiteSpace(payload) ? "null" : payload).ToBase64()),
#if !DEBUG                    
                    WindowStyle = ProcessWindowStyle.Hidden,
#endif
                    Verb = "runas",
                });

                process.WaitForExit();
                if (process.ExitCode != 0) {
                    return false;
                }
            } catch (Exception e) {
                e.Dump();
                return false;
            } finally {
                RemotingServices.Disconnect(instance);
                ChannelServices.UnregisterChannel(serverChannel);
            }
            return true;
        }

        #endregion

        public override object InitializeLifetimeService() {
            return null;
        }

        protected void InvokeImpl() {
            _invocationTask = Task.Factory.StartNew(() => {
                _invocationThread = Thread.CurrentThread;
                _invocationThread.Name = Provider.ProviderName + ":" + _c++;
                try {
                    _action(this);
                } catch (ThreadAbortException) {
#if DEEP_DEBUG
                    Console.WriteLine("Thread Aborted for {0} : {1}", _invocationThread.Name, DateTime.Now.Subtract(_callStart).TotalSeconds);
#endif
                    Thread.ResetAbort();
                } catch (Exception e) {
                    e.Dump();
                }
            }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ContinueWith(antecedent => Complete());

            // start thread, call function
            StartCall();
        }
    }
}