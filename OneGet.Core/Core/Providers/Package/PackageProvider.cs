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

namespace Microsoft.OneGet.Core.Providers.Package {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api;
    using Collections;
    using Extensions;
    using Packaging;
    using Service;
    using Tasks;
    using Callback = System.Func<string, System.Collections.Generic.IEnumerable<object>, object>;

    public class PackageProvider : MarshalByRefObject {
        private readonly IPackageProvider _provider;

        internal PackageProvider(IPackageProvider provider) {
            _provider = provider;
        }

        public string Name {
            get {
                return _provider.GetPackageProviderName();
            }
        }

        // we don't want these objects being gc's out because they remain unused...
        public override object InitializeLifetimeService() {
            return null;
        }

        public bool IsImplemented(string methodName) {
            return _provider.IsImplemented(methodName);
        }

        // Friendly APIs

        public void InitializeProvider(Callback c) {
            _provider.InitializeProvider(c);
        }

        public void AddPackageSource(string name, string location, bool trusted, Callback c) {
            _provider.AddPackageSource(name, location, trusted, new InvokableDispatcher(c, Instance.Service.Invoke));
        }

        public void RemovePackageSource(string name, Callback c) {
            _provider.RemovePackageSource(name, new InvokableDispatcher(c, Instance.Service.Invoke));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackageByUri(Uri u, int id, Callback c) {
            var providerName = Name;

            return CallAndCollectResults<SoftwareIdentity, YieldPackage>(
                c, // inherited callback
                nc => _provider.FindPackageByUri(u, id, nc), // actual call
                (collection, okToContinue) => ((fastpath, name, version, scheme, summary, source) => {
                    collection.Add(new SoftwareIdentity {
                        FastPath = fastpath,
                        Name = name,
                        Version = version,
                        VersionScheme = scheme,
                        Summary = summary,
                        ProviderName = providerName,
                        Source = source,
                        Status = "Available"
                    });
                    return okToContinue();
                }));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackageByFile(string filename, int id, Callback c) {
            var providerName = Name;

            return CallAndCollectResults<SoftwareIdentity, YieldPackage>(
                c, // inherited callback
                nc => _provider.FindPackageByFile(filename, id, nc), // actual call
                (collection, okToContinue) => ((fastpath, name, version, scheme, summary, source) => {
                    collection.Add(new SoftwareIdentity {
                        FastPath = fastpath,
                        Name = name,
                        Version = version,
                        VersionScheme = scheme,
                        Summary = summary,
                        ProviderName = providerName,
                        Source = source,
                        Status = "Available"
                    });
                    return okToContinue();
                }));
        }

        public int StartFind(Callback c) {
            return _provider.StartFind(c);
        }

        public CancellableEnumerable<SoftwareIdentity> CompleteFind(int i, Callback c) {
            var providerName = Name;

            return CallAndCollectResults<SoftwareIdentity, YieldPackage>(
                c, // inherited callback
                nc => _provider.CompleteFind(i, nc), // actual call
                (collection, okToContinue) => ((fastpath, name, version, scheme, summary, source) => {
                    collection.Add(new SoftwareIdentity {
                        FastPath = fastpath,
                        Name = name,
                        Version = version,
                        VersionScheme = scheme,
                        Summary = summary,
                        ProviderName = providerName,
                        Source = source,
                        Status = "Available"
                    });
                    return okToContinue();
                }));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackages(string[] names, string requiredVersion, string minimumVersion, string maximumVersion, Callback c) {
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), names.SelectMany(each => FindPackage(each, requiredVersion, minimumVersion, maximumVersion, id, c)).Concat(CompleteFind(id, c)));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackagesByUris(Uri[] uris, Callback c) {
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), uris.SelectMany(each => FindPackageByUri(each, id, c)).Concat(CompleteFind(id, c)));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackagesByFiles(string[] filenames, Callback c) {
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), filenames.SelectMany(each => FindPackageByFile(each, id, c)).Concat(CompleteFind(id, c)));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Callback c) {
            var providerName = Name;

            return CallAndCollectResults<SoftwareIdentity, YieldPackage>(
                c, // inherited callback
                nc => _provider.FindPackage(name, requiredVersion, minimumVersion, maximumVersion, id, nc), // actual call
                (collection, okToContinue) => ((fastpath, n, version, scheme, summary, source) => {
                    collection.Add(new SoftwareIdentity {
                        FastPath = fastpath,
                        Name = n,
                        Version = version,
                        VersionScheme = scheme,
                        Summary = summary,
                        ProviderName = providerName,
                        Source = source,
                        Status = "Available"
                    });
                    return okToContinue();
                }), false);
        }

        public CancellableEnumerable<SoftwareIdentity> GetInstalledPackages(string name, Callback c) {
            var providerName = Name;

            return CallAndCollectResults<SoftwareIdentity, YieldPackage>(
                c, // inherited callback
                nc => _provider.GetInstalledPackages(name, nc), // actual call
                (collection, okToContinue) => ((fastpath, n, version, scheme, summary, source) => {
                    collection.Add(new SoftwareIdentity {
                        FastPath = fastpath,
                        Name = n,
                        Version = version,
                        VersionScheme = scheme,
                        Summary = summary,
                        ProviderName = providerName,
                        Source = source,
                        Status = "Installed"
                    });
                    return okToContinue();
                }));
        }

        public CancellableEnumerable<SoftwareIdentity> InstallPackage(SoftwareIdentity softwareIdentity, Callback c) {
            if (softwareIdentity == null) {
                throw new ArgumentNullException("softwareIdentity");
            }

            if (c == null) {
                throw new ArgumentNullException("c");
            }
            var providerName = Name;

            if (!IsTrustedPackageSource(softwareIdentity.Source, c)) {
                try {
                    if (!(bool)c.DynamicInvoke<ShouldContinueWithUntrustedPackageSource>(softwareIdentity.Name, softwareIdentity.Source)) {
                        c.DynamicInvoke<Error>("Cancelled", "User declined to trust package source ", null);
                        throw new Exception("cancelled");
                    }
                } catch {
                    c.DynamicInvoke<Error>("Cancelled", "User declined to trust package source ", null);
                    throw new Exception("cancelled");
                }
            }

            return CallAndCollectResults<SoftwareIdentity, YieldPackage>(
                c, // inherited callback
                nc => _provider.InstallPackage(softwareIdentity.FastPath, nc), // actual call
                (collection, okToContinue) => ((fastpath, n, version, scheme, summary, source) => {
                    collection.Add(new SoftwareIdentity {
                        FastPath = fastpath,
                        Name = n,
                        Version = version,
                        VersionScheme = scheme,
                        Summary = summary,
                        ProviderName = providerName,
                        Source = source,
                        Status = "Installed"
                    });
                    return okToContinue();
                }));
        }

        public CancellableEnumerable<SoftwareIdentity> UninstallPackage(SoftwareIdentity softwareIdentity, Callback c) {
            var providerName = Name;

            return CallAndCollectResults<SoftwareIdentity, YieldPackage>(
                c, // inherited callback
                nc => _provider.UninstallPackage(softwareIdentity.FastPath, nc), // actual call
                (collection, okToContinue) => ((fastpath, n, version, scheme, summary, source) => {
                    collection.Add(new SoftwareIdentity {
                        FastPath = fastpath,
                        Name = n,
                        Version = version,
                        VersionScheme = scheme,
                        Summary = summary,
                        ProviderName = providerName,
                        Source = source,
                        Status = "Not Installed"
                    });
                    return okToContinue();
                }));
        }

        /// <summary>
        ///     I noticed that most of my functions ended up as a pattern that was extremely common.
        ///     I've therefore decided to distill this down to eliminate fat-fingered mistakes when cloning the pattern.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="OnResultFn"></typeparam>
        /// <param name="c"></param>
        /// <param name="action"></param>
        /// <param name="onResultFn"></param>
        /// <returns></returns>
        private CancellableEnumerable<TResult> CallAndCollectResults<TResult, OnResultFn>(Callback c, Action<Callback> action,
            Func<CancellableBlockingCollection<TResult>, OkToContinue, OnResultFn> onResultFn, bool cancelOnException = true) {
            var result = new CancellableBlockingCollection<TResult>();

            Task.Factory.StartNew(() => {
                try {
                    // callback.DynamicInvoke<Verbose>("Hello", "World", null);
                    var isOkToContinueFn = new OkToContinue(() => !(result.IsCancelled || (bool)c.DynamicInvoke<IsCancelled>()));

                    using (var cb = new InvokableDispatcher(c, Instance.Service.Invoke) {
                        isOkToContinueFn,
                        onResultFn(result, isOkToContinueFn)
                    }) {
                        try {
                            action(cb);
                        } catch (Exception e) {
                            if (cancelOnException) {
                                result.Cancel();
                                Event<ExceptionThrown>.Raise(e.GetType().Name, e.Message, e.StackTrace);
                            }
                        }
                    }
                } catch (Exception e) {
                    e.Dump();
                } finally {
                    result.CompleteAdding();
                }
            });

            return result;
        }

        public CancellableEnumerable<DynamicOption> GetDynamicOptions(OptionCategory operation, Callback c) {
            return CallAndCollectResults<DynamicOption, YieldOptionDefinition>(
                c,
                nc => _provider.GetDynamicOptions((int)operation, nc),
                (collection, okToContinue) => ((category, name, type, isRequired, values) => {
                    collection.Add(new DynamicOption {
                        Category = category,
                        Name = name,
                        Type = type,
                        IsRequired = isRequired,
                        PossibleValues = values
                    });
                    return okToContinue();
                }));
        }

        public bool IsValidPackageSource(string packageSource, Callback c) {
            return false;
            // return _provider.IsValidPackageSource(packageSource, new InvokableDispatcher(c, Instance.Service.Invoke));
        }

        public bool IsTrustedPackageSource(string packageSource, Callback c) {
            return false;
        }

        public CancellableEnumerable<PackageSource> GetPackageSources(Callback c) {
            return CallAndCollectResults<PackageSource, YieldSource>(
                c,
                nc => _provider.GetPackageSources(nc),
                (collection, okToContinue) => ((name, location, isTrusted) => {
                    collection.Add(new PackageSource {
                        Name = name,
                        Location = location,
                        Provider = Name,
                        IsTrusted = isTrusted
                    });
                    return okToContinue();
                }));
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

    internal static class CallbackExt {
        public static T Lookup<T>(this Callback c) where T : class {
            return c(typeof (T).Name, null) as T ?? typeof (T).CreateEmptyDelegate() as T;
        }

        public static object DynamicInvoke<T>(this Callback c, params object[] args) where T : class {
            return c(typeof (T).Name, args);
        }
    }
}