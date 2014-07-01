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
    using System.Xml;
    using System.Xml.Linq;

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
        public string Summary { get; internal set; }

        #endregion

        #region ISO-19770-2-2014 metadata

        public string Name { get; internal set; }
        public string Version { get; internal set; }
        public string VersionScheme { get; internal set; }
        public string TagVersion { get; internal set; }

#if M2
        public string TagId { get; internal set; }

        public bool? IsPatch { get; internal set; }

        public bool? IsSupplemental { get; internal set; }

        public string AppliesToMedia { get; internal set; }
#endif


        internal IEnumerable<SoftwareMetadata> Meta {
            get {
                return null;
            }
        }

        public IEnumerable<Entity> Entities {
            get {
                return null;
            }
        }

        public IEnumerable<Link> Links {
            get {
                return null;
            }
        }


#if M2
        public Evidence Evidence {get; internal set;}

        public Payload Payload {get; internal set;}
#endif

        private XDocument _swidTag;
        public XDocument SwidTag {
            get {
                var x = new XDocument();
                return null;

            }
            internal set {
                
            }
        }


        #endregion

    }
}