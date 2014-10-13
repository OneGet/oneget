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
    using Utility.Extensions;
    using Utility.Versions;
    using Utility.Xml;

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