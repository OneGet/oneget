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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumerableExtensions {
        /// <summary>
        /// Returns a ReEnumerable wrapper around the collection which timidly (cautiously) pulls items
        /// but still allows you to to re-enumerate without re-running the query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static ReEnumerable<T> Timid<T>(this IEnumerable<T> collection) {
            if (collection == null) {
                return new ReEnumerable<T>(Enumerable.Empty<T>());
            }
            return collection as ReEnumerable<T> ?? new ReEnumerable<T>(collection);
        }
    }

    /// <summary>
    ///     This IEnumerable Wrapper will cache the results incrementally on first use of the source collection
    ///     into a List, so that subsequent uses of the collection are pulled from the list.
    ///     (and it doesn't need to iterate thru the whole collection first, like ToList() )
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReEnumerable<T> : IEnumerable<T> {
        private readonly IEnumerable<T> _source;
        private List<T> _list;
        private IEnumerator<T> _sourceIterator;

        public ReEnumerable(IEnumerable<T> source) {
            _source = source ?? new T[0];
        }

        public ReEnumerable(IEnumerator<T> sourceIterator) {
            _source = null;
            _sourceIterator = sourceIterator;
        }

        public ReEnumerable<T> Concat(IEnumerable<T> additionalItems) {
            return Enumerable.Concat(this, additionalItems).Timid();
        }

        public IEnumerable<IEnumerator<T>> GetEnumerators(int copies) {
            for (var i = 0; i < copies; i++) {
                yield return GetEnumerator();
            }
        }

        private bool ItemExists(int index) {
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

        internal class Enumerator<TT> : IEnumerator<TT> {
            private ReEnumerable<TT> _collection;
            private int _index = -1;

            internal Enumerator(ReEnumerable<TT> collection) {
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
                return _collection.ItemExists(_index);
            }

            public void Reset() {
                _index = -1;
            }

            public IEnumerator<TT> Clone() {
                return new Enumerator<TT>(_collection) {
                    _index = _index
                };
            }

            #endregion
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator() {
            lock (this) {
                if (_list == null) {
                    _list = new List<T>();
                }
            }

            return new Enumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
    }
}