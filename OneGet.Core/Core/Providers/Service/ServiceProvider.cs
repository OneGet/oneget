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

namespace Microsoft.OneGet.Core.Providers.Service {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using DuckTyping;
    using Extensions;
    using Callback = System.Func<string, System.Collections.Generic.IEnumerable<object>, object>;

    public interface IServicesProvider {
        bool IsImplemented(string method);
        #region declare ServicesProvider-interface
        /// <summary>
        /// Returns the name of the Provider. Doesn't need callback .
        /// </summary>
        /// <returns></returns>
        [Required]
        string GetServicesProviderName();

        void InitializeProvider(Callback c);

        IEnumerable<string> SupportedDownloadSchemes(Callback c);
        void DownloadFile(Uri remoteLocation, string localFilename, Callback c);

        IEnumerable<string> SupportedArchiveExtensions(Callback c);
        bool IsSupportedArchive(string localFilename, Callback c);

        void UnpackArchive(string localFilename, string destinationFolder, Callback c);

        #endregion
    }

    


    public class ServicesProvider : MarshalByRefObject {
        private readonly IServicesProvider _provider;

        internal ServicesProvider(IServicesProvider provider) {
            _provider = provider;
        }

        public bool IsImplemented(string method) {
            return _provider.IsImplemented(method);
        }

        public string Name { get {
            return _provider.GetServicesProviderName();
        } }
    }
}