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

    [Cmdlet(VerbsLifecycle.Uninstall, PackageNoun, SupportsShouldProcess = true)]
    public class UninstallPackage : GetPackage {
        private Dictionary<string, List<SoftwareIdentity>> _resultsPerName;

        [Parameter]
        public SwitchParameter Force {get; set;}

        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = PackageByObjectSet)]
        public SoftwareIdentity[] Package {get; set;}

        public override bool ProcessRecordAsync() {
            if (IsPackageByObject) {
                return UninstallPackages(Package);
            }

            // otherwise, it's just packages by name 
            if (Name.IsNullOrEmpty()) {
                Error("Required value : Package Name");
                return false;
            }

            _resultsPerName = new Dictionary<string, List<SoftwareIdentity>>();
            Parallel.ForEach(SelectedProviders, provider => {
                foreach (var n in Name) {
                    var c = _resultsPerName.GetOrAdd(n, () => new List<SoftwareIdentity>());
                    foreach (var pkg in ProcessNames(provider, n)) {
                        lock (c) {
                            c.Add(pkg);
                        }
                    }
                }
            });

            return true;
        }

        public override bool EndProcessingAsync() {
            if (_resultsPerName == null) {
                return true;
            }
            // Show errors before?
            foreach (var name in UnprocessedNames) {
                Error("GET_PACKAGE_NOT_FOUND", name);
            }

            if (Stopping) {
                return false;
            }

            foreach (var n in _resultsPerName.Keys) {
                // check if we have a 1 package per name 
                if (_resultsPerName[n].Count > 1) {
                    Error("DISAMBIGUATE_FOR_UNINSTALL", n, _resultsPerName[n]);
                }

                if (Stopping) {
                    return false;
                }

                if (!UninstallPackages(_resultsPerName[n])) {
                    return false;
                }
            }
            return true;
        }

        private bool UninstallPackages(IEnumerable<SoftwareIdentity> packagesToUnInstall) {
            foreach (var pkg in packagesToUnInstall) {
                var provider = SelectProviders(pkg.ProviderName).FirstOrDefault();

                if (provider == null) {
                    Error("UNKNOWN_PROVIDER", pkg.ProviderName);
                    return false;
                }

                try {
                    foreach (var installedPkg in CancelWhenStopped(provider.UninstallPackage(pkg, this))) {
                        if (IsCancelled()) {
                            return false;
                        }
                        WriteObject(installedPkg);
                    }
                } catch (Exception e) {
                    e.Dump();
                    Error("Uninstallation Failure {0}", pkg.Name );
                    return false;
                }
            }
            return true;
        }

        public override bool ShouldProcessPackageUninstall(string packageName, string version) {
            return Force || ShouldProcess(packageName, "Uninstall Package").Result;
        }

        public override bool ShouldContinueAfterPackageUninstallFailure(string packageName, string version, string source) {
            return Force || ShouldContinue("Continue uninstalling after failing '{0}'".format(packageName), "Package uninstall Failure").Result;
        }

        public override bool ShouldContinueRunningUninstallScript(string packageName, string version, string source, string scriptLocation) {
            return Force || ShouldContinue("Should the package uninstall script at '{0}' be executed?".format(scriptLocation), "Package Contains uninstallation Script").Result;
        }
    }
}