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

namespace Microsoft.OneGet.Core.Collections {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Extensions;

    public class ByRefCollection<T> : MarshalByRefObject, ICollection<T> {
        // we don't want these objects being gc's out because they remain unused...
        public override object InitializeLifetimeService() {
            return null;
        }


        private readonly ICollection<T> _collection;

        public ByRefCollection(ICollection<T> collection) {
            _collection = collection;
        }

        public IEnumerator<T> GetEnumerator() {
            return _collection.GetEnumerator().ByRef();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(T item) {
            _collection.Add(item);
        }

        public void Clear() {
            _collection.Clear();
        }

        public bool Contains(T item) {
            return _collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            _collection.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) {
            return _collection.Remove(item);
        }

        public int Count {
            get {
                return _collection.Count;
            }
        }

        public bool IsReadOnly {
            get {
                return _collection.IsReadOnly;
            }
        }
    }
}
