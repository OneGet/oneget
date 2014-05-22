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

namespace Microsoft.OneGet.Core.Providers.Package {
    using System;
    using System.Collections.Generic;
    using Dynamic;
    using Callback = System.Func<string, System.Collections.Generic.IEnumerable<object>, object>;
    public interface IPackageProvider {
        bool IsImplemented(string methodName);

        #region declare PackageProvider-interface

        /// <summary>
        ///     Returns the name of the Provider. Doesn't need a callback .
        /// </summary>
        /// <returns>the name of the package provider</returns>
        [Required]
        string GetPackageProviderName();

        void InitializeProvider(Callback c);

        void GetFeatures(Callback c);

        void GetDynamicOptions(int category, Callback c);

        // --- Optimization features -----------------------------------------------------------------------------------------------------
        IEnumerable<string> GetMagicSignatures();

        IEnumerable<string> GetSchemes();

        IEnumerable<string> GetFileExtensions();

        bool GetIsSourceRequired(); // or should we imply this from the GetPackageSources == null/empty?

        // --- Manages package sources ---------------------------------------------------------------------------------------------------
        void AddPackageSource(string name, string location, bool trusted, Callback c);

        bool GetPackageSources(Callback c);

        void RemovePackageSource(string name, Callback c);

        int StartFind(Callback c);

        bool CompleteFind(int id, Callback c);

        // --- Finds packages ---------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Notes:
        ///     - If a call to GetPackageSources on this object returns no sources, the cmdlet won't call FindPackage on this
        ///     source
        ///     - (ie, the expectation is that you have to provide a source in order to use find package)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="requiredVersion"></param>
        /// <param name="minimumVersion"></param>
        /// <param name="maximumVersion"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        bool FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Callback c);

        bool FindPackageByFile(string file, int id, Callback c);
        bool FindPackageByUri(Uri uri, int id, Callback c);

        bool GetInstalledPackages(string name, Callback c);

        // --- operations on a package ---------------------------------------------------------------------------------------------------
        bool DownloadPackage(string fastPath, string location, Callback c);
        bool GetPackageDependencies(string fastPath, Callback c);
        bool GetPackageDetails(string fastPath, Callback c);

        bool InstallPackage(string fastPath, Callback c);
        // auto-install-dependencies
        // skip-dependency-check
        // continue-on-failure
        // location system/user/folder
        // callback for each package installed when installing dependencies?

        bool UninstallPackage(string fastPath, Callback c);

        #endregion
    }
}