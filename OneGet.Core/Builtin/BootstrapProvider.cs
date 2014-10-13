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
    using System.Linq;
    using Implementation;
    using Packaging;
    using Utility.Extensions;
    using Utility.Platform;
    using Utility.Plugin;
    using IRequestObject = System.Object;

    public class BootstrapProvider {
        private static readonly string[] _urls = {
#if LOCAL_DEBUG
            "http://localhost:81/OneGet-Bootstrap.swidtag",
#endif 
            "https://go.microsoft.com/fwlink/?LinkId=404337",
            "https://go.microsoft.com/fwlink/?LinkId=404338",
            "https://oneget.org/oneget-providers.swidtag"
        };

        private static readonly Dictionary<string, string[]> _features = new Dictionary<string, string[]> {
            {Constants.Features.SupportedSchemes, new[] {"http", "https", "file"}},
            {Constants.Features.SupportedExtensions, new[] {"exe", "msi"}},
            {Constants.Features.MagicSignatures, Constants.Empty},
            {Constants.Features.AutomationOnly, Constants.Empty}
        };

        internal static IEnumerable<string> SupportedSchemes {
            get {
                return _features[Constants.Features.SupportedSchemes];
            }
        }

        /// <summary>
        ///     Returns the name of the Provider.
        /// </summary>
        /// <required />
        /// <returns>the name of the package provider</returns>
        public string GetPackageProviderName() {
            return "Bootstrap";
        }

        public void InitializeProvider(IRequestObject requestObject) {
        }

        public void GetFeatures(IRequestObject requestObject) {
            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }

            using (var request = requestObject.As<Request>()) {
                request.Debug("Calling 'Bootstrap::GetFeatures'");
                foreach (var feature in _features) {
                    request.Yield(feature);
                }
            }
        }

        public void GetDynamicOptions(string category, IRequestObject requestObject) {
            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }

            using (var request = requestObject.As<BoostrapRequest>()) {
                try {
                    request.Debug("Calling 'Bootstrap::GetDynamicOptions ({0})'", category);

                    switch ((category ?? string.Empty).ToLowerInvariant()) {
                        case "package":
                            // request.YieldDynamicOption( "ForceCheck", OptionType.Switch, false);

                            break;

                        case "source":
                            break;

                        case "install":
                            request.YieldDynamicOption("DestinationPath", OptionType.Folder.ToString(), true);
                            break;
                    }
                } catch {
                    // this makes it ignore new OptionCategories that it doesn't know about.
                }
            }
        }

        // --- Finds packages ---------------------------------------------------------------------------------------------------
        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="requiredVersion"></param>
        /// <param name="minimumVersion"></param>
        /// <param name="maximumVersion"></param>
        /// <param name="id"></param>
        /// <param name="requestObject"></param>
        /// <returns></returns>
        public void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, IRequestObject requestObject) {
            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }
            using (var request = requestObject.As<BoostrapRequest>()) {
                request.Debug("Calling 'Bootstrap::FindPackage'");

                var master = request.DownloadSwidtag(_urls);
                if (master == null) {
                    request.Warning(Constants.Messages.ProviderSwidtagUnavailable);
                    return;
                }

                if (name != null && name.EqualsIgnoreCase("oneget")) {
                    // they are looking for OneGet itself.
                    // future todo: let oneget update itself.
                    return;
                }

                // they are looking for a provider 
                if (string.IsNullOrEmpty(name)) {
                    // return all providers 
                    var providers = request.GetProviders(master);
                    foreach (var p in providers) {
                        request.YieldFromSwidtag(p, requiredVersion, minimumVersion, maximumVersion, name);
                    }
                } else {
                    // return just the one.
                    var provider = request.GetProvider(master, name);
                    if (provider != null) {
                        request.YieldFromSwidtag(provider, requiredVersion, minimumVersion, maximumVersion, name);
                    }
                }

                // return any matches in the name
            }
        }

        public void GetInstalledPackages(string name, IRequestObject requestObject) {
            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }

            using (var request = requestObject.As<BoostrapRequest>()) {
                request.Debug("Calling 'Bootstrap::GetInstalledPackages'");
                // return all the package providers as packages
            }
        }

        // --- operations on a package ---------------------------------------------------------------------------------------------------
        public void DownloadPackage(string fastPath, string location, IRequestObject requestObject) {
            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }
            using (var request = requestObject.As<BoostrapRequest>()) {
                request.Debug("Calling 'Bootstrap::DownloadPackage'");
            }
        }

        public void InstallPackage(string fastPath, IRequestObject requestObject) {
            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }
            // ensure that mandatory parameters are present.
            using (var request = requestObject.As<BoostrapRequest>()) {
                request.Debug("Calling 'Bootstrap::InstallPackage'");

                // verify the package integrity (ie, check if it's digitally signed before installing)

                var master = request.DownloadSwidtag(_urls);
                if (master == null) {
                    request.Warning(Constants.Messages.ProviderSwidtagUnavailable);
                    return;
                }

                var provider = request.GetProvider(master, fastPath);
                if (provider != null) {
                    // install the 'package'

                    if (!provider.XPath("/swid:SoftwareIdentity/swid:Meta[@providerType = 'assembly']").Any()) {
                        request.Error(ErrorCategory.InvalidOperation, fastPath, Constants.Messages.UnsupportedProviderType, fastPath);
                        return;
                    }
                    if (!Directory.Exists(request.DestinationPath)) {
                        request.Error(ErrorCategory.InvalidOperation, fastPath, Constants.Messages.DestinationPathNotSet);
                        return;
                    }

                    var targetFilename = provider.XPath("/swid:SoftwareIdentity/swid:Meta[@targetFilename]").GetAttribute("targetFilename");
                    if (string.IsNullOrEmpty(targetFilename)) {
                        request.Error(ErrorCategory.InvalidOperation, fastPath, Constants.Messages.InvalidFilename);
                        return;
                    }
                    targetFilename = Path.GetFileName(targetFilename);
                    var targetFile = Path.Combine(request.DestinationPath, targetFilename);

                    string tmpFile = null;
                    var failedBecauseInvalid = false;
                    // download the file
                    foreach (var link in provider.XPath("/swid:SoftwareIdentity/swid:Link[@rel = 'installationmedia']")) {
                        var href = link.Attributes["href"];
                        if (string.IsNullOrEmpty(href) || !Uri.IsWellFormedUriString(href, UriKind.Absolute)) {
                            request.Debug("Bad or missing uri: {0}", href);
                            continue;
                        }

                        try {
                            tmpFile = targetFilename.GenerateTemporaryFilename();

                            request.Debug("Downloading '{0}' to '{1}'", href, tmpFile);

                            if (!request.DownloadFileToLocation(new Uri(href), tmpFile)) {
                                request.Debug("Failed download of '{0}'", href);
                                continue;
                            }

                            request.Debug("Verifying the package");
                            // verify the package
                            var wtd = new WinTrustData(tmpFile);
                            var result = NativeMethods.WinVerifyTrust(new IntPtr(-1), new Guid("{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}"), wtd);
                            if (result != WinVerifyTrustResult.Success) {
                                request.Debug("Not Valid file '{0}' => '{1}'", href, tmpFile);
                                failedBecauseInvalid = true;
                                request.Warning(Constants.Messages.FileFailedVerification, href);
#if !DEBUG
                                tmpFile.TryHardToDelete();
                                continue;
#endif
                            }

                            // looks good! let's keep it
                            if (File.Exists(targetFile)) {
                                request.Debug("Removing old file '{0}'", targetFile);
                                targetFile.TryHardToDelete();
                            }

                            // is that file still there? 
                            if (File.Exists(targetFile)) {
                                request.Error(ErrorCategory.InvalidOperation, fastPath, Constants.Messages.UnableToRemoveFile, targetFile);
                                return;
                            }

                            request.Debug("Copying file '{0}' to '{1}'", tmpFile, targetFile);
                            File.Copy(tmpFile, targetFile);
                            if (File.Exists(targetFile)) {
                                // looks good to me.
                                request.YieldFromSwidtag(provider, null, null, null, fastPath);
                                return;
                            }
                        } catch (Exception e) {
                            e.Dump();
                        } finally {
                            if (!string.IsNullOrEmpty(tmpFile)) {
                                tmpFile.TryHardToDelete();
                            }
                        }
                    }
                    if (failedBecauseInvalid) {
                        request.Error(ErrorCategory.InvalidData, fastPath, Constants.Messages.FileFailedVerification, fastPath);
                    }
                } else {
                    request.Error(ErrorCategory.InvalidData, fastPath, Constants.Messages.UnableToResolvePackage, fastPath);
                }
            }
        }
    }
}