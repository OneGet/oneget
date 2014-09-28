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
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Ipc;
    using System.Security.Cryptography;
    using Api;
    using Utility.Extensions;
    using Utility.Platform;
    using Utility.Plugin;

    /// <summary>
    ///     The ProviderServices aggregates the functionality of all
    ///     the loaded service providers.
    /// </summary>
    internal class ProviderServices : MarshalByRefObject , IProviderServices {
        private readonly PackageManagementService _packageManagementService;

        internal ProviderServices(PackageManagementService pmsi) {
            _packageManagementService = pmsi;
        }

        public override object InitializeLifetimeService() {
            return null;
        }

        public bool IsSupportedArchive(string localFilename, Object requestImpl) {
            return _packageManagementService.Archivers.Values.Any(archiver => archiver.IsSupportedArchive(localFilename));
        }

        public void DownloadFile(Uri remoteLocation, string localFilename, Object requestImpl) {
            // check the Uri type, see if we have anyone who can handle that 
            // if so, call that provider's download file
            if (remoteLocation == null) {
                throw new ArgumentNullException("remoteLocation");
            }
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            using (var request = requestImpl.As<Request>()) {
                foreach (var downloader in _packageManagementService.Downloaders.Values) {
                    if (downloader.SupportedSchemes.Contains(remoteLocation.Scheme, StringComparer.InvariantCultureIgnoreCase)) {
                        downloader.DownloadFile(remoteLocation, localFilename, request);
                        return;
                    }
                }
                request.Error(ErrorCategory.NotImplemented, Constants.Messages.ProtocolNotSupported, remoteLocation.Scheme, Constants.Messages.ProtocolNotSupported, remoteLocation.Scheme);
            }
        }

        public IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, Object requestImpl) {
            // check who supports the archive type
            // and call that provider.
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            using (var request = requestImpl.As<Request>()) {
                foreach (var archiver in _packageManagementService.Archivers.Values) {
                    if (archiver.IsSupportedArchive(localFilename) ) {
                        return archiver.UnpackArchive(localFilename, destinationFolder, request);
                    }
                }
                request.Error(ErrorCategory.NotImplemented, localFilename, Constants.Messages.UnsupportedArchive);
            }
            return Enumerable.Empty<string>();
        }

        public void AddPinnedItemToTaskbar(string item, object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }
            using (var request = requestImpl.As<Request>()) {
                request.Debug("Calling 'ProviderService::AddPinnedItemToTaskbar'");
                ShellApplication.Pin(item);
            }
        }

        public void RemovePinnedItemFromTaskbar(string item, object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }
            using (var request = requestImpl.As<Request>()) {
                request.Debug("Calling 'ProviderService::RemovePinnedItemFromTaskbar'");
                ShellApplication.Unpin(item);
            }
        }

        public void CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, Object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            using (var request = requestImpl.As<Request>()) {
                request.Debug("Calling 'ProviderService::CreateShortcutLink'");

                if (File.Exists(linkPath)) {
                    request.Verbose("Creating Shortcut '{0}' => '{1}'", linkPath, targetPath);
                    ShellLink.CreateShortcut(linkPath, targetPath, description, workingDirectory, arguments);
                }
                request.Error(ErrorCategory.InvalidData, targetPath, Constants.Messages.UnableToCreateShortcutTargetDoesNotExist, targetPath);
            }
        }

        public void SetEnvironmentVariable(string variable, string value, string context, Object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            using (var request = requestImpl.As<Request>()) {
                request.Debug("Calling 'ProviderService::SetEnvironmentVariable'");
                if (string.IsNullOrEmpty(value)) {
                    RemoveEnvironmentVariable(variable, context, requestImpl);
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

        public void RemoveEnvironmentVariable(string variable, string context, Object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            using (var request = requestImpl.As<Request>()) {
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

        public void CopyFile(string sourcePath, string destinationPath, Object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            using (var request = requestImpl.As<Request>()) {
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
                    request.Error(ErrorCategory.InvalidResult,  destinationPath, Constants.Messages.UnableToCopyFileTo, destinationPath);
                }
            }
        }

        public void Delete(string path, Object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            using (var request = requestImpl.As<Request>()) {
                request.Debug("Calling 'ProviderService::Delete'");
                if (string.IsNullOrEmpty(path)) {
                    return;
                }

                path.TryHardToDelete();
            }
        }

        public void DeleteFolder(string folder, Object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            using (var request = requestImpl.As<Request>()) {
                request.Debug("Calling 'ProviderService::DeleteFolder' {0}".format(folder));
                if (string.IsNullOrEmpty(folder)) {
                    return;
                }
                if (Directory.Exists(folder)) {
                    folder.TryHardToDelete();
                }
            }
        }

        public void CreateFolder(string folder, Object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            using (var request = requestImpl.As<Request>()) {
                request.Debug("Calling 'ProviderService::CreateFolder'");

                if (!Directory.Exists(folder)) {
                    try {
                        Directory.CreateDirectory(folder);
                        request.Verbose("CreateFolder Success {0}", folder);
                        return;
                    }
                    catch (Exception e) {
                        request.Error(ErrorCategory.InvalidResult, folder, Constants.Messages.CreatefolderFailed, folder, e.Message);
                        return;
                    }
                }
                request.Verbose("CreateFolder -- Already Exists {0}", folder);
            }
        }

        public void DeleteFile(string filename, Object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            using (var request = requestImpl.As<Request>()) {
                request.Debug("Calling 'ProviderService::DeleteFile'");
                if (string.IsNullOrEmpty(filename)) {
                    return;
                }

                if (File.Exists(filename)) {
                    filename.TryHardToDelete();
                }
            }
        }

        public string GetKnownFolder(string knownFolder, Object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            using (var request = requestImpl.As<Request>()) {
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

        public string CanonicalizePath(string path,string currentDirectory) {
            return path.CanonicalizePath(!string.IsNullOrEmpty(currentDirectory));
        }

        public bool FileExists(string path) {
            return path.FileExists();
        }
        public bool DirectoryExists(string path) {
            return path.DirectoryExists();
        }

        public bool Install(string fileName, string additionalArgs, Object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            if (string.IsNullOrEmpty(fileName)) {
                return false;
            }

            using (var request = requestImpl.As<Request>()) {

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
                                    GetOptionValues = new Func<string,IEnumerable<string>> ((key) => {
                                        if (key.EqualsIgnoreCase("additionalArguments")) {
                                            return new String[] {additionalArgs};
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

        public bool IsSignedAndTrusted(string filename, Object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            if (string.IsNullOrEmpty(filename) || !FileExists(filename)) {
                return false;
            }

            using (var request = requestImpl.As<Request>()) {
                request.Debug("Calling 'ProviderService::IsSignedAndTrusted, '{0}'",filename);

                var wtd = new WinTrustData(filename);
                var result = NativeMethods.WinVerifyTrust(new IntPtr(-1), new Guid("{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}"), wtd);
                return result == WinVerifyTrustResult.Success;
            }
        }

        public bool ExecuteElevatedAction(string provider, string payload, Object requestImpl ) {
            // launches a new elevated host that 
            // talks back to this (unelevated) host for everything in HostApi
            // everything else should be handled in the new process.
            var guid = Guid.NewGuid();

            var properties = new Hashtable();
            properties.Add("portName", "OneGet_"+guid.ToString());
            properties.Add("authorizedGroup", "Administrators");
            properties.Add("secure", "true");
            properties.Add("exclusiveAddressUse", "true");
            properties.Add("strictBinding", "false");
            properties.Add("name", "OneGetHost");

            // set up the server IPC channel
            var serverChannel = new IpcServerChannel(properties, new BinaryServerFormatterSinkProvider(properties,null));

            ChannelServices.RegisterChannel(serverChannel, true);

            var instance = new RemotableHostApi(requestImpl.As<IHostApi>());

            var objRef = RemotingServices.Marshal(instance, "Host", typeof(IHostApi));
            var remoteUris = serverChannel.GetUrlsForUri("Host");
            var uri = remoteUris[0];
            // Create the client elevated
            try {
                var process = AsyncProcess.Start(new ProcessStartInfo {
                    FileName = Assembly.GetExecutingAssembly().Location,
                    Arguments = string.Format("{0} {1} {2}", uri, provider, (string.IsNullOrWhiteSpace(payload) ? "null" : payload).ToBase64()),
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

        public bool IsElevated {
            get {
                return AdminPrivilege.IsElevated;
            }
        }
    }
}