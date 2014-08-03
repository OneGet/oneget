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
    using Utility.Plugin;

    using RequestImpl = System.Object;

    public interface IProvider {
        #region declare Provider-interface
        /* Synced/Generated code =================================================== */

        /// <summary>
        ///     Allows the Provider to do one-time initialization.
        ///     This is called after the Provider is instantiated .
        /// </summary>
        /// <param name="dynamicInterface">A reference to the DynamicInterface class -- used to implement late-binding</param>
        /// <param name="requestImpl">Object implementing some or all IRequest methods</param>
        [Required]
        void InitializeProvider(object dynamicInterface, RequestImpl requestImpl);

        /// <summary>
        ///     Gets the features advertized from the provider
        /// </summary>
        /// <param name="requestImpl"></param>
        void GetFeatures(RequestImpl requestImpl);

        /// <summary>
        ///     Gets dynamically defined options from the provider
        /// </summary>
        /// <param name="category"></param>
        /// <param name="requestImpl"></param>
        void GetDynamicOptions(int category, RequestImpl requestImpl);

        /// <summary>
        ///     Allows runtime examination of the implementing class to check if a given method is implemented.
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        bool IsMethodImplemented(string methodName);


        /// <summary>
        /// Returns the version of the provider.
        /// 
        /// This is expected to be in multipart numeric format. 
        /// </summary>
        /// <returns>The version of the provider</returns>
        string GetProviderVersion();

        #endregion
    }

    public interface IPackageProvider : IProvider {
        #region declare PackageProvider-interface
        /* Synced/Generated code =================================================== */

        /// <summary>
        ///     Returns the name of the Provider.
        /// </summary>
        /// <returns>the name of the package provider</returns>
        [Required]
        string GetPackageProviderName();

        // --- Manages package sources ---------------------------------------------------------------------------------------------------
        void AddPackageSource(string name, string location, bool trusted, RequestImpl requestImpl);

        void ResolvePackageSources(RequestImpl requestImpl);

        void RemovePackageSource(string name, RequestImpl requestImpl);

        int StartFind(RequestImpl requestImpl);

        void CompleteFind(int id, RequestImpl requestImpl);

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
        /// <param name="requestImpl"></param>
        /// <returns></returns>
        void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, RequestImpl requestImpl);

        void FindPackageByFile(string file, int id, RequestImpl requestImpl);
        void FindPackageByUri(Uri uri, int id, RequestImpl requestImpl);

        void GetInstalledPackages(string name, RequestImpl requestImpl);

        // --- operations on a package ---------------------------------------------------------------------------------------------------
        void DownloadPackage(string fastPath, string location, RequestImpl requestImpl);
        void GetPackageDependencies(string fastPath, RequestImpl requestImpl);
        void GetPackageDetails(string fastPath, RequestImpl requestImpl);

        void InstallPackage(string fastPath, RequestImpl requestImpl);
        // auto-install-dependencies
        // skip-dependency-check
        // continue-on-failure
        // location system/user/folder
        // fn call-back for each package installed when installing dependencies?

        void UninstallPackage(string fastPath, RequestImpl requestImpl);

        #endregion
    }
}