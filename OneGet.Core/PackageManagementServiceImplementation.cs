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
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.AccessControl;
    using System.Threading.Tasks;
    using Api;
    using Extensions;
    using Packaging;
    using Plugin;
    using Providers;
    using Providers.Package;
    using Providers.Service;
    using Win32;
    using Callback = System.Object;

    /// <summary>
    ///     The Client API is designed for use by installation hosts:
    ///     - OneGet Powershell Cmdlets
    ///     - WMI/OMI Management interfaces
    ///     - DSC Interfaces
    ///     - WiX's Burn
    ///     The Client API provides high-level consumer functions to support SDII functionality.
    /// </summary>
    internal class PackageManagementServiceImplementation : MarshalByRefObject, IPackageManagementService {
        private readonly IDictionary<string, PackageProvider> _packageProviders = new Dictionary<string, PackageProvider>();
        private readonly IDictionary<string, ServicesProvider> _servicesProviders = new Dictionary<string, ServicesProvider>();

        private readonly object _lockObject = new object();

        private AggregateServicesProvider _servicesProvider;

        internal AggregateServicesProvider ServicesProvider {
            get {
                if (_servicesProvider == null) {
                    _servicesProvider = new AggregateServicesProvider(_servicesProviders);
                }
                return _servicesProvider;
            }
        }

        public IEnumerable<string> PackageProviderNames {
            get {
                return _packageProviders.Keys;
            }
        }

        internal static string FormatMessage(string message, IEnumerable<object> parameters) {
            // look up message code first (ie, MISC001).
            // 
            // 
            // if we have a match, use that as the mesage to format with
            // otherwise, just use the given message as the format string.
            return message;
        }

        public IEnumerable<PackageProvider> PackageProviders {
            get {
                return _packageProviders.Values.ByRef();
            }
        }

        private bool _initialized;

        public bool Initialize(Callback callback, bool userInteractionPermitted) {
            var request = callback.As<IRequest>();

            request.Debug("starting Initialize");
            // var request = _dynamicInterface.Create<IHostAndCoreAPIs>(callback);

            lock (_lockObject) {
                if (!_initialized) {
                    LoadProviders(callback);

                    _initialized = true;
                }
            }
            return _initialized;
        }

        internal PackageManagementServiceImplementation() {
        }

        public override object InitializeLifetimeService() {
            return null;
        }

        /// <summary>
        ///     This initializes the provider registry with the list of package providers.
        ///     (currently a hardcoded list, soon, registry driven)
        /// </summary>
        /// <param name="callback"></param>
        private void LoadProviders(Callback callback) {
            var request = callback.As<IRequest>();

            request.Debug("Staring LoadProviders");

            // todo: load provider assembly list from the registry.
            IEnumerable<string> providerAssemblies = new string[] {
                "Microsoft.OneGet.MetaProvider.PowerShell.dll",
                "Microsoft.OneGet.ServicesProvider.Common.dll",
                // "OneGet.PackageProvider.Bootstrap.dll",  // M2
                // "OneGet.PackageProvider.Chocolatey.dll",  // M1
                "OneGet.PackageProvider.NuGet.dll",  // testing 
                "NuGet-AnyCPU.exe",
            }.Concat(GetProvidersFromRegistry(Registry.LocalMachine, "SOFTWARE\\MICROSOFT\\ONEGET")).Concat(GetProvidersFromRegistry(Registry.CurrentUser, "SOFTWARE\\MICROSOFT\\ONEGET"));


            // there is no trouble with loading providers concurrently.
            Parallel.ForEach(providerAssemblies, providerAssemblyName => {
                // foreach( var providerAssemblyName in providerAssemblies ) {
                try {
                    if (TryToLoadProviderAssembly(callback, providerAssemblyName)) {
                        request.Debug("Loading Provider Assembly {0}".format(providerAssemblyName));
                    } else {
                        request.Debug("Failed to load any providers {0}".format(providerAssemblyName));
                    }
                } catch (Exception e) {
                    request.ExceptionThrown(e.GetType().Name, e.Message, e.StackTrace);
                }
            });

            foreach (var provider in _packageProviders.Values) {
                provider.Initialize(callback);
            }
            foreach (var provider in _servicesProviders.Values) {
                provider.Initialize(callback);
            }
        }

        private static IEnumerator<string> GetProvidersFromRegistry(RegistryKey registryKey, string p) {
            RegistryKey key;
            try {
                key = registryKey.OpenSubKey(p, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.ReadKey);
            } catch {
                yield break;
            }

            if (key == null) {
                yield break;
            }

            foreach (var name in key.GetValueNames()) {
                yield return key.GetValue(name).ToString();
            }
        }


        public IEnumerable<PackageSource> GetAllSourceNames(Callback callback) {
            return _packageProviders.Values.SelectMany(each => each.ResolvePackageSources(callback)).ByRef();
        }

        public IEnumerable<string> ProviderNames {
            get {
                return _packageProviders.Keys.ByRef();
            }
        }

        public IEnumerable<PackageProvider> SelectProvidersWithFeature(string featureName) {
            return _packageProviders.Values.Where(each => each.Features.ContainsKey(featureName)).ByRef();
        }

        public IEnumerable<PackageProvider> SelectProvidersWithFeature(string featureName, string value) {
            return _packageProviders.Values.Where(each => each.Features.ContainsKey(featureName) && each.Features[featureName].Contains(value)).ByRef();
        }

        public IEnumerable<PackageProvider> SelectProviders(string providerName) {
            if (providerName.Is()) {
                // strict name match for now.
                return PackageProviders.Where(each => each.Name.Equals(providerName, StringComparison.CurrentCultureIgnoreCase)).ByRef();
                ;
            }

            return PackageProviders.ByRef();
        }

        private bool AddPackageProvider(string name, IPackageProvider provider) {
            // wrap this in a caller-friendly wrapper 
            if (_packageProviders.ContainsKey(name)) {
                return false;
            }
            _packageProviders.AddOrSet(name, new PackageProvider(provider));
            return true;
        }

        private bool AddServicesProvider(string name, IServicesProvider provider) {
            if (_servicesProviders.ContainsKey(name)) {
                return false;
            }
            _servicesProviders.AddOrSet(name, new ServicesProvider(provider));
            return true;
        }

        /// <summary>
        ///     Searches for the assembly, interrogates it for it's providers and then proceeds to load
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="providerAssemblyName"></param>
        /// <returns></returns>
        private bool TryToLoadProviderAssembly(Callback callback, string providerAssemblyName) {
            // find all the matches for the assembly specified, order by version (descending)

            var assemblyPath = FindAssembly(providerAssemblyName);

            if (assemblyPath == null) {
                return false;
            }

            var pluginDomain = CreatePluginDomain(assemblyPath);

            return pluginDomain.InvokeFunc(Loader.AcquireProviders, assemblyPath, callback.As<ICoreApi>(),
                (Func<string, IPackageProvider,bool>)(AddPackageProvider),
                (Func<string, IServicesProvider,bool>)(AddServicesProvider)
                );
        }

#if AFTER_CTP
        private  void UnloadAssembly(Assembly assembly) {
            PluginDomain pd = null;
            try {
                lock (_domains) {
                    pd = _domains[assembly];
                    _domains.Remove(assembly);
                }
            } catch (Exception e) {
                e.Dump();
            }
            if (pd != null) {
                ((IDisposable)pd).Dispose();
            }
            pd = null;
        }
#endif

        /// <summary>
        ///     PROTOTYPE - assembly/provider loader.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private PluginDomain CreatePluginDomain(string primaryAssemblyPath) {
            try {
                // this needs to load the assembly in it's own domain
                // so that we can drop them when necessary.
                var name = Path.GetFileNameWithoutExtension(primaryAssemblyPath) ?? primaryAssemblyPath;
                var pd = new PluginDomain(string.Format(CultureInfo.CurrentCulture, "PluginDomain [{0}]", name.Substring(name.LastIndexOf('.') + 1)));

                // inject this assembly into the target appdomain.
                pd.LoadFileWithReferences(Assembly.GetExecutingAssembly().Location);

                return pd;
            } catch (Exception e) {
                e.Dump();
            }
            return null;
        }

        /// <summary>
        ///     PROTOTYPE -- extremely simplified assembly locator.
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        private string FindAssembly(string assemblyName) {
            try {
                string fullPath;
                // is the name given a strong name?
                if (assemblyName.Contains(',')) {
                    // looks like a strong name
                    // todo: not there yet...
                    return null;
                }

                // is it a path?
                if (assemblyName.Contains('\\') || assemblyName.Contains('/') || assemblyName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase) || assemblyName.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase)) {
                    fullPath = Path.GetFullPath(assemblyName);
                    if (File.Exists(fullPath)) {
                        return fullPath;
                    }
                    if (File.Exists(fullPath + ".dll")) {
                        return fullPath;
                    }

                    // lets see if the assembly name is in the same directory as the current assembly...
                    try {
                        fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assemblyName);
                        if (File.Exists(fullPath)) {
                            return fullPath;
                        }
                        if (File.Exists(fullPath + ".dll")) {
                            return fullPath;
                        }
                    } catch {
                    }

                }
                // must be just just a plain name.

                // todo: search the GAC too?

                // search the local folder.
                fullPath = Path.GetFullPath(assemblyName + ".dll");
                if (File.Exists(fullPath)) {
                    return fullPath;
                }

                // try next to where we are.
                fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assemblyName + ".dll");
                if (File.Exists(fullPath)) {
                    return fullPath;
                }
            } catch (Exception e) {
                e.Dump();
            }
            return null;
        }
    }
}