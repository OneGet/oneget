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

namespace Microsoft.OneGet.Utility.Extensions {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public static class HashtableExtensions {
        public static bool IsNullOrEmpty(this Hashtable hashtable) {
            return hashtable == null || hashtable.Count == 0;
        }

        public static IEnumerable<string> KeysEnumerable(this Hashtable hashtable) {
            if (hashtable != null && hashtable.Keys.Count > 0) {
                return hashtable.Keys.Cast<object>().WhereNotNull().Select(each => each.ToString());
            }
            return Enumerable.Empty<string>();
        }

        public static Func<string, T> LookupFunc<T>(this Hashtable hashtable) {
            return index => {
                var obj = hashtable[hashtable.Cast<object>().Where<object>(each => each != null && index.Equals(each.ToString(), StringComparison.CurrentCultureIgnoreCase))];
                if (obj is T) {
                    return (T)obj;
                }
                return default(T);
            };
        }
        public static Func<string,string> LookupFuncString(this Hashtable hashtable) {
            if (hashtable == null || hashtable.Count == 0) {
                return (x) => null;
            }
            return (index) => {
                // get the key from the inext
                var key = hashtable.Cast<object>().FirstOrDefault<object>(each => each != null && index.Equals(each.ToString(), StringComparison.CurrentCultureIgnoreCase));

                // get a string
                return !Equals(null, key) ? hashtable[key].ToString() : string.Empty;
            };
        }


        public static Func<string, IEnumerable<string>> LookupFuncStrings(this Hashtable hashtable) {
            if (hashtable == null || hashtable.Count == 0) {
                return (x) => Enumerable.Empty<string>();
            }
            return (index) => {
                // get the key from the inext
                var key = hashtable.Cast<object>().FirstOrDefault<object>(each => each != null && index.Equals(each.ToString(), StringComparison.CurrentCultureIgnoreCase));

                if (!Equals(null, key)) {
                    // get the item in the collection
                    var obj = hashtable[key];

                    // if it's a string, return it as a single item
                    if (obj is string) {
                        return new [] {obj as string};
                    }

                    // otherwise, try to cast it to a collection of string-like-things
                    var collection = obj as IEnumerable;
                    if (collection != null) {
                        return collection.Cast<object>().Select(each => each.ToString()).ByRef();
                    }

                    // meh. ToString, and goodnight.
                    return new[] { obj.ToString() };
                }
                return Enumerable.Empty<string>();
            };
        }

        public static IEnumerable<string> GetStringCollection(this Hashtable hashtable, string path) {
            if (hashtable.IsNullOrEmpty() || string.IsNullOrEmpty(path)) {
                return Enumerable.Empty<string>();
            }
            return hashtable.GetStringCollection(path.Split('/').Select(each => each.Trim()).ToArray());
        }

        public static IEnumerable<string> GetStringCollection(this Hashtable hashtable, string[] paths) {
            if (hashtable.IsNullOrEmpty() || paths.IsNullOrEmpty() || !hashtable.ContainsKey(paths[0])) {
                return Enumerable.Empty<string>();
            }

            if (paths.Length == 1) {
                // looking for the actual result value
                var items = hashtable[paths[0]] as IEnumerable<object>;
                if (items != null) {
                    return items.Select(each => each.ToString());
                }

                return new[] {
                    (hashtable[paths[0]] ?? "").ToString()
                };
            }

            var item = hashtable[paths[0]] as Hashtable;
            return item == null ? Enumerable.Empty<string>() : item.GetStringCollection(paths.Skip(1).ToArray());
        }

        public static IEnumerable<KeyValuePair<string, string>> Flatten(this Hashtable hashTable) {
            foreach (var k in hashTable.Keys) {
                var value = hashTable[k];
                if (value == null) {
                    continue;
                }

                if (value is Hashtable) {
                    foreach (var kvp in (value as Hashtable).Flatten()) {
                        yield return new KeyValuePair<string, string>(k.ToString() + "/" + kvp.Key, kvp.Value );
                    }
                } else {
                    yield return new KeyValuePair<string, string>(k.ToString(),value.ToString());
                }
            }
        }
    }
}
