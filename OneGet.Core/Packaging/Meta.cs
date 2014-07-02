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
    using Collections;

    public class Meta : MarshalByRefObject, IReadOnlyDictionary<string, string> {
        public override object InitializeLifetimeService() {
            return null;
        }

        private XElement _element;
        internal protected Meta(XElement element) {
            _element = element;
        }


        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
            return new SerializableEnumerator<KeyValuePair<string, string>>(_element.Attributes().Select(each => new KeyValuePair<string, string>(each.Name.LocalName, each.Value)).GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public int Count {
            get {
                return _element.Attributes().Count();
            }
        }

        public bool ContainsKey(string key) {
            return _element.Attribute( key) != null;
        }

        public bool TryGetValue(string key, out string value) {
            value = _element.Get(key);
            return value != null;
        }

        public string this[string key] {
            get {
                return _element.Get( key);
            }
        }

        public IEnumerable<string> Keys {
            get {
                return new SerializableEnumerable<string>(_element.Attributes().Select(each => each.Name.LocalName));
            }
        }

        public IEnumerable<string> Values {
            get {
                return new SerializableEnumerable<string>(_element.Attributes().Select(each => each.Value));
            }
        }
    }
}