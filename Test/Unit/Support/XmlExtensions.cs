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

namespace Microsoft.OneGet.Test.Support {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Packaging;

    internal static class XmlExtensions {

        private static XmlNamespaceManager _namespaceManager;
        internal static XmlNamespaceManager NamespaceManager {
            get {
                if (_namespaceManager == null) {
                    XmlNameTable nameTable = new NameTable();
                    _namespaceManager = new XmlNamespaceManager(nameTable);
                    _namespaceManager.AddNamespace("swid", Iso19770_2.Namespace.NamespaceName);
                    _namespaceManager.AddNamespace("oneget", "http://oneget.org/swidtag");
                }
                return _namespaceManager;
            }
        }


        internal static XAttribute XPathToAttribute(this XDocument xmlDocument, string xpath) {
            return ((xmlDocument.XPathEvaluate(xpath, NamespaceManager) as IEnumerable) ?? new XAttribute[0]).Cast<XAttribute>().FirstOrDefault();
        }

        internal static IEnumerable<XAttribute> XPathToAttributes(this XDocument xmlDocument, string xpath) {
            return ((xmlDocument.XPathEvaluate(xpath, NamespaceManager) as IEnumerable) ?? new XAttribute[0]).Cast<XAttribute>();
        }

        internal static IEnumerable<XElement> XPathToElements(this XDocument xmlDocument, string xpath) {
            return ((xmlDocument.XPathEvaluate(xpath, NamespaceManager) as IEnumerable) ?? new XElement[0]).Cast<XElement>();
        }

        public static bool IsFalse(this string text) {
            return !string.IsNullOrWhiteSpace(text) && text.Equals("false", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}