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

namespace Microsoft.OneGet.Platform {
    using System;

    /// <summary>
    ///     The mode/access/sharing flags that are passed to IPersistXXX.Load.
    /// </summary>
    [Flags]
    internal enum Stgm {
        /// <summary>
        ///     Create. Subsumes Create, CreateNew and OpenOrCreate.
        /// </summary>
        Create = 0x00001000,

        /// <summary>
        ///     Select the mode bit.
        /// </summary>
        Mode = 0x00001000, 

        /// <summary>
        ///     Read access.
        /// </summary>
        Read = 0x00000000,

        /// <summary>
        ///     Write access.
        /// </summary>
        Write = 0x00000001,

        /// <summary>
        ///     Read-write access.
        /// </summary>
        Readwrite = 0x00000002,

        /// <summary>
        ///     Flag to zero in on the access bits.
        /// </summary>
        Access = 0x00000003, 

        /// <summary>
        ///     ReadWrite
        /// </summary>
        ShareDenyNone = 0x00000040,

        /// <summary>
        ///     Write
        /// </summary>
        ShareDenyRead = 0x00000030,

        /// <summary>
        ///     Read
        /// </summary>
        ShareDenyWrite = 0x00000020,

        /// <summary>
        ///     None
        /// </summary>
        ShareExclusive = 0x00000010,

        /// <summary>
        ///     Flag to select the Share bits.
        /// </summary>
        Sharing = 0x00000070, 
    }
}