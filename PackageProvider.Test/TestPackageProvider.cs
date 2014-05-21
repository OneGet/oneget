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

namespace Microsoft.OneGet.PackageProvider.Test {
    using System;
    using System.Collections.Generic;
    using Utility;
    using Callback = System.Func<string, System.Collections.Generic.IEnumerable<object>, object>;

    public class TestPackageProvider {
        #region implement PackageProvider-interface

        public void AddPackageSource(string name, string location, bool trusted, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'AddPackageSource'" );
            }

        }
        public bool FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'FindPackage'" );
            }

            return  default(bool);
        }
        public bool FindPackageByFile(string file, int id, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'FindPackageByFile'" );
            }

            return  default(bool);
        }
        public bool FindPackageByUri(Uri uri, int id, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'FindPackageByUri'" );
            }

            return  default(bool);
        }
        public bool GetInstalledPackages(string name, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'GetInstalledPackages'" );
            }

            return  default(bool);
        }
        public void GetDynamicOptions(int category, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'GetDynamicOptions'" );
            }

        }

        /// <summary>
            /// Returns the name of the Provider. Doesn't need callback .
            /// </summary>
            /// <returns></returns>
        public string GetPackageProviderName(){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.

            return  default(string);
        }
        public bool GetPackageSources(Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'GetPackageSources'" );
            }

            return  default(bool);
        }
        public void InitializeProvider(Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'InitializeProvider'" );
            }

        }
        public bool InstallPackage(string fastPath, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'InstallPackage'" );
            }

            return  default(bool);
        }

        // WhatIfInstallPackageBy* should be a good idea to fix -WhatIf
        public bool InstallPackageByFile(string filePath, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'InstallPackageByFile'" );
            }

            return  default(bool);
        }
        public bool InstallPackageByUri(string u, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'InstallPackageByUri'" );
            }

            return  default(bool);
        }
        public bool IsTrustedPackageSource(string packageSource, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'IsTrustedPackageSource'" );
            }

            return  default(bool);
        }
        public bool IsValidPackageSource(string packageSource, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'IsValidPackageSource'" );
            }

            return  default(bool);
        }
        public void RemovePackageSource(string name, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'RemovePackageSource'" );
            }

        }
        public bool UninstallPackage(string fastPath, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'UninstallPackage'" );
            }

            return  default(bool);
        }
        public void GetFeatures(Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'GetFeatures'" );
            }

        }

        // --- Optimization features -----------------------------------------------------------------------------------------------------
        public IEnumerable<string> GetMagicSignatures(){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.

            return  default(IEnumerable<string>);
        }
        public IEnumerable<string> GetSchemes(){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.

            return  default(IEnumerable<string>);
        }
        public IEnumerable<string> GetFileExtensions(){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.

            return  default(IEnumerable<string>);
        }
        public bool GetIsSourceRequired(){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.

            return  default(bool);
        }

        // --- operations on a package ---------------------------------------------------------------------------------------------------
        public bool DownloadPackage(string fastPath, string location, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'DownloadPackage'" );
            }

            return  default(bool);
        }
        public bool GetPackageDependencies(string fastPath, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'GetPackageDependencies'" );
            }

            return  default(bool);
        }
        public bool GetPackageDetails(string fastPath, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'GetPackageDetails'" );
            }

            return  default(bool);
        }
        public int StartFind(Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'StartFind'" );
            }

            return  default(int);
        }
        public bool CompleteFind(int id, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'CompleteFind'" );
            }

            return  default(bool);
        }

        #endregion

    }

    #region copy PackageProvider-types
public enum OptionCategory {
        Package = 0,
        Provider = 1,
        Source = 2,
        Install = 3
    }

    public enum OptionType {
        String = 0,
        StringArray = 1,
        Int = 2,
        Switch = 3,
        Path = 4,
        Uri = 5
    }

    #endregion

}