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

namespace Microsoft.PackageManagement.MetaProvider.PowerShell {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Reflection;
    using System.Threading.Tasks;
    using Api;
    using Utility.Extensions;
    using Utility.Plugin;
    using Utility.PowerShell;

    /// <summary>
    ///     A  MetaProvider class that loads Providers implemented as a PowerShell Module.
    ///     It connects the functions in the PowerShell module to the expected functions that the
    ///     interface expects.
    /// </summary>
    public class PowerShellMetaProvider : IDisposable {
        private static readonly HashSet<string> _exclusionList = new HashSet<string> {
            "AppBackgroundTask",
            "AppLocker",
            "Appx",
            "AssignedAccess",
            "BitLocker",
            "BitsTransfer",
            "BranchCache",
            "CimCmdlets",
            "Defender",
            "DirectAccessClientComponents",
            "Dism",
            "DnsClient",
            "Hyper-V",
            "International",
            "iSCSI",
            "ISE",
            "Kds",
            "Microsoft.PowerShell.Diagnostics",
            "Microsoft.PowerShell.Host",
            "Microsoft.PowerShell.Management",
            "Microsoft.PowerShell.Security",
            "Microsoft.PowerShell.Utility",
            "Microsoft.WSMan.Management",
            "MMAgent",
            "MsDtc",
            "NetAdapter",
            "NetConnection",
            "NetEventPacketCapture",
            "NetLbfo",
            "NetNat",
            "NetQos",
            "NetSecurity",
            "NetSwitchTeam",
            "NetTCPIP",
            "NetWNV",
            "NetworkConnectivityStatus",
            "NetworkTransition",
            "PcsvDevice",
            "PKI",
            "PrintManagement",
            "PSDiagnostics",
            "PSScheduledJob",
            "PSWorkflow",
            "PSWorkflowUtility",
            "ScheduledTasks",
            "SecureBoot",
            "SmbShare",
            "SmbWitness",
            "StartScreen",
            "Storage",
            "TLS",
            "TroubleshootingPack",
            "TrustedPlatformModule",
            "VpnClient",
            "Wdac",
            "WindowsDeveloperLicense",
            "WindowsErrorReporting",
            "WindowsSearch",
            "PackageManagement", // dont' search ourselves.
            "OneGet", // dont' search ourselves.
            "OneGet-Edge" // dont' search ourselves.
        };

        private readonly Dictionary<string, PowerShellPackageProvider> _packageProviders = new Dictionary<string, PowerShellPackageProvider>(StringComparer.OrdinalIgnoreCase);

        private static string _baseFolder;
        internal static string BaseFolder {
            get {
                if (_baseFolder == null) {
                    _baseFolder = Path.GetDirectoryName(Path.GetFullPath(Assembly.GetExecutingAssembly().Location));
                    if (_baseFolder == null || !Directory.Exists(_baseFolder)) {
                        throw new Exception("MSG:CantFindBasePowerShellModuleFolder");
                    }
                }
                return _baseFolder;
            }
        }

        private static string _powersehllProviderFunctionsPath;
        internal static string PowerShellProviderFunctions {
            get {
                if (_powersehllProviderFunctionsPath == null) {
                    // try the etc directory
                    _powersehllProviderFunctionsPath = Path.Combine(BaseFolder, "etc", "PackageProviderFunctions.psm1");
                    if (!File.Exists(_powersehllProviderFunctionsPath)) {
                        // fall back to the same directory.
                        _powersehllProviderFunctionsPath = Path.Combine(BaseFolder, "PackageProviderFunctions.psm1");
                        if (!File.Exists(_powersehllProviderFunctionsPath)) {
                            // oh-oh, no powershell functions file.
                            throw new Exception("MSG:UnableToFindPowerShellFunctionsFile");
                        }
                    }
                }
                return _powersehllProviderFunctionsPath;
            }
        }

        public IEnumerable<string> ProviderNames {
            get {
                return _packageProviders.Keys;
            }
        }

        /// <summary>
        ///     The name of this MetaProvider class
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

        internal IEnumerable<string> ScanPrivateDataForProviders(string baseFolder, Hashtable privateData) {
            var providers = privateData.GetStringCollection("PackageManagementProviders").ToArray();
            if (providers.Length > 0) {
                // found a module that is advertizing one or more  Providers.

                foreach (var provider in providers) {
                    var fullPath = provider;
                    try {
                        if (!Path.IsPathRooted(provider)) {
                            fullPath = Path.GetFullPath(Path.Combine(baseFolder, provider));
                        }
                    } catch {
                        // got an error from the path.
                        continue;
                    }
                    if (Directory.Exists(fullPath) || File.Exists(fullPath)) {
                        // looks like we have something that could definitely be a
                        // a module path.
                        yield return fullPath;
                    }
                }
            }
        }

        private IEnumerable<PSModuleInfo> ModulesFromResult(DynamicPowershellResult result) {
            if (result.Success && result.Value != null) {
                foreach (var module in result.OfType<PSModuleInfo>()) {
                    yield return module;
                }
            }
        }

        private IEnumerable<string> GetPackageManagementModules(PSModuleInfo module) {
            // skip modules that we know don't contain any PM modules
            if (!_exclusionList.Contains(module.Name)) {
                var privateData = module.PrivateData as Hashtable;
                if (privateData != null) {
                    return ScanPrivateDataForProviders(Path.GetDirectoryName(module.Path), privateData);
                }
            }
            return Enumerable.Empty<string>();
        }

        internal IEnumerable<string> ScanForModules(PsRequest request) {
            // two places we search for modules
            // 1. in this assembly's folder, look for all psd1 and psm1 files.
            //
            // 2. modules in the PSMODULEPATH
            //
            // Import each one of those, and check to see if they have a PackageManagementProviders section in their private data

            using (var ps = new DynamicPowershell()) {

                // load the powershell functions into this runspace in case something needed it on module load.
                var psf = ps.ImportModule(PowerShellProviderFunctions, true);

                // scan all the ps modules in the folders provided
                foreach (var each in AlternativeModuleScan(request)) {
                    foreach (var ogModule in ps.TestModuleManifest(each).SelectMany(GetPackageManagementModules)) {
                        yield return ogModule;
                    }
                }
            }
        }

        private IEnumerable<string> AlternativeModuleScan(PsRequest request) {
            var psModulePath = Environment.GetEnvironmentVariable("PSModulePath") ?? "";

            IEnumerable<string> paths = psModulePath.Split(new char[]{';'} , StringSplitOptions.RemoveEmptyEntries);

            var sysRoot = Environment.GetEnvironmentVariable("systemroot");
            var userProfile = Environment.GetEnvironmentVariable("userprofile");

            // add assumed paths just in case the environment variable isn't really set.
            paths = paths.ConcatSingleItem( Path.Combine(sysRoot, @"system32\WindowsPowerShell\v1.0\Modules"));
            paths = paths.ConcatSingleItem(Path.Combine(userProfile, @"Documents\WindowsPowerShell\Modules"));

            if (!string.IsNullOrWhiteSpace(BaseFolder) && BaseFolder.DirectoryExists()) {
                paths = paths.ConcatSingleItem(BaseFolder);
            }

            paths = paths.Distinct().ToArray();

            return paths.Where(each => each.DirectoryExists()).SelectMany(each => Directory.EnumerateDirectories(each).Where(dir => !_exclusionList.Contains(Path.GetFileName(dir))), (p, child) => Path.Combine(child, Path.GetFileName(child) + ".psd1")).Where(moduleName => File.Exists(moduleName) && File.ReadAllText(moduleName).IndexOf("PackageManagementProviders", StringComparison.OrdinalIgnoreCase) > -1);
        }

        public object CreateProvider(string name) {
            if (_packageProviders.ContainsKey(name)) {
                return _packageProviders[name];
            }
            // create the instance
            throw new Exception("No provider by name '{0}' registered.".format(name));
        }

        internal void ReleaseProvider() {
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                var pi = _packageProviders.Values;
                _packageProviders.Clear();

                foreach (var i in pi) {
                    i.Dispose();
                }
            }
        }

        private PowerShellPackageProvider Create(PsRequest req, string psModule) {
            var ps = new DynamicPowershell();
            try {
                // load the powershell provider functions into this runspace.
                var psf = ps.ImportModule(PowerShellProviderFunctions, true);
                DynamicPowershellResult result = ps.ImportModule(psModule, true);

                if (!result.LastIsTerminatingError) {
                    var providerModule = result.Value as PSModuleInfo;
                    if (result.Success && providerModule != null) {
                        try {
                            return new PowerShellPackageProvider(ps, providerModule);
                        } catch (Exception e) {
                            e.Dump();
                        }
                    }
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

        public void InitializeProvider(PsRequest request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            request.Debug("Initializing PowerShell MetaProvider");

            // to do : get modules to load (from configuration ?)
            var modules = ScanForModules(request).Distinct().ToArray();

            // try to create each module at least once.
            modules.ParallelForEach(modulePath => {
                request.Debug("Attempting to load PowerShell Provider Module [{0}]", modulePath);
                var provider = Create(request,modulePath);
                if (provider != null) {
                    if (provider.GetPackageProviderName() != null) {
                        request.Debug("Loaded PowerShell Package Provider Module: [{0}]", modulePath);
                        // looks good to me, let's add this to the list of moduels this meta provider can create.
                        _packageProviders.AddOrSet(provider.GetPackageProviderName(), provider);
                    } else {
                        provider.Dispose();
                        provider = null;
                    }
                }

            });

            request.Debug("Loaded PowerShell Provider Modules ");
        }
    }
}
