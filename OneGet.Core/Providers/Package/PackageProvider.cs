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

namespace Microsoft.OneGet.Providers.Package {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Api;
    using Packaging;
    using Utility.Collections;
    using Utility.Extensions;
    using Utility.Plugin;
    using RequestImpl = System.Object;

    #region generate-delegates response-apis

    public delegate bool OkToContinue();

    public delegate bool YieldSoftwareIdentity(string fastPath, string name, string version, string versionScheme, string summary, string source, string searchKey, string fullPath, string packageFileName);

    public delegate bool YieldSoftwareMetadata(string parentFastPath, string name, string value);

    public delegate bool YieldEntity(string parentFastPath, string name, string regid, string role, string thumbprint);

    public delegate bool YieldLink(string parentFastPath, string referenceUri, string relationship, string mediaType, string ownership, string use, string appliesToMedia, string artifact);

    public delegate bool YieldSwidtag(string fastPath, string xmlOrJsonDoc);

    public delegate bool YieldMetadata(string fieldId, string @namespace, string name, string value);

    public delegate bool YieldPackageSource(string name, string location, bool isTrusted,bool isRegistered, bool isValidated);

    public delegate bool YieldDynamicOption(int category, string name, int expectedType, bool isRequired);

    public delegate bool YieldKeyValuePair(string key, string value);

    public delegate bool YieldValue(string value);

    #endregion

    public delegate bool IsCancelled();

    public delegate string GetMessageString(string messageText);

    public class PackageProvider : ProviderBase<IPackageProvider> {
        private string _name;

        internal PackageProvider(IPackageProvider provider) : base(provider) {
        }

        public string Name {
            get {
                return ProviderName;
            }
        }

        public override string ProviderName {
            get {
                return _name ?? (_name = Provider.GetPackageProviderName());
            }
        }

        // Friendly APIs

        public ICancellableEnumerable<PackageSource> AddPackageSource(string name, string location, bool trusted, RequestImpl requestImpl) {
            return new Response<PackageSource>(requestImpl, this, response => Provider.AddPackageSource(name, location, trusted, response)).CompleteResult;
        }

        public ICancellableEnumerable<PackageSource> RemovePackageSource(string name, RequestImpl requestImpl) {
            return new Response<PackageSource>(requestImpl, this, response => Provider.RemovePackageSource(name, response)).CompleteResult;
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackageByUri(Uri uri, int id, RequestImpl requestImpl) {
            return new Response<SoftwareIdentity>(requestImpl, this, "Available", response => Provider.FindPackageByUri(uri, id, response)).Result;
        }

        public ICancellableEnumerable<SoftwareIdentity> GetPackageDependencies(SoftwareIdentity package, RequestImpl requestImpl) {
            return new Response<SoftwareIdentity>(requestImpl, this, "Dependency", response => Provider.GetPackageDependencies(package.FastPackageReference, response)).Result;
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackageByFile(string filename, int id, RequestImpl requestImpl) {
            return new Response<SoftwareIdentity>(requestImpl, this, "Available", response => Provider.FindPackageByFile(filename, id, response)).Result;
        }

        public int StartFind(RequestImpl requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }
            return Provider.StartFind(requestImpl.As<IRequest>());
        }

        public ICancellableEnumerable<SoftwareIdentity> CompleteFind(int i, RequestImpl requestImpl) {
            return new Response<SoftwareIdentity>(requestImpl, this, "Available", response => Provider.CompleteFind(i, response)).Result;
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackages(string[] names, string requiredVersion, string minimumVersion, string maximumVersion, RequestImpl requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            if (names == null) {
                throw new ArgumentNullException("names");
            }

            requestImpl = ExtendRequest(requestImpl);
            var cts = new CancellationTokenSource();
            return new CancellableEnumerable<SoftwareIdentity>(cts, FindPackagesImpl(cts, names, requiredVersion, minimumVersion, maximumVersion, requestImpl));
        }

        private IEnumerable<SoftwareIdentity> FindPackagesImpl(CancellationTokenSource cancellationTokenSource, string[] names, string requiredVersion, string minimumVersion, string maximumVersion, RequestImpl requestImpl) {
            var id = StartFind(requestImpl);
            foreach (var name in names) {
                foreach (var pkg in FindPackage(name, requiredVersion, minimumVersion, maximumVersion, id, requestImpl).TakeWhile(pkg => !cancellationTokenSource.IsCancellationRequested)) {
                    yield return pkg;
                }
                foreach (var pkg in CompleteFind(id, requestImpl).TakeWhile(pkg => !cancellationTokenSource.IsCancellationRequested)) {
                    yield return pkg;
                }
            }
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackagesByUris(Uri[] uris, RequestImpl requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            if (uris == null) {
                throw new ArgumentNullException("uris");
            }

            requestImpl = ExtendRequest(requestImpl);
            var cts = new CancellationTokenSource();
            return new CancellableEnumerable<SoftwareIdentity>(cts, FindPackagesByUrisImpl(cts, uris, requestImpl));
        }

        private IEnumerable<SoftwareIdentity> FindPackagesByUrisImpl(CancellationTokenSource cancellationTokenSource, Uri[] uris, RequestImpl requestImpl) {
            var id = StartFind(requestImpl);
            foreach (var uri in uris) {
                foreach (var pkg in FindPackageByUri(uri, id, requestImpl).TakeWhile(pkg => !cancellationTokenSource.IsCancellationRequested)) {
                    yield return pkg;
                }
                foreach (var pkg in CompleteFind(id, requestImpl).TakeWhile(pkg => !cancellationTokenSource.IsCancellationRequested)) {
                    yield return pkg;
                }
            }
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackagesByFiles(string[] filenames, RequestImpl requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            if (filenames == null) {
                throw new ArgumentNullException("filenames");
            }

            requestImpl = ExtendRequest(requestImpl);
            var cts = new CancellationTokenSource();
            return new CancellableEnumerable<SoftwareIdentity>(cts, FindPackagesByFilesImpl(cts, filenames, requestImpl));
        }

        private IEnumerable<SoftwareIdentity> FindPackagesByFilesImpl(CancellationTokenSource cancellationTokenSource, string[] filenames, RequestImpl requestImpl) {
            var id = StartFind(requestImpl);
            foreach (var file in filenames) {
                foreach (var pkg in FindPackageByFile(file, id, requestImpl).TakeWhile(pkg => !cancellationTokenSource.IsCancellationRequested)) {
                    yield return pkg;
                }
                foreach (var pkg in CompleteFind(id, requestImpl).TakeWhile(pkg => !cancellationTokenSource.IsCancellationRequested)) {
                    yield return pkg;
                }
            }
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, RequestImpl requestImpl) {
            return new Response<SoftwareIdentity>(requestImpl, this, "Available", response => Provider.FindPackage(name, requiredVersion, minimumVersion, maximumVersion, id, response)).Result;
        }

        public ICancellableEnumerable<SoftwareIdentity> GetInstalledPackages(string name, RequestImpl requestImpl) {
            return new Response<SoftwareIdentity>(requestImpl, this, "Installed", response => Provider.GetInstalledPackages(name, response)).Result;
        }

        public ICancellableEnumerable<SoftwareIdentity> InstallPackage(SoftwareIdentity softwareIdentity, RequestImpl requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            if (softwareIdentity == null) {
                throw new ArgumentNullException("softwareIdentity");
            }

            var request = ExtendRequest(requestImpl);
            ;

            // if the provider didn't say this was trusted, we should ask the user if it's ok.
            if (!softwareIdentity.FromTrustedSource) {
                try {
                    if (!request.ShouldContinueWithUntrustedPackageSource(softwareIdentity.Name, softwareIdentity.Source)) {
                        request.Warning( request.FormatMessageString(Constants.UserDeclinedInstallOfUntrustedPackage,softwareIdentity.Name));
                        return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), Enumerable.Empty<SoftwareIdentity>());
                    }
                } catch {
                    return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), Enumerable.Empty<SoftwareIdentity>());
                }
            }

            return new Response<SoftwareIdentity>(requestImpl, this,Constants.Installed, response => Provider.InstallPackage(softwareIdentity.FastPackageReference, response)).Result;
        }

        public ICancellableEnumerable<SoftwareIdentity> UninstallPackage(SoftwareIdentity softwareIdentity, RequestImpl requestImpl) {
            return new Response<SoftwareIdentity>(requestImpl, this, Constants.Uninstalled, response => Provider.UninstallPackage(softwareIdentity.FastPackageReference, response)).Result;
        }

        public ICancellableEnumerable<PackageSource> ResolvePackageSources(RequestImpl requestImpl) {
            return new Response<PackageSource>(requestImpl, this, response => Provider.ResolvePackageSources(response)).Result;
        }

        public void DownloadPackage(SoftwareIdentity softwareIdentity, string destinationFilename, RequestImpl requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            if (softwareIdentity== null) {
                throw new ArgumentNullException("softwareIdentity");
            }

            Provider.DownloadPackage(softwareIdentity.FastPackageReference, destinationFilename, ExtendRequest(requestImpl));
        }
    }

    #region declare PackageProvider-types
    /* Synced/Generated code =================================================== */

    public enum OptionCategory {
        Package = 0,
        Provider = 1,
        Source = 2,
        Install = 3
    }

    public enum OptionType {
        String = 0,
        StringArray = 1,
        Int = 2,
        Switch = 3,
        Folder = 4,
        File = 5,
        Path = 6,
        Uri = 7,
        SecureString = 8
    }

    public enum EnvironmentContext {
        All = 0,
        User = 1,
        System = 2
    }

    #endregion
}