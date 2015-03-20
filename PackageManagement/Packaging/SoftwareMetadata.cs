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

namespace Microsoft.PackageManagement.Packaging {
    using System;
    using System.Xml.Linq;
    using Utility.Extensions;

    public class SoftwareMetadata : Meta {
        internal SoftwareMetadata(XElement element) : base(element) {
            if (element.Name != Iso19770_2.Meta) {
                throw new ArgumentException("Element is not of type 'SoftwareMetadata'", "element");
            }
        }

        internal SoftwareMetadata()
            : base(new XElement(Iso19770_2.Meta)) {
        }

        public string ActivationStatus {
            get {
                return GetAttribute(Iso19770_2.ActivationStatusAttribute);
            }
            internal set {
                AddAttribute(Iso19770_2.ActivationStatusAttribute, value);
            }
        }

        public string ChannelType {
            get {
                return GetAttribute(Iso19770_2.ChannelTypeAttribute);
            }
            internal set {
                AddAttribute(Iso19770_2.ChannelTypeAttribute, value);
            }
        }

        public string Description {
            get {
                return GetAttribute(Iso19770_2.DescriptionAttribute);
            }
            internal set {
                AddAttribute(Iso19770_2.DescriptionAttribute, value);
            }
        }

        public string ColloquialVersion {
            get {
                return GetAttribute(Iso19770_2.ColloquialVersionAttribute);
            }
            internal set {
                AddAttribute(Iso19770_2.ColloquialVersionAttribute, value);
            }
        }

        public string Edition {
            get {
                return GetAttribute(Iso19770_2.EditionAttribute);
            }
            internal set {
                AddAttribute(Iso19770_2.EditionAttribute, value);
            }
        }

        public string EntitlementKey {
            get {
                return GetAttribute(Iso19770_2.EntitlementKeyAttribute);
            }
            internal set {
                AddAttribute(Iso19770_2.EntitlementKeyAttribute, value);
            }
        }

        public string Generator {
            get {
                return GetAttribute(Iso19770_2.GeneratorAttribute);
            }
            internal set {
                AddAttribute(Iso19770_2.GeneratorAttribute, value);
            }
        }

        public string PersistentId {
            get {
                return GetAttribute(Iso19770_2.PersistentIdAttribute);
            }
            internal set {
                AddAttribute(Iso19770_2.PersistentIdAttribute, value);
            }
        }

        public string Product {
            get {
                return GetAttribute(Iso19770_2.ProductAttribute);
            }
            internal set {
                AddAttribute(Iso19770_2.ProductAttribute, value);
            }
        }

        public string ProductFamily {
            get {
                return GetAttribute(Iso19770_2.ProductFamilyAttribute);
            }
            internal set {
                AddAttribute(Iso19770_2.ProductFamilyAttribute, value);
            }
        }

        public string Revision {
            get {
                return GetAttribute(Iso19770_2.RevisionAttribute);
            }
            internal set {
                AddAttribute(Iso19770_2.RevisionAttribute, value);
            }
        }

        public string UnspscCode {
            get {
                return GetAttribute(Iso19770_2.UnspscCodeAttribute);
            }
            internal set {
                AddAttribute(Iso19770_2.UnspscCodeAttribute, value);
            }
        }

        public string UnspscVersion {
            get {
                return GetAttribute(Iso19770_2.UnspscVersionAttribute);
            }
            internal set {
                AddAttribute(Iso19770_2.UnspscVersionAttribute, value);
            }
        }

        public bool? EntitlementDataRequired {
            get {
                return GetAttribute(Iso19770_2.EntitlementDataRequiredAttribute).IsTruePreserveNull();
            }
            internal set {
                if (value != null) {
                    AddAttribute(Iso19770_2.EntitlementDataRequiredAttribute, value.ToString());
                }
            }
        }

        public override string ToString() {
            return Attributes.ToString();
        }
    }
}
