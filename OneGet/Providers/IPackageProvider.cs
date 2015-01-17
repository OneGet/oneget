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

namespace Microsoft.OneGet.Providers {
    using System;
    using Api;
    using Utility.Plugin;

    public interface IPackageProvider : IProvider {
        #region declare PackageProvider-interface

        /* Synced/Generated code =================================================== */

        /// <summary>
        ///     Returns the name of the Provider.
        /// </summary>
        /// <returns>the name of the package provider</returns>
        [Required]
        string GetPackageProviderName();

        /// <summary>
        ///     This is called when the user is adding (or updating) a package source
        ///     If this PROVIDER doesn't support user-defined package sources, remove this method.
        /// </summary>
        /// <param name="name">
        ///     The name of the package source. If this parameter is null or empty the PROVIDER should use the
        ///     location as the name (if the PROVIDER actually stores names of package sources)
        /// </param>
        /// <param name="location">
        ///     The location (ie, directory, URL, etc) of the package source. If this is null or empty, the
        ///     PROVIDER should use the name as the location (if valid)
        /// </param>
        /// <param name="trusted">
        ///     A boolean indicating that the user trusts this package source. Packages returned from this source
        ///     should be marked as 'trusted'
        /// </param>
        /// <param name="requestObject">
        ///     An object passed in from the CORE that contains functions that can be used to interact with
        ///     the CORE and HOST
        /// </param>
        void AddPackageSource(string name, string location, bool trusted, IRequest requestObject);

        void ResolvePackageSources(IRequest requestObject);

        void RemovePackageSource(string name, IRequest requestObject);

        int StartFind(IRequest requestObject);

        void CompleteFind(int id, IRequest requestObject);

        // --- Finds packages ---------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Notes:
        ///     - If a call to ResolvePackageSources on this object returns no sources, the cmdlet won't call FindPackage on this
        ///     source
        ///     - (ie, the expectation is that you have to provide a source in order to use find package)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="requiredVersion"></param>
        /// <param name="minimumVersion"></param>
        /// <param name="maximumVersion"></param>
        /// <param name="id"></param>
        /// <param name="requestObject"></param>
        /// <returns></returns>
        void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, IRequest requestObject);

        void FindPackageByFile(string file, int id, IRequest requestObject);
        void FindPackageByUri(Uri uri, int id, IRequest requestObject);

        void GetInstalledPackages(string name, IRequest requestObject);

        // --- operations on a package ---------------------------------------------------------------------------------------------------
        void DownloadPackage(string fastPath, string location, IRequest requestObject);
        void GetPackageDependencies(string fastPath, IRequest requestObject);
        void GetPackageDetails(string fastPath, IRequest requestObject);

        void InstallPackage(string fastPath, IRequest requestObject);
        // auto-install-dependencies
        // skip-dependency-check
        // continue-on-failure
        // location system/user/folder
        // fn call-back for each package installed when installing dependencies?

        void UninstallPackage(string fastPath, IRequest requestObject);

        void ExecuteElevatedAction(string payload, IRequest requestObject);

        #endregion
    }
}