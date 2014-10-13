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
    using System.Collections;
    using System.Collections.Generic;

    public class VirtualEnumerator<T> : IEnumerator<T> {
        private readonly IEnumerator _backingEnumerator;
        private readonly Func<IEnumerator, T> _currentFunction;

        public VirtualEnumerator(IEnumerator backingEnumerator, Func<IEnumerator, T> fn) {
            _currentFunction = fn;
            _backingEnumerator = backingEnumerator;
        }

        public T Current {
            get {
                return _currentFunction(_backingEnumerator);
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        object IEnumerator.Current {
            get {
                return _currentFunction(_backingEnumerator);
            }
        }

        public bool MoveNext() {
            return _backingEnumerator.MoveNext();
        }

        public void Reset() {
            _backingEnumerator.Reset();
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_backingEnumerator is IDisposable) {
                    (_backingEnumerator as IDisposable).Dispose();
                }
            }
        }
    }
}