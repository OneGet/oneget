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

    public class ByRefCollection : MarshalByRefObject, ICollection {
        private ICollection _collection;
        // we don't want these objects being gc's out because they remain unused...

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

        public override object InitializeLifetimeService() {
            return null;
        }
    }
}