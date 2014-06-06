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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Collections;
    using Dynamic;
    using Extensions;
    using Packaging;
    using Platform;
    using Service;
    using Callback = System.Object;

    public abstract class ProviderBase<T> : MarshalByRefObject where T : IProvider {
        public ProviderBase(T provider) {
            Provider = provider;
        }

        protected T Provider {get; private set;}

        // we don't want these objects being gc's out because they remain unused...

        public abstract string Name {get;}

    
        private object _context;

        protected object Context {
            get {
                return _context ?? (_context = new object[] {
                    PackageManagementService._instance.ServicesProvider,
                    new {
                        GetPackageManagementService = new Func<object>(() => PackageManagementService._instance),
                        GetMessageString = new Func<string, string>((s) => s)
#if DEBUG
                        ,
                        Debug = new Action<string>(NativeMethods.OutputDebugString)
#endif
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

        public void InitializeProvider(Callback c) {
            Provider.InitializeProvider(DynamicInterface.Instance, c);
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

        public Dictionary<string, List<string>> GetFeatures(Callback c) {
            var isCancelled = c.As<IsCancelled>();
            var result = new Dictionary<string, List<string>>();
            Provider.GetFeatures(c.Extend<IRequest>(Context, new {
                YieldKeyValuePair = new YieldKeyValuePair((key, value) => {
                    result.GetOrAdd(key, () => new List<string>()).Add(value);
                    return !(isCancelled());
                })
            }));
            return result;
        }

        public CancellableEnumerable<DynamicOption> GetDynamicOptions(OptionCategory operation, Callback c) {
            var isCancelled = c.As<IsCancelled>();

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