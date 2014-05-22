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
    using System.Management.Automation;
    using Core;
    using Microsoft.OneGet;
    using Microsoft.OneGet.Core.Api;
    using Microsoft.OneGet.Core.Extensions;
    using Microsoft.OneGet.Core.Tasks;

    [Cmdlet(VerbsCommon.Get, PackageSourceNoun)]
    public class GetPackageSource : OneGetCmdlet {
        [Parameter(Position = 0)]
        public string Name {get; set;}

        [Parameter]
        public string Location {get; set;}

        [Parameter]
        public string Provider {get; set;}

        public override bool ProcessRecordAsync() {
            var providers = _packageManagementService.SelectProviders(Provider);
            if (providers == null) {
                Event<Error>.Raise("Unknown Provider", new string[] { Provider});
                return false;
            }

            foreach( var provider in providers){
                bool found = false;
                using(var sources = CancelWhenStopped(provider.GetPackageSources(Invoke) ) ){
                    foreach (var source in sources) {
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
                            Event<Error>.Raise("Provider '{0}' returned no package sources (Name = '{1}' Location='{2}')", new [] { provider.Name, Name, Location });
                            continue;
                        }
                        Event<Error>.Raise("Provider '{0}' returned no package sources (Name = '{1}')", new[] { provider.Name, Name });
                        continue;
                    }

                    if (!string.IsNullOrEmpty(Location)) {
                        Event<Error>.Raise("Provider '{0}' returned no package sources (Location = '{1}')", new[] { provider.Name, Location });
                        continue;
                    }
                    Event<Error>.Raise("Provider '{0}' returned no package sources".format(provider.Name ));
                }

            }

            return true;
        }
    }
}
