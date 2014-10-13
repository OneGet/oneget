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
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Utility.Extensions;

    [Cmdlet(VerbsCommon.Set, Constants.PackageSourceNoun, SupportsShouldProcess = true, DefaultParameterSetName = Constants.SourceBySearchSet)]
    public sealed class SetPackageSource : CmdletWithProvider {
        [Parameter(ValueFromPipeline = true, ParameterSetName = Constants.SourceByInputObjectSet, Mandatory = true)]
        public PackageSource InputObject;

        public SetPackageSource() : base(new[] {OptionCategory.Provider, OptionCategory.Source}) {
        }

        protected override IEnumerable<string> ParameterSets {
            get {
                return new[] {Constants.SourceByInputObjectSet, Constants.SourceBySearchSet};
            }
        }

        [Alias("ProviderName")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = Constants.SourceBySearchSet, Mandatory = true)]
        public string Provider {get; set;}

        public override string[] ProviderName {
            get {
                if (string.IsNullOrEmpty(Provider)) {
                    return null;
                }
                return new[] {Provider};
            }
            set {
                // nothing
            }
        }

        [Alias("SourceName")]
        [Parameter(Position = 0, ParameterSetName = Constants.SourceBySearchSet)]
        public string Name {get; set;}

        [Parameter(ParameterSetName = Constants.SourceBySearchSet)]
        public string Location {get; set;}

        [Parameter]
        public string NewLocation {get; set;}

        [Parameter]
        public string NewName {get; set;}

        [Parameter]
        public SwitchParameter Trusted {get; set;}

        public override IEnumerable<string> Sources {
            get {
                if (Name.IsEmptyOrNull() && Location.IsEmptyOrNull()) {
                    return Microsoft.OneGet.Constants.Empty;
                }

                return new[] {
                    Name ?? Location
                };
            }
        }

        public override IEnumerable<string> GetOptionKeys() {
            return base.GetOptionKeys().ConcatSingleItem("IsUpdatePackageSource").ByRef();
        }

        public override IEnumerable<string> GetOptionValues(string key) {
            if (key != null && key.EqualsIgnoreCase("IsUpdatePackageSource")) {
                return "true".SingleItemAsEnumerable().ByRef();
            }
            return base.GetOptionValues(key);
        }

        private void UpdatePackageSource(PackageSource source) {
            foreach (var src in source.Provider.AddPackageSource(string.IsNullOrEmpty(NewName) ? source.Name : NewName, string.IsNullOrEmpty(NewLocation) ? source.Location : NewLocation, Trusted, this)) {
                WriteObject(src);
            }
        }

        public override bool ProcessRecordAsync() {
            if (IsSourceByObject) {
                // we've already got the package source
                UpdatePackageSource(InputObject);
                return true;
            }

            if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Location)) {
                Error(Errors.NameOrLocationRequired);
                return false;
            }

            // otherwise, we're just changing a source by name
            var prov = SelectedProviders.ToArray();

            if (Stopping) {
                return false;
            }

            if (prov.Length == 0) {
                if (string.IsNullOrEmpty(Provider)) {
                    return Error(Errors.UnableToFindProviderForSource, Name ?? Location);
                }
                return Error(Errors.UnknownProvider, Provider);
            }

            if (prov.Length > 0) {
                var sources = prov.SelectMany(each => each.ResolvePackageSources(SuppressErrorsAndWarnings).Where(source => source.IsRegistered &&
                                                                                                       (Name == null || source.Name.EqualsIgnoreCase(Name)) || (Location == null || source.Location.EqualsIgnoreCase(Location))).ToArray()).ToArray();

                if (sources.Length == 0) {
                    return Error(Errors.SourceNotFound, Name);
                }

                if (sources.Length > 1) {
                    return Error(Errors.SourceFoundInMultipleProviders, Name, prov.Select(each => each.ProviderName).JoinWithComma());
                }

                UpdatePackageSource(sources[0]);
            }
            return true;
        }
    }
}