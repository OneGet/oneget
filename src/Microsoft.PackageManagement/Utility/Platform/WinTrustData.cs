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

namespace Microsoft.PackageManagement.Internal.Utility.Platform
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal class WinTrustData
    {
#if !CORECLR
        private readonly uint StructSize = (uint)Marshal.SizeOf(typeof(WinTrustData));
#else
        private UInt32 StructSize = (UInt32)Marshal.SizeOf<WinTrustData>();
#endif
        private readonly IntPtr PolicyCallbackData = IntPtr.Zero;
        private readonly IntPtr SIPClientData = IntPtr.Zero;
        private readonly uint UIChoice = 2;
        private readonly uint RevocationChecks = 0;
        private readonly uint UnionChoice = 1;
        private readonly IntPtr FileInfoPtr;
        private readonly uint StateAction = 0;
        private readonly IntPtr StateData = IntPtr.Zero;
        private readonly string URLReference;
        private readonly uint ProvFlags = 0x00000040; // check revocation chain.
        private readonly uint UIContext = 0;

        // constructor for silent WinTrustDataChoice.File check
        public WinTrustData(string filename)
        {
            WinTrustFileInfo wtfiData = new WinTrustFileInfo(filename);
#if !CORECLR
            FileInfoPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(WinTrustFileInfo)));
#else
            FileInfoPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf<WinTrustFileInfo>());
#endif
            Marshal.StructureToPtr(wtfiData, FileInfoPtr, false);
        }

        ~WinTrustData()
        {
            Marshal.FreeCoTaskMem(FileInfoPtr);
        }
    }
}