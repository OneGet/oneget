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
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.OneGet.Implementation;
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Utility.Collections;
    using Microsoft.OneGet.Utility.Extensions;
    using Microsoft.OneGet.Utility.PowerShell;
    using Utility;

    public abstract class CmdletWithProvider : CmdletBase {
        private readonly OptionCategory[] _optionCategories;

        protected CmdletWithProvider(OptionCategory[] categories) {
            _optionCategories = categories;
        }

        private string[] _providerName;

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Used in a powershell parameter.")]
        public string[] ProviderName {
            get {
                return _providerName ?? (_providerName = GetDynamicParameterValue<string[]>("ProviderName"));
            }
            set {
                _providerName = value;
            }
        }

        protected virtual IEnumerable<PackageProvider> SelectedProviders {
            get {

                var didUserSpecifyProviders = ProviderName.IsNullOrEmpty();


                // filter on provider names  - if they specify a provider name, narrow to only those provider names.
                // if this is an actual invocation, this will attempt to bootstrap a provider that the user specified
                // (which will require a prompt or -force or -forcebootstrap )
                var providers = SelectProviders(ProviderName).ReEnumerable();

                if (!providers.Any()) {
                    // the user gave us provider names that we're not able to resolve.

                    if (IsInvocation) {
                        // and we're in an actual cmdlet invocaton.
                        QueueHeldMessage(() => Error(Constants.Errors.UnknownProviders, ProviderName.JoinWithComma()));
                    }
                    // return the empty collection, for all the good it's doing.
                    return providers;
                }

                // fyi, re-enumerable insulates us against null sets.
                var userSpecifiedSources = Sources.ReEnumerable().ToArray();
                var didUserSpecifySources = userSpecifiedSources.Any();

                // filter out providers that don't have the sources that have been specified (only if we have specified a source!)
                if (didUserSpecifySources) {
                    // sources must actually match a name or location. Keeps providers from being a bit dishonest

                    var potentialSources = providers.SelectMany(each => each.ResolvePackageSources(SuppressErrorsAndWarnings).Where(source => userSpecifiedSources.ContainsAnyOfIgnoreCase(source.Name, source.Location))).ReEnumerable();

                    // prefer registered sources
                    var registeredSources = potentialSources.Where(source => source.IsRegistered).ReEnumerable();

                    var filteredproviders = registeredSources.Any() ? registeredSources.Select(source => source.Provider).Distinct().ReEnumerable() : potentialSources.Select(source => source.Provider).Distinct().ReEnumerable();

                    if (!filteredproviders.Any()) {
                        // we've filtered out everthing!
                        
                        if (!didUserSpecifyProviders) {
                            if (IsInvocation) {
                                // user didn't actually specify provider(s), the sources can't be tied to any particular provider
                                QueueHeldMessage(() => Error(Constants.Errors.SourceNotFound, userSpecifiedSources.JoinWithComma()));
                            }
                            // return the empty set.
                            return filteredproviders;
                        }

                        // they gave us both provider name(s) and source(s)
                        // and the source(s) aren't found in the providers they listed

                        if (IsInvocation) {
                            var providerNames = providers.Select(each => each.Name).JoinWithComma();
                            QueueHeldMessage(() => Error(Constants.Errors.NoMatchForProvidersAndSources, providerNames, userSpecifiedSources.JoinWithComma()));
                        }

                        return filteredproviders;
                    }

                    // make this the new subset.
                    providers = filteredproviders;
                }
                

                // filter on: dynamic options - if they specify any dynamic options, limit the provider set to providers with those options.
                var result = FilterProvidersUsingDynamicParameters(providers, didUserSpecifyProviders, didUserSpecifySources).ToArray();

                /* todo : return error messages when dynamic parameters filter everything out. Either here or in the FPUDP fn.

                if (!result.Any()) {
                    // they specified dynamic parameters that implicitly select providers 
                    // that don't fit with the providers and sources that they initially asked for.

                    if (didUserSpecifyProviders) {

                        if (didUserSpecifySources) {
                            // user said provider and source and the dynamic parameters imply providers they didn't select
                            if (IsInvocation) {
                                QueueHeldMessage(() => Error(Errors.DynamicParameters, providerNames, userSpecifiedSources.JoinWithComma()));
                            }
                            // return empty set
                            return result;
                        }

                        // user said provider and then the dynamic parameters imply providers they didn't select
                        if (IsInvocation) {
                            // error
                        }
                        // return empty set
                        return result;

                    }

                    if (didUserSpecifySources) {
                        // user gave sources which implied some providers but the dynamic parameters implied different providers 
                        if (IsInvocation) {
                            // error
                        }
                        // return empty set
                        return result;
                    }

                    // well, this is silly.
                    // if the user didn't specify a source or a provider
                    // but the FilterProvidersUsingDynamicParameters came back empty
                    // that means that they user specified parameters from two conflicting providers
                    // and they forced each other out!

                    if (IsInvocation) {
                        // error
    
                    }
                    
                }
                */
                return result;
            }
        }

       
        protected IEnumerable<PackageProvider> FilterProvidersUsingDynamicParameters(MutableEnumerable<PackageProvider> providers, bool didUserSpecifyProviders, bool didUserSpecifySources) {
            var excluded = new Dictionary<string, IEnumerable<string>>();

            var setparameters = DynamicParameterDictionary.Values.OfType<CustomRuntimeDefinedParameter>().Where(each => each.IsSet).ReEnumerable();

            var matchedProviders = (setparameters.Any() ? providers.Where(p => setparameters.All(each => each.Options.Any(opt => opt.ProviderName == p.ProviderName))) : providers).ReEnumerable();

            foreach (var provider in matchedProviders) {
                // if a 'required' parameter is not filled in, the provider should not be returned.
                // we'll collect these for warnings at the end of the filter.
                var missingRequiredParameters = DynamicParameterDictionary.Values.OfType<CustomRuntimeDefinedParameter>().Where(each => !each.IsSet && each.IsRequiredForProvider(provider.ProviderName)).ReEnumerable();
                if (!missingRequiredParameters.Any()) {
                    yield return provider;
                } else {
                    // remember these so we can warn later.
                    excluded.Add(provider.ProviderName, missingRequiredParameters.Select(each => each.Name).ReEnumerable());
                }
            }

            /* TODO: provide errors in the case where everything got filtered out. Or maybe warnings?
             * 
            var mismatchedProviders = (setparameters.Any() ? providers.Where(each => !matchedProviders.Contains(each)).Where(p => setparameters.Any(each => each.Options.Any(opt => opt.ProviderName == p.ProviderName))) : Enumerable.Empty<PackageProvider>()).ReEnumerable();

            if (!found) {
                // we didn't find anything that matched 
                // they specified dynamic parameters that implicitly select providers 
                // that don't fit with the providers and sources that they initially asked for.

                if (didUserSpecifyProviders || didUserSpecifySources) {

                    if (IsInvocation) {
                        QueueHeldMessage(() => Error(Errors.ExcludedProvidersDueToMissingRequiredParameter, excluded.Keys, userSpecifiedSources.JoinWithComma()));
                    }

                    yield break;

                }

                if (didUserSpecifySources) {
                    // user gave sources which implied some providers but the dynamic parameters implied different providers 
                    if (IsInvocation) {
                        // error
                    }
                    // return empty set
                    return result;
                }

                // well, this is silly.
                // if the user didn't specify a source or a provider
                // but the FilterProvidersUsingDynamicParameters came back empty
                // that means that they user specified parameters from two conflicting providers
                // and they forced each other out!

                if (IsInvocation) {
                    // error

                }
                    
            }
            */
            // these warnings only show for providers that would have otherwise be selected.
            // if not for the missing requrired parameter.
            foreach (var mp in excluded.OrderBy(each => each.Key)) {
                Verbose(Constants.Messages.SkippedProviderMissingRequiredOption, mp.Key, mp.Value);
            }
        }


        protected virtual void GenerateCmdletSpecificParameters(Dictionary<string, object> unboundArguments) {
            if (!IsInvocation) {
                var providerNames = PackageManagementService.AllProviderNames;
                var whatsOnCmdline = GetDynamicParameterValue<string[]>("ProviderName");
                if (whatsOnCmdline != null) {
                    providerNames = providerNames.Concat(whatsOnCmdline).Distinct();
                }

                DynamicParameterDictionary.AddOrSet("ProviderName", new RuntimeDefinedParameter("ProviderName", typeof (string[]), new Collection<Attribute> {
                    new ParameterAttribute {
                        ValueFromPipelineByPropertyName = true
                    },
                    new AliasAttribute("Provider"),
                    new ValidateSetAttribute(providerNames.ToArray())
                }));
            } else {
                DynamicParameterDictionary.AddOrSet("ProviderName", new RuntimeDefinedParameter("ProviderName", typeof(string[]), new Collection<Attribute> {
                    new ParameterAttribute {
                        ValueFromPipelineByPropertyName = true
                    },
                    new AliasAttribute("Provider")
                }));
            }
        }

        protected bool ActualGenerateDynamicParameters(Dictionary<string,object> unboundArguments ) {
            if (CachedStaticParameters == null) {
                // we're in the second call, we're just looking to find out what the static parameters actually are.
                // we're gonna just skip generating the dynamic parameters on this call.
                return true;
            }

            try {

                unboundArguments = unboundArguments ?? new Dictionary<string, object>();

                // if there are unbound arguments that are owned by a provider, we can narrow the rest of the 
                // arguments to just ones that are connected with that provider
                var dynamicOptions = CachedDynamicOptions;

                var keys = unboundArguments.Keys.ToArray();
                if (keys.Length > 0) {
                    var acceptableProviders = CachedDynamicOptions.Where(option => keys.ContainsAnyOfIgnoreCase(option.Name)).Select(option => option.ProviderName).Distinct().ToArray();
                    if (acceptableProviders.Length > 0) {
                        dynamicOptions = dynamicOptions.Where(option => acceptableProviders.Contains(option.ProviderName)).ToArray();
                    }
                }
                // generate the common parameters for our cmdlets (timeout, messagehandler, etc) 
                GenerateCommonDynamicParameters();

                // generate parameters that are specific to the cmdlet being implemented.
                GenerateCmdletSpecificParameters(unboundArguments);

                var staticParameters = GetType().Get<Dictionary<string, ParameterMetadata>>("MyInvocation.MyCommand.Parameters");

                foreach (var md in dynamicOptions) {
                    if (DynamicParameterDictionary.ContainsKey(md.Name)) {
                        // todo: if the dynamic parameters from two providers aren't compatible, then what? 

                        // for now, we're just going to mark the existing parameter as also used by the second provider to specify it.
                        var crdp = DynamicParameterDictionary[md.Name] as CustomRuntimeDefinedParameter;

                        if (crdp == null) {
                            // the package provider is trying to overwrite a parameter that is already dynamically defined by the BaseCmdlet. 
                            // just ignore it.
                            continue;
                        }

                        if (IsInvocation) {
                            // this is during an actual execution
                            crdp.Options.Add(md);
                        } else {
                            // this is for metadata sake. (get-help, etc)
                            crdp.IncludeInParameterSet(md, IsInvocation, ParameterSets);
                        }
                    } else {
                        // check if the dynamic parameter is a static parameter first.

                        // this can happen if we make a common dynamic parameter into a proper static one 
                        // and a provider didn't know that yet.

                        if (staticParameters != null && staticParameters.ContainsKey(md.Name)) {
                            // don't add it.
                            continue;
                        }

                        DynamicParameterDictionary.Add(md.Name, new CustomRuntimeDefinedParameter(md, IsInvocation, ParameterSets));
                    }
                }
            } catch (Exception e) {
                e.Dump();
            }
            return true;
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "It's a performance thing.")]
        protected PackageProvider[] CachedSelectedProviders {
            get {
                return GetType().GetOrAdd(() => SelectedProviders.ToArray(), "CachedSelectedProviders");
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "It's a performance thing.")]
        protected DynamicOption[] CachedDynamicOptions {
            get {
                return GetType().GetOrAdd(() => CachedSelectedProviders.SelectMany(provider => _optionCategories.SelectMany(category => provider.GetDynamicOptions(category, SuppressErrorsAndWarnings))).ToArray(), "CachedDynamicOptions");
            } 
        }

        protected Dictionary<string, ParameterMetadata> CachedStaticParameters {
            get {
                return GetType().Get<Dictionary<string, ParameterMetadata>>("MyInvocation.MyCommand.Parameters");
            }
        }

        public override bool GenerateDynamicParameters() {
            var thisIsFirstObject = false;
            try {
                if (!IsReentrantLocked) {
                    // we're not locked at this point. Let's turn on the lock.
                    IsReentrantLocked = true;
                    thisIsFirstObject = true;

                    try {
                        // do all the work that we need to during the lock 
                        // this includes:
                        //      one-time-per-call work
                        //      any access to MyInvocation.MyCommand.*
                        //      modifying parameter validation sets
                        // 

                        if (MyInvocation != null && MyInvocation.MyCommand != null && MyInvocation.MyCommand.Parameters != null) {
                            GetType().AddOrSet(MyInvocation.MyCommand.Parameters, "MyInvocation.MyCommand.Parameters");
                        } 
#if DEEP_DEBUG
                        else {
                            if (MyInvocation == null) {
                                Console.WriteLine("��� Attempt to get parameters MyInvocation == NULL");
                            } else {
                                if (MyInvocation.MyCommand == null) {
                                    Console.WriteLine("��� Attempt to get parameters MyCommand == NULL");
                                } else {
                                    Console.WriteLine("��� Attempt to get parameters Parameters == NULL");
                                }
                            }
                        }
#endif                         
                        

                        // the second time, it will generate all the parameters, including the dynamic ones.
                        // (not that we currently need it, but if you do, you gotta do it here!)
                        // var all_parameters = MyInvocation.MyCommand.Parameters;

                        // ask for the unbound arguments.
                          var unbound = UnboundArguments;

                        if (unbound.ContainsKey("ProviderName")) {
                            var pName = unbound["ProviderName"];
                            if (pName != null) {
                                ProviderName = pName as string[] ?? new[] { pName.ToString() };
                            }
                            
                        } else if( unbound.ContainsKey("Provider") ) {
                            var pName = unbound["Provider"];
                            if (pName != null) {
                                ProviderName = pName as string[] ?? new[] { pName.ToString() };
                            }
                        }

                        // we've now got a copy of the arguments that aren't bound 
                        // and we can potentially narrow the provider selection based 
                        // on arguments the user specified.

                        if (IsCanceled ) {
#if DEEP_DEBUG
                            Console.WriteLine("��� Cancelled before we got finished doing dynamic parameters");
#endif
                            // this happens if there is a serious failure early in the cmdlet
                            // i.e. - if the SelectedProviders comes back empty (due to agressive filtering)
                            
                            // in this case, we just want to provide a catch-all for remaining arguments so that we can make 
                            // output the error that we really want to (that the user specified conditions that filtered them all out)

                            DynamicParameterDictionary.Add("RemainingArguments", new RuntimeDefinedParameter("RemainingArguments", typeof(object), new Collection<Attribute> {
                                new ParameterAttribute() {   ValueFromRemainingArguments =  true},
                            }));
                        }

                        // at this point, we're actually calling to have the dynamic parameters generated 
                        // that are expected to be used.
                        return ActualGenerateDynamicParameters(unbound);

                    } finally {
                        IsReentrantLocked = false;
                    }
                }

                // otherwise just call the AGDP because we're in a reentrant call.
                // and this might be needed if the original call had some strange need
                // to know what the parameters that it's about to generate would be. 
                // Yeah, you heard me. 
                return ActualGenerateDynamicParameters(null);

            } finally
            {
                if (thisIsFirstObject) {
                    // clean up our once-per-call data.
                    GetType().Remove<PackageProvider[]>( "CachedSelectedProviders");
                    GetType().Remove<Dictionary<string, ParameterMetadata>>("MyInvocation.MyCommand.Parameters");
                    GetType().Remove<DynamicOption[]>("CachedDynamicOptions");
                }
            }
            // return true;
        }
    }
}