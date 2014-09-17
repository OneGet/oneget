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

            requestImpl.As<IRequest>().Error("PROTOCOL_NOT_SUPPORTED", "NotImplemented", remoteLocation.Scheme, "PROTOCOL_NOT_SUPPORTED");
        }

        public IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, Object requestImpl) {
            // check who supports the archive type
            // and call that provider.
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            requestImpl.As<IRequest>().Error("ARCHIVE_NOT_SUPPORTED", "NotImplemented", localFilename, "PROTOCOL_NOT_SUPPORTED");
            return Enumerable.Empty<string>();
        }

        public void AddPinnedItemToTaskbar(string item, object requestImpl) {
            
        }

        public void RemovePinnedItemFromTaskbar(string item, object requestImpl) {
            
        }

        public void CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, Object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            using (var request = requestImpl.As<Request>()) {
                request.Debug("Calling 'CommonServiceProvider::CreateShortcutLink'");

                if (File.Exists(linkPath)) {
                    request.Verbose("Creating Shortcut '{0}' => '{1}'", linkPath, targetPath);
                    ShellLink.CreateShortcut(linkPath, targetPath, description, workingDirectory, arguments);
                }
                request.Error("ObjectNotFound", targetPath, Constants.UnableToCreateShortcutTargetDoesNotExist, targetPath);
            }
        }

        public void SetEnvironmentVariable(string variable, string value, string context, Object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            using (var request = requestImpl.As<Request>()) {
                request.Debug("Calling 'CommonServiceProvider::SetEnvironmentVariable'");
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
                request.Debug("Calling 'CommonServiceProvider::RemoveEnvironmentVariable'");

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
                            request.Warning(Constants.RemoveEnvironmentVariableRequiresElevation, variable);
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
                request.Debug("Calling 'CommonServiceProvider::CopyFile'");
                if (sourcePath == null) {
                    throw new ArgumentNullException("sourcePath");
                }
                if (destinationPath == null) {
                    throw new ArgumentNullException("destinationPath");
                }
                if (File.Exists(destinationPath)) {
                    destinationPath.TryHardToDelete();
                    if (File.Exists(destinationPath)) {
                        request.Error("OpenError", destinationPath, Constants.UnableToOverwriteExistingFile, destinationPath);
                    }
                }
                File.Copy(sourcePath, destinationPath);
                if (!File.Exists(destinationPath)) {
                    request.Error("InvalidResult", destinationPath, Constants.UnableToCopyFileTo, destinationPath);
                }
            }
        }

        public void Delete(string path, Object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            using (var request = requestImpl.As<Request>()) {
                request.Debug("Calling 'CommonServiceProvider::Delete'");
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
                request.Debug("Calling 'CommonServiceProvider::DeleteFolder' {0}", folder);
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
                request.Debug("Calling 'CommonServiceProvider::CreateFolder'");

                if (!Directory.Exists(folder)) {
                    try {
                        Directory.CreateDirectory(folder);
                        request.Verbose("CreateFolder Success {0}", folder);
                        return;
                    }
                    catch (Exception e) {
                        request.Error("InvalidResult", folder, Constants.CreatefolderFailed, folder, e.Message);
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
                request.Debug("Calling 'CommonServiceProvider::DeleteFile'");
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
                request.Debug("Calling 'CommonServiceProvider::GetKnownFolder'");
                if (!string.IsNullOrEmpty(knownFolder)) {
                    if (knownFolder.Equals("tmp", StringComparison.OrdinalIgnoreCase) || knownFolder.Equals("temp", StringComparison.OrdinalIgnoreCase)) {
                        return FilesystemExtensions.TempPath;
                    }
                    KnownFolder folder;
                    if (Enum.TryParse(knownFolder, true, out folder)) {
                        return KnownFolders.GetFolderPath(folder);
                    }
                }

                request.Error("InvalidArgument", knownFolder, Constants.UnknownFolderId, knownFolder);
            }
            return null;
        }

        public bool IsElevated {
            get {
                return AdminPrivilege.IsElevated;
            }
        }
    }
}