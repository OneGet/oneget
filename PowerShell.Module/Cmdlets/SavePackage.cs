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
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.PackageManagement.Implementation;
    using Microsoft.PackageManagement.Internal.Implementation;
    using Microsoft.PackageManagement.Internal.Packaging;
    using Microsoft.PackageManagement.Internal.Utility.Async;
    using Microsoft.PackageManagement.Internal.Utility.Extensions;
    using Microsoft.PackageManagement.Packaging;
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
        // Use the base Source property so relative path will be resolved
        public override string[] Source
        {
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
                // don't append path and package name here
                return resolvedPath;
            }

            // it's not an existing directory
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
            // if provider does not implement downloadpackage throw error saying that save-package is not implemented by provider
            if (!provider.IsMethodImplemented("DownloadPackage"))
            {
                Error(Constants.Errors.MethodNotImplemented, provider.ProviderName, "Save-Package");
            }

            base.ProcessPackage(provider, searchKey, package);

            // if we do save-package jquery -path C:\test then savepath would be C:\test
            var savePath = SaveFileName(package.PackageFilename);

            bool mainPackageDownloaded = false;

            if (!string.IsNullOrWhiteSpace(savePath)) {                
                // let the provider handles everything
                if (ShouldProcess(savePath, FormatMessageString(Resources.Messages.SavePackage)).Result)
                {
                    foreach (var downloadedPkg in provider.DownloadPackage(package, savePath, this.ProviderSpecific(provider)).CancelWhen(CancellationEvent.Token))
                    {
                        if (IsCanceled)
                        {
                            Error(Constants.Errors.ProviderFailToDownloadFile, downloadedPkg.PackageFilename, provider.ProviderName);
                            return;
                        }

                        // check whether main package is downloaded;
                        if (string.Equals(downloadedPkg.CanonicalId, package.CanonicalId, StringComparison.OrdinalIgnoreCase))
                        {
                            mainPackageDownloaded = true;
                        }

                        WriteObject(downloadedPkg);
                    }
                }
            }

            if (!mainPackageDownloaded)
            {
                Error(Constants.Errors.ProviderFailToDownloadFile, package.PackageFilename, provider.ProviderName);
                return;
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
