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

namespace Microsoft.OneGet.Core {
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Threading;

    public class DynamicPowershellResult : IDisposable, IEnumerable<object> {
        internal ManualResetEvent CompletedEvent = new ManualResetEvent(false);
        public BlockingCollection<ErrorRecord> Errors = new BlockingCollection<ErrorRecord>();
        internal BlockingCollection<object> Output = new BlockingCollection<object>();

        internal ManualResetEvent StartedEvent = new ManualResetEvent(false);
        internal bool LastIsTerminatingError {get; set;}

        public object Value {
            get {
                StartedEvent.WaitOne();

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
                return Output.GetConsumingEnumerable();
            }
        }

        public bool Success {
            get {
                CompletedEvent.WaitOne();

                if (LastIsTerminatingError) {
                    return false;
                }
                return true;
            }
        }

        public void Dispose() {
            if (Output != null) {
                Output.Dispose();
                Output = null;
            }

            if (Errors != null) {
                Errors.Dispose();
                Errors = null;
            }

            if (StartedEvent != null) {
                StartedEvent.Dispose();
                StartedEvent = null;
            }

            if (CompletedEvent != null) {
                CompletedEvent.Dispose();
                CompletedEvent = null;
            }
        }

        public IEnumerator<object> GetEnumerator() {
            return Output.GetConsumingEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}