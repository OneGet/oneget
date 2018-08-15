﻿//
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

namespace Microsoft.PackageManagement.Internal.Utility.Platform
{
    using System.Security.Principal;

    internal class AdminPrivilege
    {
        /// <summary>
        ///     The function checks whether the current process is run as administrator. In other words, it dictates whether the
        ///     primary access token of the process belongs to user account that is a member of the local Administrators group and
        ///     it is elevated.
        /// </summary>
        /// <returns>
        ///     Returns true if the primary access token of the process belongs to user account that is a member of the local
        ///     Administrators group and it is elevated. Returns false if the token does not.
        /// </returns>
        public static bool IsElevated
        {
            get
            {
                if (!OSInformation.IsWindows)
                {
                    //it is not possible to detect whether a user is an admin/sudo or not on Linux
                    //try out first and will log error later.
                    return true;
                    //return OSInformation.IsSudoUser;
                }
                else
                {
                    WindowsIdentity id = WindowsIdentity.GetCurrent();
                    WindowsPrincipal principal = new WindowsPrincipal(id);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
        }
    }
}