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

namespace Microsoft.OneGet.Utility.Platform {
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class NativeMethods {
        [DllImport("kernel32.dll", EntryPoint = "MoveFileEx", CharSet = CharSet.Unicode)]
        internal static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);

        [DllImport("shell32.dll")]
        internal static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder pszPath, KnownFolder nFolder, bool create = false);

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessageTimeout(Int32 hwnd, Int32 msg, Int32 wparam, [MarshalAs(UnmanagedType.LPStr)] string lparam, Int32 fuFlags,
            Int32 timeout, IntPtr result);

        [DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int WNetGetConnection([MarshalAs(UnmanagedType.LPTStr)] string localName, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder remoteName, ref int length);

        [DllImport("kernel32")]
        internal static extern IntPtr LoadLibrary(string fileName);

        [DllImport("user32")]
        internal static extern int LoadString(IntPtr instance, uint stringId, StringBuilder buffer, int bufferSize);

        [DllImport("kernel32")]
        internal static extern bool FreeLibrary(IntPtr Instance);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        internal static extern void OutputDebugString(string debugMessageText);

    }
}