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
    using Microsoft.OneGet.Implementation;
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Utility.Collections;
    using Microsoft.OneGet.Utility.Extensions;
    using Utility;

    [Cmdlet(VerbsLifecycle.Install, Constants.PackageNoun, SupportsShouldProcess = true, DefaultParameterSetName = Constants.PackageBySearchSet)]
    public sealed class InstallPackage : CmdletWithSearchAndSource {
        private readonly HashSet<string> _sourcesTrusted = new HashSet<string>();

        public InstallPackage()
            : base(new[] {
                OptionCategory.Provider, OptionCategory.Source, OptionCategory.Package, OptionCategory.Install
            }) {
        }

        protected override IEnumerable<string> ParameterSets {
            get {
                return new[] { Constants.PackageBySearchSet, Constants.PackageByInputObjectSet};
            }
        }

        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0, ParameterSetName = Constants.PackageByInputObjectSet)]
        public SoftwareIdentity[] InputObject {get; set;}

        [Parameter(Position = 0, ParameterSetName = Constants.PackageBySearchSet)]
        public override string[] Name {get; set;}

        [Parameter(ParameterSetName = Constants.PackageBySearchSet)]
        public override string RequiredVersion {get; set;}

        [Parameter(ParameterSetName = Constants.PackageBySearchSet)]
        public override string MinimumVersion {get; set;}

        [Parameter(ParameterSetName = Constants.PackageBySearchSet)]
        public override string MaximumVersion {get; set;}

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = Constants.PackageBySearchSet)]
        public override string[] Source {get; set;}

        [Alias("Provider")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = Constants.PackageBySearchSet)]
        public override string[] ProviderName {get; set;}

        public override bool BeginProcessingAsync() {
            return true;
        }

        public override bool ProcessRecordAsync() {
            if (IsPackageByObject) {
                return InstallPackages(InputObject);
            }
            // otherwise, just do the search right now.
            return base.ProcessRecordAsync();
        }

        public override bool EndProcessingAsync() {
            if (IsPackageByObject) {
                // we should have handled these already.
                // buh-bye
                return true;
            }

            if (!CheckUnmatchedPackages()) {
                // there are unmatched packages
                // not going to install.
                return false;
            }

            if (!CheckMatchedDuplicates()) {
                // there are duplicate packages 
                // not going to install.
                return false;
            }

            // good list. Let's roll...
            return InstallPackages(_resultsPerName.Values.SelectMany(each => each).ToArray());
        }

        protected override void ProcessPackage(PackageProvider provider, string searchKey, SoftwareIdentity package) {
            if (WhatIf) {
                // grab the dependencies and return them *first*
                foreach (var dep in provider.GetPackageDependencies(package, this)) {
                    ProcessPackage(provider, searchKey + dep.Name, dep);
                }
            }
            base.ProcessPackage(provider, searchKey, package);
        }

        private bool InstallPackages(params SoftwareIdentity[] packagesToInstall) {
            // first, check to see if we have all the required dynamic parameters
            // for each package/provider
            foreach (var package in packagesToInstall) {
                var pkg = package;
                foreach (var parameter in DynamicParameterDictionary.Values.OfType<CustomRuntimeDefinedParameter>()
                    .Where(param => param.IsSet == false && param.Options.Any(option => option.ProviderName == pkg.ProviderName && option.Category == OptionCategory.Install && option.IsRequired))) {
                    // this is not good. there is a required parameter for the package 
                    // and the user didn't specify it. We should return the error to the user
                    // and they can try again.
                    Error(Errors.PackageInstallRequiresOption, package.Name, package.ProviderName, parameter.Name);
                    Cancel();
                }
            }

            if (IsCanceled) {
                return false;
            }
            var progressId = 0;

            if (packagesToInstall.Length > 1) {
                progressId = StartProgress(0, "Installing {0} packages", packagesToInstall.Length);
            }
            var n = 0;
            foreach (var pkg in packagesToInstall) {
                if (packagesToInstall.Length > 1) {
                    Progress(progressId, (n*100/packagesToInstall.Length) + 1, "Installing Package '{0}' ({1} of {2})", pkg.Name, ++n, packagesToInstall.Length);
                }
                var provider = SelectProviders(pkg.ProviderName).FirstOrDefault();
                if (provider == null) {
                    Error(Errors.UnknownProvider, pkg.ProviderName);
                    return false;
                }
                try {
                    // if (WhatIf) {
                    // we should just tell it which packages will be installed.
                    // todo: [M2] should we be checking the installed status before we show this
                    // todo:      or should we rethink allowing the providers to willingly support -whatif?
                    // ShouldProcessPackageInstall(pkg.Name, pkg.Version, pkg.Source);
                    //} else {
                    if (ShouldProcessPackageInstall(pkg.Name, pkg.Version, pkg.Source)) {
                        foreach (var installedPkg in provider.InstallPackage(pkg, this).CancelWhen(_cancellationEvent.Token)) {
                            if (IsCanceled) {
                                // if we're stopping, just get out asap.
                                return false;
                            }
                            WriteObject(installedPkg);
                        }
                    }
                    //}
                } catch (Exception e) {
                    e.Dump();
                    Error(Errors.InstallationFailure, pkg.Name);
                    return false;
                }
                if (packagesToInstall.Length > 1) {
                    Progress(progressId, (n*100/packagesToInstall.Length) + 1, "Installed Package '{0}' ({1} of {2})", pkg.Name, n, packagesToInstall.Length);
                }
            }

            return true;
        }

        public bool ShouldProcessPackageInstall(string packageName, string version, string source) {
            try {
                return Force || ShouldProcess(FormatMessageString(Constants.TargetPackage, packageName, version, source), FormatMessageString(Constants.ActionInstallPackage)).Result;
            } catch {
            }
            return false;
        }

        public override bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource) {
            try {
                if (_sourcesTrusted.Contains(packageSource) || Force || WhatIf) {
                    return true;
                }
                if (ShouldContinue(FormatMessageString(Constants.QueryInstallUntrustedPackage, package, packageSource), FormatMessageString(Constants.CaptionPackageNotTrusted, package)).Result) {
                    _sourcesTrusted.Add(packageSource);
                    return true;
                }
            } catch {
            }
            return false;
        }

       
    }
}