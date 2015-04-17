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

namespace Microsoft.PowerShell.PackageManagement.Cmdlets {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.PackageManagement.Packaging;
    using Microsoft.PackageManagement.Utility.Async;
    using Microsoft.PackageManagement.Utility.Extensions;

    [Cmdlet(VerbsLifecycle.Uninstall, Constants.Nouns.PackageNoun, SupportsShouldProcess = true, HelpUri = "http://go.microsoft.com/fwlink/?LinkID=517142")]
    public sealed class UninstallPackage : GetPackage {
        private Dictionary<string, List<SoftwareIdentity>> _resultsPerName = new Dictionary<string, List<SoftwareIdentity>>();

        protected override IEnumerable<string> ParameterSets {
            get {
                return new[] {Constants.ParameterSets.PackageByInputObjectSet, Constants.ParameterSets.PackageBySearchSet};
            }
        }

        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = Constants.ParameterSets.PackageByInputObjectSet)]
        public SoftwareIdentity[] InputObject {get; set;}

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        public override string[] Name {get; set;}

        [Parameter(ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        public override string RequiredVersion {get; set;}

        [Alias("Version")]
        [Parameter(ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        public override string MinimumVersion {get; set;}

        [Parameter(ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        public override string MaximumVersion {get; set;}

        protected override void GenerateCmdletSpecificParameters(Dictionary<string, object> unboundArguments) {
            if (!IsInvocation) {
                var providerNames = PackageManagementService.AllProviderNames;
                var whatsOnCmdline = GetDynamicParameterValue<string[]>("ProviderName");
                if (whatsOnCmdline != null) {
                    providerNames = providerNames.Concat(whatsOnCmdline).Distinct();
                }

                DynamicParameterDictionary.AddOrSet("ProviderName", new RuntimeDefinedParameter("ProviderName", typeof(string[]), new Collection<Attribute> {
                    new ParameterAttribute {
                        ValueFromPipelineByPropertyName = true,
                        ParameterSetName = Constants.ParameterSets.PackageBySearchSet
                    },
                    new AliasAttribute("Provider"),
                    new ValidateSetAttribute(providerNames.ToArray())
                }));
            }
            else {
                DynamicParameterDictionary.AddOrSet("ProviderName", new RuntimeDefinedParameter("ProviderName", typeof(string[]), new Collection<Attribute> {
                    new ParameterAttribute {
                        ValueFromPipelineByPropertyName = true,
                        ParameterSetName = Constants.ParameterSets.PackageBySearchSet
                    },
                    new AliasAttribute("Provider")
                }));
            }
        }

        public override bool ProcessRecordAsync() {
            if (IsPackageByObject) {
                return UninstallPackages(InputObject);
            }

            // otherwise do the same as get-package and collect the results for the EndProcessingAsync
            base.ProcessRecordAsync();

            return true;
        }

        protected override void ProcessPackage(string query, SoftwareIdentity package) {
            // mark down that we found something for that query
            _resultsPerName.GetOrAdd(query, () => new List<SoftwareIdentity>()).Add(package);
        }

        public override bool EndProcessingAsync() {
            // Show errors before?
            foreach (var name in UnprocessedNames) {
                Error(Constants.Errors.NoMatchFound, name);
            }

            if (_resultsPerName == null) {
                return true;
            }

            // if we encountered an error, we're not going to even do anything.
            if (Stopping) {
                return false;
            }

            foreach (var n in _resultsPerName.Keys) {
                // check if we have a 1 package per name
                if (_resultsPerName[n].Count > 1) {
                    Error(Constants.Errors.DisambiguateForUninstall, n, _resultsPerName[n]);
                    return false;
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
                    Error(Constants.Errors.UnknownProvider, pkg.ProviderName);
                    return false;
                }

                try {
                    if (ShouldProcessPackageUninstall(pkg.Name, pkg.Version)) {
                    foreach (var installedPkg in provider.UninstallPackage(pkg, this).CancelWhen(_cancellationEvent.Token)) {
                        if (IsCanceled) {
                            return false;
                        }
                        WriteObject(installedPkg);
                    }
                    }
                } catch (Exception e) {
                    e.Dump();
                    Error(Constants.Errors.UninstallationFailure, pkg.Name);
                    return false;
                }
            }
            return true;
        }

        public bool ShouldProcessPackageUninstall(string packageName, string version) {
            return Force || ShouldProcess(FormatMessageString(Constants.Messages.TargetPackageVersion, packageName, version), FormatMessageString(Constants.Messages.ActionUninstallPackage)).Result;
        }
    }
}
