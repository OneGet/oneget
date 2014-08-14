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

    [Serializable]
    public class SerializableEnumerable<T> : IEnumerable<T> {
        
        protected readonly IEnumerable _enumerable;

        public SerializableEnumerable(IEnumerable originalEnumerable) {
            _enumerable = originalEnumerable as ByRefEnumerable ?? new ByRefEnumerable(originalEnumerable);
        }

        public virtual IEnumerator<T> GetEnumerator() {
            return new SerializableEnumerator<T>(_enumerable.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }

    public class ByRefCollection : MarshalByRefObject, ICollection {
        private ICollection _collection;
        // we don't want these objects being gc's out because they remain unused...
        public override object InitializeLifetimeService() {
            return null;
        }

        public ByRefCollection(ICollection originalCollection) {
            _collection = originalCollection;
        }

        public IEnumerator GetEnumerator() {
            return new ByRefEnumerator(_collection.GetEnumerator());
        }

        public void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        public int Count {
            get {
                return _collection.Count;
            }
        }

        public object SyncRoot {
            get {
                return _collection.SyncRoot;
            }
        }

        public bool IsSynchronized {
            get {
                return false;
            }
        }
    }

    [Serializable]
    public class SerializableCollection<T> : ICollection<T> {

        protected readonly ICollection _collection;

        public SerializableCollection(ICollection originalCollection) {
            _collection = originalCollection as ByRefCollection ?? new ByRefCollection(originalCollection);
        }

        public IEnumerator<T> GetEnumerator() {
            return new SerializableEnumerator<T>(_collection.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(T item) {
            throw new NotImplementedException();
        }

        public void Clear() {
            throw new NotImplementedException();
        }

        public bool Contains(T item) {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public bool Remove(T item) {
            throw new NotImplementedException();
        }

        public int Count {
            get {
                return _collection.Count;
            }
        }

        public bool IsReadOnly {
            get {
                return true;
            }
        }
    }
}