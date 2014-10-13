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
    using System.Linq;
    using System.Xml.Linq;

    internal static class Iso19770_2 {
        internal static XNamespace Namespace = XNamespace.Get("http://standards.iso.org/iso/19770/-2/2014/schema.xsd");

        internal static XName SoftwareIdentity = Namespace + "SoftwareIdentity";
        internal static XName Entity = Namespace + "Entity";
        internal static XName Link = Namespace + "Link";
        internal static XName Meta = Namespace + "Meta";

        /*
        internal static XName NameAttribute = Namespace + "name";
        internal static XName PatchAttribute = Namespace + "patch";
        internal static XName MediaAttribute = Namespace + "media";
        internal static XName SupplementalAttribute = Namespace + "supplemental";
        internal static XName TagVersionAttribute = Namespace + "tagVersion";
        internal static XName TagIdAttribute = Namespace + "tagId";
        internal static XName VersionAttribute = Namespace + "version";
        internal static XName VersionSchemeAttribute = Namespace + "versionScheme";

        internal static XName SummaryAttribute = Namespace + "summary";
        internal static XName DescriptionAttribute = Namespace + "description";


        internal static XName RegIdAttribute = Namespace + "regId";
        internal static XName RoleAttribute = Namespace + "role";
        internal static XName ThumbprintAttribute = Namespace + "thumbprint";


        internal static XName HRefAttribute = Namespace + "href";
        internal static XName RelationshipAttribute = Namespace + "rel";
        internal static XName MediaTypeAttribute = Namespace + "type";
        internal static XName OwnershipAttribute = Namespace + "ownership";
        internal static XName UseAttribute = Namespace + "use";
        internal static XName ArtifactAttribute = Namespace + "artifact";
*/

        internal static XName NameAttribute = "name";
        internal static XName PatchAttribute = "patch";
        internal static XName MediaAttribute = "media";
        internal static XName SupplementalAttribute = "supplemental";
        internal static XName TagVersionAttribute = "tagVersion";
        internal static XName TagIdAttribute = "tagId";
        internal static XName VersionAttribute = "version";
        internal static XName VersionSchemeAttribute = "versionScheme";

        internal static XName SummaryAttribute = "summary";
        internal static XName DescriptionAttribute = "description";

        internal static XName RegIdAttribute = "regId";
        internal static XName RoleAttribute = "role";
        internal static XName ThumbprintAttribute = "thumbprint";

        internal static XName HRefAttribute = "href";
        internal static XName RelationshipAttribute = "rel";
        internal static XName MediaTypeAttribute = "type";
        internal static XName OwnershipAttribute = "ownership";
        internal static XName UseAttribute = "use";
        internal static XName ArtifactAttribute = "artifact";

        internal static XAttribute DefaultNamespace {
            get {
                return new XAttribute(XNamespace.Xmlns + "swidtag", Namespace);
            }
        }

        internal static XDocument NewDocument {
            get {
                return new XDocument(new XDeclaration("1.0", "UTF-8", "yes"),
                    new XElement(SoftwareIdentity));
            }
        }

        internal static XElement Root(this XDocument xmlDocument) {
            return xmlDocument.Elements(SoftwareIdentity).FirstOrDefault();
        }

        internal static string Get(this XElement element, XName attribute) {
            if (element == null) {
                return null;
            }
            var a = element.Attribute(attribute);
            return a == null ? null : a.Value;
        }

        internal static XElement Set(this XElement element, XName attribute, string value) {
            if (element == null) {
                return null;
            }

            if (string.IsNullOrEmpty(value)) {
                return element;
            }

            var current = element.Get(attribute);

            if (current != null && value != current) {
                // Debug.WriteLine( string.Format(CultureInfo.CurrentCulture,"REPLACING value in swidtag attribute {0}: {1} for {2}", attribute.LocalName, current, value));
                throw new Exception("INVALID_SWIDTAG_ATTRIBUTE_VALUE_CHANGE");
            }

            element.SetAttributeValue(attribute, value);

            return element;
        }
    }
}