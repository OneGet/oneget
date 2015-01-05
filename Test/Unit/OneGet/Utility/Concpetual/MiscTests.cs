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

namespace Microsoft.OneGet.Test.Utility.Concpetual {
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using OneGet.Utility.Extensions;
    using Xunit;
    using Console = Support.Console;

    public class MiscTests {
        [Fact]
        public void TestSecureString() {
            var password = "applesauce";
            var p = password.ToCharArray();
            var s = new SecureString();
            foreach (char c in p) {
                s.AppendChar(c);
            }
            UseSecretData(s);

            var ps = s.ToProtectedString("garrett");
            Console.WriteLine("PS: {0}", ps);

            var ss = ps.FromProtectedString("garret");
            //Console.WriteLine("ss: {0}", ss);
            UseSecretData(ss);
        }

        internal void UseSecretData(SecureString secret) {
            IntPtr bstr = Marshal.SecureStringToBSTR(secret);
            try {
                // Use the bstr here
                string plainPass = Marshal.PtrToStringUni(bstr);
                Console.WriteLine("Plain {0}", plainPass);
            } finally {
                // Make sure that the clear text data is zeroed out
                Marshal.ZeroFreeBSTR(bstr);
            }
        }

    }

    public class Item {
        public string A {get; set;}

        public string B {
            get {
                return AppDomain.CurrentDomain.FriendlyName + "::" + A;
            }
        }
    }
}