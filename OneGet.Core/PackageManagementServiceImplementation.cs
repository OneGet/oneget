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
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Core.Api;
    using Core.AppDomains;
    using Core.Dynamic;
    using Core.Extensions;
    using Core.Packaging;
    using Core.Providers.Package;
    using Core.Providers.Service;
    using Core.Tasks;
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
        private DynamicInterface _dynamicInterface = new DynamicInterface();
        private readonly IDictionary<string, PackageProvider> _packageProviders = new Dictionary<string, PackageProvider>();
        private readonly IDictionary<string, ServicesProvider> _servicesProviders = new Dictionary<string, ServicesProvider>();

        private readonly object _lockObject = new object();

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
                return _packageProviders.Values;
            }
        }

        private bool _initialized;

        public bool Initialize(Callback callback, bool userInteractionPermitted) {
            var request = callback.As<ICoreApis>();

            request.Debug("starting Initialize");
            // var request = _dynamicInterface.Create<IHostAndCoreAPIs>(callback);

            lock (_lockObject) {
                if (!_initialized) {
#if _MOVE_TO_BOOTSTRAP_PROVIDER
                    try {
                        if (Instance.Service.GetNuGetDllPath(callback).IsEmptyOrNull()) {
                            // we are unable to bootstrap NuGet correctly.
                            // We can't really declare that the providers are ready, and we should just 
                            // return as if we never really succeded (as it may have been that this got called as 
                            // the result of a tab-completion and we can't fully bootstrap if that was the case.
                            request.Error(Messages.Miscellaneous.NuGetRequired);

                            return false;

                        }
                    } catch {
                        return false;
                    }
#endif
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
        /// 
        /// (currently a hardcoded list, soon, registry driven)
        /// </summary>
        /// <param name="callback"></param>
        private void LoadProviders(Callback callback) {
            var request = callback.As<ICoreApis>();

            request.Debug("Staring LoadProviders");

            // todo: load provider assembly list from the registry.
            IEnumerable<string> providerAssemblies = new string[] {
                 "Microsoft.OneGet.MetaProvider.PowerShell.dll",
                "Microsoft.OneGet.ServiceProvider.Common.dll",
                "OneGet.PackageProvider.Bootstrap.dll",
                "OneGet.PackageProvider.Chocolatey.dll",
                "OneGet.PackageProvider.NuGet.dll",
            };

            // there is no trouble with loading providers concurrently.
            Parallel.ForEach(providerAssemblies, providerAssemblyName => {
            // foreach( var providerAssemblyName in providerAssemblies ) {
                try {
                    if (TryToLoadProviderAssembly(callback, providerAssemblyName)) {
                        request.Debug("Loading Provider Assembly {0}".format( providerAssemblyName) );
                    } else {
                        request.Debug("Failed to load any providers {0}".format( providerAssemblyName) );
                    }
                } catch (Exception e) {
                    request.ExceptionThrown(e.GetType().Name, e.Message, e.StackTrace);
                }
            }
            );
        }

        public IEnumerable<PackageSource> GetAllSourceNames(Callback callback) {
            return _packageProviders.Values.SelectMany(each => each.GetPackageSources(callback));
        }

        public IEnumerable<string> ProviderNames {
            get {
                return _packageProviders.Keys;
            }
        }

        public IEnumerable<PackageProvider> SelectProviders(string providerName) {
            return SelectProviders(providerName, null);
        }

        public IEnumerable<PackageProvider> SelectProviders(string providerName, IEnumerable<string> sourceNames) {
            var providers = PackageProviders;

            if (providerName.Is()) {
                // strict name match for now.
                providers = providers.Where(each => each.Name.Equals(providerName, StringComparison.CurrentCultureIgnoreCase));
            }
            /*
            if (!sourceNames.IsNullOrEmpty()) {
                // let the providers select which ones are good.
                providers = providers.Where(each => each.IsValidPackageSource.IsSupported() && sourceNames.Any(sourceName => each.IsValidPackageSource(sourceName)));
            }
            */
            return providers;
        }

        private void AddPackageProvider(string name, IPackageProvider provider) {
            // wrap this in a caller-friendly wrapper 
            _packageProviders.AddOrSet(name, new PackageProvider(provider));
        }

        private void AddServicesProvider(string name, IServicesProvider provider) {
            _servicesProviders.AddOrSet(name, new ServicesProvider(provider));
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

            return pluginDomain.InvokeFunc(ProviderLoader.AcquireProviders, assemblyPath, callback.As<ICoreApis>(),
                (Action<string, IPackageProvider>)(AddPackageProvider),
                (Action<string, IServicesProvider>)(AddServicesProvider)
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
        private PluginDomain CreatePluginDomain(string primaryAssemblyPath ) {
            try {
                // this needs to load the assembly in it's own domain
                // so that we can drop them when necessary.
                var name = Path.GetFileNameWithoutExtension(primaryAssemblyPath) ?? primaryAssemblyPath;
                var pd = new PluginDomain(string.Format( "PluginDomain [{0}]",name.Substring(name.LastIndexOf('.')+1)));
                /*
                // add event listeners to the new appdomain.
                pd.Invoke(c => {CurrentTask.Events += new Verbose(c.Invoke);}, typeof (Verbose).CreateWrappedProxy(new Verbose((f, o) => Event<Verbose>.Raise(f, o.ByRef()))) as WrappedFunc<string, IEnumerable<object>, bool>);
                pd.Invoke(c => {CurrentTask.Events += new Warning(c.Invoke);}, typeof (Warning).CreateWrappedProxy(new Warning((f, o) => Event<Warning>.Raise(f, o.ByRef()))) as WrappedFunc<string, IEnumerable<object>, bool>);
                pd.Invoke(c => {CurrentTask.Events += new Message(c.Invoke);}, typeof (Message).CreateWrappedProxy(new Message((f, o) => Event<Message>.Raise(f, o.ByRef()))) as WrappedFunc<string, IEnumerable<object>, bool>);
                pd.Invoke(c => {CurrentTask.Events += new Error(c.Invoke);}, typeof (Error).CreateWrappedProxy(new Error((f, o) => Event<Error>.Raise(f, o.ByRef()))) as WrappedFunc<string, IEnumerable<object>, bool>);
                pd.Invoke(c => {CurrentTask.Events += new Debug(c.Invoke);}, typeof (Debug).CreateWrappedProxy(new Debug((f, o) => Event<Debug>.Raise(f, o.ByRef()))) as WrappedFunc<string, IEnumerable<object>, bool>);

                pd.Invoke(c => {CurrentTask.Events += new StartProgress(c.Invoke);},
                    typeof (StartProgress).CreateWrappedProxy(new StartProgress((pid, f, o) => Event<StartProgress>.Raise(pid, f, o.ByRef()))) as WrappedFunc<int, string, IEnumerable<object>, int>);

                pd.Invoke(c => {CurrentTask.Events += new Progress(c.Invoke);},
                    typeof (Progress).CreateWrappedProxy(new Progress((id, i, f, o) => Event<Progress>.Raise(id, i, f, o.ByRef()))) as WrappedFunc<int, int, string, IEnumerable<object>, bool>);

                pd.Invoke(c => {CurrentTask.Events += new CompleteProgress(c.Invoke);},
                    typeof (CompleteProgress).CreateWrappedProxy(new CompleteProgress((id, c) => Event<CompleteProgress>.Raise(id, c))) as WrappedFunc<int, bool, bool>);

                pd.Invoke(c => {CurrentTask.Events += new ExceptionThrown(c.Invoke);},
                    typeof (ExceptionThrown).CreateWrappedProxy(new ExceptionThrown((e, m, s) => Event<ExceptionThrown>.Raise(e, m, s))) as WrappedFunc<string, string, string, bool>);
                */

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
                if (assemblyName.Contains('\\') || assemblyName.Contains('/') || assemblyName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase)) {
                    fullPath = Path.GetFullPath(assemblyName);
                    if (File.Exists(fullPath)) {
                        return fullPath;
                    }
                    if (File.Exists(fullPath + ".dll")) {
                        return fullPath;
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