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
    using Microsoft.PackageManagement.Implementation;
    using Microsoft.PackageManagement.Internal.Packaging;
    using Microsoft.PackageManagement.Internal.Utility.Extensions;
    using Microsoft.PackageManagement.Packaging;

    [Cmdlet(VerbsLifecycle.Install, Constants.Nouns.PackageNoun, SupportsShouldProcess = true, DefaultParameterSetName = Constants.ParameterSets.PackageBySearchSet, HelpUri = "http://go.microsoft.com/fwlink/?LinkID=517138")]
    public sealed class InstallPackage : CmdletWithSearchAndSource {

        public InstallPackage()
            : base(new[] {
                OptionCategory.Provider, OptionCategory.Source, OptionCategory.Package, OptionCategory.Install
            }) {
        }

        protected override IEnumerable<string> ParameterSets {
            get {
                return new[] {Constants.ParameterSets.PackageBySearchSet, Constants.ParameterSets.PackageByInputObjectSet};
            }
        }

        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0, ParameterSetName = Constants.ParameterSets.PackageByInputObjectSet),]
        public SoftwareIdentity[] InputObject {get; set;}

        [Parameter(Position = 0, ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        public override string[] Name {get; set;}

        [Parameter(ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        public override string RequiredVersion {get; set;}

        [Parameter(ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        public override string MinimumVersion {get; set;}

        [Parameter(ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        public override string MaximumVersion {get; set;}

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        // Use the base Source property so relative path will be resolved
        public override string[] Source {
            get
            {
                return base.Source;
            }
            set
            {
                base.Source = value;
            }
        }


        protected override void GenerateCmdletSpecificParameters(Dictionary<string, object> unboundArguments) {
#if DEEP_DEBUG
            Console.WriteLine("»» Entering GCSP ");
#endif
            if (!IsInvocation) {
#if DEEP_DEBUG
                Console.WriteLine("»»» Does not appear to be Invocation (locked:{0})", IsReentrantLocked);
#endif
                var providerNames = PackageManagementService.AllProviderNames;
                var whatsOnCmdline = GetDynamicParameterValue<string[]>("ProviderName");
                if (whatsOnCmdline != null) {
                    providerNames = providerNames.Concat(whatsOnCmdline).Distinct();
                }

                DynamicParameterDictionary.AddOrSet("ProviderName", new RuntimeDefinedParameter("ProviderName", typeof(string[]), new Collection<Attribute> {
                    new ParameterAttribute {
                        ValueFromPipelineByPropertyName = true,
                        ParameterSetName = Constants.ParameterSets.PackageBySearchSet
                    },
                    new AliasAttribute("Provider"),
                    new ValidateSetAttribute(providerNames.ToArray())
                }));
            }
            else {
#if DEEP_DEBUG
                Console.WriteLine("»»» Does appear to be Invocation (locked:{0})", IsReentrantLocked);
#endif
                DynamicParameterDictionary.AddOrSet("ProviderName", new RuntimeDefinedParameter("ProviderName", typeof(string[]), new Collection<Attribute> {
                    new ParameterAttribute {
                        ValueFromPipelineByPropertyName = true,
                        ParameterSetName = Constants.ParameterSets.PackageBySearchSet
                    },
                    new AliasAttribute("Provider")
                }));
            }
        }



        public override bool BeginProcessingAsync() {
            return true;
        }

        public override bool ProcessRecordAsync() {
            if (IsPackageByObject) {
                return InstallPackages(InputObject);
            }
            if (MyInvocation.BoundParameters.Count == 0 || (MyInvocation.BoundParameters.Count == 1 &&  MyInvocation.BoundParameters.ContainsKey("ProviderName")) ) {
                // didn't pass in anything, (except maybe Providername)
                // that's no ok -- we need some criteria
                Error(Constants.Errors.MustSpecifyCriteria);
                return false;
            }

            if (Name.Any(each => each.ContainsWildcards())) {
                Error(Constants.Errors.WildCardCharsAreNotSupported, Name.JoinWithComma());
                return false;
            }
            // otherwise, just do the search right now.
            return base.ProcessRecordAsync();
        }

        public override bool EndProcessingAsync() {
            if (IsPackageByObject) {
                // we should have handled these already.
                // buh-bye
                return true;
            }

            if (!CheckUnmatchedPackages()) {
                // there are unmatched packages
                // not going to install.
                return false;
            }

            if (!CheckMatchedDuplicates()) {
                // there are duplicate packages
                // not going to install.
                return false;
            }

            // good list. Let's roll...
            return base.InstallPackages(_resultsPerName.Values.SelectMany(each => each).ToArray());
        }

        protected override void ProcessPackage(PackageProvider provider, IEnumerable<string> searchKey, SoftwareIdentity package) {
            if (WhatIf) {
                // grab the dependencies and return them *first*

                 foreach (var dep in package.Dependencies) {
                    // note: future work may be needed if the package sources currently selected by the user don't
                    // contain the dependencies.
                    var dependendcies = PackageManagementService.FindPackageByCanonicalId(dep, this);
                    foreach (var depPackage in dependendcies) {
                        ProcessPackage( depPackage.Provider,  searchKey.Select( each => each+depPackage.Name).ToArray(), depPackage);
                    }
                }
            }
            base.ProcessPackage(provider, searchKey, package);
        }
    }
}
