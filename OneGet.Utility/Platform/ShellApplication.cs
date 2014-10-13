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
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class ShellApplication {
        private static readonly string _pin;
        private static readonly string _unpin;

        static ShellApplication() {
            var instance = IntPtr.Zero;
            try {
                var buffer = new StringBuilder(0x100);
                instance = NativeMethods.LoadLibrary("shell32.dll");
                if (NativeMethods.LoadString(instance, 0x150a, buffer, buffer.Capacity) > 0) {
                    _pin = buffer.ToString();
                }
                if (NativeMethods.LoadString(instance, 0x150b, buffer, buffer.Capacity) > 0) {
                    _unpin = buffer.ToString();
                }
            } catch {
                // whoa, something went wrong. Let it be.
            }
            if (instance != IntPtr.Zero) {
                NativeMethods.FreeLibrary(instance);
            }
        }

        public static void Pin(string lnkPath) {
            if (_pin != null) {
                DoVerbOnPath(lnkPath, _pin);
            }
        }

        public static void Unpin(string lnkPath) {
            if (_unpin != null) {
                DoVerbOnPath(lnkPath, _unpin);
            }
        }

        private static void DoVerbOnPath(string lnkPath, string vName) {
            if (string.IsNullOrEmpty(lnkPath)) {
                throw new ArgumentNullException("lnkPath");
            }
            if (string.IsNullOrEmpty(vName)) {
                throw new ArgumentNullException("vName");
            }
            if (!File.Exists(lnkPath)) {
                throw new FileNotFoundException("Target Path Not Found", lnkPath);
            }

            dynamic shell = null;

            try {
                shell = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));

                var link = shell.NameSpace(Path.GetDirectoryName(lnkPath)).ParseName(Path.GetFileName(lnkPath));

                var v = (from dynamic verb in ((IEnumerable)(link.Verbs))
                    where ((string)verb.Name).Equals(vName, StringComparison.OrdinalIgnoreCase)
                    select verb).FirstOrDefault();
                if (v != null) {
                    v.DoIt();
                }
            } catch {
            }
            if (shell != null) {
                Marshal.ReleaseComObject(shell);
            }
        }
    }
}