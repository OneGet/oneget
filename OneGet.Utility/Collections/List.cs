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

    public class List<TKey, TValue> : List<KeyValuePair<TKey, TValue>> {
        public void Add(TKey key, TValue value) {
            Add(new KeyValuePair<TKey, TValue>(key, value));
        }
    }

    public class List<T1, T2, T3> : List<Tuple<T1, T2, T3>> {
        public void Add(T1 p1, T2 p2, T3 p3) {
            Add(new Tuple<T1, T2, T3>(p1, p2, p3));
        }
    }

    public class List<T1, T2, T3, T4> : List<Tuple<T1, T2, T3, T4>> {
        public void Add(T1 p1, T2 p2, T3 p3, T4 p4) {
            Add(new Tuple<T1, T2, T3, T4>(p1, p2, p3, p4));
        }
    }

    public class List<T1, T2, T3, T4, T5> : List<Tuple<T1, T2, T3, T4, T5>> {
        public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) {
            Add(new Tuple<T1, T2, T3, T4, T5>(p1, p2, p3, p4, p5));
        }
    }
}