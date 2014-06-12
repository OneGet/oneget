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

namespace Microsoft.PowerShell.OneGet.CmdLets {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Security;
    using Microsoft.OneGet;
    using Microsoft.OneGet.Core.Extensions;
    using Microsoft.OneGet.Core.Providers.Package;
    using Utility;

    public abstract class CmdletBase : AsyncCmdlet {
        public const string PackageNoun = "Package";
        public const string PackageSourceNoun = "PackageSource";
        public const string PackageProviderNoun = "PackageProvider";

        public const string PackageBySearchSet = "PackageBySearch";
        public const string PackageByObjectSet = "PackageByObject";
        public const string SourceByObjectSet = "SourceByObject";
        public const string ProviderByObjectSet = "ProviderByObject";
        public const string ProviderByNameSet = "ProviderByName";
        public const string OverwriteExistingSourceSet = "OverwriteExistingSource";

        private static readonly object _lockObject = new object();
        private readonly Hashtable _dynamicOptions;
        private readonly IPackageManagementService _packageManagementService = new PackageManagementService().Instance;

        protected CmdletBase() {
            _dynamicOptions = new Hashtable();
        }

        protected bool IsPackageBySearch {
            get {
                return ParameterSetName == PackageBySearchSet;
            }
        }

        protected bool IsPackageByObject {
            get {
                return ParameterSetName == PackageByObjectSet;
            }
        }

        protected bool IsSourceByObject {
            get {
                return ParameterSetName == SourceByObjectSet;
            }
        }

        protected bool IsProviderByObject {
            get {
                return ParameterSetName == ProviderByObjectSet;
            }
        }

        protected bool IsOverwriteExistingSource {
            get {
                return ParameterSetName == OverwriteExistingSourceSet;
            }
        }

        protected internal IPackageManagementService PackageManagementService {
            get {
                lock (_lockObject) {
                    if (!IsCancelled() && !IsInitialized) {
                        try {
                            IsInitialized = _packageManagementService.Initialize(this, !IsInvocation);
                        } catch (Exception e) {
                            e.Dump();
                        }
                    }
                }
                return _packageManagementService;
            }
        }

        public Hashtable DynamicOptions {
            get {
                return _dynamicOptions;
            }
        }

        public virtual IEnumerable<string> SpecifiedPackageSources {
            get {
                return null;
            }
            set {
            }
        }

        protected override void Init() {
            if (!IsInitialized) {
                // get the service ( forces initialization )
                var x = PackageManagementService;
            }
        }

        protected IEnumerable<PackageProvider> SelectProviders(string[] names) {
            if (names.IsNullOrEmpty()) {
                return PackageManagementService.SelectProviders(null);
            }
            return names.SelectMany(each => PackageManagementService.SelectProviders(each));
        }

        protected IEnumerable<PackageProvider> SelectProviders(string name) {
            var result = PackageManagementService.SelectProviders(name).ToArray();
            if (result.Length == 0) {
                Warning("UNKNOWN_PROVIDER", name);
            }
            return result;
        }

        public override bool ConsumeDynamicParameters() {
            // pull data from dynamic parameters and place them into the DynamicOptions collection.
            foreach (var rdp in DynamicParameters.Keys.Select(d => DynamicParameters[d]).Where(rdp => rdp.IsSet)) {
                if (rdp.ParameterType == typeof (SwitchParameter)) {
                    _dynamicOptions[rdp.Name] = true;
                } else {
                    _dynamicOptions[rdp.Name] = rdp.Value;
                }
            }
            return true;
        }

        public virtual bool ShouldProcessPackageInstall(string packageName, string version, string source) {
            Message("ShouldProcessPackageInstall", packageName);
            return false;
        }

        public virtual bool ShouldProcessPackageUninstall(string packageName, string version) {
            Message("ShouldProcessPackageUnInstall", packageName);
            return false;
        }

        public virtual bool ShouldContinueAfterPackageInstallFailure(string packageName, string version, string source) {
            Message("ShouldContinueAfterPackageInstallFailure", packageName);
            return false;
        }

        public virtual bool ShouldContinueAfterPackageUninstallFailure(string packageName, string version, string source) {
            Message("ShouldContinueAfterPackageUnInstallFailure", packageName);
            return false;
        }

        public virtual bool ShouldContinueRunningInstallScript(string packageName, string version, string source, string scriptLocation) {
            Message("ShouldContinueRunningInstallScript", packageName);
            return false;
        }

        public virtual bool ShouldContinueRunningUninstallScript(string packageName, string version, string source, string scriptLocation) {
            Message("ShouldContinueRunningUninstallScript", packageName);
            return true;
        }

        public virtual bool AskPermission(string permission) {
            Message("ASK_PERMISSION", permission);
            return true;
        }

        public IEnumerable<string> GetOptionKeys(int category) {
            return DynamicParameters.Values.OfType<CustomRuntimeDefinedParameter>().Where(each => each.IsSet && each.Options.Any(o => (int)o.Category == category)).Select(each => each.Name).ByRef();
        }

        public IEnumerable<string> GetOptionValues(int category, string key) {
            return DynamicParameters.Values.OfType<CustomRuntimeDefinedParameter>().Where(each => each.IsSet && each.Options.Any(o => (int)o.Category == category) && each.Name == key).SelectMany(each => each.Values).ByRef();
        }

        public virtual string GetCredentialUsername() {
            return null;
        }

        public virtual SecureString GetCredentialPassword() {
            return null;
        }

        public virtual bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource) {
            Message("ShouldContinueWithUntrustedPackageSource", packageSource);
            return true;
        }
    }
}