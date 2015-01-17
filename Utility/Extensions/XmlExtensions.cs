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
    using System.Collections.Generic;
    using System.Linq;
    using Xml;

    public static class XmlExtensions {
        /// <summary>
        ///     For a collection of Elements, returns the values of the same attribute in each (if it exists).
        ///     Does not return null or empty values.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetAttributes(this IEnumerable<DynamicElement> elements, string attributeName) {
            return elements.Select(each => each.Attributes[attributeName]).Where(each => !string.IsNullOrWhiteSpace(each));
        }

        /// <summary>
        ///     Gets the first non-null/not-empty attribute value for a given collection of elements
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static string GetAttribute(this IEnumerable<DynamicElement> elements, string attributeName) {
            return elements.Select(each => each.Attributes[attributeName]).FirstOrDefault(each => !string.IsNullOrWhiteSpace(each));
        }
    }
}