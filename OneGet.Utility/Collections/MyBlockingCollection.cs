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
    using System.Threading;

    public class MyBlockingCollection<T> : MutableEnumerable<T>, IList<T>, IDisposable {
        private ManualResetEvent _added = new ManualResetEvent(false);
        private ManualResetEvent _completed = new ManualResetEvent(false);

        public bool IsCompleted {
            get {
                if (_completed == null) {
                    return true;
                }
                return _completed.WaitOne(0);
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Add(T item) {
            if (_completed.WaitOne(0)) {
                // throw new Exception("Attempt to modify completed collection");
                return;
            }

            lock (this) {
                List.Add(item);
                _added.Set();
            }
        }

        public void Clear() {
            throw new NotImplementedException();
        }

        public bool Contains(T item) {
            _completed.WaitOne();
            return List.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            _completed.WaitOne();
            List.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) {
            throw new NotImplementedException();
        }

        public int Count {
            get {
                _completed.WaitOne();
                return List.Count;
            }
        }

        public bool IsReadOnly {
            get {
                return false;
            }
        }

        public int IndexOf(T item) {
            _completed.WaitOne();
            return List.IndexOf(item);
        }

        public void Insert(int index, T item) {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index) {
            throw new NotImplementedException();
        }

        public T this[int index] {
            get {
                if (ItemExists(index)) {
                    return List[index];
                }
                throw new IndexOutOfRangeException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public void Complete() {
            if (_completed != null) {
                _completed.Set();
            }
        }

        public IEnumerable<T> GetConsumingEnumerable() {
            return this;
        }

        protected override bool ItemExists(int index) {
            while (true) {
                lock (this) {
                    if (List.Count > index) {
                        return true;
                    }
                    _added.Reset();
                }

                if (WaitHandle.WaitAny(new WaitHandle[] {_completed, _added}) == 0) {
                    return List.Count > index;
                }
            }
        }

        public void Dispose(bool disposing) {
            if (disposing) {
                if (_completed != null) {
                    _completed.Set();
                    _completed.Dispose();
                    _completed = null;
                }
                if (_added != null) {
                    _added.Set();
                    _added.Dispose();
                    _added = null;
                }
            }
        }

        public void Wait(int milliseconds = -1) {
            _completed.WaitOne(milliseconds);
        }
    }
}