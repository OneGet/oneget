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
    using System.Collections;
    using System.Collections.Generic;

    public class ByRefEnumerable<T> : MarshalByRefObject, IEnumerable<T> {
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
