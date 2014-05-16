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
    using Core.DuckTyping;
    using Core.Extensions;
    using Core.Packaging;
    using Core.Providers.Meta;
    using Core.Providers.Package;
    using Core.Providers.Service;
    using Core.Tasks;
    using Callback = System.Func<string, System.Collections.Generic.IEnumerable<object>, object>;

    /// <summary>
    ///     PROTOTYPE -- This API is nowhere near what the actual public API will resemble
    ///     The (far off ) Actual API must be built to support native clients, and the managed wrapper
    ///     Will talk to it, so we want the interim managed API to resemble that hopefully.
    ///     In the mean time, this is suited just to what I need for the cmdlets (the only
    ///     'public' api in the short term)
    ///     The Client API is designed for use by installation hosts:
    ///     - OneGet Powershell Cmdlets
    ///     - WMI/OMI Management interfaces
    ///     - DSC Interfaces
    ///     - WiX's Burn
    ///     The Client API provides high-level consumer functions to support SDII functionality.
    /// </summary>
    public class PackageManagementService : MarshalByRefObject {
        private readonly IDictionary<string, PackageProvider> _packageProviders = new Dictionary<string, PackageProvider>();
        private readonly IDictionary<string, ServiceProvider> _serviceProviders = new Dictionary<string, ServiceProvider>();

        private static PackageManagementService _instance;
        
        private static object _lockObject = new object();

        public static IEnumerable<string> PackageProviderNames {
            get {
                return _instance._packageProviders.Keys;
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

        public static IEnumerable<PackageProvider> PackageProviders {
            get {
                return _instance._packageProviders.Values;
            }
        }

        public static bool Initialize(Callback callback, IEnumerable<string> providerAssemblies, bool okToBootstrapNuGet) {
            if (_instance == null) {
                lock (_lockObject) {
                    try {
                        if (Instance.Service.GetNuGetDllPath().IsEmptyOrNull()) {
                            // we are unable to bootstrap NuGet correctly.
                            // We can't really declare that the providers are ready, and we should just 
                            // return as if we never really succeded (as it may have been that this got called as 
                            // the result of a tab-completion and we can't fully bootstrap if that was the case.
                            Event<Error>.Raise(Messages.Miscellaneous.NuGetRequired);
                            //Event<Error>.Raise("MISC001");
                            
                            return false;
                        }
                    }
                    catch {
                        return false;
                    }

                    if (_instance == null) {
                        _instance = new PackageManagementService(providerAssemblies, callback);
                    }
                }
            }
            return _instance != null;
        }

        /// <summary>
        ///     STATUS: PROTOTYPE METHOD.
        ///     This initializes the provider registry with the list of package provider, pulled from the powershell module.
        ///     In the long run, this should not rely on being called from the
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="providerAssemblies"></param>
        private PackageManagementService( IEnumerable<string> providerAssemblies,Callback callback) {
            
            // there is no trouble with loading providers concurrently.
            Parallel.ForEach(providerAssemblies, provdier => {
                try {
                    if (TryToLoadProviderAssembly(callback, provdier)) {
                        Event<Verbose>.Raise("Loading Provider", new string[] {provdier});
                    } else {
                        Event<Warning>.Raise("Failed to load any providers",  new string[] {provdier});
                    }
                } catch (Exception e) {
                    Event<ExceptionThrown>.Raise(e.GetType().Name, e.Message, e.StackTrace);
                }
            });
        }

        public static IEnumerable<PackageSource> GetAllSourceNames(Callback callback) {
            return _instance._packageProviders.Values.SelectMany(each => each.GetPackageSources(callback));
        }

        public static IEnumerable<PackageProvider> SelectProviders(string providerName) {
            return SelectProviders(providerName, null);
        }

        public static IEnumerable<PackageProvider> SelectProviders(string providerName, IEnumerable<string> sourceNames) {
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

        
        private void AddPackageProvider(string name, PackageProviderInstance provider) {
            // wrap this in a caller-friendly wrapper 
            _packageProviders.Add(name, new PackageProvider(provider));
        }
        private void AddServiceProvider(string name, ServiceProviderInstance provider) {
            _serviceProviders.Add(name, new ServiceProvider(provider));
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

            var pluginDomain = CreatePluginDomain();

            pluginDomain.Invoke(ProviderLoader.AcquireProviders, assemblyPath, callback,
                 (Action<string, PackageProviderInstance>)(AddPackageProvider),
                 (Action<string, ServiceProviderInstance>)(AddServiceProvider)
                );

            return true;

        }

#if AFTER_CTP
        private static void UnloadAssembly(Assembly assembly) {
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
        private static PluginDomain CreatePluginDomain() {
            try {
                // this needs to load the assembly in it's own domain
                // so that we can drop them when necessary.
                var pd = new PluginDomain();

                // add event listeners to the new appdomain.
                pd.Invoke(c => {CurrentTask.Events += new Verbose(c.Invoke);}, typeof (Verbose).CreateWrappedProxy(new Verbose((f, o) => Event<Verbose>.Raise( f, o.ByRef()))) as WrappedFunc<string, IEnumerable<object>, bool>);
                pd.Invoke(c => {CurrentTask.Events += new Warning(c.Invoke);}, typeof (Warning).CreateWrappedProxy(new Warning((f, o) => Event<Warning>.Raise( f, o.ByRef()))) as WrappedFunc<string, IEnumerable<object>, bool>);
                pd.Invoke(c => {CurrentTask.Events += new Message(c.Invoke);}, typeof (Message).CreateWrappedProxy(new Message(( f, o) => Event<Message>.Raise( f, o.ByRef()))) as WrappedFunc<string, IEnumerable<object>, bool>);
                pd.Invoke(c => {CurrentTask.Events += new Error(c.Invoke);}, typeof (Error).CreateWrappedProxy(new Error(( f, o) => Event<Error>.Raise( f, o.ByRef()))) as WrappedFunc< string, IEnumerable<object>, bool>);
                pd.Invoke(c => {CurrentTask.Events += new Debug(c.Invoke);}, typeof (Debug).CreateWrappedProxy(new Debug(( f, o) => Event<Debug>.Raise( f, o.ByRef()))) as WrappedFunc< string, IEnumerable<object>, bool>);

                pd.Invoke(c => { CurrentTask.Events += new StartProgress(c.Invoke); },
                    typeof(StartProgress).CreateWrappedProxy(new StartProgress((pid, f, o) => Event<StartProgress>.Raise(pid, f ,o.ByRef()))) as WrappedFunc<int, string, IEnumerable<object>, int>);

                pd.Invoke(c => {CurrentTask.Events += new Progress(c.Invoke);},
                    typeof (Progress).CreateWrappedProxy(new Progress((id, i, f, o) => Event<Progress>.Raise(id, i, f, o.ByRef()))) as WrappedFunc<int, int, string, IEnumerable<object>, bool>);

                pd.Invoke(c => {CurrentTask.Events += new CompleteProgress(c.Invoke);},
                    typeof (CompleteProgress).CreateWrappedProxy(new CompleteProgress((id, c) => Event<CompleteProgress>.Raise(id,c))) as WrappedFunc<int, bool, bool>);

                pd.Invoke(c => {CurrentTask.Events += new ExceptionThrown(c.Invoke);},
                    typeof (ExceptionThrown).CreateWrappedProxy(new ExceptionThrown((e, m, s) => Event<ExceptionThrown>.Raise(e, m, s))) as WrappedFunc<string, string, string, bool>);

                pd.Invoke(c => {CurrentTask.Events += new GetHostDelegate(c.Invoke);}, typeof (GetHostDelegate).CreateWrappedProxy(new GetHostDelegate(() => Event<GetHostDelegate>.Raise())) as WrappedFunc<Callback>);
                
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
        private static string FindAssembly(string assemblyName) {
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

#if AFTER_CTP
        private static bool TryToLoadNativeProvider(string dllName) {
            // the idea here is to allow someone to code a provider as a native C DLL
            // with a specific set of exports that would allow us to dynamically bind to it.
            //
            // quite a few options for implementation.
            // ideally, a very small exported interface that can be queried to ask for
            // a set of providers.
            //
            // I don't see a really good reason to push this one out into a seperate assembly
            // since it's just an unnecessary further abstraction.

            return false;
        }
#endif
    }

    internal static class ProviderLoader {
        internal static void AcquireProviders(string assemblyPath, Callback callback, Action<string, PackageProviderInstance> yieldPackageProvider, Action<string, ServiceProviderInstance> yieldServiceProvider) {
            var asm = Assembly.LoadFile(assemblyPath);
            if (asm == null) {
                return;
            }
            // check to see if the assembly has something that looks like a Provider class
            var publicTypes = asm.GetTypes().Where(each => each.IsPublic && each.BaseType != typeof(MulticastDelegate)).ToArray();

            // see if there is any MetaProviders that want to hand-craft us some other providers...
            var metaProviderTypes = publicTypes.Where(NewMetaProvider.IsTypeCompatible);

            foreach (var providerType in metaProviderTypes) {
                var provider = new NewMetaProvider(providerType);
                provider.InitializeProvider(callback);
                foreach (var name in provider.GetProviderNames()) {
                    var instance = provider.CreateProvider(name);
                    if (instance != null) {
                        if (PackageProviderInstance.IsInstanceCompatible(instance)) {
                            yieldPackageProvider(name, new PackageProviderInstance(instance));
                            continue;
                        }
                        if (ServiceProviderInstance.IsInstanceCompatible(instance)) {
                            yieldServiceProvider(name, new ServiceProviderInstance(instance));
                            continue;
                        }
                    }
                }
            }

            foreach (var providerType in publicTypes.Where(PackageProviderInstance.IsTypeCompatible)) {
                try {
                    var provider = new PackageProviderInstance(providerType);
                    provider.InitializeProvider(callback);
                    yieldPackageProvider(provider.GetPackageProviderName(), provider);
                } catch (Exception e) {
                    e.Dump();
                }
            }

            foreach (var providerType in publicTypes.Where(ServiceProviderInstance.IsTypeCompatible)) {
                try {
                    var provider = new ServiceProviderInstance(providerType);
                    provider.InitializeProvider(callback);
                    yieldServiceProvider(provider.GetServiceProviderName(), provider);
                } catch (Exception e) {
                    e.Dump();
                }

            }

        }
       
    }

}