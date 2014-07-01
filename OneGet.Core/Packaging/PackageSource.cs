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
    using Providers.Package;

    /// <summary>
    ///     Represents a package source (repository)
    /// </summary>
    public class PackageSource : MarshalByRefObject {
        public string Name {get; internal set;}
        public string Location {get; internal set;}

        internal Dictionary<string,string> DetailsCollection = new Dictionary<string, string>();

        public string ProviderName {
            get {
                return Provider.Name;
            }
        }

        public PackageProvider Provider {get; internal set;}

        public bool IsTrusted {get; internal set;}

        public bool IsRegistered { get; internal set; }

        public bool IsValidated {get; internal set;}


        public override object InitializeLifetimeService() {
            return null;
        }

        public IReadOnlyDictionary<string, string> Details {
            get {
                return DetailsCollection;
            }
        }
    }
}