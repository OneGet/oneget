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

namespace Microsoft.PackageManagement.Packaging
{
    using Implementation;
    using System.Collections.Generic;

    /// <summary>
    ///     Represents a package source (repository)
    /// </summary>
    public class PackageSource
    {
        internal Dictionary<string, string> DetailsCollection = new Dictionary<string, string>();
        public string Name { get; internal set; }
        public string Location { get; internal set; }

        public string Source => Name ?? Location;

        // todo: make this dictionary read only! (.net 4.0 doesn't have that!)

        public string ProviderName => Provider.ProviderName;

        public PackageProvider Provider { get; internal set; }
        public bool IsTrusted { get; internal set; }
        public bool IsRegistered { get; internal set; }
        public bool IsValidated { get; internal set; }

        public IDictionary<string, string> Details => DetailsCollection;

        public override bool Equals(object obj)
        {
            if (!(obj is PackageSource packageSource))
            {
                return false;
            }
            else
            {
                return ((Name.Equals(packageSource.Name) && Location.Equals(packageSource.Location)));
            }
        }

        public override int GetHashCode()
        {
            return ((Name ?? string.Empty) + (Location ?? string.Empty)).GetHashCode();
        }
    }
}