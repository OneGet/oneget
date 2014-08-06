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
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Providers.Package;
    using Microsoft.OneGet.Utility.Extensions;

    [Cmdlet(VerbsLifecycle.Unregister, Constants.PackageSourceNoun, SupportsShouldProcess = true)]
    public sealed class UnregisterPackageSource : CmdletWithSource {
        private string[] _sources;

        public UnregisterPackageSource()
            : base(new[] {
                OptionCategory.Provider, OptionCategory.Source
            }) {
        }

        [Parameter(ParameterSetName = Constants.ProviderByObjectSet, Position = 0, Mandatory = true)]
        [Parameter(ParameterSetName = Constants.ProviderByNameSet, Position = 0, Mandatory = true)]
        public string Source {get; set;}

        [Parameter]
        public SwitchParameter Force {get; set;}

        public override IEnumerable<string> Sources {
            get {
                if (_sources == null) {
                    if (Source.IsEmptyOrNull()) {
                        _sources = new string[0];
                    } else {
                        _sources = new string[] {
                            Source
                        };
                    }
                }
                return _sources;
            }
            set {
                _sources = (value ?? new string[0]).ToArray();
            }
        }

        public override bool ProcessRecordAsync() {
            if (IsSourceByObject) {
                if (PackageSource.IsNullOrEmpty()) {
                    return Error(Errors.NullOrEmptyPackageSource);
                }

                foreach (var source in PackageSource) {
                    if (Stopping) {
                        return false;
                    }

                    var provider = SelectProviders(source.ProviderName).FirstOrDefault();
                    if (provider == null) {
                        return Error(Errors.UnableToResolvePackageProvider);
                    }
                    Unregister(source);
                }
                return true;
            }

            // otherwise, we're just deleting a source by name
            var prov = SelectedProviders.ToArray();

            if (Stopping) {
                return false;
            }

            if (prov.Length == 0) {
                return Error(Errors.UnableToResolvePackageProvider);
            }

            if (prov.Length > 0) {
                var sources = prov.SelectMany(each => each.ResolvePackageSources(this).ToArray()).ToArray();

                if (sources.Length == 0) {
                    return Error(Errors.SourceNotFound, Source);
                }

                if (sources.Length > 1) {
                    return Error(Errors.DisambiguateSourceVsProvider, prov.Select(each => each.Name).JoinWithComma(), Source);
                }

                return Unregister(sources[0]);
            }

            return true;
        }

        public bool Unregister(PackageSource source) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }
            if (ShouldProcess(Constants.NameLocationProvider.format(source.Name, source.Location, source.ProviderName)).Result) {
                source.Provider.RemovePackageSource(source.Name, this);
                return true;
            }
            return false;
        }
    }
}