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
    using System.Collections.Concurrent;
    using System.Threading;

    public class CancellableBlockingCollection<T> : MarshalByRefObject, IDisposable {
        private readonly BlockingCollection<T> _collection = new BlockingCollection<T>();
        protected CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public CancellableBlockingCollection() {
        }

        // we don't want these objects being gc's out because they remain unused...

        public bool IsCompleted {
            get {
                return _collection.IsCompleted;
            }
        }

        public int Count {
            get {
                return _collection.Count;
            }
        }

        public bool IsCancelled {
            get {
                return _cancellationTokenSource == null || _cancellationTokenSource.Token.IsCancellationRequested;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            Cancel();
            if (disposing) {
                if (_cancellationTokenSource != null) {
                    _cancellationTokenSource.Dispose();
                }
                _cancellationTokenSource = null;
            }
        }

        public override object InitializeLifetimeService() {
            return null;
        }

        public void Add(T item) {
            _collection.Add(item);
        }

        public void CompleteAdding() {
            _collection.CompleteAdding();
        }

        public void Cancel() {
            if (_cancellationTokenSource != null) {
                _cancellationTokenSource.Cancel();
            }
        }

        public static CancellableEnumerable<T> ToCancellableEnumerable(CancellableBlockingCollection<T> collection) {
            return new CancellableEnumerable<T>(collection._cancellationTokenSource, collection._collection.GetConsumingEnumerable());
        }


        public static implicit operator CancellableEnumerable<T>(CancellableBlockingCollection<T> collection) {
            return new CancellableEnumerable<T>(collection._cancellationTokenSource, collection._collection.GetConsumingEnumerable());
        }
    }
}