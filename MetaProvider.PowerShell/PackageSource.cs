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

    public class PackageSource : Yieldable {
        public PackageSource(string name, string location, bool isTrusted, bool isRegistered) {
            Name = name;
            Location = location;
            IsTrusted = isTrusted;
            IsRegistered = isRegistered;
        }

        public PackageSource(string name, string location, bool isTrusted, bool isRegistered, Hashtable details) : this(name, location, isTrusted, isRegistered) {
            _details = details;
        }

        public string Name {get; set;}
        public string Location {get; set;}
        public bool IsTrusted {get; set;}
        public bool IsRegistered { get; set; }

        public override bool YieldResult(Request r) {
            return r.YieldPackageSource(Name, Location, IsTrusted, IsRegistered) && YieldDetails(r);
        }
    }
}