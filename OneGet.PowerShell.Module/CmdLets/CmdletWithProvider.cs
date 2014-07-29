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
    using Microsoft.OneGet.Collections;
    using Microsoft.OneGet.Extensions;
    using Microsoft.OneGet.Providers.Package;
    using Utility;

    public abstract class CmdletWithProvider : CmdletBase {
        private readonly OptionCategory[] _optionCategories;

        protected CmdletWithProvider(OptionCategory[] categories) {
            _optionCategories = categories;
        }

        [Parameter(ParameterSetName = Constants.ProviderByObjectSet, Mandatory = true, ValueFromPipeline = true)]
        public virtual PackageProvider[] PackageProvider {get; set;}

        [Parameter(ParameterSetName = Constants.ProviderByNameSet)]
        public virtual string[] Provider {get; set;}

        protected virtual IEnumerable<PackageProvider> SelectedProviders {
            get {
                if (IsProviderByObject) {
                    if (PackageProvider.IsNullOrEmpty()) {
                        Error(Messages.NoProviderSelected);
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
            var excluded = new HashSet<string, string>();

            var setparameters = DynamicParameterDictionary.Values.OfType<CustomRuntimeDefinedParameter>().Where(each => each.IsSet).ToArray();

            var matchedProviders = setparameters.Any() ? providers.Where(p => setparameters.All(each => each.Options.Any(opt => opt.ProviderName == p.Name))) : providers;

                foreach (var p in matchedProviders) {
                    // if a 'required' parameter is not filled in, the provider should not be returned.
                    // we'll collect these for warnings at the end of the filter.
                    var missingRequiredParameters = DynamicParameterDictionary.Values.OfType<CustomRuntimeDefinedParameter>().Where(each => !each.IsSet && each.IsRequiredForProvider(p.Name)).ToArray();
                    if (missingRequiredParameters.Length == 0) {
                        yield return p;
                    } else {
                        // remember these so we can warn later.
                        foreach (var mp in missingRequiredParameters) {
                            excluded.Add(p.Name, mp.Name);
                        }
                    }
                }

            // these warnings only show for providers that would have otherwise be selected.
            // if not for the missing requrired parameter.
            foreach (var mp in excluded.OrderBy( each => each.Key )) {
                Warning(Messages.ExcludedProviderDueToMissingRequiredOption, mp.Key, mp.Value );
            }
        }

        public override bool GenerateDynamicParameters() {
            // if the provider (or source) is selected, we can get package metadata keys from the provider
            // hmm. let's just grab *all* of them.
            foreach (var md in _optionCategories.SelectMany(c => SelectedProviders.SelectMany(provider => provider.GetDynamicOptions(c, this)))) {
                if (DynamicParameterDictionary.ContainsKey(md.Name)) {
                    // todo: if the dynamic parameters from two providers aren't compatible, then what? 

                    // for now, we're just going to mark the existing parameter as also used by the second provider to specify it.
                    (DynamicParameterDictionary[md.Name] as CustomRuntimeDefinedParameter).Options.Add(md);
                } else {
                    DynamicParameterDictionary.Add(md.Name, new CustomRuntimeDefinedParameter(md));
                }
            }
            return true;
        }
    }
}