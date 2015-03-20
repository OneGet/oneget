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

namespace Microsoft.PackageManagement.Native {
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using Api;
    using RGiesecke.DllExport;

    public class PackageManagement {


        // [return: MarshalAs(UnmanagedType.LPWStr)]

        public delegate int dlg( returnString result);

        public delegate int returnString([MarshalAs(UnmanagedType.LPWStr)]string s);

        public delegate IntPtr func_str();

        [DllExport("Initialize", CallingConvention.Cdecl)]
        public static int Initialize(dlg funcDelegate) {
            string resultValue = null;
            var result = new returnString((s) => {
                resultValue = s;
                return 120;
            });

            Console.WriteLine( funcDelegate( result ) );
            Console.WriteLine( resultValue);
            return 150;
        }


        [DllExport("Two", CallingConvention.Cdecl)]
        public static int Two(func_str funcDelegate) {
            var s= Marshal.PtrToStringUni(funcDelegate());
            Console.WriteLine(s);
            return 50;
        }
    }
}
