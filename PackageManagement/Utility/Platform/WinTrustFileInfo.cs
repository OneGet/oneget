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

namespace Microsoft.PackageManagement.Utility.Platform {
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal class WinTrustFileInfo {
        private UInt32 StructSize = (UInt32)Marshal.SizeOf(typeof (WinTrustFileInfo));
        private IntPtr FilePath; // required, file name to be verified
        private IntPtr hFile = IntPtr.Zero; // optional, open handle to FilePath
        private IntPtr pgKnownSubject = IntPtr.Zero; // optional, subject type if it is known

        public WinTrustFileInfo(String filePath) {
            FilePath = Marshal.StringToCoTaskMemAuto(filePath);
        }

        ~WinTrustFileInfo() {
            Marshal.FreeCoTaskMem(FilePath);
        }
    }
}