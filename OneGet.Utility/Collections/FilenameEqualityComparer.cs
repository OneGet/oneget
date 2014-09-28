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

namespace Microsoft.OneGet.Utility.Collections {
    using System;
    using System.Collections.Generic;
    using System.IO;

    public enum PathCompareOption {
        Full,
        File,
        FileWithoutExtension,
        Extension
    }

    public class PathEqualityComparer : IEqualityComparer<string> {
        private readonly PathCompareOption _option;

        public PathEqualityComparer(PathCompareOption option) {
            _option = option;
        }

        public bool Equals(string x, string y) {
            return string.Compare(ComparePath(x), ComparePath(y), StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public int GetHashCode(string obj) {
            return ComparePath(obj).ToUpperInvariant().GetHashCode();
        }

        private string ComparePath(string path) {
            if (string.IsNullOrWhiteSpace(path)) {
                return string.Empty;
            }

            switch (_option) {
                case PathCompareOption.Full:
                    return Path.GetFullPath(path);
                case PathCompareOption.File:
                    return Path.GetFileName(path);
                case PathCompareOption.FileWithoutExtension:
                    return Path.GetFileNameWithoutExtension(path);
                case PathCompareOption.Extension:
                    return Path.GetExtension(path);
            }
            return string.Empty;
        }
    }
}