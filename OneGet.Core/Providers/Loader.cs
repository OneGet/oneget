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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Api;
    using Meta;
    using Package;
    using Service;
    using Utility.Extensions;
    using Utility.Plugin;
    using Utility.Versions;

    internal delegate bool YieldPackageProvider(string name, IPackageProvider instnace, ulong version, IRequest request);
    internal delegate bool YieldMetaProvider(string name, IMetaProvider instnace, ulong version, IRequest request);
    internal delegate bool YieldServicesProvider(string name, IServicesProvider instnace, ulong version, IRequest request);

    internal static class Loader {
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile", Justification = "This is a plugin loader. It *needs* to do that.")]
        internal static bool AcquireProviders(string assemblyPath, IRequest request, YieldMetaProvider yieldMetaProvider, YieldPackageProvider yieldPackageProvider, YieldServicesProvider yieldServicesProvider) {
            try {
                var assembly = Assembly.LoadFile(assemblyPath);

                if (assembly == null) {
                    return false;
                }
                
                var asmVersion = GetAssemblyVersion(assembly);

                // process Meta Providers
                foreach (var metaProviderClass in DynamicInterface.Instance.FilterTypesCompatibleTo<IMetaProvider>(assembly)) {
                    AcquireProvidersViaMetaProvider(DynamicInterface.Instance.Create<IMetaProvider>(metaProviderClass), yieldMetaProvider, yieldPackageProvider, yieldServicesProvider, asmVersion, request);
                }

                // process Package Providers
                foreach (var packageProviderClass in DynamicInterface.Instance.FilterTypesCompatibleTo<IPackageProvider>(assembly)) {
                    ProcessPackageProvider(DynamicInterface.Instance.Create<IPackageProvider>(packageProviderClass), yieldPackageProvider, asmVersion, request);
                }

                // Process Services Providers
                foreach (var serviceProviderClass in DynamicInterface.Instance.FilterTypesCompatibleTo<IServicesProvider>(assembly)) {
                    ProcessServicesProvider(DynamicInterface.Instance.Create<IServicesProvider>(serviceProviderClass), yieldServicesProvider, asmVersion, request);
                }
            } catch (Exception e) {
                e.Dump();
            }
            return true;
        }

        private static FourPartVersion GetAssemblyVersion(Assembly asm) {
            FourPartVersion result = 0;

            var attribute = asm.GetCustomAttributes(typeof (AssemblyVersionAttribute), true).FirstOrDefault() as AssemblyVersionAttribute;
            if (attribute != null) {
                result = attribute.Version;
            }

            if (result == 0) {
                // what? No assembly version?
                // fallback to the file version of the assembly
                var assemblyLocation = asm.Location;
                if (assemblyLocation.Is() && File.Exists(assemblyLocation)) {
                    result = FileVersionInfo.GetVersionInfo(assemblyLocation);
                    if (result == 0) {
                        // no file version either?
                        // use the date I guess.
                        try {
                            result = new FileInfo(assemblyLocation).LastWriteTime;
                        } catch {
                        }
                    }
                }

                if (result == 0) {
                    // still no version? 
                    // I give up. call it 0.0.0.1
                    result = "0.0.0.1";
                }
            }
            return result;
        }

        private static void ProcessPackageProvider(IPackageProvider provider, YieldPackageProvider yieldPackageProvider, FourPartVersion asmVersion, IRequest request) {
            try {
                provider.InitializeProvider(DynamicInterface.Instance, request);
                FourPartVersion ver = provider.GetProviderVersion();
                yieldPackageProvider(provider.GetPackageProviderName(), provider, ver == 0 ? asmVersion : ver, request);
            } catch (Exception e) {
                e.Dump();
            }
        }

        private static void ProcessServicesProvider(IServicesProvider provider, YieldServicesProvider yieldServicesProvider, FourPartVersion asmVersion, IRequest request) {
            try {
                provider.InitializeProvider(DynamicInterface.Instance, request);
                FourPartVersion ver = provider.GetProviderVersion();
                yieldServicesProvider(provider.GetServicesProviderName(), provider, ver == 0 ? asmVersion : ver, request);

            } catch (Exception e) {
                e.Dump();
            }
        }

        internal static void AcquireProvidersViaMetaProvider(IMetaProvider provider, YieldMetaProvider yieldMetaProvider, YieldPackageProvider yieldPackageProvider, YieldServicesProvider yieldServicesProvider, FourPartVersion asmVersion, IRequest request) {
            var metaProviderName = provider.GetMetaProviderName();
            FourPartVersion metaProviderVersion = provider.GetProviderVersion();
            bool reloading = yieldMetaProvider(metaProviderName, provider, (metaProviderVersion == 0 ? asmVersion : metaProviderVersion), request);

            try {
                provider.InitializeProvider(DynamicInterface.Instance, request);
                var metaProvider = provider;
                Parallel.ForEach(provider.GetProviderNames(), name => {
                    // foreach (var name in provider.GetProviderNames()) {
                    var instance = metaProvider.CreateProvider(name);
                    if (instance != null) {
                        // check if it's a Package Provider
                        if (DynamicInterface.Instance.IsInstanceCompatible<IPackageProvider>(instance)) {
                            try {
                                ProcessPackageProvider(DynamicInterface.Instance.Create<IPackageProvider>(instance), yieldPackageProvider, asmVersion, request);
                                
                            } catch (Exception e) {
                                e.Dump();
                            }
                        }

                        // check if it's a Services Provider
                        if (DynamicInterface.Instance.IsInstanceCompatible<IServicesProvider>(instance)) {
                            try {
                                ProcessServicesProvider(DynamicInterface.Instance.Create<IServicesProvider>(instance), yieldServicesProvider, asmVersion, request);
                            } catch (Exception e) {
                                e.Dump();
                            }
                        }
                    }
                });
            } catch (Exception e) {
                e.Dump();
            }
        }
    }
}