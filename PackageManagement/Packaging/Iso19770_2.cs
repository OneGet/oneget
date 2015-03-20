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
    using System.Xml.Linq;
    using Utility.Extensions;

    internal static class Iso19770_2 {
        internal static XNamespace Namespace = XNamespace.Get("http://standards.iso.org/iso/19770/-2/2015/schema.xsd");
        internal static XNamespace XmlNamespace  = XNamespace.Get("http://www.w3.org/XML/1998/namespace" );
        internal static XNamespace XmlDsigNamespace = XNamespace.Get("http://www.w3.org/2000/09/xmldsig#" );

        internal static readonly XName SoftwareIdentity = Namespace + "SoftwareIdentity";
        internal static readonly XName Entity = Namespace + "Entity";
        internal static readonly XName Link = Namespace + "Link";
        internal static readonly XName Meta = Namespace + "Meta";
        internal static readonly XName Evidence = Namespace + "Evidence";
        internal static readonly XName Payload = Namespace + "Payload";
                        
        internal static readonly XName Directory = Namespace + "Directory";
        internal static readonly XName File = Namespace + "File";
        internal static readonly XName Process = Namespace + "Process";
        internal static readonly XName Resource = Namespace + "Resource";

        internal static readonly XName[] MetaElements = {
            Meta, Directory, File, Process, Resource
        };
      
        // ISO 19770-2/2015 attributes
        internal static readonly XName XmlLang = XmlNamespace + "lang";
                        
        internal static readonly XName NameAttribute = "name";
        internal static readonly XName PatchAttribute = "patch";
        internal static readonly XName MediaAttribute = "media";
        internal static readonly XName SupplementalAttribute = "supplemental";
        internal static readonly XName TagVersionAttribute = "tagVersion";
        internal static readonly XName TagIdAttribute = "tagId";
        internal static readonly XName VersionAttribute = "version";
        internal static readonly XName VersionSchemeAttribute = "versionScheme";
        internal static readonly XName CorpusAttribute = "corpus";
                        
        internal static readonly XName SummaryAttribute = "summary";
        internal static readonly XName DescriptionAttribute = "description";
        internal static readonly XName ActivationStatusAttribute = "activationStatus";
        internal static readonly XName ChannelTypeAttribute = "channelType";
        internal static readonly XName ColloquialVersionAttribute = "colloquialVersion";
        internal static readonly XName EditionAttribute = "edition";
        internal static readonly XName EntitlementDataRequiredAttribute = "entitlementDataRequired";
        internal static readonly XName EntitlementKeyAttribute = "entitlementKey";
        internal static readonly XName GeneratorAttribute = "generator";
        internal static readonly XName PersistentIdAttribute = "persistentId";
        internal static readonly XName ProductAttribute = "product";
        internal static readonly XName ProductFamilyAttribute = "productFamily";
        internal static readonly XName RevisionAttribute = "revision";
        internal static readonly XName UnspscCodeAttribute = "unspscCode";
        internal static readonly XName UnspscVersionAttribute = "unspscVersion";
                        
        internal static readonly XName RegIdAttribute = "regId";
        internal static readonly XName RoleAttribute = "role";
        internal static readonly XName ThumbprintAttribute = "thumbprint";
                        
        internal static readonly XName HRefAttribute = "href";
        internal static readonly XName RelationshipAttribute = "rel";
        internal static readonly XName MediaTypeAttribute = "type";
        internal static readonly XName OwnershipAttribute = "ownership";
        internal static readonly XName UseAttribute = "use";
        internal static readonly XName ArtifactAttribute = "artifact";
                        
        internal static readonly XName TypeAttribute = "type";
                        
        internal static readonly XName KeyAttribute = "key";
        internal static readonly XName RootAttribute = "root";
        internal static readonly XName LocationAttribute = "location";
                        
        internal static readonly XName SizeAttribute = "size";
        internal static readonly XName PidAttribute = "pid";
                        
        internal static readonly XName DateAttribute = "date";
        internal static readonly XName DeviceIdAttribute = "deviceId";

        internal static XAttribute SwidtagNamespace {
            get {
                return new XAttribute(XNamespace.Xmlns + "swid", Namespace);
            }
        }
        internal static class Relationship {
            internal const string Requires = "requires";
            internal const string InstallationMedia = "installationmedia";
            internal const string Component = "component";
            internal const string Supplemental = "supplemental";
            internal const string Parent = "parent";
            internal const string Ancestor = "ancestor";
        }

        internal static class Role {
            internal const string Aggregator = "aggregator";
            internal const string Distributor = "distributor";
            internal const string Licensor = "licensor";
            internal const string SoftwareCreator = "softwareCreator";
            internal const string Author = "author";
            internal const string Contributor = "contributor";
            internal const string Publisher = "publisher";
            internal const string TagCreator = "tagCreator";
        }

        internal static class Use {
            internal const string Required = "required";
            internal const string Recommended = "recommended";
            internal const string Optional = "optional";
        }

        internal static class VersionScheme {
            internal const string Alphanumeric = "alphanumeric";
            internal const string Decimal = "decimal";
            internal const string MultipartNumeric = "multipartnumeric";
            internal const string MultipartNumericPlusSuffix = "multipartnumeric+suffix";
            internal const string SemVer = "semver";
            internal const string Unknown = "unknown";
        }

        internal static class Ownership {
            internal const string Abandon = "abandon";
            internal const string Private = "private";
            internal const string Shared = "shared";
        }

        /// <summary>
        /// Gets the attribute value for a given element.
        /// </summary>
        /// <param name="element">the element that possesses the attribute</param>
        /// <param name="attribute">the attribute to find</param>
        /// <returns>the string value of the element. Returns null if the element or attribute does not exist.</returns>
        internal static string GetAttribute(this XElement element, XName attribute) {
            if (element == null || attribute == null || string.IsNullOrWhiteSpace(attribute.ToString()) ) {
                return null;
            }
            var a = element.Attribute(attribute);
            return a == null ? null : a.Value;
        }

        /// <summary>
        /// Adds a new attribute to the element
        /// 
        /// Does not permit modification of an existing attribute.
        /// 
        /// Does not add empty or null attributes or values.
        /// </summary>
        /// <param name="element">The element to add the attribute to</param>
        /// <param name="attribute">The attribute to add</param>
        /// <param name="value">the value of the attribute to add</param>
        /// <returns>The element passed in. (Permits fluent usage)</returns>
        internal static XElement AddAttribute(this XElement element, XName attribute, string value) {
            if (element == null) {
                return null;
            }
            
            // we quietly ignore attempts to add empty data or attributes.
            if (string.IsNullOrWhiteSpace(value) || attribute == null || string.IsNullOrWhiteSpace(attribute.ToString())) {
                return element;
            }

            // Swidtag attributes can be added but not changed -- if it already exists, that's not permitted.
            var current = element.GetAttribute(attribute);
            if (!string.IsNullOrWhiteSpace(current) ){
                if (value != current) {
                    throw new Exception("Attempt to change Attribute '{0}' present in element '{1}'".format(attribute.LocalName, element.Name.LocalName));
                }

                // if the value was set to that already, don't worry about it.
                return element;
            }

            element.SetAttributeValue(attribute, value);

            return element;
        }
    }
}