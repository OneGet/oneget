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
    using System.Collections.Generic;

    public class DelegateDictionary<TKey, TVal> : AbstractDictionary<TKey, TVal> {
        private readonly Action _clear;
        private readonly Func<TKey, bool> _containsKey;
        private readonly Func<TKey, TVal> _get;
        private readonly Func<ICollection<TKey>> _keys;
        private readonly Func<TKey, bool> _remove;
        private readonly Action<TKey, TVal> _set;

        public DelegateDictionary(Func<ICollection<TKey>> keys, Func<TKey, TVal> get, Action<TKey, TVal> set, Func<TKey, bool> remove) {
            _keys = keys;
            _get = get;
            _set = set;
            _remove = remove;
            _clear = null;
            _containsKey = null;
        }

        public DelegateDictionary(Func<ICollection<TKey>> keys, Func<TKey, TVal> get, Action<TKey, TVal> set, Func<TKey, bool> remove, Action clear) {
            _keys = keys;
            _get = get;
            _set = set;
            _remove = remove;
            _clear = clear;
            _containsKey = null;
        }

        public DelegateDictionary(Func<ICollection<TKey>> keys, Func<TKey, TVal> get, Action<TKey, TVal> set, Func<TKey, bool> remove, Func<TKey, bool> containsKey) {
            _keys = keys;
            _get = get;
            _set = set;
            _remove = remove;
            _clear = null;
            _containsKey = containsKey;
        }

        public DelegateDictionary(Func<ICollection<TKey>> keys, Func<TKey, TVal> get, Action<TKey, TVal> set, Func<TKey, bool> remove, Action clear, Func<TKey, bool> containsKey) {
            _keys = keys;
            _get = get;
            _set = set;
            _remove = remove;
            _clear = clear;
            _containsKey = containsKey;
        }

        public override TVal this[TKey key] {
            get {
                return _get(key);
            }
            set {
                _set(key, value);
            }
        }

        public override ICollection<TKey> Keys {
            get {
                return _keys() ?? new List<TKey>();
            }
        }

        public override bool Remove(TKey key) {
            return _remove(key);
        }

        public override void Clear() {
            if (_clear == null) {
                base.Clear();
                return;
            }
            _clear();
        }

        public override bool ContainsKey(TKey key) {
            if (_containsKey == null) {
                return base.ContainsKey(key);
            }
            return _containsKey(key);
        }
    }
}
