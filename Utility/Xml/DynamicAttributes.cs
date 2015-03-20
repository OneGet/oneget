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

namespace Microsoft.PackageManagement.Utility.Xml {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Xml.Linq;

    /// <summary>
    ///     A convenience wrapper class for Attributes in an XElement node.
    /// </summary>
    public class DynamicAttributes : DynamicObject, IEnumerable<XAttribute> {
        /// <summary>
        ///     The element this object is fronting for.
        /// </summary>
        private readonly XElement _element;

        /// <summary>
        ///     Creates an Attribute handler for the given XML Node.
        /// </summary>
        /// <param name="element">the XML node</param>
        public DynamicAttributes(XElement element) {
            _element = element;
        }

        /// <summary>
        ///     Provides access to attributes in the element by indexer
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns>the string value of the attribute if it exists, otherwise null</returns>
        public string this[string attributeName] {
            get {
                if (string.IsNullOrWhiteSpace(attributeName)) {
                    return null;
                }
                var result = _element.Attribute(attributeName);
                if (result != null) {
                    return result.Value;
                }
                return null;
            }
            set {
                _element.SetAttributeValue(attributeName, value);
            }
        }

        public IEnumerator<XAttribute> GetEnumerator() {
            return _element.Attributes().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        ///     checks if the attribute exists in the element
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns>true if the attribute exists</returns>
        public bool Has(string attributeName) {
            return _element.Attribute(attributeName) != null;
        }

        /// <summary>
        ///     Returns the Attribute value
        /// </summary>
        /// <param name="binder">the Attribute Name</param>
        /// <param name="result">the return value (attribute value)</param>
        /// <returns>true if successful</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            if (binder == null) {
                throw new ArgumentNullException("binder");
            }
            var attr = _element.Attribute(binder.Name);
            if (attr != null) {
                result = attr.Value;
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        ///     Sets the attribute value
        /// </summary>
        /// <param name="binder">Attribute name</param>
        /// <param name="value">Value to set</param>
        /// <returns>True</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value) {
            if (binder == null) {
                throw new ArgumentNullException("binder");
            }
            _element.SetAttributeValue(binder.Name, value);
            return true;
        }
    }
}
