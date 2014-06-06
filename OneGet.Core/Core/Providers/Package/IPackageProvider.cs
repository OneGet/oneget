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
    // using Callback = System.Object;
    using Callback = System.Object;

    public interface IProvider {
        #region declare Provider-interface
        /// <summary>
        ///     Allows the Provider to do one-time initialization.
        ///     This is called after the Provider is instantiated .
        /// 
        /// 
        /// </summary>
        /// <param name="dynamicInterface">A reference to the DynamicInterface class -- used to implement late-binding</param>
        /// <param name="c">Callback Delegate Reference</param>
        [Required]
        void InitializeProvider(object dynamicInterface, Callback c);

        /// <summary>
        /// Gets the features advertized from the provider
        /// </summary>
        /// <param name="c"></param>
        void GetFeatures(Callback c);

        /// <summary>
        /// Gets dynamically defined options from the provider
        /// </summary>
        /// <param name="category"></param>
        /// <param name="c"></param>
        void GetDynamicOptions(int category, Callback c);

        /// <summary>
        /// Allows runtime examination of the implementing class to check if a given method is implemented.
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
        void AddPackageSource(string name, string location, bool trusted, Callback c);

        void GetPackageSources(Callback c);

        void RemovePackageSource(string name, Callback c);

        int StartFind(Callback c);

        void CompleteFind(int id, Callback c);

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
        void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Callback c);

        void FindPackageByFile(string file, int id, Callback c);
        void FindPackageByUri(Uri uri, int id, Callback c);

        void GetInstalledPackages(string name, Callback c);

        // --- operations on a package ---------------------------------------------------------------------------------------------------
        void DownloadPackage(string fastPath, string location, Callback c);
        void GetPackageDependencies(string fastPath, Callback c);
        void GetPackageDetails(string fastPath, Callback c);

        void InstallPackage(string fastPath, Callback c);
        // auto-install-dependencies
        // skip-dependency-check
        // continue-on-failure
        // location system/user/folder
        // callback for each package installed when installing dependencies?

        void UninstallPackage(string fastPath, Callback c);

        #endregion
    }
}