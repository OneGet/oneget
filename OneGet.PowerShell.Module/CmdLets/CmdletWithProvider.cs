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
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.OneGet.Core.Extensions;
    using Microsoft.OneGet.Core.Providers.Package;
    using Utility;

    public abstract class CmdletWithProvider : CmdletBase {
        private readonly OptionCategory[] _optionCategories;
        private string[] _providerNames;

        protected CmdletWithProvider(OptionCategory[] categories) {
            _optionCategories = categories;
        }

        [Parameter(ParameterSetName = ProviderByObjectSet, Mandatory = true, ValueFromPipeline = true)]
        public virtual PackageProvider[] PackageProvider {get; set;}

        [Parameter(ParameterSetName = ProviderByNameSet)]
        public virtual string[] Provider {get; set;}

        protected string[] AllProviderNames {
            get {
                return _providerNames ?? (_providerNames = PackageManagementService.ProviderNames.ToArray());
            }
        }

        protected virtual PackageProvider[] SelectedProviders {
            get {
                if (IsProviderByObject) {
                    if (PackageProvider.IsNullOrEmpty()) {
                        Error("NO_PROVIDER_SELECTED");
                        return null;
                    }
                    return FilterProvidersUsingDynamicParameters(PackageProvider).ToArray();
                }

                // filter on provider names  - if they specify a provider name, narrow to only those provider names.
                var providers = SelectProviders(Provider);

                // filter on: dynamic options - if they specify any dynamic options, limit the provider set to providers with those options.
                return FilterProvidersUsingDynamicParameters(providers).ToArray();
            }
        }

        protected IEnumerable<PackageProvider> FilterProvidersUsingDynamicParameters(IEnumerable<PackageProvider> providers) {
            var setparameters = DynamicParameters.Values.OfType<CustomRuntimeDefinedParameter>().Where(each => each.IsSet).ToArray();

            if (setparameters.Any()) {
                foreach (var p in providers.Where(p => setparameters.All(each => each.Options.Any(opt => opt.ProviderName == p.Name)))) {
                    yield return p;
                }
                yield break;
            }
            foreach (var p in providers) {
                yield return p;
            }
        }

        public override bool GenerateDynamicParameters() {
            // if the provider (or source) is selected, we can get package metadata keys from the provider
            // hmm. let's just grab *all* of them.
            foreach (var md in _optionCategories.SelectMany(c => SelectedProviders.SelectMany(provider => provider.GetDynamicOptions(c, this)))) {
                if (DynamicParameters.ContainsKey(md.Name)) {
                    // todo: if the dynamic parameters from two providers aren't compatible, then what? 

                    // for now, we're just going to mark the existing parameter as also used by the second provider to specify it.
                    (DynamicParameters[md.Name] as CustomRuntimeDefinedParameter).Options.Add(md);
                } else {
                    DynamicParameters.Add(md.Name, new CustomRuntimeDefinedParameter(md));
                }
            }
            return true;
        }
    }
}