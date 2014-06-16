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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public static class CachingEnumerableExtensions {
        public static CacheEnumerable<T> ToCacheEnumerable<T>(this IEnumerable<T> collection) {
            if (collection == null) {
                return new CacheEnumerable<T>(Enumerable.Empty<T>());
            }
            return collection as CacheEnumerable<T> ?? new CacheEnumerable<T>(collection);
        }

        public static IEnumerator<T>[] Clone<T>(this IEnumerator<T> enumerator, int copies = 2) {
            return new CacheEnumerable<T>(enumerator).GetEnumerators(copies).ToArray();
        }
    }

    /// <summary>
    ///     This IEnumerable Wrapper will cache the results incrementally on first use of the source collection
    ///     into a List, so that subsequent uses of the collection are pulled from the list.
    ///     (and it doesn't need to iterate thru the whole collection first, like ToList() )
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CacheEnumerable<T> : IEnumerable<T> {
        private readonly IEnumerable<T> _source;
        private List<T> _list;
        private IEnumerator<T> _sourceIterator;

        public CacheEnumerable(IEnumerable<T> source) {
            _source = source ?? new T[0];
        }

        public CacheEnumerable(IEnumerator<T> sourceIterator) {
            _source = null;
            _sourceIterator = sourceIterator;
        }

        public CacheEnumerable<T> Concat(IEnumerable<T> additionalItems) {
            return Enumerable.Concat(this, additionalItems).ToCacheEnumerable();
        }

        public IEnumerable<IEnumerator<T>> GetEnumerators(int copies) {
            for (var i = 0; i < copies; i++) {
                yield return GetEnumerator();
            }
        }

        private bool IsOk(int index) {
            if (index < _list.Count) {
                return true;
            }

            lock (this) {
                if (_sourceIterator == null) {
                    _sourceIterator = _source.GetEnumerator();
                }

                while (_sourceIterator.MoveNext()) {
                    _list.Add(_sourceIterator.Current);
                    if (index < _list.Count) {
                        return true;
                    }
                }
            }
            return false;
        }

        #region Nested type: LazyEnumerator

        internal class LazyEnumerator<TT> : IEnumerator<TT> {
            private CacheEnumerable<TT> _collection;
            private int _index = -1;

            internal LazyEnumerator(CacheEnumerable<TT> collection) {
                _collection = collection;
            }

            #region IEnumerator<Tt> Members

            public TT Current {
                get {
                    return _collection._list[_index];
                }
            }

            public void Dispose() {
                _collection = null;
            }

            object IEnumerator.Current {
                get {
                    return Current;
                }
            }

            public bool MoveNext() {
                _index++;
                return _collection.IsOk(_index);
            }

            public void Reset() {
                _index = -1;
            }

            public IEnumerator<TT> Clone() {
                return new LazyEnumerator<TT>(_collection) {
                    _index = _index
                };
            }

            #endregion
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator() {
            lock (this) {
                if (_list == null) {
                    _list = new List<T>();
                }
            }

            return new LazyEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
    }
}