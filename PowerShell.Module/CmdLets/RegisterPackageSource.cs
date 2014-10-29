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
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Utility.Async;
    using Microsoft.OneGet.Utility.Collections;
    using Microsoft.OneGet.Utility.Extensions;
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
                var providerNames = PackageManagementService.ProviderNames;
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

#if OLD_WAY
        public override bool GenerateDynamicParameters() {
            if (_reentrancyLock.WaitOne(0)) {
                // we're in here already.
                // this happens because we're asking for the parameters below, and it creates a new instance to get them.
                // we don't want dynamic parameters for that call, so let's get out.
                return true;
            }
            _reentrancyLock.Set();

            // generate the common parameters for our cmdlets (timeout, messagehandler, etc) 
            GenerateCommonDynamicParameters();

            var providers = SelectProviders(ProviderName).ReEnumerable();

            // if more than one provider is selected, this will never work
            if (providers.Count() != 1) {
                return false;
            }

            var provider = providers.First();

            try {

                // if the provider is selected, we can get package metadata keys from the provider
                foreach (var md in provider.GetDynamicOptions(OptionCategory.Source, this)) {

                    if (MyInvocation.MyCommand.Parameters.ContainsKey(md.Name)) {
                        // don't add it.
                        continue;
                    }

                    if (DynamicParameterDictionary.ContainsKey(md.Name)) {

                        // for now, we're just going to mark the existing parameter as also used by the second provider to specify it.
                        var crdp = DynamicParameterDictionary[md.Name] as CustomRuntimeDefinedParameter;
                        if (crdp == null) {
                            // the provider is trying to overwrite a parameter that is already dynamically defined by the BaseCmdlet. 
                            // just ignore it.
                            continue;
                        }

                        if (IsInvocation) {
                            crdp.Options.Add(md);
                        }
                        else {
                            crdp.IncludeInParameterSet(md, IsInvocation, ParameterSets);
                        }

                    }
                    else {
                        DynamicParameterDictionary.Add(md.Name, new CustomRuntimeDefinedParameter(md, IsInvocation, ParameterSets));
                    }
                }
            }
            finally {
                _reentrancyLock.Reset();
            }
            return true;
        }
#endif 

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
                    Error(Constants.Errors.MatchesMultipleProviders, packageProvider.Select(provider => provider.ProviderName).JoinWithComma());
                    return false;
            }

            using (var sources = packageProvider.First().ResolvePackageSources(this).CancelWhen(_cancellationEvent.Token)) {
                // first, check if there is a source by this name already.
                var existingSources = sources.Where(each => each.IsRegistered && each.Name.Equals(Name, StringComparison.OrdinalIgnoreCase)).ToArray();

                if (existingSources.Any()) {
                    // if there is, and the user has said -Force, then let's remove it.
                    foreach (var existingSource in existingSources) {
                        if (Force) {
                            if (ShouldProcess(FormatMessageString(Constants.Messages.TargetPackageSource, existingSource.Name, existingSource.Location, existingSource.ProviderName), Constants.Messages.ActionReplacePackageSource).Result) {
                                var removedSources = packageProvider.First().RemovePackageSource(existingSource.Name, this).CancelWhen(_cancellationEvent.Token);
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

            if (ShouldProcess(FormatMessageString(Constants.Messages.TargetPackageSource, Name, Location, ProviderName), FormatMessageString(Constants.Messages.ActionRegisterPackageSource)).Result) {
                using (var added = packageProvider.First().AddPackageSource(Name, Location, Trusted, this).CancelWhen(_cancellationEvent.Token)) {
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