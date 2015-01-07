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
    using System.Collections.Generic;
    using Implementation;
    using IRequestObject = System.Object;

    public interface ICoreApi {
        #region declare core-apis

        /* Synced/Generated code =================================================== */

        bool IsCanceled {get;}
        IEnumerable<string> ProviderNames {get;}

        IEnumerable<PackageProvider> PackageProviders {get;}

        /// <summary>
        ///     Returns the internal version of the OneGet core.
        ///     This will usually only be updated if there is a breaking API or Interface change that might
        ///     require other code to know which version is running.
        /// </summary>
        /// <returns>Internal Version of OneGet</returns>
        int CoreVersion();

        bool NotifyBeforePackageInstall(string packageName, string version, string source, string destination);
        bool NotifyPackageInstalled(string packageName, string version, string source, string destination);
        bool NotifyBeforePackageUninstall(string packageName, string version, string source, string destination);
        bool NotifyPackageUninstalled(string packageName, string version, string source, string destination);

        IEnumerable<PackageProvider> SelectProvidersWithFeature(string featureName);

        IEnumerable<PackageProvider> SelectProvidersWithFeature(string featureName, string value);

        IEnumerable<PackageProvider> SelectProviders(string providerName, IRequestObject requestObject);

        bool RequirePackageProvider(string requestor, string packageProviderName, string minimumVersion, IRequestObject requestObject);

        string GetCanonicalPackageId(string providerName, string packageName, string version);
        string ParseProviderName(string canonicalPackageId);
        string ParsePackageName(string canonicalPackageId);
        string ParsePackageVersion(string canonicalPackageId);

        #endregion
    }
}