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

namespace Microsoft.OneGet.Test {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;
    using Utility.Collections;
    using Utility.Extensions;
    using Utility.Plugin;
    using Utility.PowerShell;
    using Xunit;

    public class MiscTests : MarshalByRefObject {
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
            Console.WriteLine("PS: {0}",ps);
            
            var ss = ps.FromProtectedString("garret");
            //Console.WriteLine("ss: {0}", ss);
            UseSecretData(ss);


            // var appd = AppDomain.CreateDomain("testdomain");
            // appd.SetData("password",s);
            // var newpwd = appd.GetData("password") as SecureString;
            // UseSecretData(newpwd);
        }

        internal void UseSecretData(SecureString secret) {
            IntPtr bstr = Marshal.SecureStringToBSTR(secret);
            try {
                // Use the bstr here
                string plainPass = Marshal.PtrToStringUni(bstr);
                Console.WriteLine("Plain {0}",plainPass);
            }
            finally {
                // Make sure that the clear text data is zeroed out
                Marshal.ZeroFreeBSTR(bstr);
            }
        }

        [Fact]
        public void TestIEnumerableByRef() {


            Console.WriteLine("STARTING");

            PluginDomain pd = new PluginDomain("testdomain"); 

            IEnumerable<string> x = new CancellableEnumerable<string>(new CancellationTokenSource(), new[] {"this", "is", "cool"} );

            pd.InvokeFunc(z => {
                Debug.WriteLine(string.Format("APPDOMAIN NAME :{0}",AppDomain.CurrentDomain.FriendlyName));
                foreach (var n in z) {
                    Debug.WriteLine(string.Format("Item :{0}", n));
                }
                
                return "";
            }, x);

            IEnumerable<Item> items = new CancellableEnumerable<Item>(new CancellationTokenSource(), new[] { new Item { A = "this" }, new Item { A = "aint" }, new Item { A = "nice" } });

            var r = pd.InvokeFunc(z => {
                Debug.WriteLine(string.Format("APPDOMAIN NAME :{0}", AppDomain.CurrentDomain.FriendlyName));
                foreach (var n in z) {
                    Debug.WriteLine(string.Format("Item :{0}", n.B));
                }

                IEnumerable<Item> others = new[] { new Item { A = "slop" }, new Item { A = "goop" }, new Item { A = "chop" } };

                return others;
            }, items);

            foreach (var n in r) {
                Debug.WriteLine(string.Format("Item :{0}", n.B));
            }

        }
    }

    public class Item : MarshalByRefObject {
        public string A {get; set;}
        public string B { get {
            return AppDomain.CurrentDomain.FriendlyName+"::" + A;
        } }
    }
}