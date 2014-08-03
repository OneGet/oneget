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
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using Api;
    using Packaging;
    using Providers;
    using Providers.Meta;
    using Providers.Package;
    using Providers.Service;
    using Utility.Extensions;
    using Utility.Platform;
    using Utility.Plugin;
    using Win32;
    using RequestImpl = System.Object;

    /// <summary>
    ///     The Client API is designed for use by installation hosts:
    ///     - OneGet Powershell Cmdlets
    ///     - WMI/OMI Management interfaces
    ///     - DSC Interfaces
    ///     - WiX's Burn
    ///     The Client API provides high-level consumer functions to support SDII functionality.
    /// </summary>
    internal class PackageManagementServiceImplementation : MarshalByRefObject, IPackageManagementService {
        internal const string ProviderPluginLoadFailure = "PROVIDER_PLUGIN_LOAD_FAILURE";
        internal const string Invalidoperation = "InvalidOperation";
        private readonly IDictionary<string, PackageProvider> _packageProviders = new Dictionary<string, PackageProvider>(StringComparer.OrdinalIgnoreCase);
        private readonly IDictionary<string, ServicesProvider> _servicesProviders = new Dictionary<string, ServicesProvider>(StringComparer.OrdinalIgnoreCase);
        private readonly IDictionary<string, IMetaProvider> _metaProviders = new Dictionary<string, IMetaProvider>(StringComparer.OrdinalIgnoreCase);

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

        public IEnumerable<PackageProvider> PackageProviders {
            get {
                return _packageProviders.Values.ByRef();
            }
        }

        private bool _initialized;

        public bool Initialize(RequestImpl request) {
            lock (_lockObject) {
                if (!_initialized) {
                    LoadProviders(request.As<IRequest>());
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

        // well known, built in provider assemblies.
        private readonly string[] _builtInProviders = {
            "Microsoft.OneGet.MetaProvider.PowerShell.dll",
            "Microsoft.OneGet.ServicesProvider.Common.dll",
            "Microsoft.OneGet.PackageProvider.Bootstrap.dll",
        };

        /// <summary>
        ///     This initializes the provider registry with the list of package providers.
        ///     (currently a hardcoded list, soon, registry driven)
        /// </summary>
        /// <param name="request"></param>
        private void LoadProviders(IRequest request) {
            var providerAssemblies = (_initialized ? Enumerable.Empty<string>() : _builtInProviders)
                .Concat(GetProvidersFromRegistry(Registry.LocalMachine, "SOFTWARE\\MICROSOFT\\ONEGET"))
                .Concat(GetProvidersFromRegistry(Registry.CurrentUser, "SOFTWARE\\MICROSOFT\\ONEGET"))
                .Concat(AutoloadedAssemblyLocations.SelectMany(location => {
                    if (Directory.Exists(location)) {
                        return Directory.EnumerateFiles(location, "*.exe").Concat(Directory.EnumerateFiles(location, "*.dll"));
                    }
                    return Enumerable.Empty<string>();
                }));

            // there is no trouble with loading providers concurrently.
            Parallel.ForEach(providerAssemblies, providerAssemblyName => {
                try {
                    request.Verbose("ProviderAssembly: {0} THREAD:{1}".format(providerAssemblyName, Thread.CurrentThread.ManagedThreadId));
                    TryToLoadProviderAssembly(providerAssemblyName, request);
                    request.Verbose("DONE ProviderAssembly: {0} THREAD:{1}".format(providerAssemblyName, Thread.CurrentThread.ManagedThreadId));

                } catch {
                    request.Error(ProviderPluginLoadFailure, Invalidoperation, ProviderPluginLoadFailure, providerAssemblyName);
                }
            });
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

        public IEnumerable<PackageSource> GetAllSourceNames(RequestImpl requestImpl) {
            return _packageProviders.Values.SelectMany(each => each.ResolvePackageSources(requestImpl)).ByRef();
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

        public IEnumerable<PackageProvider> SelectProviders(string providerName, RequestImpl requestImpl) {
            if (providerName.Is()) {
                // strict name match for now.
                if (_packageProviders.ContainsKey(providerName)) {
                    return _packageProviders[providerName].SingleItemAsEnumerable().ByRef();
                }

                if (requestImpl != null) {
                    // if the end user requested a provider that's not there. perhaps the bootstrap provider can find it.
                    if (RequirePackageProvider(null, providerName, "0.0.0.1", requestImpl)) {
                        // seems to think we found it.
                        if (_packageProviders.ContainsKey(providerName)) {
                            return _packageProviders[providerName].SingleItemAsEnumerable().ByRef();
                        }
                    }
                    var hostApi = requestImpl.As<IHostApi>();

                    // warn the user that that provider wasn't found.
                    hostApi.Warning(hostApi.GetMessageString(Constants.UnknownProvider).format(providerName));
                }
                return Enumerable.Empty<PackageProvider>().ByRef();
            }

            return PackageProviders.ByRef();
        }

        private bool AddMetaProvider(string name, IMetaProvider provider, ulong version, IRequest request) {
            // wrap this in a caller-friendly wrapper
            if (_metaProviders.ContainsKey(name)) {
                // Meta Providers can't be replaced at this point
                return false;
            }
            _metaProviders.AddOrSet(name, provider);
            return true;
        }

        private bool AddPackageProvider(string name, IPackageProvider provider, ulong version, IRequest request) {
            // wrap this in a caller-friendly wrapper
            if (_packageProviders.ContainsKey(name)) {
                if (version > _packageProviders[name].Version) {
                    // remove the old provider first.
                    // todo: this won't remove the plugin domain and unload the code yet
                    // we'll have to do that later.

                    _packageProviders.Remove(name);
                } else {
                    return false;
                }
            }
            _packageProviders.AddOrSet(name, new PackageProvider(provider) {
                Version = version
            }).Initialize(request);

            return true;
        }

        private bool AddServicesProvider(string name, IServicesProvider provider, ulong version, IRequest request) {
            if (_servicesProviders.ContainsKey(name)) {
                if (version > _servicesProviders[name].Version) {
                    // remove the old provider first.
                    // todo: this won't remove the plugin domain and unload the code yet
                    // we'll have to do that later.

                    _servicesProviders.Remove(name);
                } else {
                    return false;
                }
            }
            _servicesProviders.AddOrSet(name, new ServicesProvider(provider) {
                Version = version
            }).Initialize(request);
            return true;
        }

        /// <summary>
        ///     Searches for the assembly, interrogates it for it's providers and then proceeds to load
        /// </summary>
        /// <param name="request"></param>
        /// <param name="providerAssemblyName"></param>
        /// <returns></returns>
        private void TryToLoadProviderAssembly(string providerAssemblyName, IRequest request) {
            // find all the matches for the assembly specified, order by version (descending)

            var assemblyPath = FindAssembly(providerAssemblyName);

            if (assemblyPath == null) {
                return;
            }

            var pluginDomain = CreatePluginDomain(assemblyPath);

            pluginDomain.InvokeFunc(Loader.AcquireProviders, assemblyPath, request, (YieldMetaProvider)AddMetaProvider, (YieldPackageProvider)AddPackageProvider, (YieldServicesProvider)AddServicesProvider);
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
        /// <param name="primaryAssemblyPath"></param>
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

        internal IEnumerable<string> AutoloadedAssemblyLocations {
            get {
                return new[] {
                    SystemAssemblyLocation, UserAssemblyLocation
                };
            }
        }

        internal string UserAssemblyLocation {
            get {
                var basepath = KnownFolders.GetFolderPath(KnownFolder.LocalApplicationData);
                if (string.IsNullOrEmpty(basepath)) {
                    return null;
                }
                var path = Path.Combine(basepath, "OneGet", "ProviderAssemblies");
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        internal string SystemAssemblyLocation {
            get {
                var basepath = KnownFolders.GetFolderPath(KnownFolder.CommonApplicationData);
                if (string.IsNullOrEmpty(basepath)) {
                    return null;
                }
                var path = Path.Combine(basepath, "OneGet", "ProviderAssemblies");
                if (AdminPrivilege.IsElevated && !Directory.Exists(path)) {
                    var ds = new DirectorySecurity();

                    var everyone = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
                    var admins = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
                    ds.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.ReadAndExecute, AccessControlType.Allow));
                    ds.AddAccessRule(new FileSystemAccessRule(admins, FileSystemRights.Write, AccessControlType.Allow));

                    Directory.CreateDirectory(path, ds);
                }

                return path;
            }
        }

        private static int _lastCallCount = 0;
        private static HashSet<string> _providersTriedThisCall;

        public bool RequirePackageProvider(string requestor, string packageProviderName, string minimumVersion, RequestImpl requestImpl) {
            // check if the package provider is already installed
            if (_packageProviders.ContainsKey(packageProviderName)) {
                var current = _packageProviders[packageProviderName].Version;
                if (current >= minimumVersion) {
                    return true;
                }
            }

            var request = requestImpl.As<IRequest>();

            var currentCallCount = request.CallCount();

            if (_lastCallCount >= currentCallCount) {
                // we've already been here this call.

                // are they asking for the same provider again?
                if (_providersTriedThisCall.Contains(packageProviderName)) {
                    request.Debug("Already tried this call.");
                    return false;
                }
                // remember this in case we come back again.
                _providersTriedThisCall.Add(packageProviderName);
            } else {
                _lastCallCount = currentCallCount;
                _providersTriedThisCall = new HashSet<string> {
                    packageProviderName
                };
            }

            if (!request.IsInteractive()) {
                request.Debug("Skipping RequirePackageProvider due to not interactive");
                // interactive indicates that the host can respond to queries -- this doesn't happen
                // in powershell during tab-completion.
                return false;
            }

            // no?
            // ask the bootstrap provider if there is a package provider with that name available.
            var bootstrap = _packageProviders["Bootstrap"];
            if (bootstrap == null) {
                request.Debug("Skipping RequirePackageProvider due to missing bootstrap provider");
                return false;
            }

            var pkg = bootstrap.FindPackage(packageProviderName, null, minimumVersion, null, 0, requestImpl).ToArray();
            if (pkg.Length == 1) {
                // Yeah? Install it.
                var package = pkg[0];
                var metaWithProviderType = pkg[0].Meta.FirstOrDefault(each => each.ContainsKey("providerType"));
                var providerType = metaWithProviderType == null ? "unknown" : metaWithProviderType["providerType"];
                var destination = providerType == "assembly" ? (AdminPrivilege.IsElevated ? SystemAssemblyLocation : UserAssemblyLocation) : string.Empty;
                var link = package.Links.FirstOrDefault(each => each.Relationship == "installationmedia");
                var location = string.Empty;
                if (link != null) {
                    location = link.HRef;
                }

                // what can't find an installationmedia link? 
                // todo: what should we say here?
                if (request.ShouldBootstrapProvider(requestor, pkg[0].Name, pkg[0].Version, providerType, location, destination)) {
                    var newRequest = requestImpl.Extend<IRequest>(new {
                        GetOptionValues = new Func<int, string, IEnumerable<string>>((category, key) => {
                            if (key == "DestinationPath") {
                                return new string[] {
                                    destination
                                };
                            }
                            return new string[0];
                        })
                    });
                    var packagesInstalled = bootstrap.InstallPackage(pkg[0], newRequest).LastOrDefault();
                    if (packagesInstalled == null) {
                        // that's sad.
                        request.Error(Constants.FailedProviderBootstrap, Invalidoperation, package.Name, package.Name);
                        return false;
                    }
                    // so it installed something
                    // we must tell the plugin loader to reload the plugins again.
                    LoadProviders(request);
                    return true;
                }
            }

            return false;
        }
    }
}