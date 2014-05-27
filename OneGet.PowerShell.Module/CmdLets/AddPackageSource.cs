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
    using System.Diagnostics;
    using System.Linq;
    using System.Management.Automation;
    using Core;
    using Microsoft.OneGet;
    using Microsoft.OneGet.Core.Api;
    using Microsoft.OneGet.Core.Extensions;
    using Microsoft.OneGet.Core.Tasks;

    [Cmdlet(VerbsCommon.Add, PackageSourceNoun, SupportsShouldProcess = true)]
    public class AddPackageSource : OneGetCmdlet {
        [Parameter(Position = 0)]
        public string Name {get; set;}

        [Parameter(Position = 1)]
        public string Location {get; set;}

        [Parameter(Position = 2, Mandatory = true)]
        public string Provider {get; set;}

        [Parameter]
        public SwitchParameter Trusted { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

#if AFTER_CTP

        [Parameter]
        public SwitchParameter Machine {get; set;}

        [Parameter]
        public SwitchParameter User {get; set;}

        [Parameter]
        public Hashtable Options {get; set;}
#endif

        public override bool ProcessRecordAsync() {
            var provider = _packageManagementService.SelectProviders(Provider).FirstOrDefault();
            if (provider == null) {
                Error("Unknown Provider", new string[] {Provider});
                return false;
            }
            using (var sources = CancelWhenStopped(provider.GetPackageSources(Invoke))) {
                if (sources.Any(each => each.Name.Equals(Name, StringComparison.OrdinalIgnoreCase))) {
                    if (Force || ShouldProcess("Name = '{0}' Location = '{1}' Provider = '{2}' (Replace existing)".format(Name, Location, Provider)).Result) {
                        provider.AddPackageSource(Name, Location, Trusted, Invoke);
                        return true;
                    }
                    return false;
                }
            }

            if (ShouldProcess("Name = '{0}' Location = '{1}' Provider = '{2}'".format(Name, Location, Provider)).Result) {
                provider.AddPackageSource(Name, Location, Trusted, Invoke);
                return true;
            }

            return false;
        }
    }
}