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
    using System.Diagnostics.Eventing.Reader;
    using System.Linq;
    using System.Management.Automation;
    using System.Threading.Tasks;
    using Microsoft.OneGet;
    using Microsoft.OneGet.Core.Api;
    using Microsoft.OneGet.Core.Extensions;
    using Microsoft.OneGet.Core.Tasks;

    [Cmdlet(VerbsCommon.Get, PackageNoun)]
    public class GetPackage : PackagingCmdlet {
        public override bool ProcessRecordAsync() {
            var pr = _providers.Value.ToArray();
            
            Parallel.ForEach(_providers.Value, provider => {
                try {
                    if (Name.IsNullOrEmpty()) {
                        var found = false;
                        using (var packages = CancelWhenStopped(provider.GetInstalledPackages("", Invoke))) {
                            foreach (var p in packages) {
                                found = true;
                                WriteObject(p);
                            }
                        }
                        if (!found) {
                            Event<Error>.Raise("No installed packages found.");
                        }
                        return;
                    }
                    foreach (var name in Name) {
                        var found = false;
                        using (var packages = CancelWhenStopped(provider.GetInstalledPackages(name, Invoke))) {
                            foreach (var p in packages) {
                                found = true;
                                WriteObject(p);
                            }
                        }
                        if (!found) {
                            Event<Error>.Raise("No installed packages found for Name '{0}'.".format(name));
                        }
                    }
                } catch (Exception e) {
                    e.Dump();
                }
            });
            return true;
        }
    }
}