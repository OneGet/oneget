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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.AccessControl;
    using System.Threading.Tasks;
    using Api;
    using Packaging;
    using Providers;
    using Utility.Collections;
    using Utility.Extensions;
    using Utility.Platform;
    using Utility.Plugin;
    using Utility.Versions;
    using Win32;
    using Directory = System.IO.Directory;
    using File = System.IO.File;

    /// <summary>
    ///     The Client API is designed for use by installation hosts:
    ///     - OneGet Powershell Cmdlets
    ///     - WMI/OMI Management interfaces
    ///     - DSC Interfaces
    ///     - WiX's Burn
    ///     The Client API provides high-level consumer functions to support SDII functionality.
    /// </summary>
    internal class PackageManagementService : IPackageManagementService {
        private static readonly HashSet<string> _excludes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location), // already in autload list
            "CSharpTest.Net.RpcLibrary", // doesn't have any
            "Microsoft.OneGet.Utility", // doesn't have any
            "Microsoft.OneGet.Utility.PowerShell", // doesn't have any
            "Microsoft.PowerShell.OneGet", // doesn't have any
            "Microsoft.Web.XmlTransform", // doesn't have any
            "NuGet.Core", // doesn't have any
            "OneGet.PowerShell.Module.Test", // doesn't have any
            "System.Management.Automation", // doesn't have any
            "xunit", // doesn't have any
            "xunit.extensions", // doesn't have any
            "CustomCodeGenerator", // doesn't have any
            "NuGet", // doesn't have any
        };

        private static int _lastCallCount;
        private static HashSet<string> _providersTriedThisCall;
        private string[] _bootstrappableProviderNames;
        private bool _initialized;
        // well known, built in provider assemblies.
        private readonly string[] _defaultProviders = {
            Path.GetFullPath(Assembly.GetExecutingAssembly().Location), // load the providers from this assembly
            "Microsoft.OneGet.MetaProvider.PowerShell.dll"
        };

        private readonly object _lockObject = new object();
        private readonly IDictionary<string, IMetaProvider> _metaProviders = new Dictionary<string, IMetaProvider>(StringComparer.OrdinalIgnoreCase);
        private readonly IDictionary<string, PackageProvider> _packageProviders = new Dictionary<string, PackageProvider>(StringComparer.OrdinalIgnoreCase);
        internal readonly IDictionary<string, Archiver> Archivers = new Dictionary<string, Archiver>(StringComparer.OrdinalIgnoreCase);
        internal readonly IDictionary<string, Downloader> Downloaders = new Dictionary<string, Downloader>(StringComparer.OrdinalIgnoreCase);

        internal string[] BootstrappableProviderNames {
            get {
                return _bootstrappableProviderNames ?? new string[0];
            }
            set {
                if (_bootstrappableProviderNames.IsNullOrEmpty()) {
                    _bootstrappableProviderNames = value;
                }
            }
        }

        internal IEnumerable<string> AutoloadedAssemblyLocations {
            get {
                return new[] {
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), SystemAssemblyLocation, UserAssemblyLocation
                };
            }
        }

        internal string UserAssemblyLocation {
            get {
                var basepath = KnownFolders.GetFolderPath(KnownFolder.LocalApplicationData);
                if (string.IsNullOrWhiteSpace(basepath)) {
                    return null;
                }
                var path = Path.Combine(basepath, "OneGet", "ProviderAssemblies");
                if (!Directory.Exists(path)) {
                    try {
                        Directory.CreateDirectory(path);
                    } catch {
                        // if it can't be created, it's not the end of the world.
                    }
                }
                return path;
            }
        }

        internal string SystemAssemblyLocation {
            get {
                var basepath = KnownFolders.GetFolderPath(KnownFolder.ProgramFiles);
                if (string.IsNullOrWhiteSpace(basepath)) {
                    return null;
                }
                var path = Path.Combine(basepath, "OneGet", "ProviderAssemblies");

                try {
                    if (!Directory.Exists(path)) {
                        Directory.CreateDirectory(path);
                    }
                } catch {
                    // ignore non-existant directory for now.
                }
                return path;
            }
        }

        public IEnumerable<PackageProvider> PackageProviders {
            get {
                return _packageProviders.Values;
            }
        }

        public bool Initialize(IHostApi request) {
            lock (_lockObject) {
                if (!_initialized) {
                    LoadProviders(request);
                    _initialized = true;
                }
            }
            return _initialized;
        }

        public int Version {
            get {
                return Constants.OneGetVersion;
            }
        }

        public IEnumerable<string> ProviderNames {
            get {
                return _packageProviders.Keys;
            }
        }

        public IEnumerable<string> AllProviderNames {
            get {
                if (BootstrappableProviderNames.IsNullOrEmpty()) {
                    return _packageProviders.Keys;
                }

                return _packageProviders.Keys.Union(BootstrappableProviderNames);
            }
        }

        public IEnumerable<PackageProvider> SelectProvidersWithFeature(string featureName) {
            return _packageProviders.Values.Where(each => each.Features.ContainsKey(featureName));
        }

        public IEnumerable<PackageProvider> SelectProvidersWithFeature(string featureName, string value) {
            return _packageProviders.Values.Where(each => each.Features.ContainsKey(featureName) && each.Features[featureName].Contains(value));
        }

        public IEnumerable<PackageProvider> SelectProviders(string providerName, IHostApi hostApi) {
            if (!string.IsNullOrWhiteSpace(providerName)) {
                // match with wildcards
                var results = _packageProviders.Values.Where(each => each.ProviderName.IsWildcardMatch(providerName)).ReEnumerable();
                if (results.Any()) {
                    return results;
                }
                if (hostApi != null && !providerName.ContainsWildcards()) {
                    // if the end user requested a provider that's not there. perhaps the bootstrap provider can find it.
                    if (RequirePackageProvider(null, providerName, Constants.MinVersion, hostApi)) {
                        // seems to think we found it.
                        if (_packageProviders.ContainsKey(providerName)) {
                            return _packageProviders[providerName].SingleItemAsEnumerable();
                        }
                    }

                    // warn the user that that provider wasn't found.
                    hostApi.Warning(hostApi.FormatMessageString(Constants.Messages.UnknownProvider, providerName));
                }
                return Enumerable.Empty<PackageProvider>();
            }

            return PackageProviders;
        }

        public IEnumerable<SoftwareIdentity> FindPackageByCanonicalId(string packageId, IHostApi hostApi) {
            if (Uri.IsWellFormedUriString(packageId, UriKind.Absolute)) {
                var pkgId = new Uri(packageId);
                var segments = pkgId.Segments;
                if (segments.Length > 0) {
                    var provider = SelectProviders(pkgId.Scheme, hostApi).FirstOrDefault();
                    if (provider != null) {
                        var name = segments[0].Trim('/','\\');
                        var version = (segments.Length > 1) ? segments[1] : null;
                        var source = pkgId.Fragment.TrimStart('#');
                        var sources = (string.IsNullOrWhiteSpace(source) ? hostApi.Sources : source.SingleItemAsEnumerable()).ToArray();
                        
                        var host = new object[] {
                            new { GetSources = new Func<IEnumerable<string>>(() => sources), },
                            hostApi,
                        }.As<IHostApi>();

                        return provider.FindPackage(name, version, null, null, 0, host).Select( each => {each.Status = Constants.PackageStatus.Dependency; return each;}).ReEnumerable();
                    }
                }
            }
            return new SoftwareIdentity[0];
        }

        public bool RequirePackageProvider(string requestor, string packageProviderName, string minimumVersion, IHostApi hostApi) {
            // check if the package provider is already installed
            if (_packageProviders.ContainsKey(packageProviderName)) {
                var current = _packageProviders[packageProviderName].Version;
                if (current >= minimumVersion) {
                    return true;
                }
            }

            var currentCallCount = hostApi.CallCount;

            if (_lastCallCount >= currentCallCount) {
                // we've already been here this call.

                // are they asking for the same provider again?
                if (_providersTriedThisCall.Contains(packageProviderName)) {
                    hostApi.Debug("Skipping RequirePackageProvider -- tried once this call previously.");
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

            if (!hostApi.IsInteractive) {
                hostApi.Debug("Skipping RequirePackageProvider due to not interactive");
                // interactive indicates that the host can respond to queries -- this doesn't happen
                // in powershell during tab-completion.
                return false;
            }

            // no?
            // ask the bootstrap provider if there is a package provider with that name available.
            var bootstrap = _packageProviders["Bootstrap"];
            if (bootstrap == null) {
                hostApi.Debug("Skipping RequirePackageProvider due to missing bootstrap provider");
                return false;
            }

            var pkg = bootstrap.FindPackage(packageProviderName, null, minimumVersion, null, 0, hostApi).ToArray();
            if (pkg.Length == 1) {
                // Yeah? Install it.
                var package = pkg[0];
                var metaWithProviderType = pkg[0].Meta.FirstOrDefault(each => each.ContainsKey("providerType"));
                var providerType = metaWithProviderType == null ? "unknown" : metaWithProviderType.GetAttribute("providerType");
                var destination = providerType == "assembly" ? (AdminPrivilege.IsElevated ? SystemAssemblyLocation : UserAssemblyLocation) : string.Empty;
                var link = package.Links.FirstOrDefault(each => each.Relationship == "installationmedia");
                var location = string.Empty;
                if (link != null) {
                    location = link.HRef.ToString();
                }

                // what can't find an installationmedia link?
                // todo: what should we say here?
                if (hostApi.ShouldBootstrapProvider(requestor, pkg[0].Name, pkg[0].Version, providerType, location, destination)) {
                    var newRequest = hostApi.Extend<IHostApi>(new {
                        GetOptionValues = new Func<string, IEnumerable<string>>(key => {
                            if (key == "DestinationPath") {
                                return new[] {
                                    destination
                                };
                            }
                            return new string[0];
                        })
                    });
                    var packagesInstalled = bootstrap.InstallPackage(pkg[0], newRequest).LastOrDefault();
                    if (packagesInstalled == null) {
                        // that's sad.
                        hostApi.Error(Constants.Messages.FailedProviderBootstrap, ErrorCategory.InvalidOperation.ToString(), package.Name, hostApi.FormatMessageString(Constants.Messages.FailedProviderBootstrap, package.Name));
                        return false;
                    }
                    // so it installed something
                    // we must tell the plugin loader to reload the plugins again.
                    LoadProviders(hostApi);
                    return true;
                }
            }

            return false;
        }

        private bool IsExcluded(string assemblyPath) {
            return _excludes.Contains(Path.GetFileNameWithoutExtension(assemblyPath));
        }

        /// <summary>
        ///     This initializes the provider registry with the list of package providers.
        ///     (currently a hardcoded list, soon, registry driven)
        /// </summary>
        /// <param name="request"></param>
        internal void LoadProviders(IHostApi request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var providerAssemblies = (_initialized ? Enumerable.Empty<string>() : _defaultProviders)
                .Concat(GetProvidersFromRegistry(Registry.LocalMachine, "SOFTWARE\\MICROSOFT\\ONEGET"))
                .Concat(GetProvidersFromRegistry(Registry.CurrentUser, "SOFTWARE\\MICROSOFT\\ONEGET"))
                .Concat(AutoloadedAssemblyLocations.SelectMany(location => {
                    if (Directory.Exists(location)) {
                        return Directory.EnumerateFiles(location).Where(each => !IsExcluded(each) && (each.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase) || each.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase)));
                    }
                    return Enumerable.Empty<string>();
                }));

#if DEEP_DEBUG
            providerAssemblies = providerAssemblies.ToArray();

            foreach (var each in providerAssemblies) {
                request.Debug("possible assembly: {0}".format(each));
            }
#endif

            // find modules that have manifests 
            // todo: expand this out to validate the assembly is ok for this instance of OneGet.
            providerAssemblies = providerAssemblies.Where(each => Manifest.LoadFrom(each).Any(manifest => Swidtag.IsSwidtag(manifest) && new Swidtag(manifest).IsApplicable(new Hashtable())));

            // add inbox assemblies (don't require manifests, because they are versioned with the core)
            
#if !COMMUNITY_BUILD
            // todo: these should just be strong-named references. for now, just load them from the same directory.
            providerAssemblies = providerAssemblies.Concat(new[] {
                Path.Combine( baseDir, "Microsoft.OneGet.MetaProvider.PowerShell.dll"),
                Path.Combine( baseDir, "Microsoft.OneGet.ArchiverProviders.dll" ),
                Path.Combine( baseDir, "Microsoft.OneGet.CoreProviders.dll"),
                Path.Combine( baseDir, "Microsoft.OneGet.MsuProvider.dll"),
#if !CORE_CLR
                // can't load these providers here.
                Path.Combine( baseDir, "Microsoft.OneGet.MsiProvider.dll"),
#endif
            });
#endif 

#if DEEP_DEBUG
            providerAssemblies = providerAssemblies.ToArray();

            foreach (var each in providerAssemblies) {
                request.Debug("possible assembly with manifest: {0}".format(each));
            }
#endif

            providerAssemblies = providerAssemblies.OrderByDescending(each => {
                try {
                    // try to get a version from the file first
                    return (ulong)(FourPartVersion)FileVersionInfo.GetVersionInfo(each);
                } catch {
                    // otherwise we can't make a distinction.
                    return (ulong)0;
                }
            });
            providerAssemblies = providerAssemblies.Distinct(new PathEqualityComparer(PathCompareOption.FileWithoutExtension));

#if BEFORE_WE_HAD_MANIFESTS
            // hack to make sure we don't load the old version of the nuget provider
            // when we have the ability to examine a plugin without dragging it into the
            // primary appdomain, this won't be needed.
            FourPartVersion minimumnugetversion = "2.8.3.6";

            providerAssemblies = providerAssemblies.Where(assemblyFile => {
                try {
                    if ("nuget-anycpu".EqualsIgnoreCase(Path.GetFileNameWithoutExtension(assemblyFile)) && ((FourPartVersion)FileVersionInfo.GetVersionInfo(assemblyFile)) < minimumnugetversion) {
                        return false;
                    }
                } catch {
                }
                return true;
            });
#endif

            // there is no trouble with loading providers concurrently.
#if DEBUG
            providerAssemblies.SerialForEach(providerAssemblyName => {
#else
            providerAssemblies.ParallelForEach(providerAssemblyName => {
#endif
                try {
                    request.Debug(request.FormatMessageString("Trying provider assembly: {0}", providerAssemblyName));
                    if (TryToLoadProviderAssembly(providerAssemblyName, request)) {
                        request.Debug(request.FormatMessageString("SUCCESS provider assembly: {0}", providerAssemblyName));
                    } else {
                        request.Debug(request.FormatMessageString("FAILED provider assembly: {0}", providerAssemblyName));
                    }
                } catch {
                    request.Error(Constants.Messages.ProviderPluginLoadFailure, ErrorCategory.InvalidOperation.ToString(), providerAssemblyName, request.FormatMessageString(Constants.Messages.ProviderPluginLoadFailure, providerAssemblyName));
                }
            });
#if DEEP_DEBUG
            WaitForDebugger();
#endif 
        }

#if DEEP_DEBUG
        internal void WaitForDebugger() {
            if (!System.Diagnostics.Debugger.IsAttached) {
                Console.Beep(500, 2000);
                while (!System.Diagnostics.Debugger.IsAttached) {
                    System.Threading.Thread.Sleep(1000);
                    Console.Beep(500, 200);
                }
            }
        }
#endif 

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

        public IEnumerable<PackageSource> GetAllSourceNames(IHostApi request) {
            return _packageProviders.Values.SelectMany(each => each.ResolvePackageSources(request));
        }

        /// <summary>
        ///     Searches for the assembly, interrogates it for it's providers and then proceeds to load
        /// </summary>
        /// <param name="request"></param>
        /// <param name="providerAssemblyName"></param>
        /// <returns></returns>
        private bool TryToLoadProviderAssembly(string providerAssemblyName, IHostApi request) {
            // find all the matches for the assembly specified, order by version (descending)

            var assemblyPath = FindAssembly(providerAssemblyName);

            if (assemblyPath == null) {
                return false;
            }

            if (!AcquireProviders(assemblyPath, request)) {
                return false;
            }

            return true;
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

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom", Justification = "This is a plugin loader. It *needs* to do that.")]
        internal bool AcquireProviders(string assemblyPath, IHostApi request) {
            var found = false;
            try {
                var assembly = Assembly.LoadFrom(assemblyPath);

                if (assembly == null) {
                    return false;
                }

                var asmVersion = GetAssemblyVersion(assembly);

                var t1 = Task.Factory.StartNew(() => {
                    // process Meta Providers
                    foreach (var metaProviderClass in assembly.FindCompatibleTypes<IMetaProvider>()) {
                        try {
                            found = found | RegisterProvidersViaMetaProvider(metaProviderClass.Create<IMetaProvider>(), asmVersion, request);
                        } catch {
                            // ignore stuff that doesn't load.
                        }
                    }
                }, TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning);

                var t2 = Task.Factory.StartNew(() => {
                    // process Package Providers
                    foreach (var packageProviderClass in assembly.FindCompatibleTypes<IPackageProvider>()) {
                        try {
                            found = found | RegisterPackageProvider(packageProviderClass.Create<IPackageProvider>(), asmVersion, request);
                        } catch {
                            // ignore stuff that doesn't load.
                        }
                    }
                }, TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning);

                var t3 = Task.Factory.StartNew(() => {
                    // Process archiver Providers
                    foreach (var serviceProviderClass in assembly.FindCompatibleTypes<IArchiver>()) {
                        try {
                            found = found | RegisterArchiver(serviceProviderClass.Create<IArchiver>(), asmVersion, request);
                        } catch {
                            // ignore stuff that doesn't load.
                        }
                    }
                }, TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning);

                var t4 = Task.Factory.StartNew(() => {
                    // Process downloader Providers
                    foreach (var serviceProviderClass in assembly.FindCompatibleTypes<IDownloader>()) {
                        try {
                            found = found | RegisterDownloader(serviceProviderClass.Create<IDownloader>(), asmVersion, request);
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
                if (!string.IsNullOrWhiteSpace(assemblyLocation) && File.Exists(assemblyLocation)) {
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

        private bool RegisterPackageProvider(IPackageProvider provider, FourPartVersion asmVersion, IHostApi request) {
            try {
                FourPartVersion ver = provider.GetProviderVersion();
                var version = ver == 0 ? asmVersion : ver;
                var name = provider.GetPackageProviderName();

                lock (_packageProviders) {
                    if (_packageProviders.ContainsKey(name)) {
                        if (version > _packageProviders[name].Version) {
                            // remove the old provider first.
                            // todo: this won't remove the plugin domain and unload the code yet
                            // we'll have to do that later.

                            _packageProviders.Remove(name);
                        }
                        else {
                            return false;
                        }
                    }
                    request.Debug("Loading provider {0}".format(name, provider.GetPackageProviderName()));
                    provider.InitializeProvider(request.As<IRequest>());
                    _packageProviders.AddOrSet(name, new PackageProvider(provider) {
                        Version = version
                    }).Initialize(request);
                }
                return true;
            } catch (Exception e) {
                e.Dump();
            }
            return false;
        }

        private bool RegisterArchiver(IArchiver provider, FourPartVersion asmVersion, IHostApi request) {
            try {
                FourPartVersion ver = provider.GetProviderVersion();
                var version = ver == 0 ? asmVersion : ver;
                var name = provider.GetArchiverName();

                lock (Archivers) {
                    if (Archivers.ContainsKey(name)) {
                        if (version > Archivers[name].Version) {
                            // remove the old provider first.
                            // todo: this won't remove the plugin domain and unload the code yet
                            // we'll have to do that later.

                            Archivers.Remove(name);
                        }
                        else {
                            return false;
                        }
                    }
                    request.Debug("Loading Archiver {0}".format(name, provider.GetArchiverName()));
                    provider.InitializeProvider(request.As<IRequest>());
                    Archivers.AddOrSet(name, new Archiver(provider) {
                        Version = version
                    }).Initialize(request);
                }
                return true;
            } catch (Exception e) {
                e.Dump();
            }
            return false;
        }

        private bool RegisterDownloader(IDownloader provider, FourPartVersion asmVersion, IHostApi request) {
            try {
                FourPartVersion ver = provider.GetProviderVersion();
                var version = ver == 0 ? asmVersion : ver;
                var name = provider.GetDownloaderName();

                lock (Downloaders) {
                    if (Downloaders.ContainsKey(name)) {
                        if (version > Downloaders[name].Version) {
                            // remove the old provider first.
                            // todo: this won't remove the plugin domain and unload the code yet
                            // we'll have to do that later.

                            Downloaders.Remove(name);
                        }
                        else {
                            return false;
                        }
                    }
                    request.Debug("Loading Downloader {0}".format(name, provider.GetDownloaderName()));
                    provider.InitializeProvider(request.As<IRequest>());
                    Downloaders.AddOrSet(name, new Downloader(provider) {
                        Version = version
                    }).Initialize(request);
                }
                return true;
            } catch (Exception e) {
                e.Dump();
            }
            return false;
        }

        internal bool RegisterProvidersViaMetaProvider(IMetaProvider provider, FourPartVersion asmVersion, IHostApi request) {
            var found = false;
            var metaProviderName = provider.GetMetaProviderName();
            FourPartVersion metaProviderVersion = provider.GetProviderVersion();

            if (!_metaProviders.ContainsKey(metaProviderName)) {
                // Meta Providers can't be replaced at this point
                _metaProviders.AddOrSet(metaProviderName, provider);
            }

            try {
                provider.InitializeProvider(request.As<IRequest>());
                var metaProvider = provider;
                provider.GetProviderNames().ParallelForEach(name => {
                    // foreach (var name in provider.GetProviderNames()) {
                    var instance = metaProvider.CreateProvider(name);
                    if (instance != null) {
                        // check if it's a Package Provider
                        if (typeof (IPackageProvider).CanDynamicCastFrom(instance)) {
                            try {
                                found = found | RegisterPackageProvider(instance.As<IPackageProvider>(), asmVersion, request);
                            } catch (Exception e) {
                                e.Dump();
                            }
                        }

                        // check if it's a Services Provider
                        if (typeof (IArchiver).CanDynamicCastFrom(instance)) {
                            try {
                                found = found | RegisterArchiver(instance.As<IArchiver>(), asmVersion, request);
                            } catch (Exception e) {
                                e.Dump();
                            }
                        }

                        if (typeof (IDownloader).CanDynamicCastFrom(instance)) {
                            try {
                                found = found | RegisterDownloader(instance.As<IDownloader>(), asmVersion, request);
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