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

namespace Microsoft.OneGet.Collections {
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /* LATER.
     * 
      var rq = c.As<Request>();
            var pvds = rq.PackageManagementService.SelectProvidersWithFeatureSet("supports-powershell-get");
            
            
            List<PackageProvider> pp = new List<PackageProvider>();
            for (PackageProvider x; (x = pvds.Next()) != null;) {
                pp.Add(x);
            }
           


    
    [Serializable]
    public class SerializableEnumerable<T> where T : MarshalByRefObject {
        
    }
    */

    public class RemoteReference<T> : MarshalByRefObject {
        // we don't want these objects being gc's out because they remain unused...
        public override object InitializeLifetimeService() {
            return null;
        }

        private readonly T _instance;
        public RemoteReference(T instance) {
            _instance = instance;
        }

        public T Instance {
            get {
                return _instance;
            }
        }
    }

    public class SerizlizableEnumerable<T> : IEnumerable<T> {

        protected readonly IEnumerable<T> _enumerable;

        public SerizlizableEnumerable(IEnumerable<T> originalEnumerable) {
            _enumerable = originalEnumerable;
        }

        public IEnumerator<T> GetEnumerator() {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }

    public class ByRefEnumerable<T> : MarshalByRefObject,  IEnumerable<T> {
        // we don't want these objects being gc's out because they remain unused...
        public override object InitializeLifetimeService() {
            return null;
        }

        protected readonly IEnumerable<T> _enumerable;

        public ByRefEnumerable(IEnumerable<T> originalEnumerable) {
            _enumerable = originalEnumerable;
        }

        public virtual IEnumerator<T> GetEnumerator() {
            return new ByRefEnumerator<T>(_enumerable.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
