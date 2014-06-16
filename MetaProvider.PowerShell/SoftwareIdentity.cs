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

namespace Microsoft.OneGet.MetaProvider.PowerShell {
    using System.Collections;

    public class SoftwareIdentity : Yieldable {
        public SoftwareIdentity() {
        }

        public SoftwareIdentity(string fastPackageReference, string name, string version, string versionScheme, string source, string summary, string searchKey, string fullPath, string filename ) {
            FastPackageReference = fastPackageReference;
            Name = name;
            Version = version;
            VersionScheme = versionScheme;
            Source = source;
            Summary = summary;
            SearchKey = searchKey;
            FullPath = fullPath;
            Filename = filename;
        }

        public SoftwareIdentity(string fastPackageReference, string name, string version, string versionScheme, string source, string summary, string searchKey, string fullPath, string filename, Hashtable details)
            : this(fastPackageReference, name, version, versionScheme, source, summary, searchKey,fullPath, filename) {
            _details = details;
        }

        public string FastPackageReference {get; set;}
        public string Name {get; set;}
        public string Version {get; set;}
        public string VersionScheme {get; set;}
        public string Source {get; set;}
        public string Summary {get; set;}

        public string FullPath { get; set; }
        public string Filename { get; set; }

        public string SearchKey {get; set;}

        public override bool YieldResult(Request r) {
            return r.YieldPackage(FastPackageReference, Name, Version, VersionScheme, Summary, Source, SearchKey,FullPath, Filename) && YieldDetails(r);
        }
    }
}
