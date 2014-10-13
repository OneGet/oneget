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
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Utility.Async;
    using Microsoft.OneGet.Utility.Collections;
    using Microsoft.OneGet.Utility.Extensions;

    [Cmdlet(VerbsCommon.Get, Constants.PackageSourceNoun)]
    public sealed class GetPackageSource : CmdletWithProvider {
        private readonly List<PackageSource> _unregistered = new List<PackageSource>();
        private bool _found;
        private bool _noLocation;
        private bool _noName;

        public GetPackageSource()
            : base(new[] {
                OptionCategory.Provider, OptionCategory.Source
            }) {
        }

        protected override IEnumerable<string> ParameterSets {
            get {
                return new[] {""};
            }
        }

        [Parameter(Position = 0)]
        public string Name {get; set;}

        [Parameter]
        public string Location {get; set;}

        private IEnumerable<string> _sources {
            get {
                if (!string.IsNullOrEmpty(Name)) {
                    yield return Name;
                }

                if (!string.IsNullOrEmpty(Location)) {
                    yield return Location;
                }
            }
        }

        public override IEnumerable<string> Sources {
            get {
                return _sources.ByRef();
            }
        }

        private bool WriteSources(IEnumerable<PackageSource> sources) {
            foreach (var source in sources) {
                _found = true;
                WriteObject(source);
            }
            return _found;
        }

        public override bool ProcessRecordAsync() {
            var noName = string.IsNullOrEmpty(Name);
            var noLocation = string.IsNullOrEmpty(Location);
            var noCriteria = noName && noLocation;

            // store the information if we've ever had a name or location
            _noName = _noName || noName;
            _noLocation = _noLocation || noLocation;

            foreach (var provider in SelectedProviders) {
                if (Stopping) {
                    return false;
                }

                using (var sources = provider.ResolvePackageSources(this).CancelWhen(_cancellationEvent.Token)) {
                    if (noCriteria) {
                        // no criteria means just return whatever we found
                        if (WriteSources(sources)) {
                        }
                    } else {
                        var all = sources.ToArray();
                        var registered = all.Where(each => each.IsRegistered);

                        if (noName) {
                            // just location was specified
                            if (WriteSources(registered.Where(each => each.Location.EqualsIgnoreCase(Location)))) {
                                continue;
                            }
                        } else {
                            // source was specified (check both name and location fields for match)
                            if (WriteSources(registered.Where(each => each.Name.EqualsIgnoreCase(Name) || each.Location.EqualsIgnoreCase(Name)))) {
                                continue;
                            }
                        }
                        // we haven't returned anything to the user yet...
                        // hold on to the unregistered ones. Might need these at the end.
                        _unregistered.AddRangeLocked(all.Where(each => !each.IsRegistered));
                    }
                }
            }

            return true;
        }

        public override bool EndProcessingAsync() {
            if (!_found) {
                if (_noName && _noLocation) {
                    // no criteria means just return whatever we found
                    if (WriteSources(_unregistered)) {
                        return true;
                    }
                    Warning(Constants.SourceNotFoundNoCriteria);
                    return true;
                }

                if (_noName) {
                    // just location was specified
                    if (WriteSources(_unregistered.Where(each => each.Location.EqualsIgnoreCase(Location)))) {
                        return true;
                    }
                    Warning(Constants.SourceNotFoundForLocation, Location);
                    return true;
                }

                // source was specified (check both name and location fields for match)
                if (WriteSources(_unregistered.Where(each => each.Name.EqualsIgnoreCase(Name) || each.Location.EqualsIgnoreCase(Name)))) {
                    return true;
                }
                Warning(Constants.SourceNotFound, Name);
                return true;
            }
            return true;
        }
    }
}