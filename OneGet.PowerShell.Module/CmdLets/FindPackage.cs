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
    using System.Management.Automation;
    using System.Threading.Tasks;
    using Microsoft.OneGet.Core.Extensions;
    using Microsoft.OneGet.Core.Packaging;
    using Microsoft.OneGet.Core.Providers.Package;

    [Cmdlet(VerbsCommon.Find, PackageNoun), OutputType(typeof (SoftwareIdentity))]
    public class FindPackage : CmdletWithSearchAndSource {
        public FindPackage()
            : base(new[] {
                OptionCategory.Provider, OptionCategory.Source, OptionCategory.Package
            }) {
        }

        //   public override bool ProcessRecordAsync() {

        // }

        public override bool EndProcessingAsync() {
            var noMatchNames = new HashSet<string>(Name ?? new string[] {
            });

            Parallel.ForEach(SelectedProviders, provider => {
                try {
                    if (!Name.IsNullOrEmpty()) {
                        foreach (var name in Name) {
                            if (FindViaUri(provider, name, (p) => WriteObject(p))) {
                                noMatchNames.IfPresentRemoveLocked(name);
                                continue;
                            }

                            if (FindViaFile(provider, name, (p) => WriteObject(p))) {
                                noMatchNames.IfPresentRemoveLocked(name);
                                continue;
                            }

                            if (FindViaName(provider, name, (p) => WriteObject(p))) {
                                noMatchNames.IfPresentRemoveLocked(name);
                                continue;
                            }

                            // did not find anything on this provider that matches that name
                        }
                    } else {
                        // no package name passed in.
                        if (!FindViaName(provider, string.Empty, (p) => WriteObject(p))) {
                            // nothing found?
                            Warning("No Packages Found (no package names/criteria listed)");
                        }
                    }
                } catch (Exception e) {
                    e.Dump();
                }
            });

            // whine about things not matched.
            foreach (var name in noMatchNames) {
                Warning("No Package Found", new string[] {
                    name
                });
            }

            return true;
        }
    }
}