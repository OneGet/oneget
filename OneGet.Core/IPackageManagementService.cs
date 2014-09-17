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

namespace Microsoft.OneGet {
    using System.Collections.Generic;
    using Api;
    using Implementation;
    using RequestImpl = System.Object;

    /// <summary>
    ///     The current Package Management Service Interface
    ///     Binding directly to this is discouraged, as the interface is expected to be incrementally
    ///     expanded over time.
    ///     In order to access the interface, the Host (client) is encouraged to copy this interface
    ///     into their own project and use the <code>PackageManagementService.GetInstance<![CDATA[<>]]></code>
    ///     method to dynamically generate a matching implementation at load time.
    /// </summary>
    public interface IPackageManagementService {
        IEnumerable<string> ProviderNames {get;}

        IEnumerable<PackageProvider> PackageProviders { get; }

        IEnumerable<PackageProvider> SelectProviders(string providerName, RequestImpl requestImpl);

        IEnumerable<PackageProvider> SelectProvidersWithFeature(string featureName);

        IEnumerable<PackageProvider> SelectProvidersWithFeature(string featureName, string value);

        IProviderServices ProviderServices {get;}

        bool Initialize(RequestImpl requestImpl);

        bool RequirePackageProvider(string requestor, string packageProviderName, string minimumVersion, RequestImpl requestImpl);
    }
}