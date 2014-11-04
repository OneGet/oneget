﻿// 
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
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.OneGet.Implementation;
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Utility.Async;
    using Microsoft.OneGet.Utility.Extensions;

    [Cmdlet(VerbsData.Save, Constants.Nouns.PackageNoun, SupportsShouldProcess = true, HelpUri = "http://go.microsoft.com/fwlink/?LinkID=517140")]
    public sealed class SavePackage : CmdletWithSearchAndSource {
        public SavePackage()
            : base(new[] {
                OptionCategory.Provider, OptionCategory.Source, OptionCategory.Package
            }) {
        }

        protected override IEnumerable<string> ParameterSets {
            get {
                return new[] {Constants.ParameterSets.PackageByInputObjectSet, ""};
            }
        }

        [Parameter(Position = 0, ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        public override string[] Name { get; set; }

        [Parameter(ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        public override string RequiredVersion { get; set; }

        [Parameter(ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        public override string MinimumVersion { get; set; }

        [Parameter(ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        public override string MaximumVersion { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        public override string[] Source { get; set; }

        protected override void GenerateCmdletSpecificParameters(Dictionary<string, object> unboundArguments) {
            if (!IsInvocation) {
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
                DynamicParameterDictionary.AddOrSet("ProviderName", new RuntimeDefinedParameter("ProviderName", typeof(string[]), new Collection<Attribute> {
                    new ParameterAttribute {
                        ValueFromPipelineByPropertyName = true,
                        ParameterSetName = Constants.ParameterSets.PackageBySearchSet
                    },
                    new AliasAttribute("Provider")
                }));
            }
        }


        [Parameter]
        public SwitchParameter IncludeDependencies {get; set;}

        [Parameter]
        public string DestinationPath {get; set;}

        [Parameter]
        public string LiteralPath {get; set;}

        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = Constants.ParameterSets.PackageByInputObjectSet)]
        public SoftwareIdentity InputObject {get; set;}

        private string SaveFileName(string packageName) {
            string path = null;

            if (string.IsNullOrEmpty(DestinationPath)) {
                path = Path.GetFullPath(LiteralPath);
            } else {
                path = GetUnresolvedProviderPathFromPSPath(DestinationPath);
            }

            if (Directory.Exists(path)) {
                // it appears to be a directory name
                return Path.Combine(path, packageName);
            }

            var parentPath = Path.GetDirectoryName(path);
            if (Directory.Exists(parentPath)) {
                // it appears to be a full path including filename
                return path;
            }

            // it's not an existing directory, 
            // and the parent directory of that path doesn't exist
            // so I guess we're returning null, because I dunno what
            // to do.

            Warning(Constants.Messages.DestinationPathInvalid, DestinationPath, packageName);
            return null;
        }

        public override bool ProcessRecordAsync() {
            if (string.IsNullOrEmpty(DestinationPath) && string.IsNullOrEmpty(LiteralPath)) {
                Error(Constants.Errors.DestinationOrLiteralPathNotSpecified);
                return false;
            }

            if (IsPackageByObject) {
                ProcessPackage(SelectProviders(InputObject.ProviderName).FirstOrDefault(), InputObject.Name, InputObject);
                return true;
            }
            return base.ProcessRecordAsync();
        }

        protected override void ProcessPackage(PackageProvider provider, string searchKey, SoftwareIdentity package) {
            base.ProcessPackage(provider, searchKey, package);

            var savePath = SaveFileName(package.PackageFilename);

            // if we have a valid path, make a local copy of the file.
            if (!string.IsNullOrEmpty(savePath)) {
                if (ShouldProcess(savePath, Constants.Messages.SavePackage).Result) {
                    provider.DownloadPackage(package, SaveFileName(savePath), this).Wait();

                    if (File.Exists(savePath)) {
                        package.FullPath = savePath;
                    }
                }
                // return the object to the caller.
                WriteObject(package);
            }

            if (IncludeDependencies) {
                foreach (var dep in provider.GetPackageDependencies(package, this)) {
                    ProcessPackage(provider, searchKey, dep);
                }
            }
        }

        public override bool EndProcessingAsync() {
            if (IsCanceled) {
                return false;
            }
            if (!IsSourceByObject) {
                return CheckUnmatchedPackages();
            }
            return true;
        }
    }
}