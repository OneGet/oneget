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
    using System.Threading;

    public class ByRefCancellationTokenSource : MarshalByRefObject, IDisposable {
        protected CancellationTokenSource _cancellationTokenSource;

        public ByRefCancellationTokenSource() {
        }

        public ByRefCancellationTokenSource(CancellationTokenSource cts) {
            _cancellationTokenSource = cts;
        }

        public ByRefCancellationToken Token {
            get {
                return new ByRefCancellationToken(_cancellationTokenSource.Token);
            }
        }

        public bool IsCancellationRequested {
            get {
                return _cancellationTokenSource.IsCancellationRequested;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override object InitializeLifetimeService() {
            return null;
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                var cts = _cancellationTokenSource;
                _cancellationTokenSource = null;
                if (cts != null) {
                    cts.Dispose();
                }
            }
        }

        public
            void Cancel() {
            if (_cancellationTokenSource != null) {
                _cancellationTokenSource.Cancel();
            }
        }
    }
}