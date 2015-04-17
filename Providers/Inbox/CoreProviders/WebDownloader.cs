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

namespace Microsoft.PackageManagement.Providers {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading;
    using Implementation;
    using Utility.Extensions;

    public class WebDownloader {
        internal static string ProviderName = "WebDownloader";

        private static readonly Dictionary<string, string[]> _features = new Dictionary<string, string[]> {
            {Constants.Features.SupportedSchemes, new[] {"http", "https", "ftp", "file"}},
        };

        /// <summary>
        ///     Returns a collection of strings to the client advertizing features this provider supports.
        /// </summary>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with
        ///     the CORE and HOST
        /// </param>
        public void GetFeatures(Request request) {
            if( request == null ) {
                throw new ArgumentNullException("request");
            }

            // Nice-to-have put a debug message in that tells what's going on.
            request.Debug("Calling '{0}::GetFeatures' ", ProviderName);
            foreach (var feature in _features) {
                request.Yield(feature);
            }
        }

        public void InitializeProvider(Request request) {
        }

        public string GetDownloaderName() {
            return ProviderName;
        }

        public void DownloadFile(Uri remoteLocation, string localFilename, Request request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            if (remoteLocation == null) {
                throw new ArgumentNullException("remoteLocation");
            }

            request.Debug("Calling 'WebDownloader::DownloadFile' '{0}','{1}'", remoteLocation, localFilename);

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

            // request.Verbose("Downloading", "'{0}' to '{1}'", remoteLocation, localFilename);
            var pid = request.StartProgress(0, "Downloading '{0}'", remoteLocation);
            var webClient = new WebClient();

            // Apparently, places like Codeplex know to let this thru!
            webClient.Headers.Add("user-agent", "chocolatey command line");

            var done = new ManualResetEvent(false);

            webClient.DownloadFileCompleted += (sender, args) => {
                if (args.Cancelled || args.Error != null) {
                    localFilename = null;
                }

                done.Set();
            };
            var lastPercent = 0;

            webClient.DownloadProgressChanged += (sender, args) => {
                // Progress(requestObject, 2, (int)percent, "Downloading {0} of {1} bytes", args.BytesReceived, args.TotalBytesToReceive);
                var percent = (int)((args.BytesReceived*100)/args.TotalBytesToReceive);
                if (percent > lastPercent) {
                    lastPercent = percent;
                    request.Progress(pid, (int)((args.BytesReceived*100)/args.TotalBytesToReceive), "To {0}", localFilename);
                }
            };
            webClient.DownloadFileAsync(remoteLocation, localFilename);
            done.WaitOne();
            if (!File.Exists(localFilename)) {
                request.CompleteProgress(pid, false);
                request.Error(ErrorCategory.InvalidResult, remoteLocation.ToString(), Constants.Messages.UnableToDownload, remoteLocation.ToString(), localFilename);
            }
            request.CompleteProgress(pid, true);
        }

    }
}
