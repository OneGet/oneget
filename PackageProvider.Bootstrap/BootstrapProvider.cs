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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Callback = System.Object;

    public class BootstrapProvider {
        private static readonly string[] _empty = new string[0];

        private static readonly string[] _urls = new[] {
            "http://downloads.coapp.org/oneget/WellKnownProviders.swidtag",
            "https://go.microsoft.com/fwlink/?LinkId=404337",
            "https://go.microsoft.com/fwlink/?LinkId=404338",
        };

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
            return "Bootstrap";
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
                            break;

                        case OptionCategory.Source:
                            break;

                        case OptionCategory.Install:
                            break;
                    }
                } catch {
                    // this makes it ignore new OptionCategories that it doesn't know about.
                }
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

                   foreach (var location in _urls) {
                    // download the discovery swidtag
                       try {
                           var tmpSwid = Path.Combine(Path.GetTempPath(), "WellKnownProviders.swidtag");
                           if (File.Exists(tmpSwid)) {
                               File.Delete(tmpSwid);
                           }

                           request.DownloadFile(new Uri(location), tmpSwid, request);

                           if (File.Exists(tmpSwid)) {
                               File.Delete(tmpSwid);
                           }

                       } catch {
                           
                       }

                }

                // return any matches in the name
            }
        }


        /* NOT SUPPORTED -- AT THIS TIME 
         * 
        public void FindPackageByFile(string filePath, int id, Callback c) {
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::FindPackageByFile'");
            }
        }

        public void FindPackageByUri(Uri uri, int id, Callback c) {
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::FindPackageByUri'");

                // check if this URI is a valid source
                // if it is, get the list of packages from this source

                // otherwise, download the Uri and see if it's a package 
                // that we support.
            }
        }
         * 
         * 
        public void GetPackageDetails(string fastPath, Callback c) {
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::GetPackageDetails'");
            }
        }
         * 
         
        public void GetPackageDependencies(string fastPath, Callback c) {
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::GetPackageDependencies'");

            }
        }

         *  public void UninstallPackage(string fastPath, Callback c) {
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::UninstallPackage'");
            }
        }
         * 
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


        public void InstallPackage(string fastPath, Callback c) {
            // ensure that mandatory parameters are present.
            using (var request = c.As<Request>()) {
                request.Debug("Calling 'Bootstrap::InstallPackage'");

            }
        }

        // callback for each package installed when installing dependencies?

       
    }
}
