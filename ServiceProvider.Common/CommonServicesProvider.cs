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

namespace Microsoft.OneGet.ServicesProvider.Common {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading;
    using Extensions;
    using Platform;
    using Callback = System.Object;

    public class CommonServicesProvider {
        private static readonly string[] _empty = new string[0];
        private static readonly Dictionary<string, string[]> _features = new Dictionary<string, string[]> {
            { "download-schemes", new [] {"http", "https", "file"} },
            { "archive-extensions", new [] {"zip"} },
        };

        /// <summary>
        ///     Allows the Provider to do one-time initialization.
        ///     This is called after the Provider is instantiated .
        /// 
        /// 
        /// </summary>
        /// <param name="dynamicInterface">A reference to the DynamicInterface class -- used to implement late-binding</param>
        /// <param name="c">Callback Delegate Reference</param>
        public void InitializeProvider(object dynamicInterface, Callback c) {
            RequestExtensions.RemoteDynamicInterface = dynamicInterface;
        }


        /// <summary>
        /// Gets the features advertized from the provider
        /// </summary>
        /// <param name="c"></param>
        public void GetFeatures(Callback c){
            if (c == null) {
                throw new ArgumentNullException("c");
            }

            using (var request = c.As<Request>()) {
                request.Debug("Calling 'NuGet::GetFeatures'");
                foreach (var feature in _features) {
                    request.Yield(feature);
                }
            }
        }

        /// <summary>
        /// Gets dynamically defined options from the provider
        /// </summary>
        /// <param name="category"></param>
        /// <param name="c"></param>
        public void GetDynamicOptions(int category, Callback c){
            if (c == null) {
                throw new ArgumentNullException("c");
            }

            using (var request = c.As<Request>()) {
                request.Debug("Calling 'GetDynamicOptions'" );
            }
        }

        /// <summary>
        ///     Returns the name of the Provider. Doesn't need callback .
        /// </summary>
        /// <returns></returns>
        public string GetServicesProviderName() {
            return "Common";
        }


        public void DownloadFile(Uri remoteLocation, string localFilename, Callback c) {
            if (c == null) {
                throw new ArgumentNullException("c");
            }

            if (remoteLocation == null) {
                throw new ArgumentNullException("remoteLocation");
            }


            using (var request = c.As<Request>()) {
                request.Debug("Calling 'CommonServiceProvider::DownloadFile'");

                if (localFilename == null) {
                    localFilename = Path.Combine(FilesystemExtensions.TempPath, "file.bin");
                }

                localFilename = Path.GetFullPath(localFilename);
                if (Directory.Exists(localFilename)) {
                    localFilename = Path.Combine(localFilename, "file.bin");
                }

                var folder = Path.GetDirectoryName(localFilename);
                if (!Directory.Exists(folder)) {
                    Directory.CreateDirectory(folder);
                }
                if (File.Exists(localFilename)) {
                    localFilename.TryHardToDelete();
                }

                request.Verbose("Downloading", "'{0}' to '{1}'", remoteLocation, localFilename);
                var webClient = new WebClient();

                // Apparently, places like Codeplex know to let this thru!
                webClient.Headers.Add("user-agent", "chocolatey command line");

                var done = new ManualResetEvent(false);

                webClient.DownloadFileCompleted += (sender, args) => {
                    /* 
                    CompleteProgress(c, 2, true);
                     */
                    if (args.Cancelled || args.Error != null) {
                        localFilename = null;
                    }
                     
                    done.Set();
                };
                webClient.DownloadProgressChanged += (sender, args) => {
                    var percent = (args.BytesReceived*100)/args.TotalBytesToReceive;
                    // Progress(c, 2, (int)percent, "Downloading {0} of {1} bytes", args.BytesReceived, args.TotalBytesToReceive);
                };
                webClient.DownloadFileAsync(remoteLocation, localFilename);
                done.WaitOne();
                if (!File.Exists(localFilename)) {
                    request.Error("Unable to download '{0}' to file '{1}'", remoteLocation.ToString(), localFilename);
                }
            }

        }

        public IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, Callback c) {
            if (c == null) {
                throw new ArgumentNullException("c");
            }

            using (var request = c.As<Request>()) {
                request.Debug("Calling 'CommonServiceProvider::xxx'");
                request.Error("Unsupported Archive {0}", localFilename);
            }

            return null;
        }

        public void AddPinnedItemToTaskbar(string item, Callback c) {
            if (c == null) {
                throw new ArgumentNullException("c");
            }

            using (var request = c.As<Request>()) {
                request.Debug("Calling 'CommonServiceProvider::xxx'");
            }
        }

        public void RemovePinnedItemFromTaskbar(string item, Callback c) {
            if (c == null) {
                throw new ArgumentNullException("c");
            }

            using (var request = c.As<Request>()) {
                request.Debug("Calling 'CommonServiceProvider::xxx'");
            }
        }

        public void CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, Callback c) {
            if (c == null) {
                throw new ArgumentNullException("c");
            }

            using (var request = c.As<Request>()) {
                request.Debug( "Calling 'CommonServiceProvider::CreateShortcutLink'");

                if (File.Exists(linkPath)) {
                    request.Verbose( "Creating Shortcut '{0}' => '{1}'", linkPath, targetPath);
                    ShellLink.CreateShortcut(linkPath, targetPath, description, workingDirectory, arguments);
                }
                request.Error( "Unable to create shortcut - target '{0}' does not exist", targetPath);
            }
        }

        public void SetEnvironmentVariable(string variable, string value, int context, Callback c) {
            if (c == null) {
                throw new ArgumentNullException("c");
            }

            using (var request = c.As<Request>()) {
                request.Debug("Calling 'CommonServiceProvider::SetEnvironmentVariable'");
                if (string.IsNullOrEmpty(value)) {
                    RemoveEnvironmentVariable(variable, context, c);
                }

                switch ((EnvironmentContext)context) {
                    case EnvironmentContext.System:
                        if (!IsElevated(c)) {
                            request.Warning("SetEnvironmentVariable Failed", "Admin Elevation required to set variable '{0}' in machine context", variable);
                            return;
                        }
                        request.Verbose("SetEnvironmentVariable (machine)", "'{0}' = '{1}'", variable, value);
                        Environment.SetEnvironmentVariable(variable, value, EnvironmentVariableTarget.Machine);
                        break;
                    default:
                        request.Verbose( "SetEnvironmentVariable (user)", "'{0}' = '{1}'", variable, value);
                        Environment.SetEnvironmentVariable(variable, value, EnvironmentVariableTarget.User);
                        break;
                }
                Environment.SetEnvironmentVariable(variable, null, EnvironmentVariableTarget.Process);
            }
        }

        public void RemoveEnvironmentVariable(string variable, int context, Callback c) {
            if (c == null) {
                throw new ArgumentNullException("c");
            }

            using (var request = c.As<Request>()) {
                request.Debug("Calling 'CommonServiceProvider::RemoveEnvironmentVariable'");
                
                if (string.IsNullOrEmpty(variable)) {
                    return;
                }
                switch ((EnvironmentContext)context) {
                    case EnvironmentContext.User:
                        request.Verbose( "RemoveEnvironmentVariable (user)--'{0}'", variable);
                        Environment.SetEnvironmentVariable(variable, null, EnvironmentVariableTarget.User);
                        break;
                    case EnvironmentContext.System:
                        if (!IsElevated(c)) {
                            request.Warning( "RemoveEnvironmentVariable Failed--Admin Elevation required to remove variable '{0}' from machine context", variable);
                            return;
                        }
                        request.Verbose("RemoveEnvironmentVariable (machine)", "'{0}'", variable);
                        Environment.SetEnvironmentVariable(variable, null, EnvironmentVariableTarget.Machine);
                        break;
                    default:
                        request.Verbose( "RemoveEnvironmentVariable (all)", "'{0}'", variable);
                        Environment.SetEnvironmentVariable(variable, null, EnvironmentVariableTarget.User);
                        Environment.SetEnvironmentVariable(variable, null, EnvironmentVariableTarget.Machine);
                        break;
                }
                Environment.SetEnvironmentVariable(variable, null, EnvironmentVariableTarget.Process);
            }
        }

        public void CopyFile(string sourcePath, string destinationPath, Callback c) {
            if (c == null) {
                throw new ArgumentNullException("c");
            }

            using (var request = c.As<Request>()) {
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
                        request.Error("Unable to overwrite existing file '{0}'", destinationPath);
                    }
                }
                File.Copy(sourcePath, destinationPath);
                if (!File.Exists(destinationPath)) {
                    request.Error("Unable to copy file to '{0}'", destinationPath);
                }
            }
        }

        public void Delete(string path, Callback c) {
            if (c == null) {
                throw new ArgumentNullException("c");
            }

            using (var request = c.As<Request>()) {
                request.Debug("Calling 'CommonServiceProvider::Delete'");
                if (string.IsNullOrEmpty(path)) {
                    return;
                }

                path.TryHardToDelete();
            }
        }

        public void DeleteFolder(string folder, Callback c) {
            if (c == null) {
                throw new ArgumentNullException("c");
            }

            using (var request = c.As<Request>()) {
                request.Debug("Calling 'CommonServiceProvider::DeleteFolder'");
                if (string.IsNullOrEmpty(folder)) {
                    return;
                }
                if (Directory.Exists(folder)) {
                    folder.TryHardToDelete();
                }
            }
        }

        public void CreateFolder(string folder, Callback c) {
            if (c == null) {
                throw new ArgumentNullException("c");
            }

            using (var request = c.As<Request>()) {
                request.Debug("Calling 'CommonServiceProvider::CreateFolder'");

                if (!Directory.Exists(folder)) {
                    try {
                        Directory.CreateDirectory(folder);
                        request.Verbose( "CreateFolder Success {0}", folder);
                        return;
                    }
                    catch (Exception e) {
                        request.Error( "CreateFolder Failed '{0}' -- {1}", folder, e.Message);
                        return;
                    }
                }
                request.Verbose("CreateFolder -- Already Exists {0}", folder);
            }
        }

        public void DeleteFile(string filename, Callback c) {
            if (c == null) {
                throw new ArgumentNullException("c");
            }

            using (var request = c.As<Request>()) {
                request.Debug("Calling 'CommonServiceProvider::DeleteFile'");
                if (string.IsNullOrEmpty(filename)) {
                    return;
                }

                if (File.Exists(filename)) {
                    filename.TryHardToDelete();
                }
            }
        }

        public string GetKnownFolder(string knownFolder, Callback c) {
            if (c == null) {
                throw new ArgumentNullException("c");
            }

            using (var request = c.As<Request>()) {
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

                request.Error("Unknown Folder '{0}'", knownFolder);
            }
            return null;
        }

        public bool IsElevated(Callback c) {
            return AdminPrivilege.IsElevated;           
        }
    }

    public enum EnvironmentContext {
        Process = 0,
        User = 1,
        System = 2
    }
}