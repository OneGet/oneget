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

namespace Microsoft.OneGet {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Core.Dynamic;
    using Core.Extensions;
    using Core.Providers.Meta;
    using Core.Providers.Package;
    using Core.Providers.Service;

    internal static class ProviderLoader {
        internal static void AcquireProviders(string assemblyPath, Func<string, IEnumerable<object>, object> callback, Action<string, IPackageProvider> yieldPackageProvider, Action<string, IServicesProvider> yieldServicesProvider) {
            var dynInterface = new DynamicInterface();

            var asm = Assembly.LoadFile(assemblyPath);
            if (asm == null) {
                return;
            }

            foreach (var provider in dynInterface.FilterTypesCompatibleTo<IMetaProvider>(asm).Select( each => dynInterface.Create<IMetaProvider>(each) )) {
                try {
                    provider.InitializeProvider(callback);
                    foreach (var name in provider.GetProviderNames()) {
                        var instance = provider.CreateProvider(name);
                        if (instance != null) {
                            // check if it's a Package Provider
                            if (dynInterface.IsInstanceCompatible<IPackageProvider>(instance)) {
                                try {
                                    var packageProvider = dynInterface.Create<IPackageProvider>(instance);
                                    packageProvider.InitializeProvider(callback);
                                    yieldPackageProvider(packageProvider.GetPackageProviderName(), packageProvider);
                                } catch (Exception e) {
                                    e.Dump();
                                }
                            }

                            // check if it's a Services Provider
                            if (dynInterface.IsInstanceCompatible<IServicesProvider>(instance)) {
                                try {
                                    var servicesProvider = dynInterface.Create<IServicesProvider>(instance);
                                    servicesProvider.InitializeProvider(callback);
                                    yieldServicesProvider(servicesProvider.GetServicesProviderName(), servicesProvider);
                                } catch (Exception e) {
                                    e.Dump();
                                }
                            }
                        }
                    }
                } catch (Exception e) {
                    e.Dump();
                }
            }

            foreach (var provider in dynInterface.FilterTypesCompatibleTo<IPackageProvider>(asm).Select(each => dynInterface.Create<IPackageProvider>(each))) {
                try {
                    provider.InitializeProvider(callback);
                    yieldPackageProvider(provider.GetPackageProviderName(), provider);
                } catch (Exception e) {
                    e.Dump();
                }
            }

            foreach (var provider in dynInterface.FilterTypesCompatibleTo<IServicesProvider>(asm).Select(each => dynInterface.Create<IServicesProvider>(each))) {
                try {
                    provider.InitializeProvider(callback);
                    yieldServicesProvider(provider.GetServicesProviderName(), provider);
                } catch (Exception e) {
                    e.Dump();
                }
            }
        }
    }
}