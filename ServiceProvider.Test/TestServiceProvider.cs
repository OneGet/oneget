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

namespace Microsoft.OneGet.ServiceProvider.Test {
    using System;
    using System.Collections.Generic;
    using RequestImpl = System.Object;

    public class TestServicesProvider {
        #region implement ServicesProvider-interface
/// <summary>
        ///     Returns the name of the Provider. 
        /// </summary>
        /// <returns></returns>
        public string GetServicesProviderName() {
            // TODO: Fill in implementation
            // Delete this method if you do not need to implement it
            // Please don't throw an not implemented exception, it's not optimal.

            return default(string);
        }
        public void InitializeProvider(object dynamicInterface, RequestImpl requestImpl) {
            RequestExtensions.RemoteDynamicInterface = dynamicInterface;

            using (var request =requestImpl.As<Request>()) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information", "Calling 'InitializeProvider'");
            }
        }
        public IEnumerable<string> SupportedDownloadSchemes(RequestImpl requestImpl){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request =requestImpl.As<Request>()) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'SupportedDownloadSchemes'" );
            }

            return  default(IEnumerable<string>);
        }
        public void DownloadFile(Uri remoteLocation, string localFilename, RequestImpl requestImpl){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request =requestImpl.As<Request>()) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'DownloadFile'" );
            }

        }
        public IEnumerable<string> SupportedArchiveExtensions(RequestImpl requestImpl){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request =requestImpl.As<Request>()) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'SupportedArchiveExtensions'" );
            }

            return  default(IEnumerable<string>);
        }
        public bool IsSupportedArchive(string localFilename, RequestImpl requestImpl){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request =requestImpl.As<Request>()) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'IsSupportedArchive'" );
            }

            return  default(bool);
        }
        public void UnpackArchive(string localFilename, string destinationFolder, RequestImpl requestImpl){
             // TODO: Fill in implementation
             // Delete this method if you do not need to implement it
             // Please don't throw an not implemented exception, it's not optimal.
            using (var request =requestImpl.As<Request>()) {
                // use the request object to interact with the OneGet core:
                request.Debug("Information","Calling 'UnpackArchive'" );
            }

        }

        #endregion

    }

    #region copy PackageProvider-types
/* Synced/Generated code =================================================== */

    public enum OptionCategory {
        Package = 0,
        Provider = 1,
        Source = 2,
        Install = 3
    }

    public enum OptionType {
        String = 0,
        StringArray = 1,
        Int = 2,
        Switch = 3,
        Folder = 4,
        File = 5,
        Path = 6,
        Uri = 7,
        SecureString = 8
    }

    public enum EnvironmentContext {
        All = 0,
        User = 1,
        System = 2
    }

    #endregion

}