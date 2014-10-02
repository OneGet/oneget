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
    using System.Net;
    using System.Threading;
    using System.Xml;
    using System.Xml.Linq;
    using Implementation;
    using Packaging;
    using Utility.Extensions;
    using Utility.Platform;
    using Utility.Plugin;
    using Utility.Versions;
    using Utility.Xml;
    using RequestImpl = System.Object;

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
            {"schemes", new[] {"http", "https", "file"}},
            {"extensions", new[] {"exe", "msi"}},
            {"magic-signatures", Constants.Empty},
            {Constants.Features.AutomationOnly, Constants.Empty}
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

        public void InitializeProvider(RequestImpl requestImpl) {
        }

        public void GetFeatures(RequestImpl requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            using (var request = requestImpl.As<Request>()) {
                request.Debug("Calling 'Bootstrap::GetFeatures'");
                foreach (var feature in _features) {
                    request.Yield(feature);
                }
            }
        }

        public void GetDynamicOptions(string category, RequestImpl requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            using (var request = requestImpl.As<BoostrapRequest>()) {
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
        /// <param name="requestImpl"></param>
        /// <returns></returns>
        public void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, RequestImpl requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }
            using (var request = requestImpl.As<BoostrapRequest>()) {
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

        public void GetInstalledPackages(string name, RequestImpl requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            using (var request = requestImpl.As<BoostrapRequest>()) {
                request.Debug("Calling 'Bootstrap::GetInstalledPackages'");
                // return all the package providers as packages
            }
        }

        // --- operations on a package ---------------------------------------------------------------------------------------------------
        public void DownloadPackage(string fastPath, string location, RequestImpl requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }
            using (var request = requestImpl.As<BoostrapRequest>()) {
                request.Debug("Calling 'Bootstrap::DownloadPackage'");
            }
        }

        public void InstallPackage(string fastPath, RequestImpl requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }
            // ensure that mandatory parameters are present.
            using (var request = requestImpl.As<BoostrapRequest>()) {
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

    public abstract class BoostrapRequest : Request {
        private static XmlNamespaceManager _namespaceManager;

        internal static XmlNamespaceManager NamespaceManager {
            get {
                if (_namespaceManager == null) {
                    XmlNameTable nameTable = new NameTable();
                    _namespaceManager = new XmlNamespaceManager(nameTable);
                    _namespaceManager.AddNamespace("swid", "http://standards.iso.org/iso/19770/-2/2014/schema.xsd");
                    _namespaceManager.AddNamespace("oneget", "http://oneget.org/swidtag");
                }
                return _namespaceManager;
            }
        }

        internal string DestinationPath {
            get {
                return Path.GetFullPath(GetValue("DestinationPath"));
            }
        }

        private string GetValue(string name) {
            // get the value from the request
            return (GetOptionValues(name) ?? Enumerable.Empty<string>()).LastOrDefault();
        }

        internal DynamicElement DownloadSwidtag(IEnumerable<string> locations) {
            foreach (var location in locations) {
                if (Uri.IsWellFormedUriString(location, UriKind.Absolute)) {
                    var uri = new Uri(location);
                    var content = DownloadContent(uri);
                    XDocument document;
                    if (!String.IsNullOrEmpty(content)) {
                        try {
                            document = XDocument.Parse(content);
                            if (document.Root != null && document.Root.Name.LocalName == Constants.SwidTag.SoftwareIdentity) {
                                // future todo: we could do more checks here.

                                return new DynamicElement(document, NamespaceManager);
                            }
                        } catch {
                        }
                    }
                }
            }
            return null;
        }

        private string DownloadContent(Uri location) {
            string result = null;
            try {
                var client = new WebClient();

                // Apparently, places like Codeplex know to let this thru!
                client.Headers.Add("user-agent", "chocolatey command line");

                var done = new ManualResetEvent(false);

                client.DownloadStringCompleted += (sender, args) => {
                    if (!args.Cancelled && args.Error == null) {
                        result = args.Result;
                    }

                    done.Set();
                };
                client.DownloadProgressChanged += (sender, args) => {
                    // todo: insert progress indicator
                    // var percent = (args.BytesReceived * 100) / args.TotalBytesToReceive;
                    // Progress(c, 2, (int)percent, "Downloading {0} of {1} bytes", args.BytesReceived, args.TotalBytesToReceive);
                };
                client.DownloadStringAsync(location);
                done.WaitOne();
            } catch (Exception e) {
                e.Dump();
            }
            return result;
        }

        internal DynamicElement GetProvider(DynamicElement document, string name) {
            var links = document.XPath("/swid:SoftwareIdentity/swid:Link[@rel='component' and @artifact='{0}' and @oneget:type='provider']", name.ToLowerInvariant());
            return DownloadSwidtag(links.GetAttributes("href"));
        }

        internal IEnumerable<DynamicElement> GetProviders(DynamicElement document) {
            var artifacts = document.XPath("/swid:SoftwareIdentity/swid:Link[@rel='component' and @oneget:type='provider']").GetAttributes("artifact").Distinct().ToArray();
            return artifacts.Select(each => GetProvider(document, each)).Where(each => each != null);
        }

        public bool YieldFromSwidtag(DynamicElement provider, string requiredVersion, string minimumVersion, string maximumVersion, string searchKey) {
            if (provider == null) {
                throw new ArgumentNullException("provider");
            }

            var name = provider.Attributes["name"];
            FourPartVersion version = provider.Attributes["version"];
            var versionScheme = provider.Attributes["versionScheme"];
            var packageFilename = provider.XPath("/swid:SoftwareIdentity/swid:Meta[@targetFilename]").GetAttribute("targetFilename");
            var summary = provider.XPath("/swid:SoftwareIdentity/swid:Meta[@summary]").GetAttribute("summary");

            if (AnyNullOrEmpty(name, version, versionScheme, packageFilename)) {
                Debug("Skipping yield on swid due to missing field \r\n", provider.ToString());
                return true;
            }

            if (!string.IsNullOrEmpty(requiredVersion) && version != requiredVersion) {
                return true;
            }

            if (!string.IsNullOrEmpty(minimumVersion) && version < minimumVersion) {
                return true;
            }

            if (!string.IsNullOrEmpty(maximumVersion) && version > maximumVersion) {
                return true;
            }

            if (YieldSoftwareIdentity(name, name, version, versionScheme, summary, null, searchKey, null, packageFilename)) {
                // note: temporary until we actaully support swidtags in the core.

                // yield all the meta/attributes
                if (provider.XPath("/swid:SoftwareIdentity/swid:Meta").Any(
                    meta => meta.Attributes.Any(attribute => !YieldSoftwareMetadata(name, attribute.Name.LocalName, attribute.Value)))) {
                    return false;
                }

                if (provider.XPath("/swid:SoftwareIdentity/swid:Link").Any(
                    link => !YieldLink(name, link.Attributes["href"], link.Attributes["rel"], link.Attributes["type"], link.Attributes["ownership"], link.Attributes["use"], link.Attributes["media"], link.Attributes["artifact"]))) {
                    return false;
                }

                if (provider.XPath("/swid:SoftwareIdentity/swid:Entity").Any(
                    entity => !YieldEntity(name, entity.Attributes["name"], entity.Attributes["regid"], entity.Attributes["role"], entity.Attributes["thumbprint"]))) {
                    return false;
                }

                if (!YieldSoftwareMetadata(name, "FromTrustedSource", true.ToString())) {
                    return false;
                }
            }

            return true;
        }

        private static bool AnyNullOrEmpty(params string[] args) {
            return args.Any(string.IsNullOrEmpty);
        }

        public bool DownloadFileToLocation(Uri uri, string targetFile) {
            var result = false;
            var client = new WebClient();

            // Apparently, places like Codeplex know to let this thru!
            client.Headers.Add("user-agent", "chocolatey command line");

            var done = new ManualResetEvent(false);

            client.DownloadFileCompleted += (sender, args) => {
                if (args.Cancelled || args.Error != null) {
                    // failed
                    targetFile.TryHardToDelete();
                } else {
                    result = true;
                }

                done.Set();
            };
            client.DownloadProgressChanged += (sender, args) => {
                // todo: insert progress indicator
                // var percent = (args.BytesReceived * 100) / args.TotalBytesToReceive;
                // Progress(c, 2, (int)percent, "Downloading {0} of {1} bytes", args.BytesReceived, args.TotalBytesToReceive);
            };
            client.DownloadFileAsync(uri, targetFile);
            done.WaitOne();
            return result;
        }
    }
}