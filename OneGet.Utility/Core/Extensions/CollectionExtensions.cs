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

namespace Microsoft.OneGet.Core.Extensions {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Collections;

    public static class CollectionExtensions {
        /// <summary>
        ///     Determines whether the collection object is either null or an empty collection.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="collection"> The collection. </param>
        /// <returns>
        ///     <c>true</c> if [is null or empty] [the specified collection]; otherwise, <c>false</c> .
        /// </returns>
        /// <remarks>
        /// </remarks>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection) {
            return collection == null || !collection.Any();
        }

        /// <summary>
        ///     Ensures that the IEnumerable implements MarshalByRefObject so that the whole
        ///     collection is not forced to serialize.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static IEnumerable<T> ByRef<T>(this IEnumerable<T> enumerable) {
            if (enumerable == null) {
                return null;
            }

            return enumerable as ByRefEnumerable<T> ?? new ByRefEnumerable<T>(enumerable);
        }

        public static IEnumerable<T> ByRefEnumerable<T>(this IEnumerable<T> enumerable) {
            if (enumerable == null) {
                return null;
            }

            return enumerable as ByRefEnumerable<T> ?? new ByRefEnumerable<T>(enumerable);
        }

        public static IEnumerator<T> ByRef<T>(this IEnumerator<T> enumerator) {
            if (enumerator == null) {
                return null;
            }

            return enumerator as ByRefEnumerator<T> ?? new ByRefEnumerator<T>(enumerator);
        }

        public static ICollection<T> ByRef<T>(this ICollection<T> collection) {
            if (collection == null) {
                return null;
            }
            return collection as ByRefCollection<T> ?? new ByRefCollection<T>(collection);
        }

        public static ICollection<T> ByRefCollection<T>(this IEnumerable<T> enumerable) {
            if (enumerable == null) {
                return null;
            }

            return enumerable as ByRefCollection<T> ?? enumerable.ToList().ByRef();
        }

        /// <summary>
        ///     Concatenates a single item to an IEnumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<T> ConcatSingleItem<T>(this IEnumerable<T> enumerable, T item) {
            return enumerable.Concat(new[] {
                item
            });
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> enumerable) {
            return enumerable.Where(each => (object)each != null);
        }

        public static IEnumerable<T> ToEnumerable<T>(this object obj) {
            if (obj == null) {
                return Enumerable.Empty<T>();
            }
            if (obj is string) {
                return new T[] {
                    (T)obj
                };
            }

            if (obj is IEnumerable) {
                return ((IEnumerable)obj).Cast<T>();
            }
            if (obj is T) {
                return new T[] {
                    (T)obj
                };
            }
            // can we coerce the type?
            return new T[] {
                (T)typeof (T).CoerceCast(obj)
            };
        }

        public static bool IfPresentRemoveLocked<T>(this HashSet<T> list, T element) {
            if (list == null || element == null) {
                return false;
            }

            lock (list) {
                return list.Remove(element);
            }
        }

        public static T MyMax<T, TCompare>(this IEnumerable<T> collection, Func<T, TCompare> func) where TCompare : IComparable<TCompare> {
            T maxItem = default(T);
            TCompare maxValue = default(TCompare);
            foreach (var item in collection) {
                TCompare temp = func(item);
                if (maxItem == null || temp.CompareTo(maxValue) > 0) {
                    maxValue = temp;
                    maxItem = item;
                }
            }
            return maxItem;
        }
    }
}