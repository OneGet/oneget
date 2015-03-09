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

namespace Microsoft.PowerShell.OneGet.Cmdlets {
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Utility.Async;
    using Microsoft.OneGet.Utility.Collections;
    using Microsoft.OneGet.Utility.Extensions;
    using Utility;
    using System.Diagnostics.CodeAnalysis;

    [Cmdlet(VerbsCommon.Get, Constants.Nouns.PackageNoun, HelpUri = "http://go.microsoft.com/fwlink/?LinkID=517135")]
    public class GetPackage : CmdletWithSearch {
        private readonly Dictionary<string, bool> _namesProcessed = new Dictionary<string, bool>();

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

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Considered. Still No.")]
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

        protected bool IsDuplicate(SoftwareIdentity package) {
            // todo: add duplicate checking (need canonical ids)
            return false;
        }

        public override bool ProcessRecordAsync() {
            // keep track of what package names the user asked for.
            if (!Name.IsNullOrEmpty()) {
                foreach (var name in Name) {
                    _namesProcessed.GetOrAdd(name, () => false);
                }
            }

            var requests = (Name.IsNullOrEmpty() ?

                // if the user didn't specify any names 
                SelectedProviders.Select(pv => new {
                    query = "?",
                    packages = pv.GetInstalledPackages("", this.ProviderSpecific(pv)).CancelWhen(_cancellationEvent.Token)
                }) :

                // if the user specified a name,
                SelectedProviders.SelectMany(pv => {
                    // for a given provider, if we get an error, we want just that provider to stop.
                    var host = this.ProviderSpecific(pv);

                    return Name.Select(name => new {
                        query = name,
                        packages = pv.GetInstalledPackages(name, host).CancelWhen(_cancellationEvent.Token)
                    });
                })).ToArray();

            while (WaitForActivity(requests.Select(each => each.packages))) {
                // keep processing while any of the the queries is still going.

                foreach (var result in requests.Where(each => each.packages.HasData)) {
                    // look only at requests that have data waiting.

                    foreach (var package in result.packages.GetConsumingEnumerable()) {
                        // process the results for that set.

                        if (IsPackageInVersionRange(package)) {
                            // it only counts if the package is in the range we're looking for.

                            // mark down that we found something for that query
                            _namesProcessed.AddOrSet(result.query, true);

                            ProcessPackage(result.query, package);
                        }
                    }
                }

                // just work with whatever is not yet consumed
                requests = requests.FilterWithFinalizer(each => each.packages.IsConsumed, each => each.packages.Dispose()).ToArray();
            }
            return true;
        }

        protected virtual void ProcessPackage(string query, SoftwareIdentity package) {
            // Check for duplicates 
            if (!IsDuplicate(package)) {
                WriteObject(package);
            }
        }

        public override bool EndProcessingAsync() {
            // give out errors for any package names that we don't find anything for.
            foreach (var name in UnprocessedNames) {
                Error(Constants.Errors.NoMatchFound, name);
            }
            return true;
        }
    }
}