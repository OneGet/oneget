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
    using System.Diagnostics.CodeAnalysis;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Api;
    using Packaging;
    using Resources;
    using Utility.Collections;
    using Utility.Extensions;
    using Utility.Platform;
    using Utility.Plugin;
    using Utility.Versions;
    using RequestImpl = System.Object;

    public abstract class ProviderBase<T> : MarshalByRefObject where T : IProvider {
        private static Regex _canonicalPackageRegex = new Regex("(.*?):(.*?)/(.*)");
        private object _context;
        private Dictionary<string, List<string>> _features;
        private bool _initialized;
        private FourPartVersion _version;

        public ProviderBase(T provider) {
            Provider = provider;
        }

        protected T Provider {get; private set;}

        // we don't want these objects being gc's out because they remain unused...

        public abstract string ProviderName {get;}

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "This is required for the PowerShell Providers.")]
        public IDictionary<string, List<string>> Features {
            get {
                // todo: this dictionary should be read only (.net 4.0 doesn't have that!)
                return _features;
            }
        }

        public FourPartVersion Version {
            get {
                return _version;
            }
            set {
                if (_version == 0) {
                    _version = value;
                }
            }
        }

        private RequestImpl AdditionalImplementedFunctions {
            get {
                return _context ?? (_context = new object[] {
                    PackageManagementService._instance.ServicesProvider,
                    new {
                        GetPackageManagementService = new Func<object, object>((requestImpl) => PackageManagementService._instance),
                        GetIRequestInterface = new Func<Type>(() => typeof (IRequest)),
#if DEBUG
                        Debug = new Action<string>(NativeMethods.OutputDebugString),
#endif
                        // ensure that someone says 'yeah, it's ok to continue' for each package install/uninstall notification 
                        NotifyBeforePackageInstall = new Func<string, string, string, string, bool>((pkgName, pkgVersion, source, destination) => true),
                        NotifyPackageInstalled = new Func<string, string, string, string, bool>((pkgName, pkgVersion, source, destination) => true),
                        NotifyBeforePackageUninstall = new Func<string, string, string, string, bool>((pkgName, pkgVersion, source, destination) => true),
                        NotifyPackageUninstalled = new Func<string, string, string, string, bool>((pkgName, pkgVersion, source, destination) => true),
                        GetCanonicalPackageId = new Func<string, string, string, string>((providerName, packageName, version) => "{0}:{1}/{2}".format(providerName, packageName, version)),
                        ParseProviderName = new Func<string, string>((canonicalPackageId) => _canonicalPackageRegex.Match(canonicalPackageId).Groups[1].Value),
                        ParsePackageName = new Func<string, string>((canonicalPackageId) => _canonicalPackageRegex.Match(canonicalPackageId).Groups[2].Value),
                        ParsePackageVersion = new Func<string, string>((canonicalPackageId) => _canonicalPackageRegex.Match(canonicalPackageId).Groups[3].Value),
                    }
                });
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "This is required for the PowerShell Providers.")]
        public List<DynamicOption> DynamicOptions {
            get {
                var result = new List<DynamicOption>();
                result.AddRange(GetDynamicOptions(OptionCategory.Install, null));
                result.AddRange(GetDynamicOptions(OptionCategory.Package, null));
                result.AddRange(GetDynamicOptions(OptionCategory.Provider, null));
                result.AddRange(GetDynamicOptions(OptionCategory.Source, null));
                return result;
            }
        }

        internal IRequest ExtendRequest(RequestImpl requestImpl, params object[] objects) {
            var baseGetMessageString = requestImpl.As<GetMessageString>();

            return requestImpl.Extend<IRequest>(new {
                // check the caller's resource manager first, then fall back to this resource manager
                GetMessageString = new Func<string, string>((s) => baseGetMessageString(s) ?? Messages.ResourceManager.GetString(s)),
            }, objects, AdditionalImplementedFunctions);
        }

        public override object InitializeLifetimeService() {
            return null;
        }

        public bool IsMethodImplemented(string methodName) {
            return Provider.IsMethodImplemented(methodName);
        }

        internal void Initialize(IRequest request) {
            // Provider.InitializeProvider(dynamicInstance, c);
            if (!_initialized) {
                _features = GetFeatures(request);
                _initialized = true;
            }
        }

        internal CancellableEnumerable<TItem> CallAndCollect<TItem>(Action<CancellableBlockingCollection<TItem>> call, Action<CancellableBlockingCollection<TItem>> atFinally = null) {
            var collection = new CancellableBlockingCollection<TItem>();
            Task.Factory.StartNew(() => {
                try {
                    call(collection);
                } catch (Exception e) {
                    e.Dump();
                } finally {
                    if (atFinally != null) {
                        atFinally(collection);
                    }
                    collection.CompleteAdding();
                }
            });

            return collection;
        }

        internal CancellableEnumerable<TItem> CallAndCollect<TItem>(RequestImpl requestImpl, Response<TItem> response, Action<object> call) {
            Task.Factory.StartNew(() => {
                try {
                    call(ExtendRequest(requestImpl, response));
                } catch (Exception e) {
                    e.Dump();
                } finally {
                    response.Complete();
                }
            });

            return response.Result;
        }

        public Dictionary<string, List<string>> GetFeatures(RequestImpl requestImpl) {
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            var isCancelled = requestImpl.As<IsCancelled>();
            var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            Provider.GetFeatures(ExtendRequest(requestImpl, new {
                YieldKeyValuePair = new YieldKeyValuePair((key, value) => {
                    result.GetOrAdd(key, () => new List<string>()).Add(value);
                    return !(isCancelled());
                })
            }));
            return result;
        }

        public ICancellableEnumerable<DynamicOption> GetDynamicOptions(OptionCategory operation, RequestImpl requestImpl) {
            requestImpl = requestImpl ?? new {
                IsCancelled = new IsCancelled(() => false)
            };

            var isCancelled = requestImpl.As<IsCancelled>();

            DynamicOption lastItem = null;
            var list = new List<string>();

            return CallAndCollect<DynamicOption>(
                result => Provider.GetDynamicOptions((int)operation, ExtendRequest(requestImpl, new {
                    YieldDynamicOption = new YieldDynamicOption((category, name, type, isRequired) => {
                        if (lastItem != null) {
                            lastItem.PossibleValues = list.ToArray();
                            list = new List<string>();
                            result.Add(lastItem);
                        }

                        lastItem = new DynamicOption {
                            Category = (OptionCategory)category,
                            Name = name,
                            Type = (OptionType)type,
                            IsRequired = isRequired,
                            ProviderName = ProviderName,
                        };
                        return !(isCancelled() || result.IsCancelled);
                    }),
                    YieldKeyValuePair = new YieldKeyValuePair((key, value) => {
                        if (lastItem != null && lastItem.Name == key) {
                            list.Add(value);
                        }
                        return !(isCancelled() || result.IsCancelled);
                    })
                })), collection => {
                    if (lastItem != null) {
                        lastItem.PossibleValues = list.ToArray();
                        collection.Add(lastItem);
                    }
                });
        }
    }
}