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
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.OneGet.Implementation;
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Utility.Async;
    using Microsoft.OneGet.Utility.Extensions;
    using Utility;

    [Cmdlet(VerbsLifecycle.Install, Constants.Nouns.PackageNoun, SupportsShouldProcess = true, DefaultParameterSetName = Constants.ParameterSets.PackageBySearchSet, HelpUri = "http://go.microsoft.com/fwlink/?LinkID=517138")]
    public sealed class InstallPackage : CmdletWithSearchAndSource {
        private readonly HashSet<string> _sourcesTrusted = new HashSet<string>();
        private HashSet<string> __sourcesDeniedTrust = new HashSet<string>();

        public InstallPackage()
            : base(new[] {
                OptionCategory.Provider, OptionCategory.Source, OptionCategory.Package, OptionCategory.Install
            }) {
        }

        protected override IEnumerable<string> ParameterSets {
            get {
                return new[] {Constants.ParameterSets.PackageBySearchSet, Constants.ParameterSets.PackageByInputObjectSet};
            }
        }

        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0, ParameterSetName = Constants.ParameterSets.PackageByInputObjectSet),]
        public SoftwareIdentity[] InputObject {get; set;}

        [Parameter(Position = 0, ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        public override string[] Name {get; set;}

        [Parameter(ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        public override string RequiredVersion {get; set;}

        [Parameter(ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        public override string MinimumVersion {get; set;}

        [Parameter(ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        public override string MaximumVersion {get; set;}

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = Constants.ParameterSets.PackageBySearchSet)]
        public override string[] Source {get; set;}

      
        protected override void GenerateCmdletSpecificParameters(Dictionary<string, object> unboundArguments) {
#if DEEP_DEBUG
            Console.WriteLine("»» Entering GCSP ");
#endif
            if (!IsInvocation) {
#if DEEP_DEBUG
                Console.WriteLine("»»» Does not appear to be Invocation (locked:{0})", IsReentrantLocked);
#endif 
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
#if DEEP_DEBUG
                Console.WriteLine("»»» Does appear to be Invocation (locked:{0})", IsReentrantLocked);
#endif
                DynamicParameterDictionary.AddOrSet("ProviderName", new RuntimeDefinedParameter("ProviderName", typeof(string[]), new Collection<Attribute> {
                    new ParameterAttribute {
                        ValueFromPipelineByPropertyName = true,
                        ParameterSetName = Constants.ParameterSets.PackageBySearchSet
                    },
                    new AliasAttribute("Provider")
                }));
            }
        }



        public override bool BeginProcessingAsync() {
            return true;
        }

        public override bool ProcessRecordAsync() {
            if (IsPackageByObject) {
                return InstallPackages(InputObject);
            }
            if (MyInvocation.BoundParameters.Count == 0 || (MyInvocation.BoundParameters.Count == 1 &&  MyInvocation.BoundParameters.ContainsKey("ProviderName")) ) {
                // didn't pass in anything, (except maybe Providername)
                // that's no ok -- we need some criteria 
                Error(Constants.Errors.MustSpecifyCriteria);
                return false;
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

        protected override void ProcessPackage(PackageProvider provider, IEnumerable<string> searchKey, SoftwareIdentity package) {
            if (WhatIf) {
                // grab the dependencies and return them *first*
             
                 foreach (var dep in package.Dependencies) {
                    // note: future work may be needed if the package sources currently selected by the user don't
                    // contain the dependencies. 
                    var dependendcies = PackageManagementService.FindPackageByCanonicalId(dep, this);
                    foreach (var depPackage in dependendcies) {
                        ProcessPackage( depPackage.Provider,  searchKey.Select( each => each+depPackage.Name).ToArray(), depPackage);
                    }
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
                    Error(Constants.Errors.PackageInstallRequiresOption, package.Name, package.ProviderName, parameter.Name);
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
                    Error(Constants.Errors.UnknownProvider, pkg.ProviderName);
                    return false;
                }

                // quickly check to see if this package is already installed.
                var installedPkgs = provider.GetInstalledPackages(pkg.Name,pkg.Version,null,null, this.ProviderSpecific(provider)).CancelWhen(_cancellationEvent.Token).ToArray();
                if (IsCanceled) {
                    // if we're stopping, just get out asap.
                    return false;
                }

                // todo: this is a terribly simplistic way to do this, we'd better rethink this soon
                if (!Force) {
                    if (installedPkgs.Any(each => each.Name.EqualsIgnoreCase(pkg.Name) && each.Version.EqualsIgnoreCase(pkg.Version))) {
                        // it looks like it's already installed.
                        // skip it.
                        Verbose("Skipping installed package {0} {1}", pkg.Name, pkg.Version);

                        if (packagesToInstall.Length > 1) {
                            Progress(progressId, (n*100/packagesToInstall.Length) + 1, "Skipping Installed Package '{0}' ({1} of {2})", pkg.Name, n, packagesToInstall.Length);
                        }
                        continue;
                    }
                }

                try {
                    // if (WhatIf) {
                    // we should just tell it which packages will be installed.
                    // todo: [M2] should we be checking the installed status before we show this
                    // todo:      or should we rethink allowing the providers to willingly support -whatif?
                    // ShouldProcessPackageInstall(pkg.Name, pkg.Version, pkg.Source);
                    //} else {
                    if (ShouldProcessPackageInstall(pkg.Name, pkg.Version, pkg.Source)) {
                        foreach (var installedPkg in provider.InstallPackage(pkg, this.ProviderSpecific(provider)).CancelWhen(_cancellationEvent.Token)) {
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
                    Error(Constants.Errors.InstallationFailure, pkg.Name);
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
                return Force || ShouldProcess(FormatMessageString(Constants.Messages.TargetPackage, packageName, version, source), FormatMessageString(Constants.Messages.ActionInstallPackage)).Result;
            } catch {
            }
            return false;
        }

        public override bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource) {
            try {
                if (_sourcesTrusted.Contains(packageSource) || Force || WhatIf) {
                    return true;
                }
                    if(__sourcesDeniedTrust.Contains(packageSource)) {
                        return false;
                    }
                    if (ShouldContinue(FormatMessageString(Constants.Messages.QueryInstallUntrustedPackage, package, packageSource), FormatMessageString(Constants.Messages.CaptionSourceNotTrusted)).Result) {
                    _sourcesTrusted.Add(packageSource);
                    return true;
                    } else {
                        __sourcesDeniedTrust.Add(packageSource);
                }
            } catch {
            }
            return false;
        }
    }
}