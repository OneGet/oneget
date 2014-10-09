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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public static class EnumerableExtensions {
        /// <summary>
        /// Returns a ReEnumerable wrapper around the collection which timidly (cautiously) pulls items
        /// but still allows you to to re-enumerate without re-running the query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static MutableEnumerable<T> ReEnumerable<T>(this IEnumerable<T> collection) {
            if (collection == null) {
                return new ReEnumerable<T>(Enumerable.Empty<T>());
            }
            return collection as MutableEnumerable<T> ?? new ReEnumerable<T>(collection);
        }
    }

    public abstract class MutableEnumerable<T> : IEnumerable<T> {
        private List<T> _list;

        protected List<T> List {
            get {
                if (_list == null) {
                    lock (this) {
                        if (_list == null) {
                            _list = new List<T>();
                        }
                    }
                }
                return _list;
            }
        }

        protected MutableEnumerable() {
        }

        public IEnumerable<IEnumerator<T>> GetEnumerators(int copies) {
            for (var i = 0; i < copies; i++) {
                yield return GetEnumerator();
            }
        }

        protected abstract bool ItemExists(int index);

        
        internal class Enumerator<TT> : IEnumerator<TT> {
            private MutableEnumerable<TT> _collection;
            private int _index = -1;

            internal Enumerator(MutableEnumerable<TT> collection) {
                _collection = collection;
            }

            #region IEnumerator<Tt> Members

            public TT Current {
                get {
                    return _collection.List[_index];
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
            return new Enumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
    }

    public class MyBlockingCollection<T> : MutableEnumerable<T>, IList<T>, IDisposable {
        private ManualResetEvent _completed = new ManualResetEvent(false);
        private ManualResetEvent _added = new ManualResetEvent(false);

        public bool IsCompleted {
            get {
                return _completed.WaitOne(0);
            }
        }

        public void Complete() {
            _completed.Set();
        }

        public IEnumerable<T> GetConsumingEnumerable() {
            return this;
        }

        protected override bool ItemExists(int index) {
            while (true) {
                lock (this) {
                    if (List.Count > index) {
                        return true;
                    }
                    _added.Reset();
                }

                if (WaitHandle.WaitAny(new WaitHandle[] {_completed, _added}) == 0) {
                    return List.Count > index;
                }
            }
        }

        public void Add(T item) {
            if (_completed.WaitOne(0)) {
                // throw new Exception("Attempt to modify completed collection");
                return;
            }

            lock (this) {
                List.Add(item);
                _added.Set();
                
            }
        }

        public void Clear() {
            throw new System.NotImplementedException();
        }

        public bool Contains(T item) {
            _completed.WaitOne();
            return List.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            _completed.WaitOne();
            List.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) {
            throw new System.NotImplementedException();
        }

        public int Count {
            get {
                _completed.WaitOne();
                return List.Count;
            }
        }

        public bool IsReadOnly {
            get {
                return false;
            }
        }

        public int IndexOf(T item) {
            _completed.WaitOne();
            return List.IndexOf(item);
        }

        public void Insert(int index, T item) {
            throw new System.NotImplementedException();
        }

        public void RemoveAt(int index) {
            throw new System.NotImplementedException();
        }

        public T this[int index] {
            get {
                if (ItemExists(index)) {
                    return List[index];
                }
                throw new IndexOutOfRangeException();
            }
            set {
                throw new System.NotImplementedException();
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing) {
            if (disposing) {
                if (_completed != null) {
                    _completed.Set();
                    _completed.Dispose();
                    _completed = null;
                }
                if (_added != null) {
                    _added.Set();
                    _added.Dispose();
                    _added = null;
                }
            }
        }

        public void Wait(int milliseconds = -1) {
            _completed.WaitOne(milliseconds);
        }
    }

    public class ReEnumerable<T> : MutableEnumerable<T> {
        private readonly IEnumerable<T> _source;
        private IEnumerator<T> _sourceIterator;

          public ReEnumerable(IEnumerable<T> source) {
            _source = source ?? new T[0];
        }

        public ReEnumerable(IEnumerator<T> sourceIterator) {
            _source = null;
            _sourceIterator = sourceIterator;
        }

        protected override bool ItemExists(int index) {
            if (index < List.Count) {
                return true;
            }

            lock (this) {
                if (_sourceIterator == null) {
                    _sourceIterator = _source.GetEnumerator();
                }

                while (_sourceIterator.MoveNext()) {
                    List.Add(_sourceIterator.Current);
                    if (index < List.Count) {
                        return true;
                    }
                }
            }
            return false;
        }

        public MutableEnumerable<T> Concat(IEnumerable<T> additionalItems) {
            return Enumerable.Concat(this, additionalItems).ReEnumerable();
        }
    }

    /*
    /// <summary>
    ///     This IEnumerable Wrapper will cache the results incrementally on first use of the source collection
    ///     into a List, so that subsequent uses of the collection are pulled from the list.
    ///     (and it doesn't need to iterate thru the whole collection first, like ToList() )
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class _ReEnumerable<T> : IEnumerable<T> {
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
            return Enumerable.Concat(this, additionalItems).ReEnumerable();
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
     */
}