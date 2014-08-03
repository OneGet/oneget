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

namespace Microsoft.OneGet.Api {
    using System;
    using RequestImpl = System.Object;

    public interface ICoreApi {
        #region declare core-apis
        /* Synced/Generated code =================================================== */
        /// <summary>
        ///     The provider can query to see if the operation has been cancelled.
        ///     This provides for a gentle way for the caller to notify the callee that
        ///     they don't want any more results.
        /// </summary>
        /// <returns>returns TRUE if the operation has been cancelled.</returns>
        bool IsCancelled();

        /// <summary>
        ///     Returns a reference to the PackageManagementService API
        ///     The consumer of this function should either use this as a dynamic object
        ///     Or DuckType it to an interface that resembles IPacakgeManagementService
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        object GetPackageManagementService(RequestImpl requestImpl);

        /// <summary>
        ///     Returns the interface type for a Request that the OneGet Core is expecting
        ///     This is (currently) neccessary to provide an appropriately-typed version
        ///     of the Request to the core when a Plugin is calling back into the core
        ///     and has to pass a request object.
        /// </summary>
        /// <returns></returns>
        Type GetIRequestInterface();

        bool NotifyBeforePackageInstall(string packageName, string version, string source, string destination);
        bool NotifyPackageInstalled(string packageName, string version, string source, string destination);
        bool NotifyBeforePackageUninstall(string packageName, string version, string source, string destination);
        bool NotifyPackageUninstalled(string packageName, string version, string source, string destination);

        string GetCanonicalPackageId(string providerName, string packageName, string version);
        string ParseProviderName(string canonicalPackageId);
        string ParsePackageName(string canonicalPackageId);
        string ParsePackageVersion(string canonicalPackageId);

        #endregion
    }
}