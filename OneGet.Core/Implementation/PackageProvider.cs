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

namespace Microsoft.OneGet.Implementation {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Api;
    using Packaging;
    using Providers;
    using Utility.Collections;
    using Utility.Plugin;

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

        public ICancellableEnumerable<PackageSource> AddPackageSource(string name, string location, bool trusted, Object requestImpl) {
            return new Response<PackageSource>(requestImpl, this, response => Provider.AddPackageSource(name, location, trusted, response)).CompleteResult;
        }

        public ICancellableEnumerable<PackageSource> RemovePackageSource(string name, Object requestImpl) {
            return new Response<PackageSource>(requestImpl, this, response => Provider.RemovePackageSource(name, response)).CompleteResult;
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackageByUri(Uri uri, int id, Object requestImpl) {
            return new Response<SoftwareIdentity>(requestImpl, this, "Available", response => Provider.FindPackageByUri(uri, id, response)).Result;
        }

        public ICancellableEnumerable<SoftwareIdentity> GetPackageDependencies(SoftwareIdentity package, Object requestImpl) {
            return new Response<SoftwareIdentity>(requestImpl, this, "Dependency", response => Provider.GetPackageDependencies(package.FastPackageReference, response)).Result;
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackageByFile(string filename, int id, Object requestImpl) {
            return new Response<SoftwareIdentity>(requestImpl, this, "Available", response => Provider.FindPackageByFile(filename, id, response)).Result;
        }

        public int StartFind(Object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }
            return Provider.StartFind(requestImpl.As<IRequest>());
        }

        public ICancellableEnumerable<SoftwareIdentity> CompleteFind(int i, Object requestImpl) {
            return new Response<SoftwareIdentity>(requestImpl, this, "Available", response => Provider.CompleteFind(i, response)).Result;
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackages(string[] names, string requiredVersion, string minimumVersion, string maximumVersion, Object requestImpl) {
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

        private IEnumerable<SoftwareIdentity> FindPackagesImpl(CancellationTokenSource cancellationTokenSource, string[] names, string requiredVersion, string minimumVersion, string maximumVersion, Object requestImpl) {
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

        public ICancellableEnumerable<SoftwareIdentity> FindPackagesByUris(Uri[] uris, Object requestImpl) {
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

        private IEnumerable<SoftwareIdentity> FindPackagesByUrisImpl(CancellationTokenSource cancellationTokenSource, Uri[] uris, Object requestImpl) {
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

        public ICancellableEnumerable<SoftwareIdentity> FindPackagesByFiles(string[] filenames, Object requestImpl) {
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

        private IEnumerable<SoftwareIdentity> FindPackagesByFilesImpl(CancellationTokenSource cancellationTokenSource, string[] filenames, Object requestImpl) {
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

        public ICancellableEnumerable<SoftwareIdentity> FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Object requestImpl) {
            return new Response<SoftwareIdentity>(requestImpl, this, "Available", response => Provider.FindPackage(name, requiredVersion, minimumVersion, maximumVersion, id, response)).Result;
        }

        public ICancellableEnumerable<SoftwareIdentity> GetInstalledPackages(string name, Object requestImpl) {
            return new Response<SoftwareIdentity>(requestImpl, this, "Installed", response => Provider.GetInstalledPackages(name, response)).Result;
        }

        public ICancellableEnumerable<SoftwareIdentity> InstallPackage(SoftwareIdentity softwareIdentity, Object requestImpl) {
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
                        request.Warning(request.FormatMessageString(Constants.UserDeclinedUntrustedPackageInstall, softwareIdentity.Name));
                        return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), Enumerable.Empty<SoftwareIdentity>());
                    }
                } catch {
                    return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), Enumerable.Empty<SoftwareIdentity>());
                }
            }

            return new Response<SoftwareIdentity>(requestImpl, this, Constants.Installed, response => Provider.InstallPackage(softwareIdentity.FastPackageReference, response)).Result;
        }

        public ICancellableEnumerable<SoftwareIdentity> UninstallPackage(SoftwareIdentity softwareIdentity, Object requestImpl) {
            return new Response<SoftwareIdentity>(requestImpl, this, Constants.Uninstalled, response => Provider.UninstallPackage(softwareIdentity.FastPackageReference, response)).Result;
        }

        public ICancellableEnumerable<PackageSource> ResolvePackageSources(Object requestImpl) {
            return new Response<PackageSource>(requestImpl, this, response => Provider.ResolvePackageSources(response)).Result;
        }

        public void DownloadPackage(SoftwareIdentity softwareIdentity, string destinationFilename, Object requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            if (softwareIdentity == null) {
                throw new ArgumentNullException("softwareIdentity");
            }

            Provider.DownloadPackage(softwareIdentity.FastPackageReference, destinationFilename, ExtendRequest(requestImpl));
        }
    }

    #region declare PackageProvider-types

    /* Synced/Generated code =================================================== */

    #endregion
}