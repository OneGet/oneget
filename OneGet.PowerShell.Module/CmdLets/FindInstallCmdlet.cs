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
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using Core;
    using Microsoft.OneGet;
    using Microsoft.OneGet.Core.Extensions;
    using Microsoft.OneGet.Core.Packaging;
    using Microsoft.OneGet.Core.Providers.Package;
    using Microsoft.OneGet.Core.Tasks;

    public class FindInstallCmdlet : PackagingCmdlet {
        internal FindInstallCmdlet() {
            // populate the matching providers at first request.
            _providers = new Lazy<IEnumerable<PackageProvider>>(() => _packageManagementService.SelectProviders(Provider, Source));
        }

        [Parameter(ParameterSetName = "PackageBySearch")]
        public string[] Source {get; set;}

        [Parameter(ParameterSetName = "PackageBySearch")]
        public string RequiredVersion {get; set;}

        [Parameter(ParameterSetName = "PackageBySearch")]
        public string MinimumVersion {get; set;}

        [Parameter(ParameterSetName = "PackageBySearch")]
        public string MaximumVersion {get; set;}

        public override IEnumerable<string> GetPackageSources() {
            return Source;
        }

        internal bool FindViaUri(PackageProvider packageProvider, string packageuri, Action<SoftwareIdentity> onPackageFound) {
            var found = false;
            if (Uri.IsWellFormedUriString(packageuri, UriKind.Absolute)) {
                using (var packages = CancelWhenStopped(packageProvider.FindPackageByUri(new Uri(packageuri), 0, this))) {
                    foreach (var p in packages) {
                        found = true;
                        onPackageFound(p);
                    }
                }
            }
            return found;
        }

        internal bool FindViaFile(PackageProvider packageProvider, string filePath, Action<SoftwareIdentity> onPackageFound) {
            var found = false;
            if (filePath.LooksLikeAFilename()) {
                // if it does have that it *might* be a file.
                // if we don't get back anything from this query
                // then fall thru to the next type

                // first, try to resolve the filenames
                try {
                    ProviderInfo providerInfo = null;
                    var files = GetResolvedProviderPathFromPSPath(filePath, out providerInfo).Where(File.Exists);

                    if (files.Any()) {
                        // found at least some files
                        // this is probably the right path.
                        foreach (var file in files) {
                            var foundThisFile = false;
                            using (var packages = CancelWhenStopped(packageProvider.FindPackageByFile(file, 0, this))) {
                                foreach (var p in packages) {
                                    foundThisFile = true;
                                    found = true;
                                    onPackageFound(p);
                                }
                            }

                            if (foundThisFile == false) {
                                // one of the files we found on disk, isn't actually a recognized package 
                                // let's whine about this.
                                Warning("Package File Not Recognized {0}", new[] {
                                    file
                                });
                            }
                        }
                    }
                } catch {
                    // didn't actually map to a filename ...  keep movin'
                }
                // it doesn't look like we found any files.
                // either because we didn't find any file paths that match
                // or the provider couldn't make sense of the files.
            }

            return found;
        }

        public override bool GenerateDynamicParameters() {
            // Todo: We should really expand dynamic parameters to be operation specific. 
            // todo: so that we can have operations like get-package have different dynamic parameters than
            // todo: find-package

            // if the provider (or source) is selected, we can get package metadata keys from the provider
            // hmm. let's just grab *all* of them.

            foreach (var md in _providers.Value.SelectMany(provider => provider.GetDynamicOptions(OptionCategory.Package, this))) {
                DynamicParameters.Add(md.Name, md.CreateRuntimeDynamicParameter());
            }
            return true;
        }

        internal bool FindViaName(PackageProvider packageProvider, string name, Action<SoftwareIdentity> onPackageFound) {
            var found = false;

            using (var packages = CancelWhenStopped(packageProvider.FindPackage(name, RequiredVersion, MinimumVersion, MaximumVersion, 0, this))) {
                foreach (var p in packages) {
                    found = true;
                    onPackageFound(p);
                }
            }

            return found;
        }
    }
}