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
    using Microsoft.OneGet.Core.Extensions;
    using Microsoft.OneGet.Core.Packaging;
    using Microsoft.OneGet.Core.Providers.Package;

    [Cmdlet(VerbsLifecycle.Register, PackageSourceNoun, SupportsShouldProcess = true)]
    public class RegisterPackageSource : CmdletBase {

        [Parameter(Position = 0)]
        public string Name {get; set;}

        [Parameter(Position = 1)]
        public string Location {get; set;}

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = ProviderByObjectSet, ValueFromPipeline = true)]
        public PackageProvider PackageProvider { get; set; }

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = ProviderByNameSet)]
        public string Provider { get; set; }

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = OverwriteExistingSourceSet)]
        public PackageSource OriginalSource { get; set; }

        [Parameter(Position = 3)]
        public PSCredential Credential { get; set; }

        [Parameter]
        public SwitchParameter Trusted {get; set;}

        [Parameter]
        public SwitchParameter Force {get; set;}

#if AFTER_CTP
        [Parameter]
        public SwitchParameter Machine {get; set;}

        [Parameter]
        public SwitchParameter User {get; set;}
#endif

        public RegisterPackageSource() {
        }
        
        public override bool ProcessRecordAsync() {

            if (IsOverwriteExistingSource) {
                Provider = OriginalSource.ProviderName;
                Name = Name.Is() ? Name : OriginalSource.Name;
                Location = Location.Is() ? Name : OriginalSource.Name;
                if (!Trusted.IsPresent && OriginalSource.IsTrusted) {
                    Trusted = SwitchParameter.Present;
                }
            }

            if (!IsProviderByObject) {
                PackageProvider = PackageManagementService.SelectProviders(Provider).FirstOrDefault();    
            }

            if (PackageProvider == null) {
                return Error("NO_PROVIDER_SELECTED");
            }

            if (Stopping) {
                return false;
            }

            using (var sources = CancelWhenStopped(PackageProvider.GetPackageSources(this))) {
                if (sources.Any(each => each.Name.Equals(Name, StringComparison.OrdinalIgnoreCase))) {
                    if (Force || ShouldProcess("Name = '{0}' Location = '{1}' Provider = '{2}' (Replace existing)".format(Name, Location, Provider)).Result) {
                        PackageProvider.AddPackageSource(Name, Location, Trusted, this);
                        return true;
                    }
                    return false;
                }
            }

            if (ShouldProcess("Name = '{0}' Location = '{1}' Provider = '{2}'".format(Name, Location, Provider)).Result) {
                PackageProvider.AddPackageSource(Name, Location, Trusted, this);
                return true;
            }

            return false;
        }
    }
}