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
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Security;
    using System.Threading;
    using Microsoft.PackageManagement.Implementation;
    using Microsoft.PackageManagement.Packaging;
    using Microsoft.PackageManagement.Utility.Async;
    using Microsoft.PackageManagement.Utility.Collections;
    using Microsoft.PackageManagement.Utility.Extensions;
    using Utility;
    using Constants = PackageManagement.Constants;
    using Directory = System.IO.Directory;

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
                    return Microsoft.PackageManagement.Constants.Empty;
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

                    var potentialSources = providers.SelectMany(each => each.ResolvePackageSources(this.SuppressErrorsAndWarnings(IsProcessing)).Where(source => Source.ContainsAnyOfIgnoreCase(source.Name, source.Location))).ReEnumerable();

                    // prefer registered sources
                    var registeredSources = potentialSources.Where(source => source.IsRegistered).ReEnumerable();

                    providers = registeredSources.Any() ? registeredSources.Select(source => source.Provider).Distinct().ReEnumerable() : potentialSources.Select(source => source.Provider).Distinct().ReEnumerable();
                }
                // filter on: dynamic options - if they specify any dynamic options, limit the provider set to providers with those options.
                return FilterProvidersUsingDynamicParameters(providers).ToArray();
            }
        }
        */

        public override string CredentialUsername {
            get {
                return Credential != null ? Credential.UserName : null;
            }
        }

        public override  SecureString CredentialPassword {
            get {
                return Credential != null ? Credential.Password : null;
            }
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



        private MutableEnumerable<string> FindFiles(string path) {
            if (path.LooksLikeAFilename()) {
                ProviderInfo providerInfo;
                var paths = GetResolvedProviderPathFromPSPath(path, out providerInfo).ReEnumerable();
                return paths.SelectMany(each => each.FileExists() ? each.SingleItemAsEnumerable() : each.DirectoryExists() ? Directory.GetFiles(each) : Microsoft.PackageManagement.Constants.Empty).ReEnumerable();
            }
            return Microsoft.PackageManagement.Constants.Empty.ReEnumerable();
        }


        protected bool SpecifiedMinimumOrMaximum {
            get {
                return !string.IsNullOrWhiteSpace(MaximumVersion) || !string.IsNullOrWhiteSpace(MinimumVersion);
            }
        }

        private List<Uri> _uris = new List<Uri>();
        private Dictionary<string, Tuple<List<string>, byte[]>> _files = new Dictionary<string, Tuple<List<string>, byte[]>>(StringComparer.OrdinalIgnoreCase);

        private IEnumerable<string> _names;

        private bool IsUri(string name) {
            Uri packageUri;
            if (Uri.TryCreate(name, UriKind.Absolute, out packageUri)) {
                // if it's an uri, then we search via uri or file!
                if (!packageUri.IsFile) {
                    _uris.Add( packageUri );
                    return true;
                }
            }
            return false;
        }


        private bool IsFile(string name) {
            var files = FindFiles(name);
            if (files.Any()) {
                foreach (var f in files) {
                    if (_files.ContainsKey(f)) {
                        // if we've got this file already by another parameter, just update it to
                        // keep track that we've somehow got it twice.
                        _files[f].Item1.Add(name);
                    } else {
                        // otherwise, lets' grab the first chunk of this file so we can check what providers
                        // can handle it (later)
                        _files.Add(f, new List<string> { name }, f.ReadBytes(1024));
                    }
                }

                return true;
            }
            return false;
        }



        protected void SearchForPackages() {
            var providers = SelectedProviders.ToArray();


            // filter the items into three types of searches
            _names = Name.IsNullOrEmpty() ? string.Empty.SingleItemAsEnumerable() : Name.Where(each => !IsUri(each) && !IsFile(each)).ToArray();


           var requests = SelectedProviders.SelectMany(pv => {
                // for a given provider, if we get an error, we want just that provider to stop.
                var host = this.ProviderSpecific(pv);

               var a = _uris.Select(uri => new {
                   query = new List<string>{uri.AbsolutePath},
                   provider = pv,
                   packages = pv.FindPackageByUri(uri, host).CancelWhen(CancellationEvent.Token)
               });

               var b = _files.Keys.Where(file => pv.IsSupportedFile(_files[file].Item2)).Select(file => new {
                   query = _files[file].Item1,
                   provider = pv,
                   packages =  pv.FindPackageByFile(file, host)
               });

               var c = _names.Select(name => new {
                   query = new List<string>{name},
                   provider = pv,
                   packages = pv.FindPackage(name, RequiredVersion, MinimumVersion, MaximumVersion,host)
               });

               return a.Concat(b).Concat(c);
           }).ToArray();

            if (AllVersions || !SpecifiedMinimumOrMaximum) {
                // the user asked for every version or they didn't specify any version ranges
                // either way, that means that we can just return everything that we're finding.

                while(WaitForActivity(requests.Select(each => each.packages))) {
                    // keep processing while any of the the queries is still going.

                    foreach (var result in requests.Where(each => each.packages.HasData)) {
                        // look only at requests that have data waiting.

                        foreach (var package in result.packages.GetConsumingEnumerable()) {
                            // process the results for that set.
                            ProcessPackage(result.provider, result.query, package);
                        }
                    }

                    // filter out whatever we're done with.
                    requests = requests.FilterWithFinalizer(each => each.packages.IsConsumed, each => each.packages.Dispose()).ToArray();
                }


            } else {
                // now this is where it gets a bit funny.
                // the user specified a min or max
                // and so we have to only return the highest one in the set for a given package.

                while (WaitForActivity(requests.Select(each => each.packages))) {
                    // keep processing while any of the the queries is still going.
                    foreach (var perProvider in requests.GroupBy(each => each.provider)) {
                        foreach (var perQuery in perProvider.GroupBy(each => each.query)) {
                            if (perQuery.All(each => each.packages.IsCompleted && !each.packages.IsConsumed)) {
                                foreach (var pkg in from p in perQuery.SelectMany(each => each.packages.GetConsumingEnumerable())
                                                    group p by new { p.Name, p.Source }
                                    // for a given name
                                    into grouping
                                        // get the latest version only
                                                    select grouping.OrderByDescending(pp => pp, SoftwareIdentityVersionComparer.Instance).First())
                                {
                                    ProcessPackage(perProvider.Key, perQuery.Key, pkg);
                                }
                            }
                        }
                    }
                    // filter out whatever we're done with.
                    requests = requests.FilterWithFinalizer(each => each.packages.IsConsumed, each => each.packages.Dispose()).ToArray();
                }
            }

            // dispose of any requests that didn't get cleaned up earlier.
            foreach (var i in requests) {
                i.packages.Dispose();
            }
        }

        protected virtual void ProcessPackage(PackageProvider provider, IEnumerable<string> searchKey, SoftwareIdentity package) {
            foreach (var key in searchKey) {
                _resultsPerName.GetOrSetIfDefault(key, () => new List<SoftwareIdentity>()).Add(package);
            }
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
                        Error(Constants.Errors.NoPackagesFoundForProvider, _providersNotFindingAnything.Select(each => each.ProviderName).JoinWithComma());
                    } else {
                        if (WildcardPattern.ContainsWildcardCharacters(name)) {
                            Verbose(Constants.Messages.NoMatchesForWildcard, name);
                            result = false; // even tho' it's not an 'error' it is still enough to know not to actually install.
                        } else {
                            result = false;
                            NonTerminatingError(Constants.Errors.NoMatchFoundForCriteria, name);
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

            // todo: we should think this thru one more time. I'm not convinced we're doing the exact right thing.
            // if there are overmatched packages we need to know why:
            // are they found across multiple providers?
            // are they found accross multiple sources?
            // are they all from the same source?

            if (overMatched.Any()) {

                foreach (var set in overMatched) {
                    var suggestion = "";

                    var providers = set.Select(each => each.ProviderName).Distinct().ToArray();
                    var sources = set.Select(each => each.Source ).Distinct().ToArray();
                    if (providers.Length == 1) {
                        // it's matching this package multiple times in the same provider.
                        if (sources.Length == 1) {
                            // matching it from a single source.
                            // be more exact on matching name? or version?
                            // todo: make a resource for this
                            suggestion = "Please specify an exact -Name and -RequiredVersion.";
                        } else {
                            // it's matching the same package from multiple sources
                            // tell them to use -source
                            // todo: make a resource for this
                            suggestion = "Please specify a single -Source.";
                        }
                    } else {
                        // found across multiple providers
                        // must specify -provider
                        // todo: make a resource for this
                        suggestion = "Please specify a single -ProviderName.";
                    }

                    string searchKey = null;

                    foreach (var pkg in set) {
                        // todo : this is a temporary message
                        // Warning(Constants.Messages.MatchesMultiplePackages, pkg.SearchKey, pkg.CanonicalId);
                        Warning(Constants.Messages.MatchesMultiplePackages, pkg.SearchKey, pkg.ProviderName, pkg.Name, pkg.Version, pkg.Source);
                        searchKey = pkg.SearchKey;
                    }
                    Error(Constants.Errors.DisambiguateForInstall, searchKey, GetMessageString(suggestion,suggestion));
                }
                return false;
            }
            return true;
        }
    }
}
