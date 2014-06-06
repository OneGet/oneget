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

using Microsoft.OneGet.Core.Api;

public interface IRequest : IHostApis, ICoreApis, IRequestApis, IServicesApi {
}

public delegate bool IsCancelled();

public delegate bool YieldPackage(string fastPath, string name, string version, string versionScheme, string summary, string source, string searchKey);

public delegate bool YieldPackageDetails(object serializablePackageDetailsObject);

public delegate bool YieldPackageSource(string name, string location, bool isTrusted);

public delegate bool YieldDynamicOption(int category, string name, int expectedType, bool isRequired);

public delegate bool YieldKeyValuePair(string key, string value);

namespace Microsoft.OneGet.Core.Providers.Package {
    using System;
    using System.Linq;
    using System.Threading;
    using Collections;
    using Dynamic;
    using Packaging;
    using Callback = System.Object;

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

        public void AddPackageSource(string name, string location, bool trusted, Callback c) {
            Provider.AddPackageSource(name, location, trusted, DynamicInterface.Instance.Create<IRequest>(c, Context));
        }

        public void RemovePackageSource(string name, Callback c) {
            Provider.RemovePackageSource(name, DynamicInterface.Instance.Create<IRequest>(c, Context));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackageByUri(Uri uri, int id, Callback c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(collection =>
                Provider.FindPackageByUri(uri, id, c.Extend<IRequest>(Context, new {
                    YieldPackage = new YieldPackage((fastpath, name, version, scheme, summary, source, key) => {
                        collection.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = name,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Available",
                            SearchKey = key
                        });
                        return !(isCancelled() || collection.IsCancelled);
                    })
                })));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackageByFile(string filename, int id, Callback c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(result =>
                Provider.FindPackageByFile(filename, id, c.Extend<IRequest>(Context, new {
                    YieldPackage = new YieldPackage((fastpath, name, version, scheme, summary, source, key) => {
                        result.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = name,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Available",
                            SearchKey = key
                        });
                        return !(isCancelled() || result.IsCancelled);
                    })
                })));
        }

        public int StartFind(Callback c) {
            return Provider.StartFind(c.As<IRequest>());
        }

        public CancellableEnumerable<SoftwareIdentity> CompleteFind(int i, Callback c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(result =>
                Provider.CompleteFind(i, c.Extend<IRequest>(Context, new {
                    // add YieldPackage method to the callback
                    YieldPackage = new YieldPackage((fastpath, name, version, scheme, summary, source, key) => {
                        result.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = name,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Available",
                            SearchKey = key
                        });
                        return !(isCancelled() || result.IsCancelled);
                    })
                })));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackages(string[] names, string requiredVersion, string minimumVersion, string maximumVersion, Callback c) {
            c = c.Extend<IRequest>(Context);
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), names.SelectMany(each => FindPackage(each, requiredVersion, minimumVersion, maximumVersion, id, c)).ToArray().Concat(CompleteFind(id, c)).ToArray());
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackagesByUris(Uri[] uris, Callback c) {
            c = c.Extend<IRequest>(Context);
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), uris.SelectMany(each => FindPackageByUri(each, id, c)).ToArray().Concat(CompleteFind(id, c)));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackagesByFiles(string[] filenames, Callback c) {
            c = c.Extend<IRequest>(Context);
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), filenames.SelectMany(each => FindPackageByFile(each, id, c)).ToArray().Concat(CompleteFind(id, c)));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Callback c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(result =>
                Provider.FindPackage(name, requiredVersion, minimumVersion, maximumVersion, id, c.Extend<IRequest>(Context, new {
                    // add YieldPackage method to the callback
                    YieldPackage = new YieldPackage((fastpath, n, version, scheme, summary, source, key) => {
                        result.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = n,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Available",
                            SearchKey = key
                        });
                        return !(isCancelled() || result.IsCancelled);
                    })
                })));
        }

        public CancellableEnumerable<SoftwareIdentity> GetInstalledPackages(string name, Callback c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(result =>
                Provider.GetInstalledPackages(name, c.Extend<IRequest>(Context, new {
                    // add YieldPackage method to the callback
                    YieldPackage = new YieldPackage((fastpath, n, version, scheme, summary, source, key) => {
                        result.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = n,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Installed",
                            SearchKey = key
                        });
                        return !(isCancelled() || result.IsCancelled);
                    })
                })));
        }

        public CancellableEnumerable<SoftwareIdentity> InstallPackage(SoftwareIdentity softwareIdentity, Callback c) {
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
                YieldPackage = new YieldPackage((fastpath, n, version, scheme, summary, source, key) => {
                    result.Add(new SoftwareIdentity {
                        FastPackageReference = fastpath,
                        Name = n,
                        Version = version,
                        VersionScheme = scheme,
                        Summary = summary,
                        ProviderName = Name,
                        Source = source,
                        Status = "Installed",
                        SearchKey = key
                    });
                    return !(isCancelled() || result.IsCancelled);
                })
            })));
        }

        public CancellableEnumerable<SoftwareIdentity> UninstallPackage(SoftwareIdentity softwareIdentity, Callback c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(result =>
                Provider.UninstallPackage(softwareIdentity.FastPackageReference, c.Extend<IRequest>(Context, new {
                    YieldPackage = new YieldPackage((fastpath, n, version, scheme, summary, source, key) => {
                        result.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = n,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Not Installed",
                            SearchKey = key
                        });
                        return !(isCancelled() || result.IsCancelled);
                    })
                })));
        }

        public bool IsValidPackageSource(string packageSource, Callback c) {
            return false;
            // return Provider.IsValidPackageSource(packageSource, new InvokableDispatcher(c, Instance.Service.Invoke));
        }

        public bool IsTrustedPackageSource(string packageSource, Callback c) {
            return false;
        }

        public CancellableEnumerable<PackageSource> GetPackageSources(Callback c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<PackageSource>(result =>
                Provider.GetPackageSources(c.Extend<IRequest>(Context, new {
                    YieldPackageSource = new YieldPackageSource((name, location, isTrusted) => {
                        result.Add(new PackageSource {
                            Name = name,
                            Location = location,
                            Provider = Name,
                            IsTrusted = isTrusted
                        });
                        return !(isCancelled() || result.IsCancelled);
                    })
                })));
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