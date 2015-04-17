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

namespace Microsoft.PowerShell.PackageManagement.Cmdlets {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.PackageManagement.Packaging;
    using Microsoft.PackageManagement.Utility.Async;
    using Microsoft.PackageManagement.Utility.Collections;
    using Microsoft.PackageManagement.Utility.Extensions;
    using Utility;

    [Cmdlet(VerbsLifecycle.Register, Constants.Nouns.PackageSourceNoun, SupportsShouldProcess = true, HelpUri = "http://go.microsoft.com/fwlink/?LinkID=517139")]
    public sealed class RegisterPackageSource : CmdletWithProvider {
        public RegisterPackageSource()
            : base(new[] {OptionCategory.Provider, OptionCategory.Source}) {
        }

        protected override IEnumerable<string> ParameterSets {
            get {
                return new[] {""};
            }
        }

        protected override void GenerateCmdletSpecificParameters(Dictionary<string, object> unboundArguments) {
            if (!IsInvocation) {
                var providerNames = PackageManagementService.AllProviderNames;
                var whatsOnCmdline = GetDynamicParameterValue<string[]>("ProviderName");
                if (whatsOnCmdline != null) {
                    providerNames = providerNames.Concat(whatsOnCmdline).Distinct();
                }

                DynamicParameterDictionary.AddOrSet("ProviderName", new RuntimeDefinedParameter("ProviderName", typeof(string), new Collection<Attribute> {
                    new ParameterAttribute {
                        ValueFromPipelineByPropertyName = true,
                        ParameterSetName = Constants.ParameterSets.SourceBySearchSet
                    },
                    new AliasAttribute("Provider"),
                    new ValidateSetAttribute(providerNames.ToArray())
                }));
            }
            else {
                DynamicParameterDictionary.AddOrSet("ProviderName", new RuntimeDefinedParameter("ProviderName", typeof(string), new Collection<Attribute> {
                    new ParameterAttribute {
                        ValueFromPipelineByPropertyName = true,
                        ParameterSetName = Constants.ParameterSets.SourceBySearchSet
                    },
                    new AliasAttribute("Provider")
                }));
            }
        }


        [Parameter(Position = 0)]
        public string Name {get; set;}

        [Parameter(Position = 1)]
        public string Location {get; set;}

        [Parameter]
        public PSCredential Credential {get; set;}

        [Parameter]
        public SwitchParameter Trusted {get; set;}

        public override bool ProcessRecordAsync() {
            if (Stopping) {
                return false;
            }

            var packageProvider = SelectProviders(ProviderName).ReEnumerable();
                switch (packageProvider.Count()) {
                    case 0:
                        Error(Constants.Errors.UnknownProvider, ProviderName);
                        return false;

                    case 1:
                        break;

                    default:
                        Error(Constants.Errors.MatchesMultipleProviders, packageProvider.Select(p => p.ProviderName).JoinWithComma());
                        return false;
                }


            var provider = packageProvider.First();

            using (var sources = provider.ResolvePackageSources(this).CancelWhen(_cancellationEvent.Token)) {
                // first, check if there is a source by this name already.
                var existingSources = sources.Where(each => each.IsRegistered && each.Name.Equals(Name, StringComparison.OrdinalIgnoreCase)).ToArray();

                if (existingSources.Any()) {
                    // if there is, and the user has said -Force, then let's remove it.
                    foreach (var existingSource in existingSources) {
                        if (Force) {

                            if (ShouldProcess(FormatMessageString(Constants.Messages.TargetPackageSource, existingSource.Name, existingSource.Location, existingSource.ProviderName), Constants.Messages.ActionReplacePackageSource).Result) {
                                var removedSources = provider.RemovePackageSource(existingSource.Name, this).CancelWhen(_cancellationEvent.Token);
                                foreach (var removedSource in removedSources) {
                                    Verbose(Constants.Messages.OverwritingPackageSource, removedSource.Name);
                                }
                            }
                        } else {
                            Error(Constants.Errors.PackageSourceExists, existingSource.Name);
                            return false;
                        }
                    }
                }
            }

            string providerNameForProcessMessage = ProviderName.JoinWithComma();
            if (ShouldProcess(FormatMessageString(Constants.Messages.TargetPackageSource, Name, Location, providerNameForProcessMessage), FormatMessageString(Constants.Messages.ActionRegisterPackageSource)).Result)
            {
                using (var added = provider.AddPackageSource(Name, Location, Trusted, this).CancelWhen(_cancellationEvent.Token)) {
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
