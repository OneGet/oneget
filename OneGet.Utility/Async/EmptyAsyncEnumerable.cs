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

namespace Microsoft.OneGet.Utility.Async {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Extensions;

    public class EmptyAsyncEnumerable<T> : IAsyncEnumerable<T> {
        private static readonly ManualResetEventSlim _completed = new ManualResetEventSlim(true);

        public void Dispose() {
        }

        public void Cancel() {
            // no-effect
        }

        public void Abort() {
            // no-effect
        }

        public WaitHandle CompleteEvent {
            get {
                return _completed.WaitHandle;
            }
        }

        public TimeSpan Timeout {get; set;}
        public TimeSpan Responsiveness {get; set;}

        public bool IsCanceled {
            get {
                return false;
            }
        }

        public bool IsAborted {
            get {
                return false;
            }
        }

        public bool IsCompleted {
            get {
                return true;
            }
        }

        public event Action OnComplete;
        public event Action OnCancel;
        public event Action OnAbort;

        public IEnumerator<T> GetEnumerator() {
            return Enumerable.Empty<T>().GetEnumerator().ByRef();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}