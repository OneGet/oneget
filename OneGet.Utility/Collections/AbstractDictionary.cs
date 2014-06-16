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
    using System.Linq;

    public abstract class AbstractDictionary<TKey, TVal> : MarshalByRefObject , IDictionary<TKey, TVal>  {
        // we don't want these objects being gc's out because they remain unused...
        public override object InitializeLifetimeService() {
            return null;
        }


        public virtual IEnumerator<KeyValuePair<TKey, TVal>> GetEnumerator() {
            return new VirtualEnumerator<KeyValuePair<TKey, TVal>>(Keys.GetEnumerator(), enumerator => new KeyValuePair<TKey, TVal>((TKey)enumerator.Current, this[(TKey)enumerator.Current]));
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public virtual void Add(KeyValuePair<TKey, TVal> item) {
            this[item.Key] = item.Value;
        }

        public virtual bool Contains(KeyValuePair<TKey, TVal> item) {
            return ContainsKey(item.Key) && this[item.Key].Equals(item.Value);
        }

        /// <exception cref="IndexOutOfRangeException">Not enough room in target array.</exception>
        public virtual void CopyTo(KeyValuePair<TKey, TVal>[] array, int arrayIndex) {
            if (array == null) {
                throw new ArgumentNullException("array");
            }
            var enumerator = GetEnumerator();

            while (arrayIndex <= array.Length) {
                if (enumerator.MoveNext()) {
                    array[arrayIndex++] = enumerator.Current;
                } else {
                    return;
                }
            }
            throw new IndexOutOfRangeException("Not enough room in target array.");
        }

        public virtual bool Remove(KeyValuePair<TKey, TVal> item) {
            return Contains(item) && Remove(item.Key);
        }

        public virtual bool ContainsKey(TKey key) {
            return Keys.Contains(key);
        }

        public virtual void Add(TKey key, TVal value) {
            this[key] = value;
        }

        public virtual ICollection<TVal> Values {
            get {
                return Keys.Select(k => this[k]).ToArray();
            }
        }

        public virtual int Count {
            get {
                return Keys.Count;
            }
        }

        public virtual bool IsReadOnly {
            get {
                return false;
            }
        }

        // poor implementation. should override for performance reasons.
        public virtual void Clear() {
            TKey key;
            while ((key = Keys.FirstOrDefault()) != null) {
                Remove(key);
            }
        }

        public virtual bool TryGetValue(TKey key, out TVal value) {
            value = ContainsKey(key) ? this[key] : default(TVal);
            return ContainsKey(key);
        }

        public abstract TVal this[TKey key] {get; set;}
        public abstract ICollection<TKey> Keys {get;}
        public abstract bool Remove(TKey key);
    }
}
