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
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Api;
    using Utility.Extensions;
    using Utility.Platform;
    using Utility.Plugin;

    internal class ProviderServicesImpl : IProviderServices {
        internal static IProviderServices Instance = new ProviderServicesImpl();
        private static readonly Regex _canonicalPackageRegex = new Regex("(.*?):(.*?)/(.*)");

        private PackageManagementService PackageManagementService {
            get {
                return PackageManager.Instance as PackageManagementService;
            }
        }

        public bool IsElevated {
            get {
                return AdminPrivilege.IsElevated;
            }
        }

        public string GetCanonicalPackageId(string providerName, string packageName, string version) {
            return "{0}:{1}/{2}".format(providerName, packageName, version);
        }

        public string ParseProviderName(string canonicalPackageId) {
            return _canonicalPackageRegex.Match(canonicalPackageId).Groups[1].Value;
        }

        public string ParsePackageName(string canonicalPackageId) {
            return _canonicalPackageRegex.Match(canonicalPackageId).Groups[2].Value;
        }

        public string ParsePackageVersion(string canonicalPackageId) {
            return _canonicalPackageRegex.Match(canonicalPackageId).Groups[3].Value;
        }

        public bool IsSupportedArchive(string localFilename, IRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }
            if (!request.IsCanceled) {
                return PackageManagementService.Archivers.Values.Any(archiver => archiver.IsSupportedFile(localFilename));
            }
            return false;
        }

        public void DownloadFile(Uri remoteLocation, string localFilename, IRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }
            
            if (!request.IsCanceled) {

                // check the Uri type, see if we have anyone who can handle that 
                // if so, call that provider's download file
                if (remoteLocation == null) {
                    throw new ArgumentNullException("remoteLocation");
                }
                if (request == null) {
                    throw new ArgumentNullException("request");
                }

                foreach (var downloader in PackageManagementService.Downloaders.Values) {
                    if (downloader.SupportedUriSchemes.Contains(remoteLocation.Scheme, StringComparer.OrdinalIgnoreCase)) {
                        downloader.DownloadFile(remoteLocation, localFilename, request);
                        return;
                    }
                }

                Error(request, ErrorCategory.NotImplemented, Constants.Messages.ProtocolNotSupported, remoteLocation.Scheme, Constants.Messages.ProtocolNotSupported, remoteLocation.Scheme);
            }
        }

        public IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, IRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (!request.IsCanceled) {
                // check who supports the archive type
                // and call that provider.
                if (request == null) {
                    throw new ArgumentNullException("request");
                }

                foreach (var archiver in PackageManagementService.Archivers.Values) {
                    if (archiver.IsSupportedFile(localFilename)) {
                        return archiver.UnpackArchive(localFilename, destinationFolder, request);
                    }
                }
                Error(request, ErrorCategory.NotImplemented, localFilename, Constants.Messages.UnsupportedArchive);
            }
            return Enumerable.Empty<string>();
        }

        public void AddPinnedItemToTaskbar(string item, IRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (!request.IsCanceled) {
                request.Debug("Calling 'ProviderService::AddPinnedItemToTaskbar'");
                ShellApplication.Pin(item);
            }
        }

        public void RemovePinnedItemFromTaskbar(string item, IRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (!request.IsCanceled) {

                request.Debug("Calling 'ProviderService::RemovePinnedItemFromTaskbar'");
                ShellApplication.Unpin(item);
            }
        }

        public void CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, IRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (!request.IsCanceled) {
                request.Debug("Calling 'ProviderService::CreateShortcutLink'");

                if (File.Exists(linkPath)) {
                    Verbose(request,"Creating Shortcut '{0}' => '{1}'", linkPath, targetPath);
                    ShellLink.CreateShortcut(linkPath, targetPath, description, workingDirectory, arguments);
                }
                Error(request, ErrorCategory.InvalidData, targetPath, Constants.Messages.UnableToCreateShortcutTargetDoesNotExist, targetPath);
            }
        }

        public void SetEnvironmentVariable(string variable, string value, string context, IRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (!request.IsCanceled) {
                if (context == null) {
                    throw new ArgumentNullException("context");
                }

                request.Debug("Calling 'ProviderService::SetEnvironmentVariable'");
                if (String.IsNullOrWhiteSpace(value)) {
                    RemoveEnvironmentVariable(variable, context, request);
                }

                switch (context.ToLowerInvariant()) {
                    case "system":
                        if (!IsElevated) {
                            Warning(request,"SetEnvironmentVariable Failed - Admin Elevation required to set variable '{0}' in machine context", variable);
                            return;
                        }
                        Verbose(request,"SetEnvironmentVariable (machine) '{0}' = '{1}'", variable, value);
                        Environment.SetEnvironmentVariable(variable, value, EnvironmentVariableTarget.Machine);
                        break;

                    default:
                        Verbose(request,"SetEnvironmentVariable (user) '{0}' = '{1}'", variable, value);
                        Environment.SetEnvironmentVariable(variable, value, EnvironmentVariableTarget.User);
                        break;
                }
                Environment.SetEnvironmentVariable(variable, null, EnvironmentVariableTarget.Process);
            }
        }

        public void RemoveEnvironmentVariable(string variable, string context, IRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (!request.IsCanceled) {

                if (context == null) {
                    throw new ArgumentNullException("context");
                }

                request.Debug("Calling 'ProviderService::RemoveEnvironmentVariable'");

                if (String.IsNullOrWhiteSpace(variable)) {
                    return;
                }

                switch (context.ToLowerInvariant()) {
                    case "user":
                        Verbose(request,"RemoveEnvironmentVariable (user)--'{0}'", variable);
                        Environment.SetEnvironmentVariable(variable, null, EnvironmentVariableTarget.User);
                        break;

                    case "system":
                        if (!IsElevated) {
                            Warning(request, Constants.Messages.RemoveEnvironmentVariableRequiresElevation, variable);
                            return;
                        }
                        Verbose(request,"RemoveEnvironmentVariable (machine)", "'{0}'", variable);
                        Environment.SetEnvironmentVariable(variable, null, EnvironmentVariableTarget.Machine);
                        break;

                    default:
                        Verbose(request,"RemoveEnvironmentVariable (all) '{0}'", variable);
                        Environment.SetEnvironmentVariable(variable, null, EnvironmentVariableTarget.User);
                        Environment.SetEnvironmentVariable(variable, null, EnvironmentVariableTarget.Machine);
                        break;
                }
                Environment.SetEnvironmentVariable(variable, null, EnvironmentVariableTarget.Process);
            }
        }

        public void CopyFile(string sourcePath, string destinationPath, IRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (!request.IsCanceled) {
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
                        Error(request, ErrorCategory.OpenError, destinationPath, Constants.Messages.UnableToOverwriteExistingFile, destinationPath);
                    }
                }
                File.Copy(sourcePath, destinationPath);
                if (!File.Exists(destinationPath)) {
                    Error(request, ErrorCategory.InvalidResult, destinationPath, Constants.Messages.UnableToCopyFileTo, destinationPath);
                }
            }
        }

        public void Delete(string path, IRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (!request.IsCanceled) {
                request.Debug("Calling 'ProviderService::Delete'");
                if (String.IsNullOrWhiteSpace(path)) {
                    return;
                }

                path.TryHardToDelete();
            }
        }

        public void DeleteFolder(string folder, IRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (!request.IsCanceled) {
                request.Debug("Calling 'ProviderService::DeleteFolder' {0}".format(folder));
                if (String.IsNullOrWhiteSpace(folder)) {
                    return;
                }
                if (Directory.Exists(folder)) {
                    folder.TryHardToDelete();
                }
            }
        }

        public void CreateFolder(string folder, IRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (!request.IsCanceled) {
                request.Debug("Calling 'ProviderService::CreateFolder'");

                if (!Directory.Exists(folder)) {
                    try {
                        Directory.CreateDirectory(folder);
                        Verbose(request,"CreateFolder Success {0}" ,folder);
                        return;
                    } catch (Exception e) {
                        Error(request, ErrorCategory.InvalidResult, folder, Constants.Messages.CreatefolderFailed, folder, e.Message);
                        return;
                    }
                }
                Verbose(request, "CreateFolder -- Already Exists {0}", folder);
            }
        }
        public bool Error(IRequest request,ErrorCategory category, string targetObjectValue, string messageText, params object[] args) {
            return request.Error(messageText, category.ToString(), targetObjectValue, request.FormatMessageString(messageText, args));
        }

        public bool Warning(IRequest request, string messageText, params object[] args) {
            return request.Warning(request.FormatMessageString(messageText, args));
        }

        public bool Message(IRequest request, string messageText, params object[] args) {
            return request.Message(request.FormatMessageString(messageText, args));
        }

        public bool Verbose(IRequest request, string messageText, params object[] args) {
            return request.Verbose(request.FormatMessageString(messageText, args));
        }

        public bool Debug(IRequest request, string messageText, params object[] args) {
            return request.Debug(request.FormatMessageString(messageText, args));
        }

        public void DeleteFile(string filename, IRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (!request.IsCanceled) {

                request.Debug("Calling 'ProviderService::DeleteFile'");
                if (String.IsNullOrWhiteSpace(filename)) {
                    return;
                }

                if (File.Exists(filename)) {
                    filename.TryHardToDelete();
                }
            }
        }

        public string GetKnownFolder(string knownFolder, IRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (!request.IsCanceled) {

                request.Debug("Calling 'ProviderService::GetKnownFolder'");
                if (!String.IsNullOrWhiteSpace(knownFolder)) {
                    if (knownFolder.Equals("tmp", StringComparison.OrdinalIgnoreCase) || knownFolder.Equals("temp", StringComparison.OrdinalIgnoreCase)) {
                        return FilesystemExtensions.TempPath;
                    }

                    if (knownFolder.Equals("SystemAssemblyLocation", StringComparison.OrdinalIgnoreCase)) {
                        return PackageManagementService.SystemAssemblyLocation;
                    }

                    if (knownFolder.Equals("UserAssemblyLocation", StringComparison.OrdinalIgnoreCase)) {
                        return PackageManagementService.UserAssemblyLocation;
                    }

                    if (knownFolder.Equals("ProviderAssemblyLocation", StringComparison.OrdinalIgnoreCase)) {
                        return AdminPrivilege.IsElevated ? PackageManagementService.SystemAssemblyLocation : PackageManagementService.UserAssemblyLocation;
                    }

                    KnownFolder folder;
                    if (Enum.TryParse(knownFolder, true, out folder)) {
                        return KnownFolders.GetFolderPath(folder);
                    }
                }

                Error(request,ErrorCategory.InvalidArgument, knownFolder, Constants.Messages.UnknownFolderId, knownFolder);
            }
            return null;
        }

        public string CanonicalizePath(string path, string currentDirectory) {
            if (String.IsNullOrWhiteSpace(path)) {
                return null;
            }

            return path.CanonicalizePath(!String.IsNullOrWhiteSpace(currentDirectory));
        }

        public bool FileExists(string path) {
            if (String.IsNullOrWhiteSpace(path)) {
                return false;
            }

            return path.FileExists();
        }

        public bool DirectoryExists(string path) {
            if (String.IsNullOrWhiteSpace(path)) {
                return false;
            }
            return path.DirectoryExists();
        }

        public bool Install(string fileName, string additionalArgs, IRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (!request.IsCanceled) {



                if (String.IsNullOrWhiteSpace(fileName)) {
                    return false;
                }

                // high-level api for simply installing a file 
                // returns false if unsuccessful.
                foreach (var provider in PackageManager.Instance.PackageProviders) {
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
                                Debug(request,"Installed internal package {0}", installedPackage.Name);
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsSignedAndTrusted(string filename, IRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (!request.IsCanceled) {

                if (String.IsNullOrWhiteSpace(filename) || !FileExists(filename)) {
                    return false;
                }

                Debug(request,"Calling 'ProviderService::IsSignedAndTrusted, '{0}'", filename);

                var wtd = new WinTrustData(filename);
                var result = NativeMethods.WinVerifyTrust(new IntPtr(-1), new Guid("{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}"), wtd);
                return result == WinVerifyTrustResult.Success;
            }
            return false;
        }

        public bool ExecuteElevatedAction(string provider, string payload, IRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (!request.IsCanceled) {
#if WHAT_DO_WE_DO_TO_REMOTE_BETWEEN_PROCESSES_WITHOUT_REMOTING_HUH
            if (request == null) {
                throw new ArgumentNullException("request");
                    
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
#endif
            }
            return false;
        }
    }
}