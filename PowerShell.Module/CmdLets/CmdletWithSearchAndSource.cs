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
    using Microsoft.OneGet.Implementation;
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Utility.Async;
    using Microsoft.OneGet.Utility.Collections;
    using Microsoft.OneGet.Utility.Extensions;
    using Utility;
    using Constants = OneGet.Constants;

    public abstract class CmdletWithSearchAndSource : CmdletWithSearch {
        protected readonly List<string, string> _filesWithoutMatches = new List<string, string>();
        protected readonly OrderedDictionary<string, List<SoftwareIdentity>> _resultsPerName = new OrderedDictionary<string, List<SoftwareIdentity>>();
        protected List<PackageProvider> _providersNotFindingAnything = new List<PackageProvider>();

        protected CmdletWithSearchAndSource(OptionCategory[] categories)
            : base(categories) {
        }

        public virtual SwitchParameter AllVersions {get; set;}

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public virtual string[] Source {get; set;}
            
        [Parameter]
        public virtual PSCredential Credential {get; set;}

        public override IEnumerable<string> Sources {
            get {
                if (Source.IsNullOrEmpty()) {
                    return Microsoft.OneGet.Constants.Empty;
                }
                return Source;
            }
        }

        /*
        protected override IEnumerable<PackageProvider> SelectedProviders {
            get {
                // filter on provider names  - if they specify a provider name, narrow to only those provider names.
                var providers = SelectProviders(ProviderName).ReEnumerable();

                // filter out providers that don't have the sources that have been specified (only if we have specified a source!)
                if (Source != null && Source.Length > 0) {
                    // sources must actually match a name or location. Keeps providers from being a bit dishonest

                    var potentialSources = providers.SelectMany(each => each.ResolvePackageSources(SuppressErrorsAndWarnings).Where(source => Source.ContainsAnyOfIgnoreCase(source.Name, source.Location))).ReEnumerable();

                    // prefer registered sources
                    var registeredSources = potentialSources.Where(source => source.IsRegistered).ReEnumerable();

                    providers = registeredSources.Any() ? registeredSources.Select(source => source.Provider).Distinct().ReEnumerable() : potentialSources.Select(source => source.Provider).Distinct().ReEnumerable();
                }
                // filter on: dynamic options - if they specify any dynamic options, limit the provider set to providers with those options.
                return FilterProvidersUsingDynamicParameters(providers).ToArray();
            }
        }
        */


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

        internal bool FindViaUri(PackageProvider packageProvider, string searchKey, Uri packageuri) {
            var found = false;

            using (var packages = packageProvider.FindPackageByUri(packageuri, 0, this).CancelWhen(_cancellationEvent.Token)) {
                foreach (var p in packages) {
                    found = true;
                    ProcessPackage(packageProvider, searchKey, p);
                }
            }

            return found;
        }

        private MutableEnumerable<string> FindFiles(string path) {
            if (path.LooksLikeAFilename()) {
                ProviderInfo providerInfo;
                var paths = GetResolvedProviderPathFromPSPath(path, out providerInfo).ReEnumerable();
                return paths.SelectMany(each => each.FileExists() ? each.SingleItemAsEnumerable() : each.DirectoryExists() ? Directory.GetFiles(each) : Microsoft.OneGet.Constants.Empty).ReEnumerable();
            }
            return Microsoft.OneGet.Constants.Empty.ReEnumerable();
        }

        internal bool FindViaFile(PackageProvider packageProvider, string searchKey, string filePath) {
            var found = false;
            try {
                // found at least some files
                // this is probably the right path.
                //foreach (var file in filePaths) {

                var foundThisFile = false;
                using (var packages = packageProvider.FindPackageByFile(filePath, 0, this).CancelWhen(_cancellationEvent.Token)) {
                    foreach (var p in packages) {
                        foundThisFile = true;
                        found = true;
                        ProcessPackage(packageProvider, searchKey, p);
                    }
                }

                if (foundThisFile == true) {
                    lock (_filesWithoutMatches) {
                        _filesWithoutMatches.Remove(filePath, searchKey);
                    }
                }
            } catch {
                // didn't actually map to a filename ...  keep movin'
            }

            // it doesn't look like we found any files.
            // either because we didn't find any file paths that match
            // or the provider couldn't make sense of the files.

            return found;
        }

        internal bool FindViaName(PackageProvider packageProvider, string name) {
            var found = false;

            using (var packages = packageProvider.FindPackage(name, RequiredVersion, MinimumVersion, MaximumVersion, 0, this).CancelWhen(_cancellationEvent.Token)) {
                if (AllVersions) {
                    foreach (var p in packages) {
                        found = true;
                        ProcessPackage(packageProvider, name, p);
                    }
                } else {
                    if (!string.IsNullOrWhiteSpace(MaximumVersion) || !string.IsNullOrWhiteSpace(MinimumVersion)) {
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
                    } else {
                        foreach (var pkg in packages) {
                            ProcessPackage(packageProvider, name, pkg);
                        }
                    }
                }
            }
            return found;
        }

        private IEnumerable<PackageProvider> SelectProvidersSupportingFile(PackageProvider[] providers, string filename) {
            if (filename.FileExists()) {
                var buffer = new byte[1024];
                var sz = 0;
                try {
                    using (var file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        sz = file.Read(buffer, 0, 1024);
                    }
                } catch {
                    // not openable. whatever.
                }
                foreach (var p in providers) {
                    if (p.IsFileSupported(buffer)) {
                        yield return p;
                    }
                }
            }
        }

        protected void SearchForPackages() {
            var providers = SelectedProviders.ToArray();

            if (!Name.IsNullOrEmpty()) {
                Name.ParallelForEach(name => {
                    var found = false;

                    if (Uri.IsWellFormedUriString(name, UriKind.Absolute)) {
                        // try everyone as via uri
                        var packageUri = new Uri(name, UriKind.Absolute);
                        if (!packageUri.IsFile) {
                            providers.ParallelForEach(provider => {
                                try {
                                    found = found | FindViaUri(provider, name, packageUri);
                                } catch (Exception e) {
                                    e.Dump();
                                }
                            });
                            // we're done searching for this provider
                            return;
                        }
                        // file uris should be treated like files, not uris.
                    }

                    var files = FindFiles(name);
                    if (files.Any()) {
                        lock (_filesWithoutMatches) {
                            foreach (var file in files) {
                                _filesWithoutMatches.Add(file, name);
                            }
                        }

                        // they specified something that looked kinda like a 
                        // file path, and it actually matched some files on disk
                        // so we're going to assume that they meant to treat it 
                        // as a file.
                        files.ParallelForEach(file => {
                            SelectProvidersSupportingFile(providers, file).ParallelForEach(pv => {
                                try {
                                    FindViaFile(pv, name, file);
                                } catch (Exception e) {
                                    e.Dump();
                                }
                            });
                        });
                        return;
                    }

                    _resultsPerName.GetOrAdd(name, null);

                    // it didn't match any files
                    // and it's not a uri of any kind
                    // so we'll just ask if there is a package by that name.
                    providers.ParallelForEach(provider => {
                        try {
#if DEEP_DEBUG
                             Console.WriteLine("Processing find via name [{0}]",provider.Name);
#endif
                            FindViaName(provider, name);
#if DEEP_DEBUG
                             Console.WriteLine("Done Processing find via name [{0}]", provider.Name);
#endif
                        } catch (Exception e) {
                            e.Dump();
                        }
                    });
                });
            } else {
                
                providers.ParallelForEach(provider => {
                    try {
                        if (!FindViaName(provider, string.Empty)) {
                            // nothing found?
                            _providersNotFindingAnything.AddLocked(provider);
                        }
                    } catch (Exception e) {
                        e.Dump();
                    }
                });
            }
            /*
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
 */
        }

        protected virtual void ProcessPackage(PackageProvider provider, string searchKey, SoftwareIdentity package) {
            _resultsPerName.GetOrSetIfDefault(searchKey, () => new List<SoftwareIdentity>()).Add(package);
        }

        protected bool CheckUnmatchedPackages() {
            var unmatched = _resultsPerName.Keys.Where(each => _resultsPerName[each] == null).ReEnumerable();
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

            foreach (var unmatchedFile in _filesWithoutMatches) {
                Verbose("Didn't Match File: {0}", unmatchedFile.Key);
            }
            return result;
        }

        protected bool CheckMatchedDuplicates() {
            var overMatched = _resultsPerName.Keys.Select(each => _resultsPerName[each])
                .Where(each => each != null && each.Count > 1).ReEnumerable();

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