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

namespace Microsoft.PowerShell.OneGet.Core {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.OneGet;
    using Microsoft.OneGet.Core.Extensions;
    using Callback = System.Object;

    public class OneGetCmdlet : AsyncCmdlet {
        public const string PackageNoun = "Package";
        public const string PackageSourceNoun = "PackageSource";
        public const string PackageProviderNoun = "PackageProvider";
        private static readonly object _lockObject = new object();

        protected static IPackageManagementService _packageManagementService = new PackageManagementService().Instance;

        protected override void Init() {
            if (IsCancelled()) {
                return;
            }
            if (!IsInitialized) {
                lock (_lockObject) {
                    if (!IsInitialized) {
                        try {
                            IsInitialized = _packageManagementService.Initialize(Invoke, !IsInvocation);
                        } catch (Exception e) {
                            e.Dump();
                        }
                    }
                }
            }
        }

        public virtual IEnumerable<string> GetPackageSources() {
            return Enumerable.Empty<string>();
        }

        public virtual bool ShouldProcessPackageInstall(string packageName, string version, string source) {
            Message("ShouldProcessPackageInstall", new string[] {
                packageName
            });
            return false;
        }

        public virtual bool ShouldProcessPackageUninstall(string packageName, string version) {
            Message("ShouldProcessPackageUnInstall", new string[] {
                packageName
            });
            return false;
        }

        public virtual bool ShouldContinueAfterPackageInstallFailure(string packageName, string version, string source) {
            Message("ShouldContinueAfterPackageInstallFailure", new string[] {
                packageName
            });
            return false;
        }

        public virtual bool ShouldContinueAfterPackageUninstallFailure(string packageName, string version, string source) {
            Message("ShouldContinueAfterPackageUnInstallFailure", new string[] {
                packageName
            });
            return false;
        }

        public virtual bool ShouldContinueRunningInstallScript(string packageName, string version, string source, string scriptLocation) {
            Message("ShouldContinueRunningInstallScript", new string[] {
                packageName
            });
            return false;
        }

        public virtual bool ShouldContinueRunningUninstallScript(string packageName, string version, string source, string scriptLocation) {
            Message("ShouldContinueRunningUninstallScript", new string[] {
                packageName
            });
            return true;
        }

        public virtual bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource) {
            Message("ShouldContinueWithUntrustedPackageSource", new string[] {
                packageSource
            });
            return true;
        }
    }
}