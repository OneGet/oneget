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
    using Microsoft.OneGet.Core.Extensions;
    using Microsoft.OneGet.Core.Packaging;
    using Microsoft.OneGet.Core.Providers.Package;

    [Cmdlet(VerbsLifecycle.Unregister, PackageSourceNoun, SupportsShouldProcess = true)]
    public class UnregisterPackageSource : CmdletWithSource {
        private string[] _specifiedPackageSources;

        public UnregisterPackageSource()
            : base(new[] {
                OptionCategory.Provider, OptionCategory.Source
            }) {
        }

        [Parameter(ParameterSetName = ProviderByObjectSet, Position = 0, Mandatory = true)]
        [Parameter(ParameterSetName = ProviderByNameSet, Position = 0, Mandatory = true)]
        public string Source {get; set;}

        [Parameter]
        public SwitchParameter Force {get; set;}

        public override IEnumerable<string> SpecifiedPackageSources {
            get {
                if (_specifiedPackageSources == null) {
                    if (Source.IsEmptyOrNull()) {
                        return new string[0];
                    }
                    return new string[] {
                        Source
                    };
                }
                return _specifiedPackageSources;
            }
            set {
                _specifiedPackageSources = value.ToArray();
            }
        }

        public override bool ProcessRecordAsync() {
            if (IsSourceByObject) {
                if (PackageSource.IsNullOrEmpty()) {
                    return Error("NULL_OR_EMPTY_PACKAGE_SOURCE");
                }

                foreach (var source in PackageSource) {
                    if (Stopping) {
                        return false;
                    }

                    var provider = PackageManagementService.SelectProviders(source.ProviderName).FirstOrDefault();
                    if (provider == null) {
                        return Error("UNABLE_TO_RESOLVE_PACKAGE_PROVIDER");
                    }
                    Unregister(source);
                }
                return true;
            }

            // otherwise, we're just deleting a source by name
            var prov = SelectedProviders;

            if (Stopping) {
                return false;
            }

            if (prov.Length == 0) {
                return Error("PROVIDER_NOT_FOUND");
            }

            if (prov.Length > 1) {

                var sources = prov.SelectMany(each => each.GetPackageSources(this).ToArray()).ToArray();

                if (sources.Length == 0) {
                    return Error("SOURCE_NOT_FOUND", Source);
                }

                if (sources.Length > 1) {
                    return Error("DISAMBIGUATE_SOURCE_VS_PROVIDER", prov.Select(each => each.Name).JoinWithComma(), Source);
                }

                return Unregister(sources[0]);
            }

            
            return true;
        }

        public bool Unregister(PackageSource source) {
            if (ShouldProcess("Name = '{0}' Location = '{1}' Provider = '{2}'".format(source.Name, source.Location, source.ProviderName)).Result) {
                source.Provider.RemovePackageSource(source.Name, this);
                return true;
            }
            return false;
        }
    }
}