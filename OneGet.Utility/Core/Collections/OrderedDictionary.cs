using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.OneGet.Core.Collections {
    using System.Collections;
    using System.Collections.Specialized;

    public class OrderedDictionary<TKey, TValue> : OrderedDictionary, IDictionary<TKey, TValue> {

        internal class KVPEnumerator : IEnumerator<KeyValuePair<TKey, TValue>> {
            private IDictionaryEnumerator _enumerator;
            internal KVPEnumerator(IDictionaryEnumerator e) {
                _enumerator = e;
            }

            public void Dispose() {
            }

            public bool MoveNext() {
                return _enumerator.MoveNext();
            }

            public void Reset() {
                _enumerator.Reset();
            }

            public KeyValuePair<TKey, TValue> Current {
                get {
                    return new KeyValuePair<TKey, TValue>((TKey)_enumerator.Key, (TValue)_enumerator.Value);
                }
            }

            object IEnumerator.Current {
                get {
                    return Current;
                }
            }
        }

        internal class KeyCollection : ICollection<TKey> {
            private readonly OrderedDictionary _dictionary;

            public KeyCollection(OrderedDictionary dictionary) {
                _dictionary = dictionary;
            }
            public IEnumerator<TKey> GetEnumerator() {
                return _dictionary.Keys.Cast<TKey>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            public void Add(TKey item) {
                throw new NotImplementedException();
            }

            public void Clear() {
                throw new NotImplementedException();
            }

            public bool Contains(TKey item) {
                return _dictionary.Contains(item);
            }

            public void CopyTo(TKey[] array, int arrayIndex) {
                _dictionary.Keys.CopyTo(array, arrayIndex);
            }

            public bool Remove(TKey item) {
                throw new NotImplementedException();
            }

            public int Count {
                get {
                    return _dictionary.Keys.Count;
                }
            }

            public bool IsReadOnly {
                get {
                    return true;
                }
            }
        }

        internal class ValueCollection : ICollection<TValue> {
            private readonly OrderedDictionary _dictionary;

            public ValueCollection(OrderedDictionary dictionary) {
                _dictionary = dictionary;
            }
            public IEnumerator<TValue> GetEnumerator() {
                return _dictionary.Values.Cast<TValue>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            public void Add(TValue item) {
                throw new NotImplementedException();
            }

            public void Clear() {
                throw new NotImplementedException();
            }

            public bool Contains(TValue item) {
                return _dictionary.Values.Cast<TValue>().Contains(item);
            }

            public void CopyTo(TValue[] array, int arrayIndex) {
                _dictionary.Values.CopyTo(array, arrayIndex);
            }

            public bool Remove(TValue item) {
                throw new NotImplementedException();
            }

            public int Count {
                get {
                    return _dictionary.Values.Count;
                }
            }

            public bool IsReadOnly {
                get {
                    return true;
                }
            }
        }
        

        public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return new KVPEnumerator(base.GetEnumerator());
        }

        public void Add(KeyValuePair<TKey, TValue> item) {
            base.Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            return base.Contains(item.Key) && item.Value.Equals(base[item.Key]);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            for(var e = GetEnumerator();e.MoveNext();) {
                array[arrayIndex++] = e.Current;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            if (Contains(item)) {
                base.Remove(item.Key);
                return true;
            }
            return false;
        }

        public bool ContainsKey(TKey key) {
            return base.Contains(key);
        }

        public void Add(TKey key, TValue value) {
            base.Add(key, value);
        }

        public bool Remove(TKey key) {
            if (base.Contains(key)) {
                base.Remove(key);
                return true;
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            if (base.Contains(key)) {
                value = (TValue)base[key];
                return true;
            }
            value = default(TValue);
            return false;
        }

        public TValue this[TKey key] {
            get {
                return (TValue)base[key];
            }
            set {
                base[key] = value;
            }
        }

        public new ICollection<TKey> Keys {
            get {
                return new KeyCollection(this);
            }
        }

        public new ICollection<TValue> Values {
            get {
                return new ValueCollection(this);
            }
        }
    }

    public class List<TKey, TValue> : List<KeyValuePair<TKey, TValue>> {
        public void Add(TKey key, TValue value) {
            Add(new KeyValuePair<TKey, TValue>(key, value));
        }
    }
    
    public class List<T1, T2, T3> : List<Tuple<T1,T2,T3>> {
        public void Add(T1 p1, T2 p2, T3 p3) {
            Add(new Tuple<T1, T2,T3>(p1,p2,p3));
        }
    }
    public class List<T1, T2, T3,T4> : List<Tuple<T1, T2, T3,T4>> {
        public void Add(T1 p1, T2 p2, T3 p3, T4 p4) {
            Add(new Tuple<T1, T2, T3,T4>(p1, p2, p3,p4));
        }
    }
    public class List<T1, T2, T3, T4, T5> : List<Tuple<T1, T2, T3, T4,T5>> {
        public void Add(T1 p1, T2 p2, T3 p3, T4 p4,T5 p5) {
            Add(new Tuple<T1, T2, T3, T4,T5>(p1, p2, p3, p4,p5));
        }
    }

    
}
