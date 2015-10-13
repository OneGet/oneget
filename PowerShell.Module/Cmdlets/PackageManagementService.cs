
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

namespace Microsoft.PackageManagement.Internal.Implementation {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security.AccessControl;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Api;
    using PackageManagement.Implementation;
    using PackageManagement.Packaging;
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
    ///     - PackageManagement Powershell Cmdlets
    ///     The Client API provides high-level consumer functions to support SDII functionality.
    /// </summary>
    internal class PackageManagementService : IPackageManagementService {
       
        private static int _lastCallCount;
        private static HashSet<string> _providersTriedThisCall;
        private string[] _bootstrappableProviderNames;
        private bool _initialized;
        
        // well known, built in provider assemblies.
        private readonly string[] _defaultProviders = {
            Path.GetFullPath(Assembly.GetExecutingAssembly().Location), // load the providers from this assembly
            "Microsoft.PackageManagement.MetaProvider.PowerShell.dll"
        };

        private readonly object _lockObject = new object();
        private readonly IDictionary<string, IMetaProvider> _metaProviders = new Dictionary<string, IMetaProvider>(StringComparer.OrdinalIgnoreCase);
        private readonly IDictionary<string, PackageProvider> _packageProviders = new Dictionary<string, PackageProvider>(StringComparer.OrdinalIgnoreCase);
        internal readonly IDictionary<string, Archiver> Archivers = new Dictionary<string, Archiver>(StringComparer.OrdinalIgnoreCase);
        internal readonly IDictionary<string, Downloader> Downloaders = new Dictionary<string, Downloader>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, byte[]> _providerFiles = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        private string _baseDir;
        internal string BaseDir {
            get {
                return _baseDir ?? ( _baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) );
            }
        }

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
                string[] folders = {Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)};
          
               // AutoloadedAssemblyLocationHelp(folders);

                folders.Concat(AutoloadedAssemblyLocationsHelp(SystemAssemblyLocation));
                folders.Concat(AutoloadedAssemblyLocationsHelp(UserAssemblyLocation));

                //folders.Concat(new[] { SystemAssemblyLocation, UserAssemblyLocation });
                //(AutoloadedAssemblyLocationHelp(folder);

                //folder = UserAssemblyLocation;
               // AutoloadedAssemblyLocationHelp(folder);

                
                var programsFolder = Environment.GetEnvironmentVariable("ProgramFiles");
                if (programsFolder != null)
                {
                    folders.Concat(AutoloadedAssemblyLocationsHelp(Path.Combine(programsFolder, "WindowsPowerShell\\Modules\\PackageManagement\\Providers")));

                    //AutoloadedAssemblyLocationHelp(folder);
                }
                var userprofileFolder = Environment.GetEnvironmentVariable("USERPROFILE");
                if (userprofileFolder != null)
                {
                   // folders = Path.Combine(userprofileFolder, "WindowsPowerShell\\Modules\\PackageManagement\\Providers");
                    folders.Concat(AutoloadedAssemblyLocationsHelp( Path.Combine(userprofileFolder, "WindowsPowerShell\\Modules\\PackageManagement\\Providers")));

                   // AutoloadedAssemblyLocationHelp(folder);
                }
                return folders;
            }
        }

        private IEnumerable<string> AutoloadedAssemblyLocationsHelp(string folder)
        {

            if (!String.IsNullOrWhiteSpace(folder) && folder.DirectoryExists())
            {
                yield return folder;
            }
        }

        internal string UserAssemblyLocation {
            get {
                try {
                    var basepath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    if (String.IsNullOrWhiteSpace(basepath)) {
                        return null;
                    }
                    var path = Path.Combine(basepath, "PackageManagement", "ProviderAssemblies");
                    if (!Directory.Exists(path)) {
                        Directory.CreateDirectory(path);
                    }
                    return path;
                } catch {
                    // if it can't be created, it's not the end of the world.
                }
                return null;
            }
        }

        internal string SystemAssemblyLocation {
            get {
                try {
                    var basepath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    if (String.IsNullOrWhiteSpace(basepath)) {
                        return null;
                    }
                    var path = Path.Combine(basepath, "PackageManagement", "ProviderAssemblies");

                    if (!Directory.Exists(path)) {
                        Directory.CreateDirectory(path);
                    }
                    return path;
                } catch {
                    // ignore non-existant directory for now.
                }
                return null;
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
                return Constants.PackageManagementVersion;
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
            if (!String.IsNullOrWhiteSpace(providerName)) {
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
                   
                    // SelectProviders() is iterating through the loaded provider list. As we still need to go through the
                    // unloaded provider list, we should not warn users yet at this point of time.
                    // If the provider is not found, eventually we will error out in SelectProviders()/cmdletbase.cs(). 
                    
                    //hostApi.Warn(hostApi.FormatMessageString(Constants.Messages.UnknownProvider, providerName));                   
                }
                return Enumerable.Empty<PackageProvider>();
            }

            return PackageProviders;
        }

        public IEnumerable<SoftwareIdentity> FindPackageByCanonicalId(string packageId, IHostApi hostApi) {
            Uri pkgId;
            if (Uri.TryCreate(packageId, UriKind.Absolute, out pkgId)) {
                var segments = pkgId.Segments;
                if (segments.Length > 0) {
                    var provider = SelectProviders(pkgId.Scheme, hostApi).FirstOrDefault();
                    if (provider != null) {
                        var name = Uri.UnescapeDataString(segments[0].Trim('/', '\\'));
                        var version = (segments.Length > 1) ? Uri.UnescapeDataString(segments[1]) : null;
                        var source = pkgId.Fragment.TrimStart('#');
                        var sources = (String.IsNullOrWhiteSpace(source) ? hostApi.Sources : Uri.UnescapeDataString(source).SingleItemAsEnumerable()).ToArray();

                        var host = new object[] {
                            new {
                                GetSources = new Func<IEnumerable<string>>(() => sources),
                                GetOptionValues = new Func<string, IEnumerable<string>>(key => key.EqualsIgnoreCase("FindByCanonicalId") ? new[] {"true"} : hostApi.GetOptionValues(key)),
                                GetOptionKeys = new Func<IEnumerable<string>>(() => hostApi.OptionKeys.ConcatSingleItem("FindByCanonicalId")),
                            },
                            hostApi,
                        }.As<IHostApi>();

                        return provider.FindPackage(name, version, null, null, host).Select(each => {
                            each.Status = Constants.PackageStatus.Dependency;
                            return each;
                        }).ReEnumerable();
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
            if (!_packageProviders.ContainsKey(packageProviderName)){
                return false;
            }
            var bootstrap = _packageProviders["Bootstrap"];
            if (bootstrap == null) {
                hostApi.Debug("Skipping RequirePackageProvider due to missing bootstrap provider");
                return false;
            }

            var pkg = bootstrap.FindPackage(packageProviderName, null, minimumVersion, null, hostApi).OrderByDescending(p =>  p, SoftwareIdentityVersionComparer.Instance).GroupBy(package => package.Name).ToArray();
            if (pkg.Length == 1) {
                // Yeah? Install it.
                var package = pkg[0].FirstOrDefault();
                var metaWithProviderType = package.Meta.FirstOrDefault(each => each.ContainsKey("providerType"));
                var providerType = metaWithProviderType == null ? "unknown" : metaWithProviderType.GetAttribute("providerType");
                var destination = providerType == "assembly" ? (AdminPrivilege.IsElevated ? SystemAssemblyLocation : UserAssemblyLocation) : String.Empty;
                var link = package.Links.FirstOrDefault(each => each.Relationship == "installationmedia");
                var location = String.Empty;
                if (link != null) {
                    location = link.HRef.ToString();
                }

                // what can't find an installationmedia link?
                // todo: what should we say here?
                if (hostApi.ShouldBootstrapProvider(requestor, package.Name, package.Version, providerType, location, destination)) {
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
                    var packagesInstalled = bootstrap.InstallPackage(package, newRequest).LastOrDefault();
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

        /// <summary>
        /// Get available unloaded providers. if not loaded, load those specified in providerNames.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="providerNames">providers to be loaded.</param>
        /// <param name="listAvailable"></param>
        public IEnumerable<PackageProvider> GetAvailableProviders(IHostApi request, string[] providerNames, bool listAvailable) {

            var powerShellMetaProvider = GetMetaProviderObject(request);
            if (powerShellMetaProvider == null) {
                return Enumerable.Empty<PackageProvider>();
            }

            //Handling two cases
            //1. Both "-Name" and "-Listavailable" exist
            //2. "-Name" only. This should be the same behavior as #1

            if (!providerNames.IsNullOrEmpty()) {
                return providerNames.SelectMany(each => GetAvailableProvider(request, each, powerShellMetaProvider));
            }

            if (!listAvailable) {
                //Noop if a user input No -Name nor -ListAvailable 
                return Enumerable.Empty<PackageProvider>();
            }

            //no -Name but -ListAvailable
            return GetAvailableProvider(request, String.Empty, powerShellMetaProvider);
        }

        /// <summary>
        /// Get available provider if -ListAvailable present;load a provider specified in providerName.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="providerName">provider to be loaded.</param>
        /// <param name="powerShellMetaProvider"></param>
        private IEnumerable<PackageProvider> GetAvailableProvider(IHostApi request, string providerName, IMetaProvider powerShellMetaProvider)
        {
            var results = _packageProviders.Values.Where(each => each.ProviderName.IsWildcardMatch(providerName)).ReEnumerable();

            if (results.Any()) {
                //the provider is already loaded and tracked in the loaded master list: _packageProviders 
                yield break;
            }

            //Get available powershell providers
            var availableProviders = powerShellMetaProvider.GetAvailableLocallyProviders(request.As<IRequest>()).ReEnumerable();                        

            if (!availableProviders.Any()) {
                //No available providers.
                if (!String.IsNullOrWhiteSpace(providerName)) {
                    //Error out if a user specific -Name
                    request.Error(Constants.Messages.UnknownProvider, ErrorCategory.InvalidOperation.ToString(),
                        providerName, request.FormatMessageString(Constants.Messages.UnknownProvider, providerName));
                }
                yield break;
            }

            if (String.IsNullOrWhiteSpace(providerName)) {
                // "-Name" does not exist, we return all we can find
                foreach (var module in availableProviders) {
                    yield return new PackageProvider(module.As<DefaultPackageProvider>());
                }
            } else {
                //A user inputs both -Name and -ListAvailable


                var matches = powerShellMetaProvider.GetProviderNames().Where(each => each.IsWildcardMatch(providerName)).ReEnumerable();
                if (!matches.Any()) {
                    request.Error(Constants.Messages.UnknownProvider, ErrorCategory.InvalidOperation.ToString(),
                        providerName, request.FormatMessageString(Constants.Messages.UnknownProvider, providerName));
                }

                foreach (var match in matches) {
                    yield return new PackageProvider(powerShellMetaProvider.GetAvailableLocallyProvider(match).As<DefaultPackageProvider>());
                    //yield return new PackageProvider(availableProviders[match].As<DefaultPackageProvider>());
                }
            }
        }

        //TODO Name, Version,....
        public IEnumerable<PackageProvider> ImportPackageProvider(IHostApi request, string providerName, bool isPathRooted) {
            if (providerName.IsNullOrEmpty())
            {
                return Enumerable.Empty<PackageProvider>();
            }

            //foreach (var name in providerNames) {
                //check if it;s loaded.
            var results = _packageProviders.Values.Where(each => each.ProviderName.IsWildcardMatch(providerName)).ReEnumerable();

            if (results.Any())
            {
                //the provider is already loaded and tracked in the loaded master list: _packageProviders 
                request.Verbose(string.Format("The provider '{0}' has already been imported.", providerName));
                return Enumerable.Empty<PackageProvider>();
                
            }

                if (isPathRooted)
                {
                    if (File.Exists(providerName))
                    {
                        // looks like we have something that could definitely be a
                        // a module path.
                        //yield return new KeyValuePair<string, string>(fullPath, (moduleVersion ?? new Version(0, 0)).ToString());
                        return ImportPackageProviderViaPath(request, providerName);
                    } else {
                        //error out
                        request.Warning(String.Format("{0} is not found", providerName));
                    }
                } else {
                     return ImportPackageProviderViaName(request, providerName);
                }
                // return providerNames.SelectMany(each => ImportPackageProvider(request, each));
           // }
            return Enumerable.Empty<PackageProvider>();

        }

        private IEnumerable<PackageProvider> ImportPackageProviderViaPath(IHostApi request, string providerName) {
            var extension = Path.GetExtension(providerName);
            if (extension != null && extension.Equals(".dll")) {
                //TODO need to check if the assembly is from the known designated folder

               // List<string> knownAssemblyFolders = new List<string>();
                //todo: get the current ps module folder instead of program files
                //todo build a internal asemlby table for Get-packageprovider to display.
                //todo: PackageProviderItem: providername, version, path, loaded, type (PS or dll), providerobject sorted by loaded

                var exist = AutoloadedAssemblyLocations.Where(each => each.Equals(Path.GetDirectoryName(providerName))).Any();

                
                //if (knownAssemblyFolders.Contains(Path.GetDirectoryName(providerName))) {
                if (exist)
                {
                    //If so, load it.
                    var loaded = LoadProviderAssembly(request, providerName);
                    if (loaded)
                    {
                        request.Verbose("loaded successfully");
                        //TODO  not need to be so sily, reference to the key of master list is enough
                        yield return _packageProviders[Path.GetFileNameWithoutExtension(providerName)];
                        //return SelectProviders(Path.GetFileNameWithoutExtension(providerName), request);
                    }

                    request.Warning(string.Format("cannot load assembly '{0}'. you need to add your assembly here...", providerName));
                    yield return null;
                }
                else
                {
                    request.Warning(string.Format("The specified provider '{0}' was not loaded because no valid provider file was found", providerName));
                }
                
            }

            //deal with modules with full path
            var powerShellMetaProvider = GetMetaProviderObject(request);
            if (powerShellMetaProvider == null)
            {
                yield return null;
            }

            //load the proviver
            //TODO firstordefault
            yield return ImportProvider(request, providerName, powerShellMetaProvider);
        }

        private IEnumerable<PackageProvider> ImportPackageProviderViaName(IHostApi request, string providerName)
        {
            var powerShellMetaProvider = GetMetaProviderObject(request);
            if (powerShellMetaProvider == null) {
                yield break;
            }

            //var availableProvider = powerShellMetaProvider.GetAvailableLocallyProviders(request.As<IRequest>()).ReEnumerable();
            var availableProviderNames = powerShellMetaProvider.GetProviderNames(); //.ReEnumerable();

            if (!availableProviderNames.Any())
            {
                //No available providers.
                if (!String.IsNullOrWhiteSpace(providerName)) {
                    //Error out if a user specific -Name
                    request.Error(Constants.Messages.UnknownProvider, ErrorCategory.InvalidOperation.ToString(),
                        providerName, request.FormatMessageString(Constants.Messages.UnknownProvider, providerName));
                }
                yield break;
            }

            // a user inputs "-Name" property, we load the specific provider

            //Find if it exists in the available list
            var matches = availableProviderNames.Where(each => each.IsWildcardMatch(providerName)).ReEnumerable();
            if (!matches.Any()) {
                request.Error(Constants.Messages.UnknownProvider, ErrorCategory.InvalidOperation.ToString(),
                    providerName, request.FormatMessageString(Constants.Messages.UnknownProvider, providerName));
            }
            foreach (var match in matches) {


                //var matchedProviderName = match.As<DefaultPackageProvider>().GetPackageProviderName();

                yield return ImportProvider(request, match, powerShellMetaProvider);

                //var instance = powerShellMetaProvider.LoadAvailableProvider(request.As<IRequest>(), matchedProviderName);
                //if (instance == null) {
                //    yield break;
                //}

                ////Register newly created provider
                //if (typeof (IPackageProvider).CanDynamicCastFrom(instance)) {

                //    var packageProvider = RegisterPackageProvider(instance.As<IPackageProvider>(), String.Empty, request);

                //    if (packageProvider != null) {
                //        packageProvider.ProviderPath = powerShellMetaProvider.GetProviderPath(matchedProviderName);
                //        yield return packageProvider;
                //    }
                //} else {
                //    //A provider is not found
                //    request.Error(Constants.Messages.UnknownProvider, ErrorCategory.InvalidOperation.ToString(),
                //        providerName, request.FormatMessageString(Constants.Messages.UnknownProvider, providerName));
                //}
            }
        }
        private PackageProvider ImportProvider(IHostApi request, string providerName, IMetaProvider powerShellMetaProvider)
        {


            //foreach (var providerName in providers)
            {
                //var matchedProviderName = provider.As<DefaultPackageProvider>().GetPackageProviderName();
                //TODO pass in Maxi, mini, and requiredVersion to LoadAvailableProvider
                var instance = powerShellMetaProvider.LoadAvailableProvider(request.As<IRequest>(), providerName);
                if (instance == null)
                {
                   return null;
                }

                //Register newly created provider
                if (typeof(IPackageProvider).CanDynamicCastFrom(instance))
                {

                    var packageProvider = RegisterPackageProvider(instance.As<IPackageProvider>(), providerName, String.Empty, request);

                    if (packageProvider != null)
                    {
                        packageProvider.ProviderPath = powerShellMetaProvider.GetProviderPath(providerName);
                        return packageProvider;
                    }
                }
                else
                {
                    //A provider is not found
                    request.Error(Constants.Messages.UnknownProvider, ErrorCategory.InvalidOperation.ToString(),
                        providerName, request.FormatMessageString(Constants.Messages.UnknownProvider, providerName));
                }
            }
        }

  
        private IMetaProvider GetMetaProviderObject(IHostApi requestObject) {
            if (_metaProviders.ContainsKey("powershell")) {
                var powerShellMetaProvider = _metaProviders["powershell"];
                if (powerShellMetaProvider != null) {
                    return powerShellMetaProvider;
                }
            }

            requestObject.Error(Constants.Messages.FailedPowerShellMetaProvider, ErrorCategory.InvalidOperation.ToString(), "LoadProvider", requestObject.FormatMessageString(Constants.Messages.FailedPowerShellMetaProvider));
            return null;
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

            var providerAssemblies = (_initialized ? Enumerable.Empty<string>() : _defaultProviders)
                .Concat(GetProvidersFromRegistry(Registry.LocalMachine, "SOFTWARE\\MICROSOFT\\PACKAGEMANAGEMENT"))
                .Concat(GetProvidersFromRegistry(Registry.CurrentUser, "SOFTWARE\\MICROSOFT\\PACKAGEMANAGEMENT"))
                .Concat(AutoloadedAssemblyLocations.SelectMany(location => {
                    if (Directory.Exists(location)) {
                        return Directory.EnumerateFiles(location).Where(each => (each.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || each.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)));
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
            // todo: expand this out to validate the assembly is ok for this instance of PackageManagement.
            providerAssemblies = providerAssemblies.Where(each => Manifest.LoadFrom(each).Any(manifest => Swidtag.IsSwidtag(manifest) && new Swidtag(manifest).IsApplicable(new Hashtable())));

            // add inbox assemblies (don't require manifests, because they are versioned with the core)

#if !COMMUNITY_BUILD
            // todo: these should just be strong-named references. for now, just load them from the same directory.
            providerAssemblies = providerAssemblies.Concat(new[] {
                Path.Combine(BaseDir, "Microsoft.PackageManagement.MetaProvider.PowerShell.dll"),
                Path.Combine(BaseDir, "Microsoft.PackageManagement.ArchiverProviders.dll"),
                Path.Combine(BaseDir, "Microsoft.PackageManagement.CoreProviders.dll"),
                Path.Combine(BaseDir, "Microsoft.PackageManagement.MsuProvider.dll"),
                Path.Combine(BaseDir, "Microsoft.PackageManagement.MsiProvider.dll")
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

            // there is no trouble with loading providers concurrently.
#if DEEP_DEBUG
            providerAssemblies.SerialForEach(providerAssemblyName => {
#else
            providerAssemblies.ParallelForEach(providerAssemblyName => {
#endif
                LoadProviderAssembly(request, providerAssemblyName);
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
        /// <summary>
        /// Dynamic providers are the ones that are not installed with the core itself.
        /// </summary>
        internal IEnumerable<PackageProvider> DynamicProviders {
            get {
                return _packageProviders.Values.Where(each => !each.ProviderPath.StartsWith(BaseDir, StringComparison.OrdinalIgnoreCase));
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

        public IEnumerable<PackageSource> GetAllSourceNames(IHostApi request) {
            return _packageProviders.Values.SelectMany(each => each.ResolvePackageSources(request));
        }


        /// <summary>
        ///     Searches for the assembly, interrogates it for it's providers and then proceeds to load
        /// </summary>
        /// <param name="request"></param>
        /// <param name="providerAssemblyName"></param>
        /// <returns></returns>
        public bool LoadProviderAssembly(IHostApi request, string providerAssemblyName) {
            request.Debug(request.FormatMessageString("Trying provider assembly: {0}", providerAssemblyName));

            var assemblyPath = FindAssembly(providerAssemblyName);
            if (assemblyPath != null) {

                try {
                    byte[] hash = null;
                    using (var stream = File.Open(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        hash = MD5.Create().ComputeHash(stream);
                    }
                    lock (_providerFiles) {
                        if (_providerFiles.ContainsKey(assemblyPath)) {
                            // have we tried this file before?
                            if (_providerFiles[assemblyPath].SequenceEqual(hash)) {
                                // and it's the exact same file?
                                request.Debug(request.FormatMessageString("Skipping previously processed assembly: {0}", assemblyPath));
                                return false;
                            }
                            request.Debug(request.FormatMessageString("New assembly in location: {0}", assemblyPath));
                            // it's a different file in the same path? 
                            // we're gonna let it try the new file. 
                            _providerFiles.Remove(assemblyPath);
                        } else {
                            request.Debug(request.FormatMessageString("Attempting loading of assembly: {0}", assemblyPath));
                        }

                        // record that this file is being loaded.
                        _providerFiles.Add(assemblyPath, hash);
                    }
                    if (AcquireProviders(assemblyPath, request)) {
                        request.Debug(request.FormatMessageString("SUCCESS provider assembly: {0}", providerAssemblyName));
                        return true;
                    }
                } catch (Exception e) {
                    e.Dump();

                    lock (_providerFiles) {
                        // can't create hash from file? 
                        // we're not going to try and load this.
                        // all we can do is record the name.
                        if (!_providerFiles.ContainsKey(assemblyPath)) {
                            _providerFiles.Add(assemblyPath, new byte[0]);
                        }
                    }
                }
            }
            request.Debug(request.FormatMessageString("FAILED provider assembly: {0}", providerAssemblyName));
            return false;
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
                if (assemblyName.Contains('\\') || assemblyName.Contains('/') || assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || assemblyName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) {
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

                assembly.FindCompatibleTypes<IMetaProvider>().AsyncForEach(metaProviderClass => {
                    found |= RegisterProvidersViaMetaProvider(metaProviderClass.Create<IMetaProvider>(),
                        Path.GetFileNameWithoutExtension(assembly.Location), asmVersion, request);
                })
                    .Concat(assembly.FindCompatibleTypes<IPackageProvider>().AsyncForEach(packageProviderClass => {
                        var packageProvider = RegisterPackageProvider(packageProviderClass.Create<IPackageProvider>(), asmVersion, request);
                        if (packageProvider != null) {
                            found = true;
                            packageProvider.ProviderPath = assemblyPath;
                        }
                    }))

                    .Concat(assembly.FindCompatibleTypes<IArchiver>().AsyncForEach(serviceProviderClass => {
                        var archiver = RegisterArchiver(serviceProviderClass.Create<IArchiver>(), asmVersion, request);
                        if (archiver != null) {
                            found = true;
                            archiver.ProviderPath = assemblyPath;
                        }
                    }))
                    .Concat(assembly.FindCompatibleTypes<IDownloader>().AsyncForEach(serviceProviderClass => {
                        var downloader = RegisterDownloader(serviceProviderClass.Create<IDownloader>(), asmVersion, request);
                        if (downloader != null) {
                            found = true;
                            downloader.ProviderPath = assemblyPath;
                        }
                    })).WaitAll();

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
                if (!String.IsNullOrWhiteSpace(assemblyLocation) && File.Exists(assemblyLocation)) {
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

        private PackageProvider RegisterPackageProvider(IPackageProvider provider, string name, FourPartVersion asmVersion, IHostApi request) {
           // string name = null;
            try {
                FourPartVersion ver = provider.GetProviderVersion();
                var version = ver == 0 ? asmVersion : ver;

                //TODO: Need to write a blog file name is the provider name. Provider is no longer required to impl ProvierName property
                //name = provider.GetPackageProviderName();
                //if (String.IsNullOrWhiteSpace(name)) {
                //    return null;
                //}

                // Initialize the provider before locking the collection
                // that way we're not blocking others on non-deterministic actions.
                request.Debug("Initializing provider '{0}'".format(name));
                provider.InitializeProvider(request.As<IRequest>());
                request.Debug("Provider '{0}' Initialized".format(name));


                lock (_packageProviders) {
                    if (_packageProviders.ContainsKey(name)) {
                        if (version > _packageProviders[name].Version) {
                            // remove the old provider first.
                            // todo: this won't remove the plugin domain and unload the code yet
                            // we'll have to do that later.

                            _packageProviders.Remove(name);
                        } else {
                            return null;
                        }
                    }
                }

                request.Debug("Using Package Provider {0}".format(name));
                var packageProvider = new PackageProvider(provider) {
                    Version = version,
                    IsLoaded = true
                };
                packageProvider.Initialize(request);

                // addOrSet locks the collection anyway.
                _packageProviders.AddOrSet(name, packageProvider);
                return packageProvider;
            } catch (Exception e) {
                request.Debug("Provider '{0}' Failed".format(name));
                e.Dump();
            }
            return null;
        }

        private Archiver RegisterArchiver(IArchiver provider, FourPartVersion asmVersion, IHostApi request) {
            string name = null;
            try {
                FourPartVersion ver = provider.GetProviderVersion();
                var version = ver == 0 ? asmVersion : ver;
                name = provider.GetArchiverName();
                if (String.IsNullOrWhiteSpace(name)) {
                    return null;
                }

                // Initialize the provider before locking the collection
                // that way we're not blocking others on non-deterministic actions.
                request.Debug("Initializing provider '{0}'".format(name));
                provider.InitializeProvider(request.As<IRequest>());
                request.Debug("Provider '{0}' Initialized".format(name));

                lock (Archivers) {
                    if (Archivers.ContainsKey(name)) {
                        if (version > Archivers[name].Version) {
                            // remove the old provider first.
                            // todo: this won't remove the plugin domain and unload the code yet
                            // we'll have to do that later.

                            Archivers.Remove(name);
                        }
                        else {
                            return null;
                        }
                    }
                    request.Debug("Using Archiver Provider {0}".format(name));
                    var archiver = new Archiver(provider) {
                        Version = version,
                        IsLoaded = true
                    };

                    archiver.Initialize(request);
                    Archivers.AddOrSet(name, archiver);
                    return archiver;
                }
            }
            catch (Exception e) {
                request.Debug("Provider '{0}' Failed".format(name));
                e.Dump();
            }
            return null;
        }

        private Downloader RegisterDownloader(IDownloader provider, FourPartVersion asmVersion, IHostApi request) {
            string name = null;
            try {
                FourPartVersion ver = provider.GetProviderVersion();
                var version = ver == 0 ? asmVersion : ver;
                name = provider.GetDownloaderName();
                if (String.IsNullOrWhiteSpace(name)) {
                    return null;
                }

                // Initialize the provider before locking the collection
                // that way we're not blocking others on non-deterministic actions.
                request.Debug("Initializing provider '{0}'".format(name));
                provider.InitializeProvider(request.As<IRequest>());
                request.Debug("Provider '{0}' Initialized".format(name));

                lock (Downloaders) {
                    if (Downloaders.ContainsKey(name)) {
                        if (version > Downloaders[name].Version) {
                            // remove the old provider first.
                            // todo: this won't remove the plugin domain and unload the code yet
                            // we'll have to do that later.

                            Downloaders.Remove(name);
                        } else {
                            return null;
                        }
                    }
                    request.Debug("Using Downloader Provider {0}".format(name));

                    var downloader = new Downloader(provider) {
                        Version = version,
                        IsLoaded = true
                    };

                    downloader.Initialize(request);
                    Downloaders.AddOrSet(name, downloader);
                    return downloader;
                }
            } catch (Exception e) {
                request.Debug("Provider '{0}' Failed".format(name));
                e.Dump();
            }
            return null;
        }

        internal bool TryLoadProviderViaMetaProvider(string metaproviderName, string providerNameOrPath, IHostApi request ) {
            if (_metaProviders.ContainsKey(metaproviderName)) {
                var metaProvider = _metaProviders[metaproviderName];

                request.Debug("Using MetaProvider '{0}' to attempt to load provider from '{1}'".format(metaproviderName, providerNameOrPath));

                return LoadViaMetaProvider( _metaProviders[metaproviderName], providerNameOrPath, metaProvider.GetProviderVersion(),request);
            }
            request.Debug("MetaProvider '{0}' is not recognized".format(metaproviderName));
            return false;
        }

        internal bool RegisterProvidersViaMetaProvider(IMetaProvider provider, FourPartVersion asmVersion, IHostApi request) {
            var found = false;
            var metaProviderName = provider.GetMetaProviderName();

            lock (_metaProviders) {
                if (!_metaProviders.ContainsKey(metaProviderName)) {
                    // Meta Providers can't be replaced at this point
                    _metaProviders.AddOrSet(metaProviderName.ToLowerInvariant(), provider);
                }
            }

            try {
                provider.InitializeProvider(request.As<IRequest>());
                provider.GetProviderNames().ParallelForEach(name => {
                    found = LoadViaMetaProvider(provider, name,asmVersion, request);
                });
            } catch (Exception e) {
                e.Dump();
            }
            return found;
        }

        private bool LoadViaMetaProvider(IMetaProvider metaProvider, string name, FourPartVersion asmVersion, IHostApi request ) {
            var found = false;

            var instance = metaProvider.CreateProvider(name);
            if (instance != null) {
                // check if it's a Package Provider
                if (typeof (IPackageProvider).CanDynamicCastFrom(instance)) {
                    try {
                        var packageProvider = RegisterPackageProvider(instance.As<IPackageProvider>(), name, asmVersion, request);
                        if (packageProvider != null) {
                            found = true;
                            packageProvider.IsLoaded = true;
                            packageProvider.ProviderPath = metaProvider.GetProviderPath(name);
                        }
                    } catch (Exception e) {
                        e.Dump();
                    }
                }

                // check if it's a Services Provider
                if (typeof (IArchiver).CanDynamicCastFrom(instance)) {
                    try {
                        var archiver = RegisterArchiver(instance.As<IArchiver>(), asmVersion, request);
                        if (archiver != null) {
                            found = true;
                            archiver.ProviderPath = metaProvider.GetProviderPath(name);
                            archiver.IsLoaded = true;
                        }
                    } catch (Exception e) {
                        e.Dump();
                    }
                }

                if (typeof (IDownloader).CanDynamicCastFrom(instance)) {
                    try {
                        var downloader = RegisterDownloader(instance.As<IDownloader>(), asmVersion, request);
                        if (downloader != null) {
                            found = true;
                            downloader.ProviderPath = metaProvider.GetProviderPath(name);
                            downloader.IsLoaded = true;
                        }
                    } catch (Exception e) {
                        e.Dump();
                    }
                }
            }
            return found;
        }
    }
}