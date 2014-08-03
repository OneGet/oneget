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

namespace OneGet.PackageProvider.Bootstrap {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.OneGet.Utility.Extensions;
    using Microsoft.OneGet.Utility.Platform;
    using RequestImpl = System.Object;

    public class BootstrapProvider {
        
        private static readonly string[] _urls = new[] {
#if DEBUG
            "http://localhost:81/OneGet-Bootstrap.swidtag",
#endif 
            "http://downloads.coapp.org/oneget/WellKnownProviders.swidtag",
            "https://go.microsoft.com/fwlink/?LinkId=404337",
            "https://go.microsoft.com/fwlink/?LinkId=404338",
        };

        private static readonly Dictionary<string, string[]> _features = new Dictionary<string, string[]> {
            { "schemes", new[] { "http", "https", "file" } }, 
            { "extensions", new[] { "exe", "msi" } }, 
            { "magic-signatures", Constants.Empty },
            { Constants.AutomationOnlyFeature,Constants.Empty}
        };

        internal static IEnumerable<string> SupportedSchemes {
            get {
                return _features["schemes"];
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

        public void InitializeProvider(object dynamicInterface, RequestImpl requestImpl) {
            RequestExtensions.RemoteDynamicInterface = dynamicInterface;
        }

        public void GetFeatures(RequestImpl requestImpl) {
            using (var request =requestImpl.As<Request>()) {
                request.Debug("Calling 'Bootstrap::GetFeatures'");
                foreach (var feature in _features) {
                    request.Yield(feature);
                }
            }
        }

        public void GetDynamicOptions(int category, RequestImpl requestImpl) {
            using (var request =requestImpl.As<Request>()) {
                try {
                    var cat = (OptionCategory)category;
                    request.Debug("Calling 'Bootstrap::GetDynamicOptions ({0})'", cat);

                    switch (cat) {
                        case OptionCategory.Package:
                            // request.YieldDynamicOption(cat, "ForceCheck", OptionType.Switch, false);

                            break;

                        case OptionCategory.Source:
                            break;

                        case OptionCategory.Install:
                            request.YieldDynamicOption(cat, "DestinationPath", OptionType.Folder, true);
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
        /// <param name="c"></param>
        /// <returns></returns>
        public void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, RequestImpl requestImpl) {
            using (var request =requestImpl.As<Request>()) {
                request.Debug("Calling 'Bootstrap::FindPackage'");

                var master = request.DownloadSwidtag(_urls);
                if (master == null) {
                    request.Warning(Constants.ProviderSwitdagUnavailable);
                    return;
                }

                if (name.EqualsIgnoreCase("oneget")) {
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

        /* NOT SUPPORTED -- AT THIS TIME 
         * 
        public void FindPackageByFile(string filePath, int id, RequestImpl requestImpl) {
            using (var request =requestImpl.As<Request>()) {
                request.Debug("Calling 'Bootstrap::FindPackageByFile'");
            }
        }

        public void FindPackageByUri(Uri uri, int id, RequestImpl requestImpl) {
            using (var request =requestImpl.As<Request>()) {
                request.Debug("Calling 'Bootstrap::FindPackageByUri'");

                // check if this URI is a valid source
                // if it is, get the list of packages from this source

                // otherwise, download the Uri and see if it's a package 
                // that we support.
            }
        }
         * 
         * 
        public void GetPackageDetails(string fastPath, RequestImpl requestImpl) {
            using (var request =requestImpl.As<Request>()) {
                request.Debug("Calling 'Bootstrap::GetPackageDetails'");
            }
        }
         * 
         
        public void GetPackageDependencies(string fastPath, RequestImpl requestImpl) {
            using (var request =requestImpl.As<Request>()) {
                request.Debug("Calling 'Bootstrap::GetPackageDependencies'");

            }
        }

         *  public void UninstallPackage(string fastPath, RequestImpl requestImpl) {
            using (var request =requestImpl.As<Request>()) {
                request.Debug("Calling 'Bootstrap::UninstallPackage'");
            }
        }
         * 
         */

        public void GetInstalledPackages(string name, RequestImpl requestImpl) {
            using (var request =requestImpl.As<Request>()) {
                request.Debug("Calling 'Bootstrap::GetInstalledPackages'");
                // return all the package providers as packages
            }
        }

        // --- operations on a package ---------------------------------------------------------------------------------------------------
        public void DownloadPackage(string fastPath, string location, RequestImpl requestImpl) {
            using (var request =requestImpl.As<Request>()) {
                request.Debug("Calling 'Bootstrap::DownloadPackage'");
            }
        }

        public void InstallPackage(string fastPath, RequestImpl requestImpl) {
            // ensure that mandatory parameters are present.
            using (var request =requestImpl.As<Request>()) {
                request.Debug("Calling 'Bootstrap::InstallPackage'");

                // verify the package integrity (ie, check if it's digitally signed before installing)

                var master = request.DownloadSwidtag(_urls);
                if (master == null) {
                    request.Warning(Constants.ProviderSwitdagUnavailable);
                    return;
                }

                var provider = request.GetProvider(master, fastPath);
                if (provider != null) {
                    // install the 'package'

                    if (!provider["/swid:SoftwareIdentity/swid:Meta[@providerType = 'assembly']"].Any()) {
                        request.Error(ErrorCategory.InvalidOperation, fastPath, Constants.UnsupportedProviderType);
                        return;
                    }
                    if (!Directory.Exists(request.DestinationPath)) {
                        request.Error(ErrorCategory.InvalidOperation, fastPath, Constants.DestinationPathNotSet);
                        return;
                    }

                    var targetFilename = provider["/swid:SoftwareIdentity/swid:Meta[@targetFilename]"].GetAttribute("targetFilename");
                    if (string.IsNullOrEmpty(targetFilename)) {
                        request.Error(ErrorCategory.InvalidOperation, fastPath, Constants.InvalidFilename);
                        return;
                    }
                    targetFilename = Path.GetFileName(targetFilename);
                    var targetFile = Path.Combine(request.DestinationPath, targetFilename);

                    string tmpFile = null;
                    var failedBecauseInvalid = false;
                    // download the file
                    foreach (var link in provider["/swid:SoftwareIdentity/swid:Link[@rel = 'installationmedia']"]) {
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
                                request.Warning(Constants.FileFailedVerification, href);
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
                                request.Error(ErrorCategory.InvalidOperation, fastPath, Constants.UnableToRemoveFile, targetFile);
                                return;
                            }

                            request.Debug("Copying file '{0}' to '{1}'", tmpFile, targetFile);
                            File.Copy(tmpFile, targetFile);
                            if (File.Exists(targetFile)) {
                                // looks good to me.
                                request.YieldFromSwidtag(provider, null, null, null, fastPath);
                                break;
                            }
                        } catch (Exception e) {
                            e.Dump();
                        } finally {
                            if (!string.IsNullOrEmpty(tmpFile)) {
                                tmpFile.TryHardToDelete();
                            }
                        }
                    }
                } else {
                    request.Error(ErrorCategory.InvalidData, fastPath, Constants.UnableToResolvePackage);
                }
            }
        }
    }
}