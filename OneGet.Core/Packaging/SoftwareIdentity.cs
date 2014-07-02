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
    using System.Collections.Generic;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using Extensions;

    /// <summary>
    ///     This class represents a package (retrieved from Find-SoftwareIdentity or Get-SoftwareIdentity)
    ///     Will eventually also represent a swidtag.
    ///     todo: Should this be serializable instead?
    /// </summary>
    public class SoftwareIdentity : MarshalByRefObject {
        public override object InitializeLifetimeService() {
            return null;
        }

        #region OneGet specific data
        internal string FastPackageReference {get; set;}

        public string ProviderName {get; internal set;}
        public string Source {get; internal set;}
        public string Status {get; internal set;}


        public string SearchKey {get; internal set;}
        
        public string FullPath {get; internal set;}
        public string PackageFilename {get; internal set;}

        // OneGet shortcut property -- Summary *should* be stored in SoftwareMetadata
        public string Summary {
            get {
                return Swid.Root().Elements(Iso19770_2.Meta).Select( each => each.Get(Iso19770_2.SummaryAttribute)).FirstOrDefault();
            }
            internal set {
                Swid.Root().Set(Iso19770_2.SummaryAttribute, value);
            }
        }

        #endregion

        #region ISO-19770-2-2014 metadata

        public string Name {
            get {
                return Swid.Root().Get(Iso19770_2.NameAttribute);
            }
            internal set {
                Swid.Root().Set(Iso19770_2.NameAttribute, value);
            }
        }

        public string Version {
            get {
                return Swid.Root().Get(Iso19770_2.VersionAttribute);
            }
            internal set {
                Swid.Root().Set(Iso19770_2.VersionAttribute, value);
            }
        }

        public string VersionScheme {
            get {
                return Swid.Root().Get(Iso19770_2.VersionSchemeAttribute);
            }
            internal set {
                Swid.Root().Set(Iso19770_2.VersionSchemeAttribute, value);
            }
        }
        public string TagVersion {
            get {
                return Swid.Root().Get(Iso19770_2.TagVersionAttribute);
            }
            internal set {
                Swid.Root().Set(Iso19770_2.TagVersionAttribute, value);
            }
        }

        public string TagId {
            get {
                return Swid.Root().Get(Iso19770_2.TagIdAttribute);
            }
            internal set {
                Swid.Root().Set(Iso19770_2.TagIdAttribute, value);
            }
        }

        public bool? IsPatch {
            get {
                return Swid.Root().Get(Iso19770_2.PatchAttribute).IsTrue();
            }
            internal set {
                Swid.Root().Set(Iso19770_2.PatchAttribute, value.ToString());
            }
        }

        public bool? IsSupplemental {
            get {
                return Swid.Root().Get(Iso19770_2.SupplementalAttribute).IsTrue();
            }
            internal set {
                Swid.Root().Set(Iso19770_2.SupplementalAttribute, value.ToString());
            }
        }

        public string AppliesToMedia {  get {
                return Swid.Root().Get(Iso19770_2.MediaAttribute);
            }
            internal set {
                Swid.Root().Set(Iso19770_2.MediaAttribute, value);
            }
        }

        public IEnumerable<string> this[string index] {
            get {
                return Meta.Where(each => each.ContainsKey(index)).Select(each => each[index]);
            }
        }

        internal IEnumerable<SoftwareMetadata> Meta {
            get {
                return Swid.Elements(Iso19770_2.Meta).Select(each => new SoftwareMetadata(each));
            }
        }

        public IEnumerable<Entity> Entities {
            get {
                return Swid.Elements(Iso19770_2.Entity).Select(each => new Entity(each));
            }
        }

        public IEnumerable<Link> Links {
            get {
                return Swid.Elements(Iso19770_2.Link).Select(each => new Link(each));
            }
        }


#if M2
        public Evidence Evidence {get; internal set;}

        public Payload Payload {get; internal set;}
#endif

        private XDocument _swidTag;
        public XDocument Swid {
            get {
                if (_swidTag == null) {
                    _swidTag = Iso19770_2.NewDocument;
                }
                return _swidTag;
            }
            internal set {
                
            }
        }

        #endregion

    }

    internal static class Iso19770_2 {
        internal static XNamespace Namespace = XNamespace.Get("http://standards.iso.org/iso/19770/-2/2014/schema.xsd");

        internal static XAttribute DefaultNamespace {
            get {
                return new XAttribute(XNamespace.Xmlns + "swidtag", Namespace);
            }
        }

        internal static XDocument NewDocument {
            get {
                return new XDocument(new XElement(SoftwareIdentity));
            }
        }

        internal static XName SoftwareIdentity = Namespace + "SoftwareIdentity";
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

        internal static XName Entity = Namespace + "Entity";
        internal static XName Link = Namespace + "Link";
        internal static XName Meta = Namespace + "Meta";


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

            var current = element.Get(attribute);


            if (current != null && value != current ) {
                Debug.WriteLine( string.Format("REPLACING value in swidtag attribute {0}: {1} for {2}", attribute.LocalName, current, value));
                throw new Exception("INVALID_SWIDTAG_ATTRIBUTE_VALUE_CHANGE");
            }

            element.SetAttributeValue(attribute, value);

            return element;
        }
    }
}