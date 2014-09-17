using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.OneGet.Builtin {
    using System.IO;
    using System.Net;
    using System.Threading;
    using Implementation;
    using Providers;
    using Utility.Extensions;
    using Utility.Plugin;

    class HttpDownloader {
        private static readonly string[] _schemes = new [] {
            "http", "https"
        };
        public void InitializeProvider(object dynamicInterface, object requestImpl) {
            
        }

        public IEnumerable<string> SupportedSchemes {
            get {
                return _schemes;
            }
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
                    request.Error(Constants.InvalidResult, remoteLocation.ToString(), Constants.SchemeNotSupported, remoteLocation.Scheme);
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
                    var percent = (args.BytesReceived * 100) / args.TotalBytesToReceive;
                    // Progress(requestImpl, 2, (int)percent, "Downloading {0} of {1} bytes", args.BytesReceived, args.TotalBytesToReceive);
                };
                webClient.DownloadFileAsync(remoteLocation, localFilename);
                done.WaitOne();
                if (!File.Exists(localFilename)) {
                    request.Error(Constants.InvalidResult, remoteLocation.ToString(), "Unable to download '{0}' to file '{1}'", remoteLocation.ToString(), localFilename);
                }
            }
        }
    }
}
