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
    using System.IO;
    using System.Management.Automation;
    using System.Threading.Tasks;
    using Microsoft.OneGet.Extensions;
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Providers.Package;

    [Cmdlet(VerbsCommon.Find, PackageNoun), OutputType(typeof (SoftwareIdentity))]
    public class FindPackage : CmdletWithSearchAndSource {
        public FindPackage()
            : base(new[] {
                OptionCategory.Provider, OptionCategory.Source, OptionCategory.Package
            }) {
        }

        //   public override bool ProcessRecordAsync() {

        // }

        [Parameter()]
        public string SavePackageTo {get; set;}

        [Parameter()]
        public SwitchParameter ListDependencies { get; set; }

        private string SaveFileName(string packageName) {
            if (string.IsNullOrEmpty(SavePackageTo)) {
                return null;
            }

            var path = Path.GetFullPath(SavePackageTo);

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

            Warning("SAVE_TO_PATH_NOT_VALID", SavePackageTo, packageName);
            return null;
        }

        private void ProcessPackage(PackageProvider provider, SoftwareIdentity package) {
            var savePath = SaveFileName(package.PackageFilename);

            // if we have a valid path, make a local copy of the file.
            if (!string.IsNullOrEmpty(savePath)) {
                provider.DownloadPackage(package, SaveFileName(savePath), this);
                if (File.Exists(savePath)) {
                    package.FullPath = savePath;
                }
            }

            // return the object to the caller.
            WriteObject(package);

            if (ListDependencies) {
                foreach (var dep in provider.GetPackageDependencies(package, this)) {
                    ProcessPackage(provider,dep);
                }
            }
        }

        public override bool EndProcessingAsync() {
            var noMatchNames = new HashSet<string>(Name ?? new string[] {
            });

            Parallel.ForEach(SelectedProviders, provider => {
                try {
                    if (!Name.IsNullOrEmpty()) {
                        foreach (var name in Name) {
                            if (FindViaUri(provider, name, (p) => ProcessPackage(provider,p))) {
                                noMatchNames.IfPresentRemoveLocked(name);
                                continue;
                            }

                            if (FindViaFile(provider, name, (p) => ProcessPackage(provider, p))) {
                                noMatchNames.IfPresentRemoveLocked(name);
                                continue;
                            }

                            if (FindViaName(provider, name, (p) => ProcessPackage(provider, p))) {
                                noMatchNames.IfPresentRemoveLocked(name);
                                continue;
                            }

                            // did not find anything on this provider that matches that name
                        }
                    } else {
                        // no package name passed in.
                        if (!FindViaName(provider, string.Empty, (p) => ProcessPackage(provider,p))) {
                            // nothing found?
                            Warning("No Packages Found (no package names/criteria listed)");
                        }
                    }
                } catch (Exception e) {
                    e.Dump();
                }
            });

            // whine about things not matched.
            foreach (var name in noMatchNames) {
                Warning("No Package Found", name );
            }

            return true;
        }
    }
}