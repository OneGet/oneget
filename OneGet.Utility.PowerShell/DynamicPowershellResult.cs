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

namespace Microsoft.OneGet {
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Threading;

    public class DynamicPowershellResult : IDisposable, IEnumerable<object> {
        private ManualResetEvent _completedEvent = new ManualResetEvent(false);
        private ManualResetEvent _startedEvent = new ManualResetEvent(false);
        private BlockingCollection<object> _output = new BlockingCollection<object>();

        public BlockingCollection<ErrorRecord> Errors = new BlockingCollection<ErrorRecord>();
        
        internal bool LastIsTerminatingError {get; set;}
        public bool IsFailing {get; internal set;}
        public void WaitForCompletion() {
            _completedEvent.WaitOne();
        }

        public void WaitForStart() {
            _startedEvent.WaitOne();
        }

        public object Value {
            get {
                WaitForStart();

                var result = this.FirstOrDefault();

                if (LastIsTerminatingError) {
                    throw new Exception("Cmdlet reported error");
                }

                if (result == null) {
                    return true;
                }

                return result;
            }
        }

        public IEnumerable<object> Values {
            get {
                if (LastIsTerminatingError) {
                    throw new Exception("Cmdlet reported error");
                }
                return _output.GetConsumingEnumerable();
            }
        }

        public bool Success {
            get {
                _completedEvent.WaitOne();

                if (LastIsTerminatingError) {
                    return false;
                }
                return true;
            }
        }

        public void Started() {
            _startedEvent.Set();
        }

        public void Completed() {
            Errors.CompleteAdding();
            _output.CompleteAdding();

            _startedEvent.Set();
            _completedEvent.Set();
        }

        public bool IsCompleted {
            get {
                return _output.IsCompleted;
            }
        }

        public void Add(object obj) {
            _output.Add( obj);
        }

        public void Dispose() {
            if (_output != null) {
                _output.Dispose();
                _output = null;
            }

            if (Errors != null) {
                Errors.Dispose();
                Errors = null;
            }

            if (_startedEvent != null) {
                _startedEvent.Set();
                _startedEvent.Dispose();
                _startedEvent = null;
            }

            if (_completedEvent != null) {
                _completedEvent.Set();
                _completedEvent.Dispose();
                _completedEvent = null;
            }
        }

        public IEnumerator<object> GetEnumerator() {
            return _output.GetConsumingEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}