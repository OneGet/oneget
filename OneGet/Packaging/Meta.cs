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

namespace Microsoft.OneGet.Packaging {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Utility.Collections;

    public class Meta :  IDictionary<string, string> {
        private XElement _element;

        protected internal Meta(XElement element) {
            _element = element;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
            return _element.Attributes().Select(each => new KeyValuePair<string, string>(each.Name.LocalName, each.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, string> item) {
            throw new NotImplementedException();
        }

        public void Clear() {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, string> item) {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, string> item) {
            throw new NotImplementedException();
        }

        public int Count {
            get {
                return _element.Attributes().Count();
            }
        }

        public bool IsReadOnly {
            get {
                return true;
            }
        }

        public bool ContainsKey(string key) {
            return _element.Attribute(key) != null;
        }

        public void Add(string key, string value) {
            throw new NotImplementedException();
        }

        public bool Remove(string key) {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out string value) {
            value = _element.Get(key);
            return value != null;
        }

        public string this[string key] {
            get {
                return _element.Get(key);
            }
            set {
                throw new NotImplementedException();
            }
        }

        public ICollection<string> Keys {
            get {
                return _element.Attributes().Select(each => each.Name.LocalName).ToArray();
            }
        }

        public ICollection<string> Values {
            get {
                return _element.Attributes().Select(each => each.Value).ToArray();
            }
        }
    }
}