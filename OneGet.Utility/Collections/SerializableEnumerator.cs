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

namespace Microsoft.OneGet.Collections {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Extensions;

    [Serializable]
    public class SerializableEnumerator<T> : IEnumerator<T> {
        private IEnumerator _enumerator;

        public SerializableEnumerator(IEnumerator<T> original) {
            // get a byref enumerator 
            _enumerator = original as ByRefEnumerator<T> ?? original.ByRef();
        }

        public void Dispose() {
            if (_enumerator is IDisposable) {
                (_enumerator as IDisposable).Dispose();
            }
            _enumerator = null;
        }

        public bool MoveNext() {
            return _enumerator.MoveNext();
        }

        public void Reset() {
            _enumerator.Reset();
        }

        public T Current {
            get {
                return (T)_enumerator.Current;
            }
        }

        object IEnumerator.Current {
            get {
                return _enumerator.Current;
            }
        }
    }
}