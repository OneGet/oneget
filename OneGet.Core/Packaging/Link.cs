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
    using System.Xml.Linq;

    public class Link : Meta {
        internal Link(XElement element)
            : base(element) {
        }

        public string Artifact {
            get {
                return this[Iso19770_2.ArtifactAttribute.LocalName];
            }
        }

        public string HRef {
            get {
                return this[Iso19770_2.HRefAttribute.LocalName];
            }
        }

        public string Media {
            get {
                return this[Iso19770_2.MediaAttribute.LocalName];
            }
        }

        public string Ownership {
            get {
                return this[Iso19770_2.OwnershipAttribute.LocalName];
            }
        }

        public string Relationship {
            get {
                return this[Iso19770_2.RelationshipAttribute.LocalName];
            }
        }

        public string MediaType {
            get {
                return this[Iso19770_2.MediaTypeAttribute.LocalName];
            }
        }

        public string Use {
            get {
                return this[Iso19770_2.UseAttribute.LocalName];
            }
        }
    }
}