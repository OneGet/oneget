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

namespace OneGet.PackageProvider.Bootstrap {
    using System.Collections.Generic;
    using Callback = System.Object;

    public class BootstrapProvider {
        private static readonly string[] _empty = new string[0];

        private static readonly Dictionary<string,string[]> _features = new Dictionary<string, string[]> {
            { "schemes", new [] {"http", "https", "file"} },
            { "extensions", new [] {"exe", "msi" } },
            { "magic-signatures", _empty },
        };

        internal static IEnumerable<string> SupportedSchemes {
            get {
                return _features["schemes"];
            }
        }
        /// <summary>
        ///     Returns the name of the Provider. Doesn't need a callback .
        /// </summary>
        /// <required />
        /// <returns>the name of the package provider</returns>
        public string GetPackageProviderName() {
            return "NuGet";
        }

        public void InitializeProvider(object dynamicInterface, Callback c) {
            RequestExtensions.RemoteDynamicInterface = dynamicInterface;
        }

        public void GetFeatures(Callback c) {
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::GetFeatures'");
                foreach (var feature in _features) {
                    request.Yield(feature);
                }
            }
        }

        public void GetDynamicOptions(int category, Callback c) {
            using (var request = c.As<Request>()) {
                try {
                    var cat = (OptionCategory)category;
                    request.Debug("Calling 'Bootstrap::GetDynamicOptions ({0})'", cat);

                    switch (cat) {
                        case OptionCategory.Package:
                            request.YieldDynamicOption(cat, "Tag", OptionType.StringArray, false);
                            request.YieldDynamicOption(cat, "Contains", OptionType.String, false);
                            request.YieldDynamicOption(cat, "AllowPrereleaseVersions", OptionType.Switch, false);
                            request.YieldDynamicOption(cat, "AllVersions", OptionType.Switch, false);
                            break;

                        case OptionCategory.Source:
                            request.YieldDynamicOption(cat, "ConfigFile", OptionType.String, false);
                            request.YieldDynamicOption(cat, "SkipValidate", OptionType.Switch, false);
                            break;

                        case OptionCategory.Install:
                            request.YieldDynamicOption(cat, "Destination", OptionType.Path, true);
                            request.YieldDynamicOption(cat, "SkipDependencies", OptionType.Switch, false);
                            request.YieldDynamicOption(cat, "ContinueOnFailure", OptionType.Switch, false);
                            request.YieldDynamicOption(cat, "ExcludeVersion", OptionType.Switch, false);
                            request.YieldDynamicOption(cat, "PackageSaveMode", OptionType.String, false,new [] {"nuspec", "nupkg", "nuspec;nupkg"} );
                            break;
                    }
                } catch {
                    // this makes it ignore new OptionCategories that it doesn't know about.
                }
            }
        }


        // --- Manages package sources ---------------------------------------------------------------------------------------------------
        public void AddPackageSource(string name, string location, bool trusted, Callback c) {
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::AddPackageSource'");
            }
        }

        public void ResolvePackageSources(Callback c) {
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::ResolvePackageSources'");
            }
        }

        public void RemovePackageSource(string name, Callback c) {
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::RemovePackageSource'");
            }
        }

        // --- Finds packages ---------------------------------------------------------------------------------------------------
        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="requiredVersion"></param>
        /// <param name="minimumVersion"></param>
        /// <param name="maximumVersion"></param>
        /// <param name="id"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Callback c) {
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::FindPackage'");
            }
        }

        public void FindPackageByFile(string filePath, int id, Callback c) {
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::FindPackageByFile'");
            }
        }

        /* NOT SUPPORTED -- AT THIS TIME 
        public void FindPackageByUri(Uri uri, int id, Callback c) {
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::FindPackageByUri'");

                // check if this URI is a valid source
                // if it is, get the list of packages from this source

                // otherwise, download the Uri and see if it's a package 
                // that we support.
            }
        }
         */

        public void GetInstalledPackages(string name, Callback c) {
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::GetInstalledPackages'");

            }
        }

        // --- operations on a package ---------------------------------------------------------------------------------------------------
        public void DownloadPackage(string fastPath, string location, Callback c) {
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::DownloadPackage'");

            }
        }

        public void GetPackageDependencies(string fastPath, Callback c) {
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::GetPackageDependencies'");

            }
        }

        public void GetPackageDetails(string fastPath, Callback c) {
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::GetPackageDetails'");
            }
        }

        public void InstallPackage(string fastPath, Callback c) {
            // ensure that mandatory parameters are present.
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::InstallPackage'");

            }
        }

        // callback for each package installed when installing dependencies?

        public void UninstallPackage(string fastPath, Callback c) {
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::UninstallPackage'");
            }
        }
    }
}
