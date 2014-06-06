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
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Net.Configuration;
    using System.Reflection;
    using System.Threading.Tasks;
    using Core;
    using Core.Extensions;
    using Callback = System.Object;

    /// <summary>
    /// A OneGet MetaProvider class that loads Providers implemented as a PowerShell Module.
    /// 
    /// It connects the functions in the PowerShell module to the expected functions that the 
    /// interface expects.
    /// </summary>
    public class PowerShellMetaProvider : IDisposable {
        private readonly Dictionary<string, string> _providerModules = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private List<PowerShellPackageProvider> _providerInstances = new List<PowerShellPackageProvider>();

        internal IEnumerable<string> ScanPrivateDataForProviders(string baseFolder, Hashtable privateData) {
            var providers = privateData.GetStringCollection("OneGet.Providers").ToArray();
            if (providers.Length > 0) {
                // found a module that is advertizing one or more OneGet Providers.

                foreach (var provider in providers) {
                    Debug.WriteLine("Is {0} a oneget module?".format(provider));
                    var fullPath = provider;
                    if (!Path.IsPathRooted(provider)) {
                        fullPath = Path.GetFullPath(Path.Combine(baseFolder, provider));
                    }
                    if (Directory.Exists(fullPath) || File.Exists(fullPath)) {
                        // looks like we have something that could definitely be a 
                        // a module path.
                        Debug.WriteLine("Possible module {0} ?".format(fullPath));
                        yield return fullPath;
                    }
                }
            }
        }

        internal static string PowerShellProviderFunctions {
            get {
                 var thisFolder = Path.GetDirectoryName(Path.GetFullPath(Assembly.GetExecutingAssembly().Location));
                 return Path.Combine(thisFolder, "etc", "PackageProviderFunctions.psm1");
            }
        }

        private IEnumerable<PSModuleInfo> ModulesFromResult(DynamicPowershellResult result) {
            if (result.Success && result.Value != null) {
                foreach (var r in result) {
                    var module = r as PSModuleInfo;
                    if (module != null) {
                        yield return module;
                    }
                }
            }
        }

        private IEnumerable<string> GetOneGetModules(PSModuleInfo module) {
            var privateData = module.PrivateData as Hashtable;
            if (privateData != null) {
                return ScanPrivateDataForProviders(Path.GetDirectoryName(module.Path), privateData);
            }
            return Enumerable.Empty<string>();
        }

        internal IEnumerable<string> ScanForModules(Request request) {
            
            // two places we search for modules 
            // 1. in this assembly's folder, look for all psd1 and psm1 files. 
            // 
            // 2. modules in the PSMODULEPATH
            //
            // Import each one of those, and check to see if they have a OneGet.Providers section in their private data

            using (dynamic ps = new DynamicPowershell()) {

                var thisFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (thisFolder != null) {
                    var files = Directory.EnumerateFiles(thisFolder, "*.psm1").Concat(Directory.EnumerateFiles(thisFolder, "*.psd1"));

                    // request.Debug("PSF: {0}", PowerShellProviderFunctions);

                    var psf = ps.ImportModule(Name: PowerShellProviderFunctions, PassThru: true);
                    

                    foreach (var each in files) {
                        
                        DynamicPowershellResult items =ps.ImportModule(Name: each, PassThru: true);
                        items.Wait();
                        var errors = items.Errors.ToArray();

                        if (errors.Any()) {
                            request.Debug("\r\n\r\n==================================================================================\r\n===In Module '{0}'",each);

                            foreach (var error in errors) {
                                try {
                                    switch (error.CategoryInfo.Category) {
                                        case ErrorCategory.ResourceUnavailable:
                                            // file not found
                                            continue;

                                        default:
                                            request.Debug("  PowerShell {0} {1} ", error.CategoryInfo.Category, error.Exception.Message);
                                            break;
                                    }

                                } catch (Exception e) {
                                    e.Dump();
                                }
                            }
                            continue;
                        }

                        
                        foreach (var i in items) {
                            var module = i as PSModuleInfo;
                            if (module != null) {
                                foreach (var o in GetOneGetModules(module)) {
                                    yield return o;
                                }
                            }
                        }
                    }
                    // results = files.SelectMany(each => ModulesFromResult((DynamicPowershellResult)ps.ImportModule(Name: each, PassThru: true))).SelectMany(GetOneGetModules).ToArray();
                }

#if !xDEBUG

                foreach (var m in ModulesFromResult((DynamicPowershellResult)ps.GetModule(ListAvailable: true)).SelectMany(GetOneGetModules)) {
                    yield return m;
                };
#endif

            }
        }

        public object CreateProvider(string name) {
            if (_providerModules.ContainsKey(name)) {
                var provider = Create(_providerModules[name]);
                _providerInstances.Add(provider);
                return provider;
            }
            // create the instance
            throw new Exception("No provider by name '{0}' registered.".format(name));
        }

        public IEnumerable<string> ProviderNames {
            get {
                return _providerModules.Keys;
            }
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

                var psf = ps.ImportModule(Name: PowerShellProviderFunctions, PassThru: true);

                DynamicPowershellResult result = ps.ImportModule(Name: psModule, PassThru: true);
                var providerModule = result.Value as PSModuleInfo;
                if (result.Success && providerModule != null) {

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

        public void InitializeProvider(object dynamicInterface, Callback c) {
            DynamicExtensions.DynamicInterface = dynamicInterface;

            if (c == null) {
                throw new ArgumentNullException("c");
            }

            var req = c.As<Request>();

            // to do : get modules to load (from configuration ?)
            var modules = ScanForModules(req).ToArray();

            // try to create each module at least once.
            Parallel.ForEach(modules, modulePath => {

                // foreach (var modulePath in modules) {
                Debug.WriteLine(string.Format("Looking at {0}", modulePath));
                var provider = Create(modulePath);
                if (provider != null) {
                    // looks good to me, let's add this to the list of moduels this meta provider can create.
                    _providerModules.AddOrSet(provider.GetPackageProviderName(), modulePath);
                }
            });

            Debug.WriteLine(string.Format("Done Looking at PowerShell Modules"));

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
