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
    using System.Linq;
    using Api;
    using Implementation;
    using Utility.Async;
    using Utility.Extensions;
    using Utility.Plugin;
    using Utility.Xml;
    using System.Diagnostics.CodeAnalysis;


    public class BootstrapProvider {

        private PackageManagementService PackageManagementService {
            get {
                return PackageManager.Instance as PackageManagementService;
            }
        }

        private static readonly string[] _urls = {
#if LOCAL_DEBUG
            "http://localhost:81/providers.swidtag",
#endif
            // starting in 2015/04 builds, we bootstrap from here:
            "https://oneget.org/providers.1504.swidtag"
        };


        private static readonly Dictionary<string, string[]> _features = new Dictionary<string, string[]> {
            {Constants.Features.SupportedSchemes, new[] {"http", "https", "file"}},
            {Constants.Features.SupportedExtensions, new[] {"exe", "msi"}},
            {Constants.Features.MagicSignatures, Constants.Empty},
            {Constants.Features.AutomationOnly, Constants.Empty}
        };

        public static IEnumerable<string> SupportedSchemes {
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

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Plugin requirement.")]
        public void InitializeProvider(BootstrapRequest request) {
            // we should go find out what's available once here just to make sure that
            // we have a list
            try {
                PackageManagementService.BootstrappableProviderNames = GetProviders(request).Select(provider => provider.Attributes["name"]).ToArray();
            } catch {

            }
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Plugin requirement.")]
        public void GetFeatures(BootstrapRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            request.Debug("Calling 'Bootstrap::GetFeatures'");
            foreach (var feature in _features) {
                request.Yield(feature);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Plugin requirement.")]
        public void GetDynamicOptions(string category, BootstrapRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            request.Debug("Calling 'Bootstrap::GetDynamicOptions ({0})'", category);

            switch ((category ?? string.Empty).ToLowerInvariant()) {
                case "package":
                    // request.YieldDynamicOption( "ForceCheck", OptionType.Switch, false);

                    break;

                case "source":
                    break;

                case "install":
                    request.YieldDynamicOption("DestinationPath", "Folder", true);
                    break;
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
        /// <param name="request"></param>
        /// <returns></returns>
        public void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, BootstrapRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            request.Debug("Calling 'Bootstrap::FindPackage'");

            var master = request.DownloadSwidtag(_urls);
            if (master == null) {
                request.Warning(Constants.Messages.ProviderSwidtagUnavailable);
                return;
            }

            if (name != null && name.EqualsIgnoreCase("PackageManagement")) {
                // they are looking for PackageManagement itself.
                // future todo: let PackageManagement update itself.
                return;
            }

            // they are looking for a provider
            if (string.IsNullOrWhiteSpace(name)) {
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

        /// <summary>
        /// Returns the packages that are installed
        /// </summary>
        /// <param name="name">the package name to match. Empty or null means match everything</param>
        /// <param name="requiredVersion">the specific version asked for. If this parameter is specified (ie, not null or empty string) then the minimum and maximum values are ignored</param>
        /// <param name="minimumVersion">the minimum version of packages to return . If the <code>requiredVersion</code> parameter is specified (ie, not null or empty string) this should be ignored</param>
        /// <param name="maximumVersion">the maximum version of packages to return . If the <code>requiredVersion</code> parameter is specified (ie, not null or empty string) this should be ignored</param>
        /// <param name="request">
        ///     An object passed in from the CORE that contains functions that can be used to interact with
        ///     the CORE and HOST
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Plugin requirement.")]
        public void GetInstalledPackages(string name, string requiredVersion, string minimumVersion, string maximumVersion, BootstrapRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            request.Debug("Calling '{0}::GetInstalledPackages' '{1}','{2}','{3}','{4}'", GetPackageProviderName(), name, requiredVersion, minimumVersion, maximumVersion);
            // return all the dynamic package providers as packages
        }

        // --- operations on a package ---------------------------------------------------------------------------------------------------
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Plugin requirement.")]
        public void DownloadPackage(string fastPath, string location, BootstrapRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }
            request.Debug("Calling 'Bootstrap::DownloadPackage'");
        }

        private void InstallProviderFromInstaller(DynamicElement provider, string fastPath, BootstrapRequest request) {
            string tmpFile = null;
            var failedBecauseInvalid = false;
            // download the file
            foreach (var link in provider.XPath("/swid:SoftwareIdentity/swid:Link[@rel = 'installationmedia']")) {
                var href = link.Attributes["href"];
                if (string.IsNullOrWhiteSpace(href) || !Uri.IsWellFormedUriString(href, UriKind.Absolute)) {
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

                    var valid = request.ProviderServices.IsSignedAndTrusted(tmpFile, request);

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
                    if (request.ProviderServices.Install(tmpFile, "", request)) {
                        // it installed ok!
                        request.YieldFromSwidtag(provider, null, null, null, fastPath);
                        PackageManagementService.LoadProviders(request.As<IRequest>());
                    } else {
                        request.Error(ErrorCategory.InvalidOperation, fastPath, Constants.Messages.FailedProviderBootstrap, fastPath);
                    }

                } catch (Exception e) {
                    e.Dump();
                }
                finally {
                    if (!string.IsNullOrWhiteSpace(tmpFile)) {
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
                if (string.IsNullOrWhiteSpace(href) || !Uri.IsWellFormedUriString(href, UriKind.Absolute)) {
                    request.Debug("Bad or missing uri: {0}", href);
                    continue;
                }

                var artifact = link.Attributes["artifact"];

                try {
                    var uri = new Uri(href);
                    // @futuregarrett this may die?
                    // todo make sure this works.
                    var providers = request.PackageManagementService.SelectProviders(uri.Scheme, request).ToArray();
                    if (providers.Length == 0) {
                        // no known provider by that name right now.
                        continue;
                    }

                    var packageId = uri.Host;
                    var packageVersion = uri.PathAndQuery;
                    var source = uri.Fragment;

                    if (string.IsNullOrWhiteSpace(packageId)) {
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

                    var packages = providers[0].FindPackage(packageId, packageVersion, null, null,customRequest).Wait(60);

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
                        PackageManagementService.LoadProviders(request);
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
            if (string.IsNullOrWhiteSpace(targetFilename)) {
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
                if (string.IsNullOrWhiteSpace(href) || !Uri.IsWellFormedUriString(href, UriKind.Absolute)) {
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

                    var valid = request.ProviderServices.IsSignedAndTrusted(tmpFile, request);

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
                        PackageManagementService.LoadProviders(request);
                        return;
                    }
                }
                catch (Exception e) {
                    e.Dump();
                }
                finally {
                    if (!string.IsNullOrWhiteSpace(tmpFile)) {
                        tmpFile.TryHardToDelete();
                    }
                }
            }
            if (failedBecauseInvalid) {
                request.Error(ErrorCategory.InvalidData, fastPath, Constants.Messages.FileFailedVerification, fastPath);
            }
        }

        public void InstallPackage(string fastPath, BootstrapRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }
            // ensure that mandatory parameters are present.
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
                    InstallAssemblyProvider(provider, fastPath, request);
                    return;
                }

                if (provider.XPath("/swid:SoftwareIdentity/swid:Meta[@providerType = 'psmodule']").Any()) {
                    InstallModuleProvider(provider, fastPath, request);
                    return;
                }


                InstallProviderFromInstaller(provider, fastPath, request);

            } else {
                request.Error(ErrorCategory.InvalidData, fastPath, Constants.Messages.UnableToResolvePackage, fastPath);
            }

        }
    }
}
