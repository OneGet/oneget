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

namespace Microsoft.OneGet.Implementation {
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Api;
    using Providers;
    using Utility.Extensions;
    using Utility.Plugin;
    using Utility.Versions;

    internal static class Loader {
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile", Justification = "This is a plugin loader. It *needs* to do that.")]
        internal static bool AcquireProviders(string assemblyPath, IRequest request, YieldMetaProvider yieldMetaProvider, YieldPackageProvider yieldPackageProvider, YieldArchiver yieldArchiver, YieldDownloader yieldDownloader) {
            var found = false;
            try {
                var assembly = Assembly.LoadFrom(assemblyPath);
                

                if (assembly == null) {
                    return false;
                }

                var asmVersion = GetAssemblyVersion(assembly);

                var t1 = Task.Factory.StartNew(() => {
                    // process Meta Providers
                    foreach (var metaProviderClass in DynamicInterface.FilterTypesCompatibleTo<IMetaProvider>(assembly)) {
                        try {
                            found = found | AcquireProvidersViaMetaProvider(DynamicInterface.Create<IMetaProvider>(metaProviderClass), yieldMetaProvider, yieldPackageProvider, yieldArchiver, yieldDownloader, asmVersion, request);
                        } catch {
                            // ignore stuff that doesn't load.
                        }
                    }
                }, TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning);

                var t2 = Task.Factory.StartNew(() => {
                    // process Package Providers
                        foreach (var packageProviderClass in DynamicInterface.FilterTypesCompatibleTo<IPackageProvider>(assembly)) {
                        try {
                            found = found | ProcessPackageProvider(DynamicInterface.Create<IPackageProvider>(packageProviderClass), yieldPackageProvider, asmVersion, request);
                        } catch (Exception ex) {
                            // ignore stuff that doesn't load.
                        }
                    } 
                }, TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning);

                var t3 = Task.Factory.StartNew(() => {
                    // Process archiver Providers
                        foreach (var serviceProviderClass in DynamicInterface.FilterTypesCompatibleTo<IArchiver>(assembly)) {
                            try {
                                found = found | ProcessArchiver(DynamicInterface.Create<IArchiver>(serviceProviderClass), yieldArchiver, asmVersion, request);
                            } catch {
                                // ignore stuff that doesn't load.
                            }
                        } 
                }, TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning);

                var t4 = Task.Factory.StartNew(() => {
                    // Process downloader Providers
                        foreach (var serviceProviderClass in DynamicInterface.FilterTypesCompatibleTo<IDownloader>(assembly)) {
                            try {
                                found = found | ProcessDownloader(DynamicInterface.Create<IDownloader>(serviceProviderClass), yieldDownloader, asmVersion, request);
                            } catch {
                                // ignore stuff that doesn't load`
                            }
                        }
                }, TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning);

                t1.Wait();
                t2.Wait();
                t3.Wait();
                t4.Wait();
            } catch (Exception e) {
                e.Dump();
            }
            return found;
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

        private static bool ProcessPackageProvider(IPackageProvider provider, YieldPackageProvider yieldPackageProvider, FourPartVersion asmVersion, IRequest request) {
            try {
                FourPartVersion ver = provider.GetProviderVersion();
                if (yieldPackageProvider(provider.GetPackageProviderName(), provider, ver == 0 ? asmVersion : ver, request)) {
                    // provider.InitializeProvider(request);
                    return true;
                }
            } catch (Exception e) {
                e.Dump();
            }
            return false;
        }

        private static bool ProcessArchiver(IArchiver provider, YieldArchiver yieldArchiver, FourPartVersion asmVersion, IRequest request) {
            try {
                FourPartVersion ver = provider.GetProviderVersion();
                if (yieldArchiver(provider.GetArchiverName(), provider, ver == 0 ? asmVersion : ver, request)) {
                    // provider.InitializeProvider(request);
                    return true;
                }
            } catch (Exception e) {
                e.Dump();
            }
            return false;
        }

        private static bool ProcessDownloader(IDownloader provider, YieldDownloader yieldDownloader, FourPartVersion asmVersion, IRequest request) {
            try {
                FourPartVersion ver = provider.GetProviderVersion();
                if (yieldDownloader(provider.GetDownloaderName(), provider, ver == 0 ? asmVersion : ver, request)) {
                    // provider.InitializeProvider(request);
                    return true;
                }
            } catch (Exception e) {
                e.Dump();
            }
            return false;
        }

        internal static bool AcquireProvidersViaMetaProvider(IMetaProvider provider, YieldMetaProvider yieldMetaProvider, YieldPackageProvider yieldPackageProvider, YieldArchiver yieldArchiver, YieldDownloader yieldDownloader, FourPartVersion asmVersion,
            IRequest request) {
            var found = false;
            var metaProviderName = provider.GetMetaProviderName();
            FourPartVersion metaProviderVersion = provider.GetProviderVersion();
            var reloading = yieldMetaProvider(metaProviderName, provider, (metaProviderVersion == 0 ? asmVersion : metaProviderVersion), request);

            try {
                provider.InitializeProvider(request);
                var metaProvider = provider;
                provider.GetProviderNames().ParallelForEach(name => {
                    // foreach (var name in provider.GetProviderNames()) {
                    var instance = metaProvider.CreateProvider(name);
                    if (instance != null) {
                        // check if it's a Package Provider
                        if (DynamicInterface.IsInstanceCompatible<IPackageProvider>(instance)) {
                            try {
                                found = found | ProcessPackageProvider(DynamicInterface.Create<IPackageProvider>(instance), yieldPackageProvider, asmVersion, request);
                            } catch (Exception e) {
                                e.Dump();
                            }
                        }

                        // check if it's a Services Provider
                        if (DynamicInterface.IsInstanceCompatible<IArchiver>(instance)) {
                            try {
                                found = found | ProcessArchiver(DynamicInterface.Create<IArchiver>(instance), yieldArchiver, asmVersion, request);
                            } catch (Exception e) {
                                e.Dump();
                            }
                        }

                        if (DynamicInterface.IsInstanceCompatible<IDownloader>(instance)) {
                            try {
                                found = found | ProcessDownloader(DynamicInterface.Create<IDownloader>(instance), yieldDownloader, asmVersion, request);
                            } catch (Exception e) {
                                e.Dump();
                            }
                        }
                    }
                });
            } catch (Exception e) {
                e.Dump();
            }
            return found;
        }
    }
}