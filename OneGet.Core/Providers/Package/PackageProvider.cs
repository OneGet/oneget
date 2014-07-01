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

    public delegate bool YieldSoftwareMetadata(string parentFastPath, string name, string value, string fieldPath);

    public delegate bool YieldEntity(string parentFastPath, string name, string regid, string role, string thumbprint, string fieldPath);

    public delegate bool YieldLink(string parentFastPath, string artifact, string referenceUrl, string appliesToMedia, string ownership, string relativeTo, string mediaType, string use,string fieldPath);

    public delegate bool YieldSwidtag(string fastPath, string xmlOrJsonDoc);

    public delegate bool YieldMetadata(string fieldId, string @namespace, string name, string value);

    public delegate bool YieldPackageSource(string name, string location, bool isTrusted,bool isRegistered, bool isValidated);

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

        public ICancellableEnumerable<PackageSource> AddPackageSource(string name, string location, bool trusted, Object c) {
            // Provider.AddPackageSource(name, location, trusted, DynamicInterface.Instance.Create<IRequest>(c, Context));
            var isCancelled = c.As<IsCancelled>();
            PackageSource lastItem = null;

            var result = new CancellableBlockingCollection<PackageSource>();

            Provider.AddPackageSource(name, location, trusted, c.Extend<IRequest>(Context, new {

                YieldKeyValuePair = new YieldKeyValuePair((key, value) => {
                    if (lastItem != null) {
                        lastItem.DetailsCollection.AddOrSet(key, value);
                    }
                    return !(isCancelled() || result.IsCancelled);
                }),

                YieldPackageSource = new YieldPackageSource((n, l, isTrusted, isRegistered, isValidated) => {
                    if (lastItem != null) {
                        result.Add(lastItem);
                    }

                    lastItem = new PackageSource {
                        Name = n,
                        Location = l,
                        Provider = this,
                        IsTrusted = isTrusted,
                        IsRegistered = isRegistered,
                        IsValidated = isValidated,
                    };
                    return !(isCancelled() || result.IsCancelled);
                })
            }));
            if (lastItem != null) {
                result.Add(lastItem);
            }
            result.CompleteAdding();
            return (CancellableEnumerable<PackageSource>)result;
        }

        public ICancellableEnumerable<PackageSource> RemovePackageSource(string name, Object c) {
            // Provider.RemovePackageSource(name, DynamicInterface.Instance.Create<IRequest>(c, Context));
            var isCancelled = c.As<IsCancelled>();
            PackageSource lastItem = null;

            var result = new CancellableBlockingCollection<PackageSource>();

            Provider.RemovePackageSource(name, c.Extend<IRequest>(Context, new {

                YieldKeyValuePair = new YieldKeyValuePair((key, value) => {
                    if (lastItem != null) {
                        lastItem.DetailsCollection.AddOrSet(key, value);
                    }
                    return !(isCancelled() || result.IsCancelled);
                }),

                YieldPackageSource = new YieldPackageSource((n, l, isTrusted, isRegistered, isValidated) => {
                    if (lastItem != null) {
                        result.Add(lastItem);
                    }

                    lastItem = new PackageSource {
                        Name = n,
                        Location = l,
                        Provider = this,
                        IsTrusted = isTrusted,
                        IsRegistered = isRegistered,
                        IsValidated = isValidated,
                    };
                    return !(isCancelled() || result.IsCancelled);
                })
            }));
            if (lastItem != null) {
                result.Add(lastItem);
            }
            result.CompleteAdding();

            return (CancellableEnumerable<PackageSource>)result;   
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackageByUri(Uri uri, int id, Object c) {
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
                })));
        }

        public ICancellableEnumerable<SoftwareIdentity> GetPackageDependencies(SoftwareIdentity package, Object c) {
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
                })));
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackageByFile(string filename, int id, Object c) {
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
                })));
        }

        public int StartFind(Object c) {
            return Provider.StartFind(c.As<IRequest>());
        }

        public ICancellableEnumerable<SoftwareIdentity> CompleteFind(int i, Object c) {
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
                })));
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackages(string[] names, string requiredVersion, string minimumVersion, string maximumVersion, Object c) {
            c = c.Extend<IRequest>(Context);
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), names.SelectMany(each => FindPackage(each, requiredVersion, minimumVersion, maximumVersion, id, c)).ToArray().Concat(CompleteFind(id, c)).ToArray());
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackagesByUris(Uri[] uris, Object c) {
            c = c.Extend<IRequest>(Context);
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), uris.SelectMany(each => FindPackageByUri(each, id, c)).ToArray().Concat(CompleteFind(id, c)));
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackagesByFiles(string[] filenames, Object c) {
            c = c.Extend<IRequest>(Context);
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), filenames.SelectMany(each => FindPackageByFile(each, id, c)).ToArray().Concat(CompleteFind(id, c)));
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Object c) {
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
                })));
        }

        public ICancellableEnumerable<SoftwareIdentity> GetInstalledPackages(string name, Object c) {
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
                })));
        }

        public ICancellableEnumerable<SoftwareIdentity> InstallPackage(SoftwareIdentity softwareIdentity, Object c) {
            var isCancelled = c.As<IsCancelled>();

            var request = c.Extend<IRequest>(Context);

            if (softwareIdentity == null) {
                throw new ArgumentNullException("softwareIdentity");
            }

            if (c == null) {
                throw new ArgumentNullException("c");
            }

            // check if this source is trusted first.
            var src = ResolvePackageSources(c.Extend<IRequest>(new {
                GetSources = new Func<IEnumerable<string>>(() => {
                    return new string[] {
                        softwareIdentity.Source
                    };
                })
            }, Context)).FirstOrDefault();

            var trusted = (src != null && src.IsTrusted);

            if (!trusted) {
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
            })));
        }

        public ICancellableEnumerable<SoftwareIdentity> UninstallPackage(SoftwareIdentity softwareIdentity, Object c) {
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
                })));
        }

        public ICancellableEnumerable<PackageSource> ResolvePackageSources(Object c) {
            var isCancelled = c.As<IsCancelled>();
            PackageSource lastItem = null;

            return CallAndCollect<PackageSource>(result =>
                Provider.ResolvePackageSources(c.Extend<IRequest>(Context, new {

                    YieldKeyValuePair = new YieldKeyValuePair((key, value) => {
                        if (lastItem != null) {
                            lastItem.DetailsCollection.AddOrSet(key, value);
                        }
                        return !(isCancelled() || result.IsCancelled);
                    }),

                    YieldPackageSource = new YieldPackageSource((name, location, isTrusted, isRegistered, isValidated) => {
                        if (lastItem != null) {
                            result.Add(lastItem);
                        }

                        lastItem = new PackageSource {
                            Name = name,
                            Location = location,
                            Provider = this,
                            IsTrusted = isTrusted,
                            IsRegistered = isRegistered,
                            IsValidated = isValidated,
                        };
                        return !(isCancelled() || result.IsCancelled);
                    })
                })), collection => {
                    if (lastItem != null) {
                        collection.Add(lastItem);
                    }
                    ;
                });
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