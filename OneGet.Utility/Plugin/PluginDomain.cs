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


namespace Microsoft.OneGet.Plugin {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Extensions;

    internal partial class PluginDomain : MarshalByRefObject, IDisposable {
        private AppDomain _appDomain;
        private string _identity;

        private Proxy<PluginAssemblyResolver> _proxyResolver;
        private PluginAssemblyResolver _resolver;
        public static Dictionary<string,string> DynamicAssemblyPaths = new Dictionary<string, string>();

        public override object InitializeLifetimeService() {
            return null;
        }

        internal string ResolveFromThisDomain(string name) {
            if (DynamicAssemblyPaths != null && DynamicAssemblyPaths.ContainsKey(name)) {
                return DynamicAssemblyPaths[name];
            }
            return AppDomain.CurrentDomain.GetAssemblies().Where( each => each.FullName == name ).Select( each => each.Location).FirstOrDefault();
        }

        internal void RegisterDynamicAssembly(string fullName, string fullPath) {
            DynamicAssemblyPaths.Add(fullName, fullPath);
        }

        internal PluginDomain(string name) :
            this(name,new AppDomainSetup {
                ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                PrivateBinPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                LoaderOptimization = LoaderOptimization.MultiDomain
            }) {
        }

        internal PluginDomain(string name, AppDomainSetup appDomainSetup) {
            if (appDomainSetup == null) {
                throw new ArgumentNullException("appDomainSetup");
            }
            _identity = name ?? Guid.NewGuid().ToString();
            appDomainSetup.ApplicationName = appDomainSetup.ApplicationName ?? "PluginDomain" + _identity;

            _appDomain = AppDomain.CreateDomain(_identity, null, appDomainSetup);
            
            _resolver = new PluginAssemblyResolver();
            _resolver.AddPath(appDomainSetup.ApplicationBase);
            _resolver.AddPath(appDomainSetup.PrivateBinPath);
            _resolver.SetAlternatePathResolver(ResolveFromThisDomain);
            AppDomain.CurrentDomain.AssemblyResolve += _resolver.Resolve;
            
            _proxyResolver = new Proxy<PluginAssemblyResolver>(this);
            
            ((PluginAssemblyResolver)_proxyResolver).AddPath(appDomainSetup.ApplicationBase);
            ((PluginAssemblyResolver)_proxyResolver).AddPath(appDomainSetup.PrivateBinPath);
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies()) {
                if (!a.IsDynamic) {
                    var folder = Path.GetDirectoryName(a.Location);
                    if (folder != null && Directory.Exists(folder)) {
                        ((PluginAssemblyResolver)_proxyResolver).AddPath(folder);
                    }
                }
            }

            // push this assembly into the other domain.
            LoadFileWithReferences(Assembly.GetExecutingAssembly().Location);
            
            // setup the assembly resolver
            Invoke(resolver => {_appDomain.AssemblyResolve+= resolver.Resolve;}, ((PluginAssemblyResolver)_proxyResolver));

            // if that can't find an assembly, ask this domain.
            Invoke(resolver => { resolver.SetAlternatePathResolver(ResolveFromThisDomain); }, ((PluginAssemblyResolver)_proxyResolver));
            
            // Give the plugin domain a way of telling us what Dynamic Assemblies they are creating.
            _appDomain.SetData("RegisterDynamicAssembly", new Action<string, string>(RegisterDynamicAssembly));
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_proxyResolver != null) {
                    _proxyResolver.Dispose();
                    _proxyResolver = null;
                }

                _resolver = null;
                if (_appDomain != null) {
                    try {
                        AppDomain.Unload(_appDomain);
                    } catch (Exception e) {
                        e.Dump();
                    }
                }
                _appDomain = null;
            }
        }

        public Assembly LoadFrom(string path) {
            return ((PluginAssemblyResolver)_proxyResolver).LoadFromWithReferences(path).FirstOrDefault();
        }

        public Assembly LoadFile(string path) {
            return ((PluginAssemblyResolver)_proxyResolver).LoadFileWithReferences(path).FirstOrDefault();
        }

        public Assembly LoadBinary(string path) {
            return ((PluginAssemblyResolver)_proxyResolver).LoadBinaryWithReferences(path).FirstOrDefault();
        }

        public IList<Assembly> LoadFromWithReferences(string path) {
            return ((PluginAssemblyResolver)_proxyResolver).LoadFromWithReferences(path);
        }

        public IList<Assembly> LoadFileWithReferences(string path) {
            return ((PluginAssemblyResolver)_proxyResolver).LoadFileWithReferences(path);
        }

        public IList<Assembly> LoadBinaryWithReferences(string path) {
            return ((PluginAssemblyResolver)_proxyResolver).LoadBinaryWithReferences(path);
        }

        public static implicit operator AppDomain(PluginDomain domain) {
            if (domain == null) {
                throw new ArgumentNullException("domain");
            }

            return domain._appDomain;
        }
    }
}
