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

namespace Microsoft.OneGet.Builtin {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Implementation;
    using Utility.Plugin;
    using IRequestObject = System.Object;

    public class CabArchiver {
        private string ProviderName = "cabfile";

        private static readonly Dictionary<string, string[]> _features = new Dictionary<string, string[]> {
            {Constants.Features.SupportedExtensions, new[] {"cab", "msu"}},
            {Constants.Features.MagicSignatures, new[] {"4D534346"}}
        };

         /// <summary>
        ///     Returns a collection of strings to the client advertizing features this provider supports.
        /// </summary>
        /// <param name="requestObject">
        ///     An object passed in from the CORE that contains functions that can be used to interact with
        ///     the CORE and HOST
        /// </param>
        public void GetFeatures(IRequestObject requestObject) {
            try {
                
                // create a strongly-typed request object.
                using (var request = requestObject.As<Request>()) {
                    // Nice-to-have put a debug message in that tells what's going on.
                    request.Debug("Calling '{0}::GetFeatures' ", ProviderName);
                    foreach (var feature in _features) {
                        request.Yield(feature);
                    }
                }
            } catch (Exception e) {
                // We shoudn't throw exceptions from here, it's not-optimal. And if the exception class wasn't properly Serializable, it'd cause other issues.
                // Really this is just here as a precautionary to behave correctly.
                // At the very least, we'll write it to the system debug channel, so a developer can find it if they are looking for it.
                Debug.WriteLine("Unexpected Exception thrown in '{0}::GetFeatures' -- {1}\\{2}\r\n{3}", ProviderName, e.GetType().Name, e.Message, e.StackTrace);
            }
        }
        
        /// <summary>
        ///     Returns the name of the Provider.
        /// </summary>
        /// <returns></returns>
        public string GetArchiverName() {
            return ProviderName;
        }

        public IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, IRequestObject requestObject) {
            return null;
        }

        public bool IsSupportedFile(string localFilename) {
            return false;
        }
    }
}