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

namespace Microsoft.PackageManagement.Providers {
    using System.Collections.Generic;
    using Utility.Plugin;

    public interface IMetaProvider : IProvider {
        #region declare MetaProvider-interface

        /* Synced/Generated code =================================================== */

        /// <summary>
        ///     Will instantiate an instance of a provider given it's name
        /// </summary>
        /// <param name="name">the name of the provider to create</param>
        /// <returns>an instance of the provider.</returns>
        [Required]
        object CreateProvider(string name);

        /// <summary>
        ///     Gets the name of this MetaProvider
        /// </summary>
        /// <returns>the name of the MetaProvider.</returns>
        [Required]
        string GetMetaProviderName();

        /// <summary>
        ///     Returns a collection of all the names of Providers this MetaProvider can create.
        /// </summary>
        /// <returns>a collection of all the names of Providers this MetaProvider can create</returns>
        [Required]
        IEnumerable<string> GetProviderNames();

        #endregion
    }
}
