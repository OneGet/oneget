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

namespace Microsoft.OneGet.Utility.Extensions {
    using System;

    internal class TwoTypes {
        private readonly Type _first;
        private readonly Type _second;

        public TwoTypes(Type first, Type second) {
            _first = first;
            _second = second;
        }

        public override int GetHashCode() {
            return 31*_first.GetHashCode() + _second.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }
            var other = obj as TwoTypes;
            return other != null && (_first == other._first && _second == other._second);
        }
    }
}