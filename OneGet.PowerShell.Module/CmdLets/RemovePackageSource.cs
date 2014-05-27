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
    using System.Linq;
    using System.Management.Automation;
    using Core;
    using Microsoft.OneGet;
    using Microsoft.OneGet.Core.Api;
    using Microsoft.OneGet.Core.Extensions;
    using Microsoft.OneGet.Core.Providers.Package;
    using Microsoft.OneGet.Core.Tasks;

    [Cmdlet(VerbsCommon.Remove, PackageSourceNoun, SupportsShouldProcess = true)]
    public class RemovePackageSource : OneGetCmdlet {
        [Parameter(Mandatory = true, Position = 0)]
        public string Name {get; set;}

        [Parameter(Position = 1)]
        public string Provider {get; set;}

#if AFTER_CTP
        [Parameter]
        public SwitchParameter Machine {get; set;}

        [Parameter]
        public SwitchParameter User {get; set;}

        [Parameter]
        public SwitchParameter Force {get; set;}
#endif

        public override bool ProcessRecordAsync() {
            PackageProvider packageProvider = null;

            if (string.IsNullOrEmpty(Provider)) {
                var providers = _packageManagementService.SelectProviders(Provider, new[] {
                    Name
                }).ToArray();

                if (providers.Length == 1) {
                    packageProvider = providers[0];
                    Provider = packageProvider.Name;
                } else {
                    Error("Conflict/Multiple providers have source '{0}'; must specify -Provider ".format(Name));
                    return false;
                }
            } else {
                packageProvider = _packageManagementService.SelectProviders(Provider).FirstOrDefault();
                if (packageProvider == null) {
                    Error("Unknown Provider : {0}", new string[] {Provider});
                    return false;
                }
            }

            using (var sources = CancelWhenStopped(packageProvider.GetPackageSources(Invoke))) {
                var src = sources.FirstOrDefault(each => each.Name.Equals(Name, StringComparison.OrdinalIgnoreCase));
                if (src != null) {
                    if (ShouldProcess("Name = '{0}' Location = '{1}' Provider = '{2}'".format(src.Name, src.Location, src.Provider)).Result) {
                        packageProvider.RemovePackageSource(Name, Invoke);
                        return true;
                    }
                    return false;
                } else {
                    Error("Unknown Source : {0}", new string[] {Name});
                }
            }

            return true;
        }
    }
}