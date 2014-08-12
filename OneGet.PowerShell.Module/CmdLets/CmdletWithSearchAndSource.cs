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
    using System.Threading.Tasks;
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Providers.Package;
    using Microsoft.OneGet.Utility.Collections;
    using Microsoft.OneGet.Utility.Extensions;

    public abstract class CmdletWithSearchAndSource : CmdletWithSearch {
        protected readonly OrderedDictionary<string, List<SoftwareIdentity>> _resultsPerName = new OrderedDictionary<string, List<SoftwareIdentity>>();
        protected List<PackageProvider> _providersNotFindingAnything = new List<PackageProvider>();

        protected CmdletWithSearchAndSource(OptionCategory[] categories)
            : base(categories) {
        }

        public virtual SwitchParameter AllVersions {get; set;}

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public virtual string[] Source {get; set;}

        [Parameter()]
        public virtual PSCredential Credential {get; set;}

        public override IEnumerable<string> Sources {
            get {
                if (Source.IsNullOrEmpty()) {
                    return new string[0];
                }
                return Source;
            }
        }

        protected override IEnumerable<PackageProvider> SelectedProviders {
            get {
                // filter on provider names  - if they specify a provider name, narrow to only those provider names.
                var providers = SelectProviders(ProviderName);

                // filter out providers that don't have the sources that have been specified (only if we have specified a source!)
                if (Source != null && Source.Length > 0) {
                    providers = providers.Where(each => each.ResolvePackageSources(this).Any());
                }

                // filter on: dynamic options - if they specify any dynamic options, limit the provider set to providers with those options.
                return FilterProvidersUsingDynamicParameters(providers).ToArray();
            }
        }

        public override string GetCredentialUsername() {
            if (Credential != null) {
                return Credential.UserName;
            }
            return null;
        }

        public override string GetCredentialPassword() {
            if (Credential != null) {
                return Credential.Password.ToProtectedString("salt");
            }
            return null;
        }

        public override bool ProcessRecordAsync() {
            // record the names in the collection.
            if (!Name.IsNullOrEmpty()) {
                foreach (var name in Name) {
                    _resultsPerName.GetOrAdd(name, () => null);
                }
            }

            // it's searching for packages. 
            // we need to do the actual search for the packages now,
            // and hold the results until EndProcessingAsync() 
            // where we can determine if we we have no ambiguity left.
            SearchForPackages();

            return true;
        }

        internal bool FindViaUri(PackageProvider packageProvider, string packageuri) {
            var found = false;
            if (Uri.IsWellFormedUriString(packageuri, UriKind.Absolute)) {
                using (var packages = CancelWhenStopped(packageProvider.FindPackageByUri(new Uri(packageuri), 0, this))) {
                    foreach (var p in packages) {
                        found = true;
                        ProcessPackage(packageProvider, packageuri, p);
                    }
                }
            }
            return found;
        }

        internal bool FindViaFile(PackageProvider packageProvider, string filePath) {
            var found = false;
            if (filePath.LooksLikeAFilename()) {
                // if it does have that it *might* be a file.
                // if we don't get back anything from this query
                // then fall thru to the next type

                // first, try to resolve the filenames
                try {
                    ProviderInfo providerInfo = null;
                    var files = GetResolvedProviderPathFromPSPath(filePath, out providerInfo).Where(File.Exists).ToCacheEnumerable();

                    if (files.Any()) {
                        // found at least some files
                        // this is probably the right path.
                        foreach (var file in files) {
                            var foundThisFile = false;
                            using (var packages = CancelWhenStopped(packageProvider.FindPackageByFile(file, 0, this))) {
                                foreach (var p in packages) {
                                    foundThisFile = true;
                                    found = true;
                                    ProcessPackage(packageProvider, filePath, p);
                                }
                            }

                            if (foundThisFile == false) {
                                // one of the files we found on disk, isn't actually a recognized package 
                                // let's whine about this.
                                Warning(Constants.FileNotRecognized, file);
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

        internal bool FindViaName(PackageProvider packageProvider, string name) {
            var found = false;

            using (var packages = CancelWhenStopped(packageProvider.FindPackage(name, RequiredVersion, MinimumVersion, MaximumVersion, 0, this))) {
                if (AllVersions) {
                    foreach (var p in packages) {
                        found = true;
                        ProcessPackage(packageProvider, name, p);
                    }
                } else {
                    foreach (var pkg in from p in packages
                        group p by p.Name
                        // for a given name
                        into grouping
                            // get the latest version only
                        select grouping.OrderByDescending(pp => pp, SoftwareIdentityVersionComparer.Instance).First()) {
                        found = true;
                        // each package name should only show up once here.
                        ProcessPackage(packageProvider, name, pkg);
                    }
                }
            }
            return found;
        }

        protected void SearchForPackages() {
            Parallel.ForEach(SelectedProviders, provider => {
                try {
                    if (!Name.IsNullOrEmpty()) {
                        foreach (var each in Name) {
                            // check if the parameter is an uri
                            if (FindViaUri(provider, each)) {
                                continue;
                            }

                            // then if it's a file
                            if (FindViaFile(provider, each)) {
                                continue;
                            }

                            // otherwise, it's just a name
                            FindViaName(provider, each);
                        }
                        return;
                    }

                    // no package name passed in.
                    if (!FindViaName(provider, string.Empty)) {
                        // nothing found?
                        _providersNotFindingAnything.AddLocked(provider);
                    }
                } catch (Exception e) {
                    e.Dump();
                }
            });
        }

        protected virtual void ProcessPackage(PackageProvider provider, string searchKey, SoftwareIdentity package) {
            _resultsPerName.GetOrSetIfDefault(searchKey, () => new List<SoftwareIdentity>()).Add(package);
        }

        protected bool CheckUnmatchedPackages() {
            var unmatched = _resultsPerName.Keys.Where(each => _resultsPerName[each] == null).ToCacheEnumerable();
            var result = true;

            if (unmatched.Any()) {
                // whine about things not matched.
                foreach (var name in unmatched) {
                    if (name == string.Empty) {
                        // no name 
                        result = false;
                        Error(Errors.NoPackagesFoundForProvider, _providersNotFindingAnything.Select(each => each.ProviderName).JoinWithComma());
                    } else {
                        if (WildcardPattern.ContainsWildcardCharacters(name)) {
                            Verbose(Constants.NoMatchesForWildcard, name);
                        } else {
                            result = false;
                            Error(Errors.NoMatchFound, name);
                        }
                    }
                }
            }
            return result;
        }

        protected bool CheckMatchedDuplicates() {
            var overMatched = _resultsPerName.Keys.Select(each => _resultsPerName[each])
                .Where(each => each != null && each.Count > 1).ToCacheEnumerable();

            if (overMatched.Any()) {
                foreach (var set in overMatched) {
                    string searchKey = null;

                    foreach (var pkg in set) {
                        Warning(Constants.MatchesMultiplePackages, pkg.SearchKey, pkg.Name, pkg.Version, pkg.ProviderName);
                        searchKey = pkg.SearchKey;
                    }
                    Error(Errors.DisambiguateForInstall, searchKey);
                }
                return false;
            }
            return true;
        }
    }
}