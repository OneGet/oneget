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

using Microsoft.PackageManagement.Packaging;
using System;
using System.Collections.Generic;

namespace Microsoft.PackageManagement.Internal.Packaging
{
    /// <summary>
    /// 2 Swids will be equal if their names and versions are the same
    /// </summary>
    public class SoftwareIdentityNameVersionComparer : IEqualityComparer<SoftwareIdentity>
    {
        private static SoftwareIdentityVersionComparer VersionComparer = new SoftwareIdentityVersionComparer();

        public bool Equals(SoftwareIdentity swidOne, SoftwareIdentity swidTwo)
        {
            // True if both are null
            if (swidOne == null && swidTwo == null)
            {
                return true;
            }

            // False if 1 is null and the other is not
            if (swidOne == null || swidTwo == null)
            {
                return false;
            }

            // true if name is same and version is same
            return string.Equals(swidOne.Name, swidTwo.Name, StringComparison.OrdinalIgnoreCase) && (VersionComparer.Compare(swidOne, swidTwo) == 0);
        }

        public int GetHashCode(SoftwareIdentity obj)
        {
            if (obj == null)
            {
                return 0;
            }

            return (string.IsNullOrWhiteSpace(obj.Name) ? string.Empty : obj.Name).GetHashCode() * 31
                + (string.IsNullOrWhiteSpace(obj.Version) ? string.Empty : obj.Version).GetHashCode();
        }
    }
}