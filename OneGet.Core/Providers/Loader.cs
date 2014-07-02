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

namespace Microsoft.OneGet.Providers {
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Extensions;
    using Meta;
    using Package;
    using Plugin;
    using Service;

    internal static class Loader {
        internal static bool AcquireProviders(string assemblyPath, Object callback, Action<string, IPackageProvider> YieldSoftwareIdentityProvider, Action<string, IServicesProvider> yieldServicesProvider) {
            var dynInterface = new DynamicInterface();
            var result = false;

            var asm = Assembly.LoadFile(assemblyPath);
            if (asm == null) {
                return false;
            }

            foreach (var provider in dynInterface.FilterTypesCompatibleTo<IMetaProvider>(asm).Select(each => dynInterface.Create<IMetaProvider>(each))) {
                Debug.WriteLine(string.Format("START MetaProvider {0}", provider.GetMetaProviderName()));
                try {
                    provider.InitializeProvider(DynamicInterface.Instance, callback);
                    var metaProvider = provider;
                    Parallel.ForEach(provider.GetProviderNames(), name => {
                        // foreach (var name in provider.GetProviderNames()) {
                        var instance = metaProvider.CreateProvider(name);
                        if (instance != null) {
                            // check if it's a Package Provider
                            if (dynInterface.IsInstanceCompatible<IPackageProvider>(instance)) {
                                try {
                                    var packageProvider = dynInterface.Create<IPackageProvider>(instance);
                                    packageProvider.InitializeProvider(DynamicInterface.Instance, callback);
                                    YieldSoftwareIdentityProvider(packageProvider.GetPackageProviderName(), packageProvider);
                                    result = true;
                                } catch (Exception e) {
                                    e.Dump();
                                }
                            }

                            // check if it's a Services Provider
                            if (dynInterface.IsInstanceCompatible<IServicesProvider>(instance)) {
                                try {
                                    var servicesProvider = dynInterface.Create<IServicesProvider>(instance);
                                    servicesProvider.InitializeProvider(DynamicInterface.Instance, callback);
                                    yieldServicesProvider(servicesProvider.GetServicesProviderName(), servicesProvider);
                                    result = true;
                                } catch (Exception e) {
                                    e.Dump();
                                }
                            }
                        }
                    });
                } catch (Exception e) {
                    e.Dump();
                }
                Debug.WriteLine(string.Format("FINISH MetaProvider {0}", provider.GetMetaProviderName()));
            }

            foreach (var provider in dynInterface.FilterTypesCompatibleTo<IPackageProvider>(asm).Select(each => dynInterface.Create<IPackageProvider>(each))) {
                try {
                    provider.InitializeProvider(DynamicInterface.Instance, callback);
                    YieldSoftwareIdentityProvider(provider.GetPackageProviderName(), provider);
                    result = true;
                } catch (Exception e) {
                    e.Dump();
                }
            }

            foreach (var provider in dynInterface.FilterTypesCompatibleTo<IServicesProvider>(asm).Select(each => dynInterface.Create<IServicesProvider>(each))) {
                try {
                    provider.InitializeProvider(DynamicInterface.Instance, callback);
                    yieldServicesProvider(provider.GetServicesProviderName(), provider);
                    result = true;
                } catch (Exception e) {
                    e.Dump();
                }
            }
            return result;
        }
    }
}