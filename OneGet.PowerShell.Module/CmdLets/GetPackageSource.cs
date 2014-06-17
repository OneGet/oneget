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
    using System.Management.Automation;
    using Microsoft.OneGet.Extensions;
    using Microsoft.OneGet.Providers.Package;

    [Cmdlet(VerbsCommon.Get, PackageSourceNoun)]
    public class GetPackageSource : CmdletWithProvider {
        private readonly List<string> _warnings = new List<string>();

        public GetPackageSource()
            : base(new[] {
                OptionCategory.Provider, OptionCategory.Source
            }) {
        }

        [Parameter(Position = 0)]
        public string Name {get; set;}

        [Parameter]
        public string Location {get; set;}

        public override bool ProcessRecordAsync() {
            foreach (var provider in SelectedProviders) {
                if (Stopping) {
                    return false;
                }

                var found = false;
                using (var sources = CancelWhenStopped(provider.ResolvePackageSources(this))) {
                    foreach (var source in sources.ToIEnumerable()) {
                        if (!string.IsNullOrEmpty(Name)) {
                            if (!Name.EqualsIgnoreCase(source.Name)) {
                                continue;
                            }
                        }

                        if (!string.IsNullOrEmpty(Location)) {
                            if (!Location.EqualsIgnoreCase(source.Location)) {
                                continue;
                            }
                        }

                        WriteObject(source);
                        found = true;
                    }
                }

                if (!found) {
                    if (!string.IsNullOrEmpty(Name)) {
                        if (!string.IsNullOrEmpty(Location)) {
                            _warnings.Add(string.Format("Provider '{0}' returned no package sources (Name = '{1}' Location='{2}')", provider.Name, Name, Location));
                            continue;
                        }
                        _warnings.Add(string.Format("Provider '{0}' returned no package sources (Name = '{1}')", provider.Name, Name));
                        continue;
                    }

                    if (!string.IsNullOrEmpty(Location)) {
                        _warnings.Add(string.Format("Provider '{0}' returned no package sources (Location = '{1}')", provider.Name, Location));
                        continue;
                    }
                    _warnings.Add(string.Format("Provider '{0}' returned no package sources".format(provider.Name)));
                }
            }

            return true;
        }

        public override bool EndProcessingAsync() {
            // we're collecting our warnings for the end.
            foreach (var warning in _warnings) {
                Warning(warning);
            }
            return true;
        }
    }
}