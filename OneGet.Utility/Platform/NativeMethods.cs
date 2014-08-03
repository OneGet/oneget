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

    internal enum WinVerifyTrustResult : uint {
        Success = 0,
        ProviderUnknown = 0x800b0001, // The trust provider is not recognized on this system
        ActionUnknown = 0x800b0002, // The trust provider does not support the specified action
        SubjectFormUnknown = 0x800b0003, // The trust provider does not support the form specified for the subject
        SubjectNotTrusted = 0x800b0004, // The subject failed the specified verification action
        UntrustedRootCert = 0x800B0109 //A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider. 
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal class WinTrustFileInfo {
        private UInt32 StructSize = (UInt32)Marshal.SizeOf(typeof(WinTrustFileInfo));
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal class WinTrustData {
        private UInt32 StructSize = (UInt32)Marshal.SizeOf(typeof(WinTrustData));
        private IntPtr PolicyCallbackData = IntPtr.Zero;
        private IntPtr SIPClientData = IntPtr.Zero;
        private uint UIChoice = 2;
        private uint RevocationChecks = 0;
        private uint UnionChoice = 1;
        private IntPtr FileInfoPtr;
        private uint StateAction = 0;
        private IntPtr StateData = IntPtr.Zero;
        private String URLReference;
        private uint ProvFlags = 0x00000040; // check revocation chain.
        private uint UIContext = 0;

        // constructor for silent WinTrustDataChoice.File check
        public WinTrustData(String filename) {
            var wtfiData = new WinTrustFileInfo(filename);
            FileInfoPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(WinTrustFileInfo)));
            Marshal.StructureToPtr(wtfiData, FileInfoPtr, false);
        }

        ~WinTrustData() {
            Marshal.FreeCoTaskMem(FileInfoPtr);
        }
    }

    public static class NativeMethods {
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
        public static extern void OutputDebugString(string debugMessageText);


        [DllImport("wintrust.dll", ExactSpelling = true, SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern WinVerifyTrustResult WinVerifyTrust(IntPtr hwnd, [MarshalAs(UnmanagedType.LPStruct)] Guid pgActionID, WinTrustData pWVTData);

    }
}