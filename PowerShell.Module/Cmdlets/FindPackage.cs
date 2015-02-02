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

namespace Microsoft.PowerShell.OneGet.Cmdlets {
    using System.Collections.Generic;
    using System.Management.Automation;
    using Microsoft.OneGet.Implementation;
    using Microsoft.OneGet.Packaging;
    using Utility;

    [Cmdlet(VerbsCommon.Find, Constants.Nouns.PackageNoun, HelpUri = "http://go.microsoft.com/fwlink/?LinkID=517132"), OutputType(typeof(SoftwareIdentity))]
    public sealed class FindPackage : CmdletWithSearchAndSource {
        public FindPackage()
            : base(new[] {
                OptionCategory.Provider, OptionCategory.Source, OptionCategory.Package
            }) {
        }

        protected override IEnumerable<string> ParameterSets {
            get {
                return new[] {"",};
            }
        }

        [Parameter]
        public SwitchParameter IncludeDependencies {get; set;}

        [Parameter]
        public override SwitchParameter AllVersions {get; set;}

        protected override void ProcessPackage(PackageProvider provider, IEnumerable<string> searchKey, SoftwareIdentity package) {
            base.ProcessPackage(provider, searchKey, package);

            // return the object to the caller now.
            WriteObject(package);

            if (IncludeDependencies) {
                foreach (var dep in provider.GetPackageDependencies(package, this.ProviderSpecific(provider))) {
                    ProcessPackage(provider, searchKey, dep);
                }
            }
        }

        public override bool EndProcessingAsync() {
            return CheckUnmatchedPackages();
        }
    }
}