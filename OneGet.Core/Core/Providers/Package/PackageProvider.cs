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

using System.Collections.Generic;
using Microsoft.OneGet;
using Microsoft.OneGet.Core.Api;

public interface IRequest : IHostApis, ICoreApis, IRequestApis, IServicesApi {
}

public delegate bool IsCancelled();
public delegate bool YieldPackage(string fastPath, string name, string version, string versionScheme, string summary, string source);
public delegate bool YieldPackageDetails(object serializablePackageDetailsObject);
public delegate bool YieldPackageSource(string name, string location, bool isTrusted);
public delegate bool YieldDynamicOption(int category, string name, int expectedType, bool isRequired, IEnumerable<string> permittedValues);

namespace Microsoft.OneGet.Core.Providers.Package {
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Collections;
    using Dynamic;
    using Extensions;
    using Packaging;
    using Platform;
    using Callback = System.Object;

    public class PackageProvider : MarshalByRefObject {
        private readonly IPackageProvider _provider;

        private string _name;

     
        internal PackageProvider(IPackageProvider provider) {
            _provider = provider;
        }

        public string Name {
            get {
                return _name ?? (_name = _provider.GetPackageProviderName());
            }
        }


        public object GetPackageManagementService() {
            return new PackageManagementService().Instance;
        }

        public string GetMessageString(string message) {
            return message;
        }

        // we don't want these objects being gc's out because they remain unused...
        public override object InitializeLifetimeService() {
            return null;
        }

        public bool IsMethodImplemented(string methodName) {
            return _provider.IsMethodImplemented(methodName);
        }

        private object RequestStuff {
            get {
                return new {
                    GetPackageManagementService = new Func<object>(() => new PackageManagementService().Instance),
                    GetMessageString = new Func<string, string>((s) => s)
#if DEBUG
                    ,Debug = new Action<string>((s) => { NativeMethods.OutputDebugString(s); })
#endif
                };
            }
        }

        // Friendly APIs

        public void InitializeProvider(Callback c) {
            _provider.InitializeProvider(DynamicInterface.Instance , c);
        }

        public void AddPackageSource(string name, string location, bool trusted, Callback c) {
            _provider.AddPackageSource(name, location, trusted, DynamicInterface.Instance.Create<IRequest>(c, this));
        }

        public void RemovePackageSource(string name, Callback c) {
            _provider.RemovePackageSource(name, DynamicInterface.Instance.Create<IRequest>(c, RequestStuff));
        }

        private CancellableEnumerable<T> CallAndCollect<T>(Action<CancellableBlockingCollection<T>> call) {
            var collection = new CancellableBlockingCollection<T>();
            Task.Factory.StartNew(() => {
                try {
                    call(collection);
                } catch (Exception e) {
                    e.Dump();
                } finally {
                    collection.CompleteAdding();
                }
            });
            return collection;
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackageByUri(Uri uri, int id, Callback c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(collection =>
                _provider.FindPackageByUri(uri, id, c.Extend<IRequest>(RequestStuff, new {
                    YieldPackage = new YieldPackage((fastpath, name, version, scheme, summary, source) => {
                        collection.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = name,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Available"
                        });
                        return !(isCancelled() || collection.IsCancelled);
                    })
                })));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackageByFile(string filename, int id, Callback c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(result =>
                _provider.FindPackageByFile(filename, id, c.Extend<IRequest>(RequestStuff, new {
                    YieldPackage = new YieldPackage((fastpath, name, version, scheme, summary, source) => {
                        result.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = name,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Available"
                        });
                        return !(isCancelled() || result.IsCancelled);
                    })
                })));
        }

        public int StartFind(Callback c) {
            return _provider.StartFind(c);
        }

        public CancellableEnumerable<SoftwareIdentity> CompleteFind(int i, Callback c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(result =>
                _provider.CompleteFind(i, c.Extend<IRequest>(RequestStuff, new {
                    // add YieldPackage method to the callback
                    YieldPackage = new YieldPackage((fastpath, name, version, scheme, summary, source) => {
                        result.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = name,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Available"
                        });
                        return !(isCancelled() || result.IsCancelled);
                    })
                })));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackages(string[] names, string requiredVersion, string minimumVersion, string maximumVersion, Callback c) {
            c = c.Extend<IRequest>(RequestStuff);
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), names.SelectMany(each => FindPackage(each, requiredVersion, minimumVersion, maximumVersion, id, c)).Concat(CompleteFind(id, c)));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackagesByUris(Uri[] uris, Callback c) {
            c = c.Extend<IRequest>(RequestStuff);
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), uris.SelectMany(each => FindPackageByUri(each, id, c)).Concat(CompleteFind(id, c)));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackagesByFiles(string[] filenames, Callback c) {
            c = c.Extend<IRequest>(RequestStuff);
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), filenames.SelectMany(each => FindPackageByFile(each, id, c)).Concat(CompleteFind(id, c)));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Callback c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(result =>
                _provider.FindPackage(name, requiredVersion, minimumVersion, maximumVersion, id, c.Extend<IRequest>(RequestStuff, new {
                    // add YieldPackage method to the callback
                    YieldPackage = new YieldPackage((fastpath, n, version, scheme, summary, source) => {
                        result.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = n,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Available"
                        });
                        return !(isCancelled() || result.IsCancelled);
                    })
                })));
        }

        public CancellableEnumerable<SoftwareIdentity> GetInstalledPackages(string name, Callback c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(result =>
                _provider.GetInstalledPackages(name, c.Extend<IRequest>(RequestStuff , new {
                    // add YieldPackage method to the callback
                    YieldPackage = new YieldPackage((fastpath, n, version, scheme, summary, source) => {
                        result.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = n,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Installed"
                        });
                        return !(isCancelled() || result.IsCancelled);
                    })
                })));
        }

        public CancellableEnumerable<SoftwareIdentity> InstallPackage(SoftwareIdentity softwareIdentity, Callback c) {
            var isCancelled = c.As<IsCancelled>();

            var request = c.Extend<IRequest>(RequestStuff);

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

            return CallAndCollect<SoftwareIdentity>(result => _provider.InstallPackage(softwareIdentity.FastPackageReference, c.Extend<IRequest>(RequestStuff, new {
                YieldPackage = new YieldPackage((fastpath, n, version, scheme, summary, source) => {
                    result.Add(new SoftwareIdentity {
                        FastPackageReference = fastpath,
                        Name = n,
                        Version = version,
                        VersionScheme = scheme,
                        Summary = summary,
                        ProviderName = Name,
                        Source = source,
                        Status = "Installed"
                    });
                    return !(isCancelled() || result.IsCancelled);
                })
            })));
        }

        public CancellableEnumerable<SoftwareIdentity> UninstallPackage(SoftwareIdentity softwareIdentity, Callback c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<SoftwareIdentity>(result =>
                _provider.UninstallPackage(softwareIdentity.FastPackageReference, c.Extend<IRequest>(RequestStuff, new {
                    YieldPackage = new YieldPackage((fastpath, n, version, scheme, summary, source) => {
                        result.Add(new SoftwareIdentity {
                            FastPackageReference = fastpath,
                            Name = n,
                            Version = version,
                            VersionScheme = scheme,
                            Summary = summary,
                            ProviderName = Name,
                            Source = source,
                            Status = "Not Installed"
                        });
                        return !(isCancelled() || result.IsCancelled);
                    })
                })));
        }

        public CancellableEnumerable<DynamicOption> GetDynamicOptions(OptionCategory operation, Callback c) {
            var isCancelled = c.As<IsCancelled>();
            /*
            var extended = c.Extend<IRequest>(RequestStuff, new {
                YieldDynamicOption = new YieldDynamicOption((category, name, type, isRequired, values) => {
                    Console.WriteLine("YIELD DYNAMIC OPTION");
                    return !(isCancelled());
                })
            });

            _provider.GetDynamicOptions((int)operation, extended);
            */

            return CallAndCollect<DynamicOption>(
                result => _provider.GetDynamicOptions((int)operation, c.Extend<IRequest>(RequestStuff, new {
                    GetMessageString = new Func<string, string>(s => s),
                    YieldDynamicOption = new YieldDynamicOption((category, name, type, isRequired, values) => {
                        result.Add(new DynamicOption {
                            Category = (OptionCategory)category,
                            Name = name,
                            Type = (OptionType)type,
                            IsRequired = isRequired,
                            PossibleValues = values
                        });
                        return !(isCancelled() || result.IsCancelled);
                    })
                })));
        }

        public bool IsValidPackageSource(string packageSource, Callback c) {
            return false;
            // return _provider.IsValidPackageSource(packageSource, new InvokableDispatcher(c, Instance.Service.Invoke));
        }

        public bool IsTrustedPackageSource(string packageSource, Callback c) {
            return false;
        }

        public CancellableEnumerable<PackageSource> GetPackageSources(Callback c) {
            var isCancelled = c.As<IsCancelled>();

            return CallAndCollect<PackageSource>(result =>
                _provider.GetPackageSources(c.Extend<IRequest>(RequestStuff, new {
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

    #endregion

}