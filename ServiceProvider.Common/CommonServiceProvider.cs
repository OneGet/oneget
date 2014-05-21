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

namespace Microsoft.OneGet.ServicesProvider.Common {
    using System;
    using System.Collections.Generic;
    using Utility;
    using Callback = System.Func<string, System.Collections.Generic.IEnumerable<object>, object>;

    public class CommonServicesProvider {
        #region implement ServicesProvider-interface
/// <summary>
        ///     Returns the name of the Provider. Doesn't need callback .
        /// </summary>
        /// <returns></returns>
        public string GetServicesProviderName() {
            return "Common";
        }
        public void InitializeProvider(Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'InitializeProvider'" );
            }

        }
        public IEnumerable<string> SupportedDownloadSchemes(Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'SupportedDownloadSchemes'" );
            }

            return  default(IEnumerable<string>);
        }
        public void DownloadFile(Uri remoteLocation, string localFilename, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'DownloadFile'" );
            }

        }
        public IEnumerable<string> SupportedArchiveExtensions(Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'SupportedArchiveExtensions'" );
            }

            return  default(IEnumerable<string>);
        }
        public bool IsSupportedArchive(string localFilename, Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'IsSupportedArchive'" );
            }

            return  default(bool);
        }
        public void UnpackArchive(string localFilename, string destinationFolder ,Callback c){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request = new Request(c)) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'UnpackArchive'" );
            }

        }

        #endregion

    }
}