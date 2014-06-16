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

namespace Microsoft.OneGet.Platform {
    using System;
    using System.Text;

    public static class KnownFolders {
        public static string GetFolderPath(KnownFolder folder) {
            var ret = new StringBuilder(260);
            try {
                var output = NativeMethods.SHGetSpecialFolderPath(IntPtr.Zero, ret, folder);
                if (!output) {
                    return null;
                }
            } catch /* (Exception e) */ {
                return null;
            }
            return ret.ToString();
        }
    }
}