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
    using System.Threading.Tasks;
    using Api;
    using Collections;
    using Extensions;
    using Packaging;
    using Platform;
    using Plugin;

    public abstract class ProviderBase<T> : MarshalByRefObject where T : IProvider {
        private object _context;

        public ProviderBase(T provider) {
            Provider = provider;
        }

        protected T Provider {get; private set;}

        // we don't want these objects being gc's out because they remain unused...

        public abstract string Name {get;}

        private Dictionary<string, List<string>> _features;
        public IReadOnlyDictionary<string, List<string>> Features {
            get {
                return _features;
            }
        }

        protected object Context {
            get {
                return _context ?? (_context = new object[] {
                    PackageManagementService._instance.ServicesProvider,
                    new {
                        GetPackageManagementService = new Func<object,object>((c) => PackageManagementService._instance),
                        GetMessageString = new Func<string, string>((s) => s),
                        GetIRequestInterface = new Func<Type>(() => typeof (IRequest)),
#if DEBUG
                        Debug = new Action<string>(NativeMethods.OutputDebugString),
#endif
                        // ensure that someone says 'yeah, it's ok to continue' for each package install/uninstall notification 
                        NotifyBeforePackageInstall = new Func<string , string , string , string , bool>( (pkgName, pkgVersion, source, destination) => true),
                        NotifyPackageInstalled= new Func<string , string , string , string , bool>( (pkgName, pkgVersion, source, destination) => true),
                        NotifyBeforePackageUninstall= new Func<string , string , string , string , bool>( (pkgName, pkgVersion, source, destination) => true),
                        NotifyPackageUninstalled= new Func<string , string , string , string , bool>( (pkgName, pkgVersion, source, destination) => true),
                    }
                });
            }
        }

        public override object InitializeLifetimeService() {
            return null;
        }

        public bool IsMethodImplemented(string methodName) {
            return Provider.IsMethodImplemented(methodName);
        }

        public void Initialize(Object c) {
            // Provider.InitializeProvider(dynamicInstance, c);
            _features = GetFeatures(c);
        }

        internal CancellableEnumerable<T> CallAndCollect<T>(Action<CancellableBlockingCollection<T>> call, Action<CancellableBlockingCollection<T>> atFinally = null) {
            var collection = new CancellableBlockingCollection<T>();
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

        public Dictionary<string, List<string>> GetFeatures(Object c) {
            var isCancelled = c.As<IsCancelled>();
            var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            Provider.GetFeatures(c.Extend<IRequest>(Context, new {
                YieldKeyValuePair = new YieldKeyValuePair((key, value) => {
                    result.GetOrAdd(key, () => new List<string>()).Add(value);
                    return !(isCancelled());
                })
            }));
            return result;
        }

        public ICancellableEnumerator<DynamicOption> GetDynamicOptions(OptionCategory operation, Object c) {
            var isCancelled = c == null ? ()=> false : c.As<IsCancelled>();

            DynamicOption lastItem = null;
            var list = new List<string>();

            return CallAndCollect<DynamicOption>(
                result => Provider.GetDynamicOptions((int)operation, c.Extend<IRequest>(Context, new {
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
                            ProviderName = Name,
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
                }).GetCancellableEnumerator();
        }
    }
}