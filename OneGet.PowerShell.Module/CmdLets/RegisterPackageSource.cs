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
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Utility.Collections;
    using Microsoft.OneGet.Utility.Extensions;
    using Utility;

    [Cmdlet(VerbsLifecycle.Register, Constants.PackageSourceNoun, SupportsShouldProcess = true)]
    public sealed class RegisterPackageSource : CmdletWithProvider {
        public RegisterPackageSource()
            : base(new[] {OptionCategory.Provider, OptionCategory.Source}) {
        }

        [Parameter(ValueFromPipelineByPropertyName = true, Mandatory = true)]
        public new string ProviderName {get; set;}

        [Parameter(Position = 0)]
        public string Name {get; set;}

        [Parameter(Position = 1)]
        public string Location {get; set;}

        [Parameter]
        public PSCredential Credential {get; set;}

        [Parameter]
        public SwitchParameter Trusted {get; set;}

        public override bool GenerateDynamicParameters() {
            var packageProvider = SelectProviders(ProviderName).ReEnumerable();

            // if more than one provider is selected, this will never work
            if (packageProvider.Count() != 1) {
                return false;
            }

            // if the provider is selected, we can get package metadata keys from the provider
            foreach (var md in packageProvider.First().GetDynamicOptions(OptionCategory.Source, this)) {
                if (DynamicParameterDictionary.ContainsKey(md.Name)) {
                    // for now, we're just going to mark the existing parameter as also used by the second provider to specify it.
                    (DynamicParameterDictionary[md.Name] as CustomRuntimeDefinedParameter).Options.Add(md);
                } else {
                    DynamicParameterDictionary.Add(md.Name, new CustomRuntimeDefinedParameter(md));
                }
            }
            return true;
        }

        public override bool ProcessRecordAsync() {
            if (Stopping) {
                return false;
            }

            var packageProvider = SelectProviders(ProviderName).ReEnumerable();

            switch (packageProvider.Count()) {
                case 0:
                    Error(Errors.UnknownProvider, ProviderName);
                    return false;

                case 1:
                    break;

                default:
                    Error(Errors.MatchesMultipleProviders, packageProvider.Select(provider => provider.ProviderName).JoinWithComma());
                    return false;
            }

            using (var sources = CancelWhenStopped(packageProvider.First().ResolvePackageSources(this))) {
                // first, check if there is a source by this name already.
                var existingSources = sources.Where(each => each.IsRegistered && each.Name.Equals(Name, StringComparison.OrdinalIgnoreCase)).ToArray();

                if (existingSources.Any()) {
                    // if there is, and the user has said -Force, then let's remove it.
                    foreach (var existingSource in existingSources) {
                        if (Force) {
                            if (ShouldProcess(FormatMessageString(Constants.TargetPackageSource, existingSource.Name, existingSource.Location, existingSource.ProviderName), Constants.ActionReplacePackageSource).Result) {
                                using (var removedSources = CancelWhenStopped(packageProvider.First().RemovePackageSource(existingSource.Name, this))) {
                                    foreach (var removedSource in removedSources) {
                                        Verbose(Constants.OverwritingPackageSource, removedSource.Name);
                                    }
                                }
                            }
                        } else {
                            Error(Errors.PackageSourceExists, existingSource.Name);
                            return false;
                        }
                    }
                }
            }

            if (ShouldProcess(FormatMessageString(Constants.TargetPackageSource, Name, Location, ProviderName), FormatMessageString(Constants.ActionRegisterPackageSource)).Result) {
                using (var added = CancelWhenStopped(packageProvider.First().AddPackageSource(Name, Location, Trusted, this))) {
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