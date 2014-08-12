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
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Providers.Package;
    using Microsoft.OneGet.Utility.Extensions;

    [Cmdlet(VerbsLifecycle.Unregister, Constants.PackageSourceNoun, SupportsShouldProcess = true)]
    public sealed class UnregisterPackageSource : CmdletWithSource {
        
        public UnregisterPackageSource()
            : base(new[] {
                OptionCategory.Provider, OptionCategory.Source
            }) {
        }

        [Alias("ProviderName")]
        [Parameter(ValueFromPipelineByPropertyName = true,ParameterSetName = Constants.SourceBySearchSet)]

        public string Provider {get; set;}

        public override string[] ProviderName {
            get {
                if (string.IsNullOrEmpty(Provider)) {
                    return null;
                }
                return new[] {
                    Provider
                };
            }
            set {
                // nothing
            }
        }

        [Alias("Name")]
        [Parameter(Position = 0 ,Mandatory = true,ParameterSetName = Constants.SourceBySearchSet)]
        public string Source {get; set;}

        [Parameter(ParameterSetName = Constants.SourceBySearchSet)]
        public string Location { get; set; }

        public override IEnumerable<string> Sources {
            get {
                if (Source.IsEmptyOrNull()) {
                    return new string[0];
                }
                return new string[] {
                    Source
                };
            }
        }

        public override bool ProcessRecordAsync() {
            if (IsSourceByObject) {

                foreach (var source in InputObject) {
                    if (Stopping) {
                        return false;
                    }

                    var provider = SelectProviders(source.ProviderName).FirstOrDefault();
                    if (provider == null) {
                        if (string.IsNullOrEmpty(source.ProviderName)) {
                            return Error(Errors.UnableToFindProviderForSource, source.Name );
                        }
                        return Error(Errors.UnknownProvider, source.ProviderName);
                    }
                    Unregister(source);
                }
                return true;
            }

            // otherwise, we're just deleting a source by name
            var prov = SelectedProviders.ToArray();

            if (Stopping) {
                return false;
            }

            if (prov.Length == 0) {
                if (string.IsNullOrEmpty(Provider)) {
                    return Error(Errors.UnableToFindProviderForSource, Source ?? Location);
                }
                return Error(Errors.UnknownProvider, Provider);
            }

            if (prov.Length > 0) {
                var sources = prov.SelectMany(each => each.ResolvePackageSources(this).Where( source => source.IsRegistered && (source.Name.EqualsIgnoreCase(Source) || source.Location.EqualsIgnoreCase(Source) )).ToArray()).ToArray();

                if (sources.Length == 0) {
                    return Error(Errors.SourceNotFound, Source);
                }

                if (sources.Length > 1) {
                    return Error(Errors.SourceFoundInMultipleProviders,Source, prov.Select(each => each.ProviderName).JoinWithComma());
                }

                return Unregister(sources[0]);
            }

            return true;
        }

        public bool Unregister(PackageSource source) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }
            if (ShouldProcess(FormatMessageString(Constants.TargetPackageSource,source.Name, source.Location, source.ProviderName),FormatMessageString(Constants.ActionUnregisterPackageSource)).Result) {
                source.Provider.RemovePackageSource(source.Name, this);
                return true;
            }
            return false;
        }
    }
}