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

namespace Microsoft.PackageManagement.Providers.Bootstrap {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Implementation;
    using Packaging;
    using Utility.Collections;
    using Utility.Extensions;
    using Utility.Platform;

    public abstract class BootstrapRequest : Request {
        private static readonly Uri[] _urls = {
#if LOCAL_DEBUG
            new Uri("http://localhost:81/providers.swidtag"),
#endif
            // starting in 2015/05 builds, we bootstrap from here:
            new Uri("http://go.microsoft.com/fwlink/?LinkID=535044&clcid=0x409"),
            new Uri("http://go.microsoft.com/fwlink/?LinkID=535045&clcid=0x409")
        };

        private IEnumerable<Feed> _feeds;

        private IEnumerable<Feed> Feeds {
            get {
                if (_feeds == null) {
                    if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) {
                        Warning(Constants.Messages.NetworkNotAvailable);
                    }
                    // right now, we only have one feed (can have many urls tho')
                    // so we just return a single feed in the collection
                    // but later, we can expand it to support multiple feeds.
                    var feed = new Feed(this, _urls);
                    if (feed.IsValid) {
                        _feeds = feed.SingleItemAsEnumerable().ReEnumerable();
                    } else {
                        Warning(Constants.Messages.ProviderSwidtagUnavailable);
                        return Enumerable.Empty<Feed>();
                    }
                }
                return _feeds;
            }
        }

        internal string DestinationPath {
            get {
                var pms = PackageManagementService as PackageManagementService;

                var v = GetValue("DestinationPath");
                if (String.IsNullOrWhiteSpace(v)) {
                    // use a well-known path.
                    v = AdminPrivilege.IsElevated ? pms.SystemAssemblyLocation : pms.UserAssemblyLocation;
                    if (String.IsNullOrWhiteSpace(v)) {
                        return null;
                    }
                }
                return Path.GetFullPath(v);
            }
        }

        internal IEnumerable<Package> Providers {
            get {
                return Feeds.SelectMany(feed => feed.Query());
            }
        }

        private string GetValue(string name) {
            // get the value from the request
            return (GetOptionValues(name) ?? Enumerable.Empty<string>()).LastOrDefault();
        }

        internal Package GetProvider(Uri uri) {
            return new Package(this, uri.SingleItemAsEnumerable());
        }

        internal Package GetProvider(string name) {
            return Feeds.SelectMany(feed => feed.Query(name)).FirstOrDefault();
        }

        internal Package GetProvider(string name, string version) {
            return Feeds.SelectMany(feed => feed.Query(name, version)).FirstOrDefault();
        }

        internal IEnumerable<Package> GetProvider(string name, string minimumversion, string maximumversion) {
            return Feeds.SelectMany(feed => feed.Query(name, minimumversion, maximumversion));
        }

        internal string DownloadAndValidateFile(string name, IEnumerable<Link> links) {
            var failedBecauseInvalid = false;
            string file = null;
            foreach (var link in links) {
                var tmpFile = link.Artifact.GenerateTemporaryFilename();

                file = ProviderServices.DownloadFile(link.HRef, tmpFile, -1, true, this);
                if (file == null || !file.FileExists()) {
                    Debug("Failed download of '{0}'", link.HRef);
                    file = null;
                    continue;
                }

                Debug("Verifying the package");

                var valid = ProviderServices.IsSignedAndTrusted(file, this);
                if (!valid) {
                    Debug("Not Valid file '{0}' => '{1}'", link.HRef, tmpFile);
                    Warning(Constants.Messages.FileFailedVerification, link.HRef);
                    failedBecauseInvalid = true;
#if !DEBUG
                    tmpFile.TryHardToDelete();
                    continue;
#endif
                }
            }
            if (failedBecauseInvalid) {
                Error(ErrorCategory.InvalidData, name, Constants.Messages.FileFailedVerification, name);
                return null;
            }
            return file;
        }

        internal bool YieldFromSwidtag(Package provider, string requiredVersion, string minimumVersion, string maximumVersion, string searchKey) {
            if (provider == null) {
                // if the provider isn't there, just return.
                return !IsCanceled;
            }

            if (AnyNullOrEmpty(provider.Name, provider.Version, provider.VersionScheme)) {
                Debug("Skipping yield on swid due to missing field \r\n", provider.ToString());
                return !IsCanceled;
            }

            if (!String.IsNullOrWhiteSpace(requiredVersion)) {
                if (provider.Version != requiredVersion) {
                    return !IsCanceled;
                }
            } else {
                if (!String.IsNullOrWhiteSpace(minimumVersion) && SoftwareIdentityVersionComparer.CompareVersions(provider.VersionScheme, provider.Version, minimumVersion) < 0) {
                    return !IsCanceled;
                }

                if (!String.IsNullOrWhiteSpace(maximumVersion) && SoftwareIdentityVersionComparer.CompareVersions(provider.VersionScheme, provider.Version, maximumVersion) > 0) {
                    return !IsCanceled;
                }
            }
            return YieldFromSwidtag(provider, searchKey);
        }

        internal bool YieldFromSwidtag(Package pkg, string searchKey) {
            if (pkg == null) {
                return !IsCanceled;
            }

            var provider = pkg._swidtag;
            var targetFilename = provider.Links.Select(each => each.Attributes[Iso19770_2.Discovery.TargetFilename]).WhereNotNull().FirstOrDefault();
            var summary = new MetadataIndexer(provider)[Iso19770_2.Attributes.Summary.LocalName].FirstOrDefault();

            var fastPackageReference = pkg.Location.AbsoluteUri;

            if (YieldSoftwareIdentity(fastPackageReference, provider.Name, provider.Version, provider.VersionScheme, summary, null, searchKey, null, targetFilename) != null) {
                // yield all the meta/attributes
                if (provider.Meta.Any(
                    m => {
                        var element = AddMeta(fastPackageReference);
                        var attributes = m.Attributes;
                        return attributes.Keys.Any(key => {
                            var nspace = key.Namespace.ToString();
                            if (String.IsNullOrWhiteSpace(nspace)) {
                                return AddMetadata(element, key.LocalName, attributes[key]) == null;
                            }

                            return AddMetadata(element, new Uri(nspace), key.LocalName, attributes[key]) == null;
                        });
                    })) {
                    return !IsCanceled;
                }

                if (provider.Links.Any(link => AddLink(link.HRef, link.Relationship, link.MediaType, link.Ownership, link.Use, link.Media, link.Artifact) == null)) {
                    return !IsCanceled;
                }

                if (provider.Entities.Any(entity => AddEntity(entity.Name, entity.RegId, entity.Role, entity.Thumbprint) == null)) {
                    return !IsCanceled;
                }

                if (AddMetadata(fastPackageReference, "FromTrustedSource", true.ToString()) == null) {
                    return !IsCanceled;
                }
            }
            return !IsCanceled;
        }

        private static bool AnyNullOrEmpty(params string[] args) {
            return args.Any(String.IsNullOrWhiteSpace);
        }
    }
}