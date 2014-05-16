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
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using System.Threading.Tasks;
    using Microsoft.OneGet;
    using Microsoft.OneGet.Core.Api;
    using Microsoft.OneGet.Core.Extensions;
    using Microsoft.OneGet.Core.Packaging;
    using Microsoft.OneGet.Core.Tasks;

    [Cmdlet(VerbsLifecycle.Uninstall, PackageNoun, SupportsShouldProcess = true)]
    public class UninstallPackage : PackagingCmdlet {
        [Parameter]
        public Hashtable InstallOptions {get; set;}

        [Parameter]
        public SwitchParameter Force {get; set;}

        [Parameter(Mandatory = true, Position=0,ValueFromPipeline = true, ParameterSetName = "PackageByObject")]
        public SoftwareIdentity[] Package { get; set; }


        private List<SoftwareIdentity> _packages = new List<SoftwareIdentity>();

        public override bool BeginProcessingAsync() {
            return true;
        }

        public override bool ProcessRecordAsync() {
            if (!Package.IsNullOrEmpty()) {
                // we've been given packages. Easy from here.
                _packages.AddRange(Package);
            }
            return true;
        }

        public override bool EndProcessingAsync() {
            if (!_packages.IsNullOrEmpty()) {
                // we've been given packages. Easy from here.
                return UninstallPackages(_packages.ToArray());
            }

            if (Name.IsNullOrEmpty()) {
                Event<Error>.Raise("Required value : Package Name");
                return false;
            }
            
            var noMatchNames = new HashSet<string>(Name);
            var resultsPerName = new Dictionary<string, List<SoftwareIdentity>>();


            Parallel.ForEach(_providers.Value, provider => {
                try {
                    foreach (var name in Name) {
                        using (var packages = CancelWhenStopped(provider.GetInstalledPackages(name, Invoke))) {
                            foreach (var p in packages) {
                                lock (resultsPerName) {
                                    resultsPerName.GetOrAdd(name, () => new List<SoftwareIdentity>()).Add(p);
                                }
                                noMatchNames.IfPresentRemoveLocked(name);
                            }
                        }
                    }
                } catch (Exception e) {
                    e.Dump();
                }

            });

            // check if we have a 1 package per name 
            if (noMatchNames.Any()) {
                // whine about things not matched.
                foreach (var name in noMatchNames) {
                    Event<Error>.Raise("No Package Found {0}",new [] { name });
                }

                // not going to uninstall.
                return false;
            }

            var failing = false;
            foreach (var key in resultsPerName.Keys) {
                if (resultsPerName[key].Count > 1) {
                    failing = true;
                    foreach (var foundPackage in resultsPerName[key]) {
                        Event<Error>.Raise( " '{0}' matches multiple packages '{1}'", new[] {
                        key, foundPackage.Name
                    }); 
                    }
                }
            }

            
            if (failing) {
                return false;
            }

            var packagesToUnInstall = resultsPerName.Values.SelectMany(each => each);

            return UninstallPackages(packagesToUnInstall);
        }

        private bool UninstallPackages(IEnumerable<SoftwareIdentity> packagesToUnInstall) {
            foreach (var pkg in packagesToUnInstall) {
                var provider = PackageManagementService.SelectProviders(pkg.ProviderName).FirstOrDefault();

                try {
                    foreach (var installedPkg in CancelWhenStopped(provider.UninstallPackage(pkg, Invoke))) {
                        if (IsCancelled()) {
                            return false;
                        }
                        WriteObject(installedPkg);
                    }
                } catch (Exception e) {
                    e.Dump();
                    Event<Error>.Raise("Uninstallation Failure {0}",new [] {pkg.Name});
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