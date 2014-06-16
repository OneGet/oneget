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

    /// <summary>
    ///     This class represents a package (retrieved from Find-SoftwareIdentity or Get-SoftwareIdentity)
    ///     Will eventually also represent a swidtag.
    ///     todo: Should this be serializable instead?
    /// </summary>
    public class SoftwareIdentity : MarshalByRefObject {
        public override object InitializeLifetimeService() {
            return null;
        }

        internal string FastPackageReference {get; set;}

        public string Name {get; internal set;}
        public string Version {get; internal set;}
        public string VersionScheme {get; internal set;}
        public string Summary {get; internal set;}

        public string ProviderName {get; internal set;}
        public string Source {get; internal set;}
        public string Status {get; internal set;}


        public string SearchKey {get; internal set;}
        
        public string FullPath {get; internal set;}
        public string PackageFilename {get; internal set;}


#if AFTER_CTP
        public string Description { get; internal set; }


        public IEnumerable<Entity> Entities {get; internal set;}
        public IEnumerable<Link> Links {get; internal set;}
        internal IEnumerable<Meta> Meta { get; set; }



        
        public bool IsDelta {get; internal set;}

        public bool IsSupplemental {get; internal set;}

        public string AppliesToMedia {get; internal set;}

        public string TagVersion {get; internal set;}

        public string UniqueId {get; internal set;}

        public Evidence Evidence {get; internal set;}

        public Payload Payload {get; internal set;}
 

        public string SwidTag {get; internal set;}
        public InstallationOptions InstallationOptions {get; internal set;}



#endif
    }
}