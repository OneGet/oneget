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
    using Microsoft.OneGet.Extensions;
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Providers.Package;
    using Utility;

    [Cmdlet(VerbsLifecycle.Register, PackageSourceNoun, SupportsShouldProcess = true)]
    public class RegisterPackageSource : CmdletBase {
        [Parameter(Position = 0)]
        public string Name {get; set;}

        [Parameter(Position = 1)]
        public string Location {get; set;}

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = ProviderByObjectSet, ValueFromPipeline = true)]
        public PackageProvider PackageProvider {get; set;}

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = ProviderByNameSet)]
        public string Provider {get; set;}

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = OverwriteExistingSourceSet)]
        public PackageSource OriginalSource {get; set;}

        [Parameter(Position = 3)]
        public PSCredential Credential {get; set;}

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

        public override bool GenerateDynamicParameters() {
            if (IsOverwriteExistingSource) {
                Provider = OriginalSource.ProviderName;
                Name = Name.Is() ? Name : OriginalSource.Name;
                Location = Location.Is() ? Name : OriginalSource.Name;
                if (!Trusted.IsPresent && OriginalSource.IsTrusted) {
                    Trusted = SwitchParameter.Present;
                }
            }

            if (!IsProviderByObject) {
                PackageProvider = SelectProviders(Provider).FirstOrDefault();
            }

            // if the provider (or source) is selected, we can get package metadata keys from the provider
            // hmm. let's just grab *all* of them.
            foreach (var md in PackageProvider.GetDynamicOptions(OptionCategory.Source, this)) {
                if (DynamicParameters.ContainsKey(md.Name)) {
                    // for now, we're just going to mark the existing parameter as also used by the second provider to specify it.
                    (DynamicParameters[md.Name] as CustomRuntimeDefinedParameter).Options.Add(md);
                }
                else {
                    DynamicParameters.Add(md.Name, new CustomRuntimeDefinedParameter(md));
                }
            }
            return true;
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
                PackageProvider = SelectProviders(Provider).FirstOrDefault();
            }

            if (PackageProvider == null) {
                return Error("NO_PROVIDER_SELECTED");
            }

            if (Stopping) {
                return false;
            }


            using (var sources = CancelWhenStopped(PackageProvider.ResolvePackageSources(this))) {
                if (sources.Any(each => each.Name.Equals(Name, StringComparison.OrdinalIgnoreCase))) {
                    if (Force || ShouldProcess("Name = '{0}' Location = '{1}' Provider = '{2}' (Replace existing)".format(Name, Location, Provider)).Result) {
                        using (var added =  CancelWhenStopped(PackageProvider.AddPackageSource(Name, Location, Trusted, this))) {
                            foreach (var addedSource in added) {
                                WriteObject(addedSource);
                            }
                        }
                        return true;
                    }
                    return false;
                }
            }

            if (ShouldProcess("Name = '{0}' Location = '{1}' Provider = '{2}'".format(Name, Location, Provider)).Result) {
                using (var added = CancelWhenStopped(PackageProvider.AddPackageSource(Name, Location, Trusted, this))) {
                    foreach (var addedSource in added) {
                        WriteObject(addedSource);
                    }
                }
                return true;
            }

            return false;
        }
    }
}