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
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.OneGet.Implementation;
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Utility.Async;
    using Microsoft.OneGet.Utility.Collections;
    using Microsoft.OneGet.Utility.Extensions;

    [Cmdlet(VerbsCommon.Get, Constants.PackageNoun)]
    public class GetPackage : CmdletWithSearch {
        private readonly Dictionary<string, bool> _namesProcessed = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> _providersProcessed = new Dictionary<string, bool>();

        public GetPackage()
            : base(new[] {
                OptionCategory.Provider, OptionCategory.Install
            }) {
        }

        protected override IEnumerable<string> ParameterSets {
            get {
                return new[] {""};
            }
        }

        protected IEnumerable<string> UnprocessedNames {
            get {
                return _namesProcessed.Keys.Where(each => !_namesProcessed[each]);
            }
        }

        protected IEnumerable<string> UnprocessedProviders {
            get {
                return _providersProcessed.Keys.Where(each => !_providersProcessed[each]);
            }
        }

        protected bool IsPackageInVersionRange(SoftwareIdentity pkg) {
            if (RequiredVersion != null && SoftwareIdentityVersionComparer.CompareVersions(pkg.VersionScheme, pkg.Version, RequiredVersion) != 0) {
                return false;
            }

            if (MinimumVersion != null && SoftwareIdentityVersionComparer.CompareVersions(pkg.VersionScheme, pkg.Version, MinimumVersion) < 0) {
                return false;
            }

            if (MaximumVersion != null && SoftwareIdentityVersionComparer.CompareVersions(pkg.VersionScheme, pkg.Version, MaximumVersion) > 0) {
                return false;
            }

            return true;
        }

        public override bool ProcessRecordAsync() {
            SelectedProviders.ParallelForEach(provider => {
                _providersProcessed.GetOrAdd(provider.ProviderName, () => false);

                try {
                    if (Name.IsNullOrEmpty()) {
                        foreach (var pkg in ProcessProvider(provider)) {
                            if (IsPackageInVersionRange(pkg)) {
                                WriteObject(pkg);
                            }
                        }
                    } else {
                        foreach (var n in Name) {
                            foreach (var pkg in ProcessNames(provider, n)) {
                                if (IsPackageInVersionRange(pkg)) {
                                    WriteObject(pkg);
                                }
                            }
                        }
                    }
                } catch (Exception e) {
                    e.Dump();
                }
            });
            return true;
        }

        protected IEnumerable<SoftwareIdentity> ProcessProvider(PackageProvider provider) {
            using (var packages = provider.GetInstalledPackages("", this).CancelWhen(_cancellationEvent.Token)) {
                foreach (var p in packages) {
                    _providersProcessed.AddOrSet(provider.ProviderName, true);
                    yield return p;
                }
            }
        }

        protected IEnumerable<SoftwareIdentity> ProcessNames(PackageProvider provider, string name) {
            _namesProcessed.GetOrAdd(name, () => false);
            using (var packages = provider.GetInstalledPackages(name, this).CancelWhen(_cancellationEvent.Token)) {
                foreach (var p in packages) {
                    _namesProcessed.AddOrSet(name, true);
                    _providersProcessed.AddOrSet(provider.ProviderName, true);
                    yield return p;
                }
            }
        }

        public override bool EndProcessingAsync() {
            foreach (var name in UnprocessedNames) {
                Error(Errors.NoMatchFound, name);
            }
            if (!Stopping) {
                foreach (var provider in UnprocessedProviders) {
                    Debug(Constants.NoPackagesFoundForProvider, provider);
                }
            }
            return true;
        }
    }
}