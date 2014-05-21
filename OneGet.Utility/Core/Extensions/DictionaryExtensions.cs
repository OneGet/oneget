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
    using System.Collections.Generic;
    using System.Linq;

    public static class DictionaryExtensions {
        public static TValue AddOrSet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) {
            lock (dictionary) {
                if (dictionary.ContainsKey(key)) {
                    dictionary[key] = value;
                } else {
                    dictionary.Add(key, value);
                }
            }
            return value;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFunction) {
            lock (dictionary) {
                return dictionary.ContainsKey(key) ? dictionary[key] : dictionary.AddOrSet(key, valueFunction());
            }
        }

        public static TValue GetAndRemove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) {
            lock (dictionary) {
                var result = dictionary[key];
                if (dictionary.ContainsKey(key)) {
                    dictionary.Remove(key);
                }
                return result;
            }
        }

        public static void AddObjectPairPair<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, object key, object value)
            where TKey : class
            where TValue : class {
            dictionary.Add(key as TKey, value as TValue);
        }

        public static TKey MatchKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, object key) {
            if (dictionary == null || key == null) {
                return default(TKey);
            }
            return dictionary.Keys.FirstOrDefault<TKey>(each => key.ToString().Equals(each.ToString(), StringComparison.CurrentCultureIgnoreCase));
        }

        public static TValue GetByMatchedKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, object key) {
            if (dictionary == null || key == null) {
                return default(TValue);
            }
            var actualKey = dictionary.MatchKey(key);
            return Equals(actualKey, null) ? default(TValue) : dictionary[actualKey];
        }


        public static Dictionary<TKey, TElement> ToDictionaryNicely<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) {
            if (source == null)
                throw new ArgumentNullException("source");
            if (keySelector == null)
                throw new ArgumentNullException("keySelector");
            if (elementSelector == null)
                throw new ArgumentNullException("elementSelector");

            var d = new Dictionary<TKey, TElement>(comparer);
            foreach (var element in source)
                d.AddOrSet(keySelector(element), elementSelector(element));

            return d;
        }

    }
}
