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

namespace Microsoft.OneGet.MetaProvider.PowerShell {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Net.Configuration;
    using Core;
    using Core.Extensions;
    using Callback = System.Func<string, System.Collections.Generic.IEnumerable<object>, object>;

    /// <summary>
    /// A OneGet MetaProvider class that loads Providers implemented as a PowerShell Module.
    /// 
    /// It connects the functions in the PowerShell module to the expected functions that the 
    /// interface expects.
    /// </summary>
    public class PowerShellMetaProvider : IDisposable {
        private readonly Dictionary<string, string> _providerModules = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private List<PowerShellPackageProvider> _providerInstances = new List<PowerShellPackageProvider>();

        internal IEnumerable<string> ScanForModules() {
            using (dynamic ps = new DynamicPowershell()) {
                DynamicPowershellResult results = ps.ImportModule(ListAvailable: true);
                foreach (var result in results) {
                    var module = result as PSModuleInfo;
                    if (module != null) {

                        var privateData = module.PrivateData as Hashtable;
                        if (privateData != null) {
                            var providers = privateData.GetStringCollection("OneGet.Providers").ToArray();
                            if (providers.Length > 0) {
                                // found a module that is advertizing one or more OneGet Providers.
                                var folder = Path.GetDirectoryName(module.Path);

                                foreach (var provider in providers) {
                                    var fullPath = provider;
                                    if (!Path.IsPathRooted(provider)) {
                                        fullPath = Path.GetFullPath(Path.Combine(folder, provider));
                                    }
                                    if (Directory.Exists(fullPath) || File.Exists(fullPath)) {
                                        // looks like we have something that could definitely be a 
                                        // a module path.

                                        yield return fullPath;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal object CreateProvider(string name) {
            if (_providerModules.ContainsKey(name)) {
                var provider = Create(_providerModules[name]);
                _providerInstances.Add(provider);
                return provider;
            }
            // create the instance
            throw new Exception("No provider by name '{0}' registered.".format(name));
        }

        internal IEnumerable<string> GetProviderNames() {
            return _providerModules.Keys;
        }

        internal void ReleaseProvider() {
            
        }

        /// <summary>
        /// The name of this MetaProvider class
        /// </summary>
        public string MetaProviderName {
            get {
                return "PowerShell";
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                var pi = _providerInstances;
                _providerInstances = null;

                foreach(var i in pi) {
                    i.Dispose();
                }
            }
        }

      
        private PowerShellPackageProvider Create(string psModule) {
            dynamic ps = new DynamicPowershell();
            try {
                DynamicPowershellResult result = ps.ImportModule(Name: psModule, PassThru: true);
                var providerModule = result.Value as PSModuleInfo;
                if (result.Success && result.Value != null) {

                    //result = ps.GetPackageProviderName();
                    //if (result.Success) {
                    try {
                        var newInstance = new PowerShellPackageProvider(ps, providerModule);
                        _providerInstances.Add(newInstance);
                        return newInstance;

                    } catch (Exception e) {
                        e.Dump();
                    }
                    //}
                }
            } catch (Exception e) {
                // something didn't go well.
                // skip it.
                e.Dump();
            }

            // didn't import correctly.
            ps.Dispose();
            return null;
        }

        public void InitializeProvider(Callback c) {
            if (c == null) {
                throw new ArgumentNullException("c");
            }

            // to do : get modules to load (from configuration ?)
            var modules = ScanForModules().ToArray();

            // try to create each module at least once.
            foreach (var modulePath in modules) {
                var provider = Create(modulePath);
                if (provider != null) {
                    // looks good to me, let's add this to the list of moduels this meta provider can create.
                    _providerModules.AddOrSet(provider.GetPackageProviderName(), modulePath);
                }
            }

            // check to see if dynamic powershell is working:
            /*
            Core.DynamicPowershellResult results = _powerShell.dir("c:\\");

            foreach (var result in results) {
                Console.WriteLine(result);
            }
            */

            // search the configuration data for any PowerShell providers we're supposed to load.
            /* var modules = getConfigStrings.GetStringCollection("Providers/Module");
            _powerShell = new Core.DynamicPowershell();

            var results = _powerShell.GetChildItem("c:\\");
            foreach ( var result in results ) {
                Console.WriteLine(result);
            }
             */
        }

    }
}
