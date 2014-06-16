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

namespace Microsoft.OneGet.Providers.Package {
    using System;
    using Plugin;

    // using Callback = System.Object;

    public interface IProvider {
        #region declare Provider-interface

        /// <summary>
        ///     Allows the Provider to do one-time initialization.
        ///     This is called after the Provider is instantiated .
        /// </summary>
        /// <param name="dynamicInterface">A reference to the DynamicInterface class -- used to implement late-binding</param>
        /// <param name="c">Callback Delegate Reference</param>
        [Required]
        void InitializeProvider(object dynamicInterface, Object c);

        /// <summary>
        ///     Gets the features advertized from the provider
        /// </summary>
        /// <param name="c"></param>
        void GetFeatures(Object c);

        /// <summary>
        ///     Gets dynamically defined options from the provider
        /// </summary>
        /// <param name="category"></param>
        /// <param name="c"></param>
        void GetDynamicOptions(int category, Object c);

        /// <summary>
        ///     Allows runtime examination of the implementing class to check if a given method is implemented.
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        bool IsMethodImplemented(string methodName);

        #endregion
    }

    public interface IPackageProvider : IProvider {
        #region declare PackageProvider-interface

        /// <summary>
        ///     Returns the name of the Provider. Doesn't need a callback .
        /// </summary>
        /// <returns>the name of the package provider</returns>
        [Required]
        string GetPackageProviderName();

        // --- Manages package sources ---------------------------------------------------------------------------------------------------
        void AddPackageSource(string name, string location, bool trusted, Object c);

        void GetPackageSources(Object c);

        void RemovePackageSource(string name, Object c);

        int StartFind(Object c);

        void CompleteFind(int id, Object c);

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
        void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Object c);

        void FindPackageByFile(string file, int id, Object c);
        void FindPackageByUri(Uri uri, int id, Object c);

        void GetInstalledPackages(string name, Object c);

        // --- operations on a package ---------------------------------------------------------------------------------------------------
        void DownloadPackage(string fastPath, string location, Object c);
        void GetPackageDependencies(string fastPath, Object c);
        void GetPackageDetails(string fastPath, Object c);

        void InstallPackage(string fastPath, Object c);
        // auto-install-dependencies
        // skip-dependency-check
        // continue-on-failure
        // location system/user/folder
        // callback for each package installed when installing dependencies?

        void UninstallPackage(string fastPath, Object c);

        #endregion
    }
}