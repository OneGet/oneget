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
    using System.Threading;

    public class BlockingCollection<T> : System.Collections.Concurrent.BlockingCollection<T>, IEnumerable<T>  {
        private MutableEnumerable<T> _blockingEnumerable;

        public IEnumerator<T> GetEnumerator() {
            // make sure that iterating on this as enumerable is blocking.
            return this.GetBlockingEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        protected override void Dispose(bool isDisposing) {
            if (isDisposing) {
                _blockingEnumerable = null;
            }
        }

        public new IEnumerable<T> GetConsumingEnumerable() {
            return GetConsumingEnumerable(CancellationToken.None);
        }

        public new IEnumerable<T> GetConsumingEnumerable(CancellationToken cancellationToken) {
            T item;
            while (!IsCompleted && SafeTryTake(out item, 0, cancellationToken)) {
                yield return item;
            }
        }

        private bool SafeTryTake(out T item, int time, CancellationToken cancellationToken) {
            try {
                if (!cancellationToken.IsCancellationRequested && Count > 0 ) {
                    return TryTake(out item, time, cancellationToken);
                }
            } catch {
                // if this throws, that just means that we're done. (ie, canceled)
            }
            item = default(T);
            return false;
        }

        public IEnumerable<T> GetBlockingEnumerable() {
            return GetBlockingEnumerable(CancellationToken.None);
        }
        public  IEnumerable<T> GetBlockingEnumerable( CancellationToken cancellationToken) {
            return _blockingEnumerable ?? (_blockingEnumerable = SafeGetBlockingEnumerable(cancellationToken).ReEnumerable());
        }

        private IEnumerable<T> SafeGetBlockingEnumerable(CancellationToken cancellationToken) {
            while (!IsCompleted && !cancellationToken.IsCancellationRequested) {
                T item;
                if (SafeTryTake(out item, -1, cancellationToken)) {
                    yield return item;
                }
            }
        }

        public void Complete() {
            CompleteAdding();
        }

        public bool HasData {
            get {
                return Count > 0;
            }
        }
    }
}