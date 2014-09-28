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

namespace Microsoft.OneGet.Utility.Plugin {
    using System;
    using System.Reflection;

#if USE_APPDOMAINS
    internal class Proxy<T> : IDisposable where T : MarshalByRefObject {
        private PluginDomain _domain;
        private bool _isDisposed;
        private T _target;

        internal Proxy(PluginDomain domain, params object[] args) {
            if (domain == null) {
                throw new ArgumentNullException("domain");
            }

            _domain = domain;
            _target = (T)((AppDomain)domain).CreateInstanceAndUnwrap(typeof (T).Assembly.FullName, typeof (T).FullName, false, BindingFlags.CreateInstance, null, args, null, null);
        }

        public void Dispose() {
            if (!_isDisposed) {
                // _domain = _domain.Delete();
            }
            _target = null;
            _isDisposed = true;
        }

        public static implicit operator T(Proxy<T> proxy) {
            if (proxy == null) {
                throw new ArgumentNullException("proxy");
            }

            if (proxy._isDisposed) {
                throw new ObjectDisposedException("domain");
            }

            return proxy._target;
        }
    }
#endif
}
