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

namespace Microsoft.OneGet.Builtin {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading;
    using Implementation;
    using Utility.Extensions;
    using Utility.Plugin;

    public class HttpDownloader {
        private static readonly string[] _schemes = new[] {
            "http", "https"
        };

        public IEnumerable<string> SupportedSchemes {
            get {
                return _schemes;
            }
        }

        public void InitializeProvider(object dynamicInterface, object requestImpl) {
        }

        public string GetDownloaderName() {
            return "HttpDownloader";
        }

        public void DownloadFile(Uri remoteLocation, string localFilename, object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            if (remoteLocation == null) {
                throw new ArgumentNullException("remoteLocation");
            }

            using (var request = requestImpl.As<Request>()) {
                request.Debug("Calling 'HttpDownloader::DownloadFile' '{0}','{1}'", remoteLocation, localFilename);

                if (remoteLocation.Scheme.ToLowerInvariant() != "http" && remoteLocation.Scheme.ToLowerInvariant() != "https") {
                    request.Error(ErrorCategory.InvalidResult, remoteLocation.ToString(), Constants.Messages.SchemeNotSupported, remoteLocation.Scheme);
                    return;
                }

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
                    CompleteProgress(requestImpl, 2, true);
                     */
                    if (args.Cancelled || args.Error != null) {
                        localFilename = null;
                    }

                    done.Set();
                };
                webClient.DownloadProgressChanged += (sender, args) => {
                    var percent = (args.BytesReceived*100)/args.TotalBytesToReceive;
                    // Progress(requestImpl, 2, (int)percent, "Downloading {0} of {1} bytes", args.BytesReceived, args.TotalBytesToReceive);
                };
                webClient.DownloadFileAsync(remoteLocation, localFilename);
                done.WaitOne();
                if (!File.Exists(localFilename)) {
                    request.Error(ErrorCategory.InvalidResult, remoteLocation.ToString(), Constants.Messages.UnableToDownload, remoteLocation.ToString(), localFilename);
                }
            }
        }
    }
}