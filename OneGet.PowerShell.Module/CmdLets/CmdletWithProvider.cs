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
    using System.Threading;
    using Microsoft.OneGet.Implementation;
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Utility.Collections;
    using Utility;

    public abstract class CmdletWithProvider : CmdletBase {
        public static ManualResetEvent _reentrancyLock = new ManualResetEvent(false);
        private readonly OptionCategory[] _optionCategories;

        protected CmdletWithProvider(OptionCategory[] categories) {
            _optionCategories = categories;
        }

        [Alias("Provider")]
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public virtual string[] ProviderName {get; set;}

        protected virtual IEnumerable<PackageProvider> SelectedProviders {
            get {
                // filter on provider names  - if they specify a provider name, narrow to only those provider names.
                var providers = SelectProviders(ProviderName);

                // filter on: dynamic options - if they specify any dynamic options, limit the provider set to providers with those options.
                return FilterProvidersUsingDynamicParameters(providers).ToArray();
            }
        }

        protected IEnumerable<PackageProvider> FilterProvidersUsingDynamicParameters(IEnumerable<PackageProvider> providers) {
            var excluded = new HashSet<string, string>();

            var setparameters = DynamicParameterDictionary.Values.OfType<CustomRuntimeDefinedParameter>().Where(each => each.IsSet).ToArray();

            var matchedProviders = setparameters.Any() ? providers.Where(p => setparameters.All(each => each.Options.Any(opt => opt.ProviderName == p.ProviderName))) : providers;

            foreach (var p in matchedProviders) {
                // if a 'required' parameter is not filled in, the provider should not be returned.
                // we'll collect these for warnings at the end of the filter.
                var missingRequiredParameters = DynamicParameterDictionary.Values.OfType<CustomRuntimeDefinedParameter>().Where(each => !each.IsSet && each.IsRequiredForProvider(p.ProviderName)).ToArray();
                if (missingRequiredParameters.Length == 0) {
                    yield return p;
                } else {
                    // remember these so we can warn later.
                    foreach (var mp in missingRequiredParameters) {
                        excluded.Add(p.ProviderName, mp.Name);
                    }
                }
            }

            // these warnings only show for providers that would have otherwise be selected.
            // if not for the missing requrired parameter.
            foreach (var mp in excluded.OrderBy(each => each.Key)) {
                Warning(Constants.SkippedProviderMissingRequiredOption, mp.Key, mp.Value);
            }
        }

        public override bool GenerateDynamicParameters() {
            // if the provider (or source) is selected, we can get package metadata keys from the provider
            //var providers = SelectedProviders.ToArray();

            if (_reentrancyLock.WaitOne(0)) {
                // we're in here already.
                // this happens because we're asking for the parameters below, and it creates a new instance to get them.
                // we don't want dynamic parameters for that call, so let's get out.
                return true;
            }
            _reentrancyLock.Set();

            try {
                foreach (var md in SelectedProviders.SelectMany(provider => _optionCategories.SelectMany(category => provider.GetDynamicOptions(category, this)))) {
                    // check if the dynamic parameter is a static parameter first.
                    // this can happen if we make a common dynamic parameter into a proper static one 
                    // and a provider didn't know that yet.

                    if (MyInvocation.MyCommand.Parameters.ContainsKey(md.Name)) {
                        // don't add it.
                        continue;
                    }

                    // foreach (var md in _optionCategories.SelectMany(category => providers.SelectMany(provider => provider.GetDynamicOptions(category, this)))) {
                    if (DynamicParameterDictionary.ContainsKey(md.Name)) {
                        // todo: if the dynamic parameters from two providers aren't compatible, then what? 

                        // for now, we're just going to mark the existing parameter as also used by the second provider to specify it.
                        (DynamicParameterDictionary[md.Name] as CustomRuntimeDefinedParameter).Options.Add(md);
                    } else {
                        DynamicParameterDictionary.Add(md.Name, new CustomRuntimeDefinedParameter(md));
                    }
                }
            } finally {
                _reentrancyLock.Reset();
            }

            return true;
        }
    }
}