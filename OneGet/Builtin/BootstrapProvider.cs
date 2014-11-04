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
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using Implementation;
    using Packaging;
    using Utility.Async;
    using Utility.Extensions;
    using Utility.Platform;
    using Utility.Plugin;
    using Utility.Xml;
    using IRequestObject = System.Object;

    public class BootstrapProvider {
        private static readonly string[] _urls = {
#if LOCAL_DEBUG
            "http://localhost:81/providers.swidtag",
#endif 
            "http://go.microsoft.com/fwlink/?LinkID=517832",
            "https://oneget.org/providers.swidtag"
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
            // we should go find out what's available once here just to make sure that 
            // we have a list 
            try {
                PackageManager._instance.BootstrappableProviderNames = GetProviders(requestObject.As<BootstrapRequest>()).Select(provider => provider.Attributes["name"]).ToArray();
            } catch {
                
            }
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

            using (var request = requestObject.As<BootstrapRequest>()) {
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

        private IEnumerable<DynamicElement> GetProviders(BootstrapRequest request) {
            var master = request.DownloadSwidtag(_urls);
            if (master == null) {
                request.Warning(Constants.Messages.ProviderSwidtagUnavailable);
                return Enumerable.Empty<DynamicElement>();
            }

            // they are looking for a provider 
            // return all providers 
            return request.GetProviders(master);
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
            using (var request = requestObject.As<BootstrapRequest>()) {
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

            using (var request = requestObject.As<BootstrapRequest>()) {
                request.Debug("Calling 'Bootstrap::GetInstalledPackages'");
                // return all the package providers as packages
            }
        }

        // --- operations on a package ---------------------------------------------------------------------------------------------------
        public void DownloadPackage(string fastPath, string location, IRequestObject requestObject) {
            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }
            using (var request = requestObject.As<BootstrapRequest>()) {
                request.Debug("Calling 'Bootstrap::DownloadPackage'");
            }
        }

        private void InstallProviderFromInstaller(DynamicElement provider, string fastPath, BootstrapRequest request) {
            string tmpFile = null;
            var failedBecauseInvalid = false;
            // download the file
            foreach (var link in provider.XPath("/swid:SoftwareIdentity/swid:Link[@rel = 'installationmedia']")) {
                var href = link.Attributes["href"];
                if (string.IsNullOrEmpty(href) || !Uri.IsWellFormedUriString(href, UriKind.Absolute)) {
                    request.Debug("Bad or missing uri: {0}", href);
                    continue;
                }

                var artifact = link.Attributes["artifact"];


                try {
                    tmpFile = artifact.GenerateTemporaryFilename();

                    request.Debug("Downloading '{0}' to '{1}'", href, tmpFile);

                    if (!request.DownloadFileToLocation(new Uri(href), tmpFile)) {
                        request.Debug("Failed download of '{0}'", href);
                        continue;
                    }

                    request.Debug("Verifying the package");

                    var valid = request.IsSignedAndTrusted(tmpFile, request);

                    if (!valid) {
                        request.Debug("Not Valid file '{0}' => '{1}'", href, tmpFile);
                        failedBecauseInvalid = true;
                        request.Warning(Constants.Messages.FileFailedVerification, href);
#if !DEBUG
                                tmpFile.TryHardToDelete();
                                continue;
#endif
                    }


                    // we have a valid file.
                    // run the installer
                    if (request.Install(tmpFile, "", request)) {
                        // it installed ok!
                        request.YieldFromSwidtag(provider, null, null, null, fastPath);
                        PackageManager._instance.LoadProviders(request);
                    } else {
                        request.Error(ErrorCategory.InvalidOperation, fastPath, Constants.Messages.FailedProviderBootstrap, fastPath);
                    }

                } catch (Exception e) {
                    e.Dump();
                }
                finally {
                    if (!string.IsNullOrEmpty(tmpFile)) {
                        tmpFile.TryHardToDelete();
                    }
                }
            }
            if (failedBecauseInvalid) {
                request.Error(ErrorCategory.InvalidData, fastPath, Constants.Messages.FileFailedVerification, fastPath);
            }
        }

        private void InstallModuleProvider(DynamicElement provider, string fastPath, BootstrapRequest request) {
            // This is a prototype for a general provider installer
            // really, this is going away as soon as we have a real swidtag provider.

            // 'package' may not be the best rel= type.
            foreach (var link in provider.XPath("/swid:SoftwareIdentity/swid:Link[@rel = 'package']")) {
                var href = link.Attributes["href"];

                // NOT THIS -> at this point href should either be url to a location (that a provider will recognize) 
                // JUST THIS -> or more likely should be a prototype canonical id: <provider>:<packagename>[/version][#source]
                // 
                if (string.IsNullOrEmpty(href) || !Uri.IsWellFormedUriString(href, UriKind.Absolute)) {
                    request.Debug("Bad or missing uri: {0}", href);
                    continue;
                }

                var artifact = link.Attributes["artifact"];

                try {
                    var uri = new Uri(href);
                    var providers = request.SelectProviders(uri.Scheme, request).ToArray();
                    if (providers.Length == 0) {
                        // no known provider by that name right now.
                        continue;
                    }

                    var packageId = uri.Host;
                    var packageVersion = uri.PathAndQuery;
                    var source = uri.Fragment;

                    if (string.IsNullOrEmpty(packageId)) {
                        continue;
                    }

                    var customRequest = request.Extend<Request>(new {
                        GetSources = new Func<IEnumerable<string>>(() => {
                            if (string.IsNullOrWhiteSpace(source)) {
                                return new string[0];
                            }
                            return new string[] {
                                source
                            };
                        })
                    });

                    var packages = providers[0].FindPackage(packageId, packageVersion, null, null, 0,customRequest).Wait(60);

                    var pkgs = packages.ToArray();
                    if (pkgs.Length < 1) {
                        if (string.IsNullOrWhiteSpace(packageVersion)) {
                            request.Warning("Unable to find package '{0}' to bootstrap", packageId);
                        } else {
                            request.Warning("Unable to find package '{0}/{1}' to bootstrap", packageId, packageVersion);
                        }
                        continue;
                    }
                    if (pkgs.Length > 1) {
                        if (string.IsNullOrWhiteSpace(packageVersion)) {
                            request.Warning("Package '{0}' matched more than one package", packageId);
                        }
                        else {
                            request.Warning("Package '{0}/{1}' matched more than one package", packageId, packageVersion);
                        }
                        continue;
                    }
                    var installedPackages = providers[0].InstallPackage(pkgs[0], customRequest).Wait(120).ToArray();
                    if (request.IsCanceled) {
                        return;
                    }

                    bool installed = false;

                    foreach (var pkg in installedPackages) {
                        installed = true;
                        request.YieldSoftwareIdentity(pkg.FastPackageReference, pkg.Name, pkg.Version, pkg.VersionScheme, pkg.Summary, pkg.Source, pkg.SearchKey, pkg.FullPath, pkg.PackageFilename);
                    }
                    if (request.IsCanceled) {
                        return;
                    }
                    
                    if (installed) {
                        // it installed ok!
                        PackageManager._instance.LoadProviders(request);
                    }
                    else {
                        request.Error(ErrorCategory.InvalidOperation, fastPath, Constants.Messages.FailedProviderBootstrap, fastPath);
                    }
                }
                catch (Exception e) {
                    e.Dump();
                }
            }
        }

        private void InstallAssemblyProvider(DynamicElement provider, string fastPath, BootstrapRequest request) {
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

                    var valid = request.IsSignedAndTrusted(tmpFile, request);

                    if (!valid) {
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
                        PackageManager._instance.LoadProviders(request);
                        return;
                    }
                }
                catch (Exception e) {
                    e.Dump();
                }
                finally {
                    if (!string.IsNullOrEmpty(tmpFile)) {
                        tmpFile.TryHardToDelete();
                    }
                }
            }
            if (failedBecauseInvalid) {
                request.Error(ErrorCategory.InvalidData, fastPath, Constants.Messages.FileFailedVerification, fastPath);
            }
        }

        public void InstallPackage(string fastPath, IRequestObject requestObject) {
            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }
            // ensure that mandatory parameters are present.
            using (var request = requestObject.As<BootstrapRequest>()) {
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

                    if (provider.XPath("/swid:SoftwareIdentity/swid:Meta[@providerType = 'assembly']").Any()) {
                        InstallAssemblyProvider(provider,fastPath, request);
                        return;
                    }

                    if (provider.XPath("/swid:SoftwareIdentity/swid:Meta[@providerType = 'psmodule']").Any()) {
                        InstallModuleProvider(provider, fastPath, request);
                        return;
                    }

                    
                    InstallProviderFromInstaller(provider,fastPath,request);

                } else {
                    request.Error(ErrorCategory.InvalidData, fastPath, Constants.Messages.UnableToResolvePackage, fastPath);
                }
            }
        }
    }
}