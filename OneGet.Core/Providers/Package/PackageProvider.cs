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
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Api;
    using Collections;
    using Extensions;
    using Packaging;
    using Plugin;

    #region generate-delegates response-apis

    public delegate bool OkToContinue();

    public delegate bool YieldPackage(string fastPath, string name, string version, string versionScheme, string summary, string source, string searchKey, string fullPath, string packageFileName);

    public delegate bool YieldPackageDetails(object serializablePackageDetailsObject);

    public delegate bool YieldPackageSwidtag(string fastPath, string xmlOrJsonDoc);

    public delegate bool YieldPackageSource(string name, string location, bool isTrusted,bool isRegistered);

    public delegate bool YieldDynamicOption(int category, string name, int expectedType, bool isRequired);

    public delegate bool YieldKeyValuePair(string key, string value);

    public delegate bool YieldValue(string value);

    #endregion

    public delegate bool IsCancelled();

    public class PackageProvider : ProviderBase<IPackageProvider> {
        private string _name;

        internal PackageProvider(IPackageProvider provider) : base(provider) {
        }

        public override string Name {
            get {
                return _name ?? (_name = Provider.GetPackageProviderName());
            }
        }

        // Friendly APIs

        public void AddPackageSource(string name, string location, bool trusted, Object c) {
            Provider.AddPackageSource(name, location, trusted, DynamicInterface.Instance.Create<IRequest>(c, Context));
        }

        public void RemovePackageSource(string name, Object c) {
            Provider.RemovePackageSource(name, DynamicInterface.Instance.Create<IRequest>(c, Context));
        }

        public ICancellableEnumerator<SoftwareIdentity> FindPackageByUri(Uri uri, int id, Object c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(collection =>
                Provider.FindPackageByUri(uri, id, c.Extend<IRequest>(Context, new {
                    YieldPackage = new YieldPackage((fastpath, name, version, scheme, summary, source, key, path, filename ) => {
                        collection.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = name,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Available",
                            SearchKey = key,
                            FullPath = path,
                            PackageFilename = filename 
                        });
                        return !(isCancelled() || collection.IsCancelled);
                    })
                }))).GetCancellableEnumerator();
        }

        public ICancellableEnumerator<SoftwareIdentity> GetPackageDependencies(SoftwareIdentity package, Object c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(collection =>
                Provider.GetPackageDependencies(package.FastPackageReference, c.Extend<IRequest>(Context, new {
                    YieldPackage = new YieldPackage((fastpath, name, version, scheme, summary, source, key, path, filename) => {
                        collection.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = name,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Dependency",
                            SearchKey = key,
                            FullPath = path,
                            PackageFilename = filename
                        });
                        return !(isCancelled() || collection.IsCancelled);
                    })
                }))).GetCancellableEnumerator();
        }

        public ICancellableEnumerator<SoftwareIdentity> FindPackageByFile(string filename, int id, Object c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(result =>
                Provider.FindPackageByFile(filename, id, c.Extend<IRequest>(Context, new {
                    YieldPackage = new YieldPackage((fastpath, name, version, scheme, summary, source, key, path, pfilename ) => {
                        result.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = name,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Available",
                            SearchKey = key,
                            FullPath = path, 
                            PackageFilename = pfilename,
                        });
                        return !(isCancelled() || result.IsCancelled);
                    })
                }))).GetCancellableEnumerator();
        }

        public int StartFind(Object c) {
            return Provider.StartFind(c.As<IRequest>());
        }

        public ICancellableEnumerator<SoftwareIdentity> CompleteFind(int i, Object c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(result =>
                Provider.CompleteFind(i, c.Extend<IRequest>(Context, new {
                    // add YieldPackage method to the callback
                    YieldPackage = new YieldPackage((fastpath, name, version, scheme, summary, source, key,path, filename) => {
                        result.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = name,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Available",
                            SearchKey = key,
                            FullPath = path,
                            PackageFilename = filename,
                        });
                        return !(isCancelled() || result.IsCancelled);
                    })
                }))).GetCancellableEnumerator();
        }

        public ICancellableEnumerator<SoftwareIdentity> FindPackages(string[] names, string requiredVersion, string minimumVersion, string maximumVersion, Object c) {
            c = c.Extend<IRequest>(Context);
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), names.SelectMany(each => FindPackage(each, requiredVersion, minimumVersion, maximumVersion, id, c).ToIEnumerable()).ToArray().Concat(CompleteFind(id, c).ToIEnumerable()).ToArray()).GetCancellableEnumerator();
        }

        public ICancellableEnumerator<SoftwareIdentity> FindPackagesByUris(Uri[] uris, Object c) {
            c = c.Extend<IRequest>(Context);
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), uris.SelectMany(each => FindPackageByUri(each, id, c).ToIEnumerable()).ToArray().Concat(CompleteFind(id, c).ToIEnumerable())).GetCancellableEnumerator();
        }

        public ICancellableEnumerator<SoftwareIdentity> FindPackagesByFiles(string[] filenames, Object c) {
            c = c.Extend<IRequest>(Context);
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), filenames.SelectMany(each => FindPackageByFile(each, id, c).ToIEnumerable()).ToArray().Concat(CompleteFind(id, c).ToIEnumerable())).GetCancellableEnumerator();
        }

        public ICancellableEnumerator<SoftwareIdentity> FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Object c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(result =>
                Provider.FindPackage(name, requiredVersion, minimumVersion, maximumVersion, id, c.Extend<IRequest>(Context, new {
                    // add YieldPackage method to the callback
                    YieldPackage = new YieldPackage((fastpath, n, version, scheme, summary, source, key,path, filename ) => {
                        result.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = n,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Available",
                            SearchKey = key,
                            FullPath = path,
                            PackageFilename = filename,
                        });
                        return !(isCancelled() || result.IsCancelled);
                    })
                }))).GetCancellableEnumerator();
        }

        public ICancellableEnumerator<SoftwareIdentity> GetInstalledPackages(string name, Object c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(result =>
                Provider.GetInstalledPackages(name, c.Extend<IRequest>(Context, new {
                    // add YieldPackage method to the callback
                    YieldPackage = new YieldPackage((fastpath, n, version, scheme, summary, source, key,path, filename) => {
                        result.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = n,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Installed",
                            SearchKey = key,
                            FullPath = path,
                            PackageFilename = filename,
                        });
                        return !(isCancelled() || result.IsCancelled);
                    })
                }))).GetCancellableEnumerator();
        }

        public ICancellableEnumerator<SoftwareIdentity> InstallPackage(SoftwareIdentity softwareIdentity, Object c) {
            var isCancelled = c.As<IsCancelled>();

            var request = c.Extend<IRequest>(Context);

            if (softwareIdentity == null) {
                throw new ArgumentNullException("softwareIdentity");
            }

            if (c == null) {
                throw new ArgumentNullException("c");
            }

            if (!IsTrustedPackageSource(softwareIdentity.Source, c)) {
                try {
                    if (!request.ShouldContinueWithUntrustedPackageSource(softwareIdentity.Name, softwareIdentity.Source)) {
                        request.Error("User declined to trust package source ");
                        throw new Exception("cancelled");
                    }
                } catch {
                    request.Error("User declined to trust package source ");
                    throw new Exception("cancelled");
                }
            }

            return CallAndCollect<SoftwareIdentity>(result => Provider.InstallPackage(softwareIdentity.FastPackageReference, c.Extend<IRequest>(Context, new {
                YieldPackage = new YieldPackage((fastpath, n, version, scheme, summary, source, key,path, filename) => {
                    result.Add(new SoftwareIdentity {
                        FastPackageReference = fastpath,
                        Name = n,
                        Version = version,
                        VersionScheme = scheme,
                        Summary = summary,
                        ProviderName = Name,
                        Source = source,
                        Status = "Installed",
                        SearchKey = key,
                        FullPath = path,
                        PackageFilename = filename,
                    });
                    return !(isCancelled() || result.IsCancelled);
                })
            }))).GetCancellableEnumerator();
        }

        public ICancellableEnumerator<SoftwareIdentity> UninstallPackage(SoftwareIdentity softwareIdentity, Object c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(result =>
                Provider.UninstallPackage(softwareIdentity.FastPackageReference, c.Extend<IRequest>(Context, new {
                    YieldPackage = new YieldPackage((fastpath, n, version, scheme, summary, source, key, path, filename) => {
                        result.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = n,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Not Installed",
                            SearchKey = key,
                            FullPath = path,
                            PackageFilename = filename,
                        });
                        return !(isCancelled() || result.IsCancelled);
                    })
                }))).GetCancellableEnumerator();
        }

        public bool IsValidPackageSource(string packageSource, Object c) {
            return false;
            // return Provider.IsValidPackageSource(packageSource, new InvokableDispatcher(c, Instance.Service.Invoke));
        }

        public bool IsTrustedPackageSource(string packageSource, Object c) {
            return false;
        }

        public ICancellableEnumerator<PackageSource> GetPackageSources(Object c) {
            var isCancelled = c.As<IsCancelled>();
            PackageSource lastItem = null;

            return CallAndCollect<PackageSource>(result =>
                Provider.GetPackageSources(c.Extend<IRequest>(Context, new {

                    YieldKeyValuePair = new YieldKeyValuePair((key, value) => {
                        if (lastItem != null) {
                            lastItem.DetailsCollection.AddOrSet(key, value);
                        }
                        return !(isCancelled() || result.IsCancelled);
                    }),

                    YieldPackageSource = new YieldPackageSource((name, location, isTrusted, isRegistered) => {
                        if (lastItem != null) {
                            result.Add(lastItem);
                        }

                        lastItem = new PackageSource {
                            Name = name,
                            Location = location,
                            Provider = this,
                            IsTrusted = isTrusted,
                            IsRegistered = isRegistered
                        };
                        return !(isCancelled() || result.IsCancelled);
                    })
                })), collection => {
                    if (lastItem != null) {
                        collection.Add(lastItem);
                    }
                    ;
                }).GetCancellableEnumerator();
        }

        public void DownloadPackage(SoftwareIdentity softwareIdentity, string destinationFilename, Object c) {
            Provider.DownloadPackage(softwareIdentity.FastPackageReference, destinationFilename,c.Extend<IRequest>(Context) );
        }
    }

    #region declare PackageProvider-types

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
        Path = 4,
        Uri = 5
    }

    public enum EnvironmentContext {
        All = 0,
        User = 1,
        System = 2
    }

    #endregion
}