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
    using System.Threading.Tasks;
    using Microsoft.OneGet.Extensions;
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Providers.Package;

    [Cmdlet(VerbsCommon.Get, Constants.PackageNoun)]
    public class GetPackage : CmdletWithSearch {
        private readonly Dictionary<string, bool> _namesProcessed = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> _providersProcessed = new Dictionary<string, bool>();

        public GetPackage()
            : base(new[] {
                OptionCategory.Provider, OptionCategory.Install,
            }) {
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

        public override bool ProcessRecordAsync() {
            Parallel.ForEach(SelectedProviders, provider => {
                _providersProcessed.GetOrAdd(provider.Name, () => false);

                try {
                    if (Name.IsNullOrEmpty()) {
                        foreach (var pkg in ProcessProvider(provider)) {
                            WriteObject(pkg);
                        }
                    } else {
                        foreach (var n in Name) {
                            foreach (var pkg in ProcessNames(provider, n)) {
                                WriteObject(pkg);
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
            using (var packages = CancelWhenStopped(provider.GetInstalledPackages("", this))) {
                foreach (var p in packages) {
                    _providersProcessed.AddOrSet(provider.Name, true);
                    yield return p;
                }
            }
        }

        protected IEnumerable<SoftwareIdentity> ProcessNames(PackageProvider provider, string name) {
            _namesProcessed.GetOrAdd(name, () => false);
            using (var packages = CancelWhenStopped(provider.GetInstalledPackages(name, this))) {
                foreach (var p in packages) {
                    _namesProcessed.AddOrSet(name, true);
                    _providersProcessed.AddOrSet(provider.Name, true);
                    yield return p;
                }
            }
        }

        public override bool EndProcessingAsync() {
            foreach (var name in UnprocessedNames) {
                Error(Messages.GetPackageNotFound, name);
            }
            if (!Stopping) {
                foreach (var provider in UnprocessedProviders) {
                    Warning(Constants.NoPackagesFoundForProvider, provider);
                }
            }
            return true;
        }
    }
}