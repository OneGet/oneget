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

namespace Microsoft.OneGet.Utility.Xml {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;

    /// <summary>
    ///     Represents a dynamic interface to an XElement.
    ///     This allows simplified access to XML Documents
    ///     using c# 4.0 and the dynamic keyword, along with
    ///     a convenient access for XPath.
    /// </summary>
    public class DynamicElement : DynamicObject, IEnumerable<DynamicElement> {
        private static readonly Dictionary<Type, Func<string, object>> _xmlConverters;

        /// <summary>
        ///     The XML Node this object is fronting for.
        /// </summary>
        protected readonly XElement _element;

        /// <summary>
        ///     The object representing the XML Attributes for the node. Created on demand only.
        /// </summary>
        protected DynamicAttributes _attributes;

        private XmlNamespaceManager _namespaceManager;
        protected XNamespace _xmlns = null;

        static DynamicElement() {
            _xmlConverters = new Dictionary<Type, Func<string, object>> {
                {
                    typeof (Boolean), s => XmlConvert.ToBoolean(s)
                }, {
                    typeof (Byte), s => XmlConvert.ToByte(s)
                }, {
                    typeof (Char), s => XmlConvert.ToChar(s)
                }, {
                    typeof (DateTime), s => XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.RoundtripKind)
                }, {
                    typeof (DateTimeOffset), s => XmlConvert.ToDateTimeOffset(s)
                }, {
                    typeof (Decimal), s => XmlConvert.ToDecimal(s)
                }, {
                    typeof (Double), s => XmlConvert.ToDouble(s)
                }, {
                    typeof (Guid), s => XmlConvert.ToGuid(s)
                }, {
                    typeof (Int16), s => XmlConvert.ToInt16(s)
                }, {
                    typeof (Int32), s => XmlConvert.ToInt32(s)
                }, {
                    typeof (Int64), s => XmlConvert.ToInt64(s)
                }, {
                    typeof (SByte), s => XmlConvert.ToSByte(s)
                }, {
                    typeof (Single), s => XmlConvert.ToSingle(s)
                }, {
                    typeof (TimeSpan), s => XmlConvert.ToTimeSpan(s)
                }, {
                    typeof (UInt16), s => XmlConvert.ToUInt16(s)
                }, {
                    typeof (UInt32), s => XmlConvert.ToUInt32(s)
                }, {
                    typeof (UInt64), s => XmlConvert.ToUInt64(s)
                },
            };
        }

        public DynamicElement(XDocument document, XmlNamespaceManager namespaceManager) {
            if (document == null) {
                throw new ArgumentNullException("document");
            }
            _namespaceManager = namespaceManager;
            _element = document.Root;
            _xmlns = _element.Name.Namespace;
        }

        /// <summary>
        ///     Creates a DynamicXmlNode from an XElement
        /// </summary>
        /// <param name="element">An XElement node to use as the actual XML node for this DynamicXmlNode</param>
        public DynamicElement(XElement element, XmlNamespaceManager namespaceManager) {
            if (element == null) {
                throw new ArgumentNullException("element");
            }
            _namespaceManager = namespaceManager;
            _element = element;
            _xmlns = _element.Name.Namespace;
        }

        /// <summary>
        ///     Creates a DynamicXmlNode From an new XElement with the given name for the node
        /// </summary>
        /// <param name="elementName">The new XElement node name to use as the actual XML node for this DynamicXmlNode</param>
        public DynamicElement(string elementName, XmlNamespaceManager namespaceManager) {
            if (string.IsNullOrEmpty(elementName)) {
                throw new ArgumentNullException("elementName");
            }
            _namespaceManager = namespaceManager;
            _element = new XElement(elementName);
            _xmlns = _element.Name.Namespace;
        }

        public DynamicElement(string elementName, string defaultNamespace, XmlNamespaceManager namespaceManager) {
            if (string.IsNullOrEmpty(elementName)) {
                throw new ArgumentNullException("elementName");
            }
            if (string.IsNullOrEmpty(defaultNamespace)) {
                throw new ArgumentNullException("defaultNamespace");
            }
            _namespaceManager = namespaceManager;
            XNamespace ns = defaultNamespace;
            _element = new XElement(ns + elementName);
            _xmlns = _element.Name.Namespace;
        }

        /// <summary>
        ///     Returns the number of descendent nodes
        /// </summary>
        public int Count {
            get {
                return _element.DescendantNodes().Count();
            }
        }

        /// <summary>
        ///     Returns the actual XElement node
        /// </summary>
        public XElement Element {
            get {
                return _element;
            }
        }

        /// <summary>
        ///     Provides an indexer for the decendent child nodes.
        /// </summary>
        /// <param name="index">the index of the node requested</param>
        /// <returns></returns>
        public DynamicElement this[int index] {
            get {
                return new DynamicElement(_element.Descendants().ElementAt(index), _namespaceManager);
            }
        }

        public string LocalName {
            get {
                return _element.Name.LocalName;
            }
        }

        public IEnumerable<DynamicElement> this[string query] {
            get {
                return _element.XPathSelectElements(query, _namespaceManager).Select(each => new DynamicElement(each, _namespaceManager));
            }
        }

        public IEnumerable<DynamicElement> this[string query, params object[] args] {
            get {
                return _element.XPathSelectElements(string.Format(CultureInfo.CurrentCulture, query, args), _namespaceManager).Select(each => new DynamicElement(each, _namespaceManager));
            }
        }

        public DynamicAttributes Attributes {
            get {
                return _attributes ?? (_attributes = new DynamicAttributes(_element));
            }
        }

        public IEnumerator<DynamicElement> GetEnumerator() {
            return _element.Elements().Select(each => new DynamicElement(each, _namespaceManager)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        private static bool TryXmlConvert(string value, Type returnType, out object result) {
            if (returnType == typeof (string)) {
                result = value;
                return true;
            } else if (returnType.IsEnum) {
                // First try enum try parse:
                if (Enum.IsDefined(returnType, value)) {
                    result = Enum.Parse(returnType, value);
                    return true;
                }

                // We know we support all underlying types for enums, 
                // which are all numeric.
                var enumType = Enum.GetUnderlyingType(returnType);
                var rawValue = _xmlConverters[enumType].Invoke(value);

                result = Enum.ToObject(returnType, rawValue);
                return true;
            } else {
                Func<string, object> converter;
                if (_xmlConverters.TryGetValue(returnType, out converter)) {
                    result = converter(value);
                    return true;
                }
            }

            result = null;
            return false;
        }

        private XName ActualXName(string elementName) {
            return _xmlns == null ? elementName : _xmlns + elementName;
        }

        public void Save(string path) {
            Element.Save(path);
        }

        /// <summary>
        ///     Provides the implementation for operations that set member values. Classes derived from the DynamicObject class can
        ///     override this method to specify dynamic behavior for operations such as setting a value for a property.
        /// </summary>
        /// <param name="binder">
        ///     Provides information about the object that called the dynamic operation. The binder.Name property
        ///     provides the name of the member to which the value is being assigned. For example, for the statement
        ///     sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the DynamicObject
        ///     class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is
        ///     case-sensitive.
        /// </param>
        /// <param name="value">
        ///     The value to set to the member. For example, for sampleObject.SampleProperty = "Test", where
        ///     sampleObject is an instance of the class derived from the DynamicObject class, the value is "Test".
        /// </param>
        /// <returns>True, if successful</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value) {
            var setNode = _element.Element(ActualXName(binder.Name));

            if (value == null) {
                // delete the node? 
                if (setNode != null) {
                    setNode.Remove();
                }
                return true;
            }

            if (setNode != null) {
                setNode.SetValue(value);
            } else {
                _element.Add(value.GetType() == typeof (DynamicElement) ? new XElement(ActualXName(binder.Name)) : new XElement(ActualXName(binder.Name), value));
            }

            return true;
        }

        public bool Has(string name) {
            return (_element.Element(ActualXName(name)) != null);
        }

        /// <summary>
        ///     Provides the implementation for operations that get member values. Classes derived from the DynamicObject class can
        ///     override this method to specify dynamic behavior for operations such as getting a value for a property.
        ///     Provides a special case for XML Attributes. If the Member name requested is "Attributes", this will return an
        ///     DynamicXmlAttributes object
        /// </summary>
        /// <param name="binder">
        ///     Provides information about the object that called the dynamic operation. The binder.Name property
        ///     provides the name of the member on which the dynamic operation is performed. For example, for the
        ///     Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived
        ///     from the DynamicObject class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies
        ///     whether the member name is case-sensitive.
        /// </param>
        /// <param name="result">
        ///     The result of the get operation. For example, if the method is called for a property, you can
        ///     assign the property value to result.
        /// </param>
        /// <returns>True if successful</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            var getNode = _element.Element(ActualXName(binder.Name));

            if (getNode == null) {
                getNode = new XElement(ActualXName(binder.Name));
                _element.Add(getNode);
            }

            result = new DynamicElement(getNode, _namespaceManager);
            return true;
        }

        /// <summary>
        ///     Some sort of casting thing.
        /// </summary>
        /// <param name="binder">the member</param>
        /// <param name="result">the result</param>
        /// <returns>True if succesful</returns>
        public override bool TryConvert(ConvertBinder binder, out object result) {
            return TryXmlConvert(_element.Value, binder.ReturnType, out result);
        }

        /// <summary>
        ///     Passes thru function calls to the XElement node when there is no matching function in this class.
        /// </summary>
        /// <param name="binder">Method to call</param>
        /// <param name="args">Arguments</param>
        /// <param name="result">Result from function</param>
        /// <returns>True if successful</returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            var xmlType = typeof (XElement);
            try {
                result = xmlType.InvokeMember(binder.Name, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, _element, args);
                return true;
            } catch {
                result = null;
                return false;
            }
        }

        /// <summary>
        ///     Returns the XML Text for the node.
        /// </summary>
        /// <returns>The XML Text</returns>
        public override string ToString() {
            return _element.ToString();
        }

        /// <summary>
        ///     Adds a new child node
        /// </summary>
        /// <param name="name">the new node name</param>
        /// <returns>the DynamicXmlNode for the new node</returns>
        public DynamicElement Add(string name) {
            var e = new XElement(ActualXName(name));
            _element.Add(e);
            return new DynamicElement(e, _namespaceManager);
        }

        public DynamicElement Add(DynamicElement dynamicElement) {
            _element.Add(dynamicElement._element);
            return dynamicElement;
        }

        public DynamicElement Add(XElement element) {
            _element.Add(element);
            return new DynamicElement(element, _namespaceManager);
        }

        public DynamicElement Add(string name, string value) {
            var e = new XElement(ActualXName(name)) {
                Value = value
            };
            _element.Add(e);
            return new DynamicElement(e, _namespaceManager);
        }
    }
}