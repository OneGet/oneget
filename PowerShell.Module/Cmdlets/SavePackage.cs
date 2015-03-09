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
    using Utility;
    using Directory = System.IO.Directory;
    using File = System.IO.File;

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
        public string Path {get; set;}

        [Parameter]
        public string LiteralPath {get; set;}

        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = Constants.ParameterSets.PackageByInputObjectSet)]
        public SoftwareIdentity InputObject {get; set;}

        private string SaveFileName(string packageName) {
            string resolvedPath = null;

            try
            {
                // Validate Path
                if (!String.IsNullOrEmpty(Path))
                {
                    ProviderInfo provider = null;
                    Collection<string> resolvedPaths = GetResolvedProviderPathFromPSPath(Path, out provider);

                    // Ensure the path is a single path from the file system provider
                    if ((resolvedPaths.Count > 1) ||
                        (!String.Equals(provider.Name, "FileSystem", StringComparison.OrdinalIgnoreCase)))
                    {
                        Error(Constants.Errors.FilePathMustBeFileSystemPath, Path);
                        return null;
                    }

                    resolvedPath = resolvedPaths[0];
                }

                if (!String.IsNullOrEmpty(LiteralPath))
                {
                    // Validate that the path exists
                    SessionState.InvokeProvider.Item.Get(new string[] { LiteralPath }, false, true);
                    resolvedPath = LiteralPath;
                }
            }
            catch(Exception e)
            {
                Error(Constants.Errors.SavePackageError, e.Message);
                return null;
            }

            if (Directory.Exists(resolvedPath))
            {
                // it appears to be a directory name
                return System.IO.Path.Combine(resolvedPath, packageName);
            }

            var parentPath = System.IO.Path.GetDirectoryName(resolvedPath);
            if (Directory.Exists(parentPath))
            {
                // it appears to be a full path including filename
                return resolvedPath;
            }

            // it's not an existing directory, 
            // and the parent directory of that path doesn't exist.
            // So throw a terminating error.
            Error(Constants.Errors.DestinationPathInvalid, resolvedPath, packageName);
            return null;
        }

        public override bool ProcessRecordAsync() {
            if (string.IsNullOrWhiteSpace(Path) && string.IsNullOrWhiteSpace(LiteralPath)) {
                Error(Constants.Errors.DestinationOrLiteralPathNotSpecified);
                return false;
            }

            if (IsPackageByObject) {
                ProcessPackage(SelectProviders(InputObject.ProviderName).FirstOrDefault(), InputObject.Name.SingleItemAsEnumerable(), InputObject);
                return true;
            }
            return base.ProcessRecordAsync();
        }

        protected override void ProcessPackage(PackageProvider provider, IEnumerable<string> searchKey, SoftwareIdentity package) {
            base.ProcessPackage(provider, searchKey, package);

            var savePath = SaveFileName(package.PackageFilename);

            if (savePath.FileExists()) {
                if (Force) {
                    savePath.TryHardToDelete();
                    if (savePath.FileExists()) {
                        Error(Constants.Errors.UnableToOverwrite, savePath);
                        return;
                    }
                } else {
                    Error(Constants.Errors.PackageFileExists, savePath);
                    return;
                }
            }

            // if we have a valid path, make a local copy of the file.
            if (!string.IsNullOrWhiteSpace(savePath)) {
                if (ShouldProcess(savePath, Constants.Messages.SavePackage).Result) {
                    provider.DownloadPackage(package, SaveFileName(savePath), this.ProviderSpecific(provider)).Wait();

                    if (File.Exists(savePath)) {
                        package.FullPath = savePath;
                    }
                }
                // return the object to the caller.
                WriteObject(package);
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