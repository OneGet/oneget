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

namespace Microsoft.OneGet.Implementation {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using Api;
    using Utility.Async;
    using Utility.Collections;

    public class EnumerableRequestObject<T> : RequestObject, IAsyncEnumerable<T> {
        protected readonly BlockingCollection<T> Results = new BlockingCollection<T>();

        internal EnumerableRequestObject(ProviderBase provider, IHostApi request, Action<RequestObject> action)
            : base(provider, request, action) {
            OnCancel += Complete;
            OnAbort += Complete;
        }

        public IEnumerator<T> GetEnumerator() {
            return new CancellableEnumerator<T>(_cancellationTokenSource, Results.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        protected override void Complete() {
            Results.Complete();
            base.Complete();
        }

        public IEnumerable<T> GetConsumingEnumerable() {
            return Results.GetConsumingEnumerable(_cancellationTokenSource.Token);
        }

        public IEnumerable<T> GetBlockingEnumerable() {
            return Results.GetBlockingEnumerable(_cancellationTokenSource.Token);
        }

        public bool IsConsumed {
            get {
                return IsCanceled || IsAborted || Results.IsCompleted;
            }
        }
        public bool HasData {
            get {
                return !IsCanceled || !IsAborted || Results.HasData;
            }
        }
    }
}