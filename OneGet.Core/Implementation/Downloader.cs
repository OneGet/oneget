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

namespace Microsoft.OneGet.Implementation {
    using System;
    using System.Collections.Generic;
    using Providers;
    using Utility.Extensions;

    internal class Downloader : ProviderBase<IDownloader> {
        private string _name;

        internal Downloader(IDownloader provider)
            : base(provider) {
        }

        public override string ProviderName {
            get {
                return _name ?? (_name = Provider.GetDownloaderName());
            }
        }

        public IEnumerable<string> SupportedSchemes {
            get {
                return Provider.SupportedSchemes.ByRefEnumerable();    
            }
        }

        public void DownloadFile(Uri remoteLocation, string localFilename, Object requestImpl) {
            Provider.DownloadFile(remoteLocation, localFilename, ExtendRequest(requestImpl));
        }

    }
}