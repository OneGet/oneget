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
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Runtime.Hosting;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using PowerShell.OneGet.CmdLets;
    using Utility.Extensions;
    using Utility.Plugin;
    using Xunit;

    public class DynamicTest {
  

        [Fact]
        public void TestAssumptionAboutParams() {
            var a = ItemsViaParams("very", "happy", "person");
            var b = ItemsWithoutParams(new[] {
                "very", "happy", "person"
            });
            var c = ItemsViaParams(a);

            Assert.True(a.SequenceEqual(b));
            Assert.True(a.SequenceEqual(c));
            Assert.True(c.SequenceEqual(b));

            var items = Flatten(a, "more").ToArray();
            

            foreach (var i in items) {
                Console.WriteLine(i);
            }

            var q = new int[] {
                1, 2, 3
            };

            IEnumerable twoItems = a.Take(2).Concat(c).ConcatSingleItem(new string[] {"Help",null, "me"}).ConcatSingleItem(q);
            
            foreach (var i in Flatten(twoItems)) {
                Console.WriteLine(i);
            }

        }


        private IEnumerable<object> Flatten(IEnumerable<object> items) {
            if (items == null) {
                yield break;
            }
            foreach (var item in items) {
                if (item is object[] || item is IEnumerable<object>) {
                    foreach (var inner in Flatten(item as IEnumerable<object>)) {
                        if (inner != null) {
                            yield return inner;
                        }
                    }
                    continue;
                }
                yield return item;
            }
        }

        private IEnumerable<object> Flatten(params object[] items) {
            return Flatten(items as IEnumerable<object>);
        }

        internal static object[] ItemsViaParams(params object[] items) {
            
            return items;
        }
        internal static object[] ItemsWithoutParams(object[] items) {
            return items;
        }

        [Fact]
        public void TestWhichWayWorksForAggregate() {
            var a = new string[] {
                "one"
            };
            var b = new string[] {
                "one", "two"
            };
            var c = new string[] {
                "one", "two", "three"
            };

            var aa = a.Aggregate((current, each) => current + "," + each);
            var bb = b.Aggregate((current, each) => current + "," + each);
            var cc = c.Aggregate((current, each) => current + "," + each);

            Console.WriteLine(aa);
            Console.WriteLine(bb);
            Console.WriteLine(cc);

            Console.WriteLine(GetType().GetMethod("SampleMethod").ToSignatureString());
        }

        public void SampleMethod(int a, int b, string c, Func<object, Int32> d) {
            
        }

        [Fact]
        public void TestInterfaceQuestion() {
            var foobar = new FooBar();
            IOne iOne = foobar;
            ITwo iTwo = foobar;

            // Pity:
            // IOneAndTwo iBoth = foobar;

        }
        public interface IOne {
            void Foo();
        }

        public interface ITwo {
            void Bar();
        }

        public interface IOneAndTwo : IOne, ITwo {
            
        }

        public class FooBar : IOne, ITwo {
            public void Foo() {
                
            }

            public void Bar() {
                
            }
        }
    }


    public class PackageProviderProxy {
        public readonly Func<string, string, bool > AddPackageSource;
        public readonly AddPackageSourceDelegate AddPackageSource2;
        public readonly AddPackageSourceDelegate AddPackageSource3;
        public readonly AddPackageSourceDelegate AddPackageSource4;

        public delegate bool AddPackageSourceDelegate(string name, string location);

        public PackageProviderProxy(object instance) {

#if OLD_SAD_WAY
            var createDelegate = instance.CreateProxiedDelegate<CreateDelegate>("CreateDelegate" );

            instance.As<CreateDelegate>();

            AddPackageSource = instance.CreateProxiedDelegate<Func<string, string, bool>>("AddPackageSource", createDelegate);


            AddPackageSource2 = instance.CreateProxiedDelegate<AddPackageSourceDelegate>("AddPackageSource", createDelegate);

            AddPackageSource3 = instance.CreateProxiedDelegate<AddPackageSourceDelegate>("AddPackageSourceFunc", createDelegate);
            AddPackageSource4 = instance.CreateProxiedDelegate<AddPackageSourceDelegate>("AddPackageSource4", createDelegate);
#endif
        }

        public PackageProviderProxy(Type type): this(Activator.CreateInstance(type)) {
        }

     
        

    }

    public class PPInstance {
        public bool AddPackageSource(string name, string location) {
            Console.WriteLine("name: {0}, location: {1}",name, location);
            return true;
        }

        public Func<string, string, bool> AddPackageSourceFunc {
            get {
                return (s, s1) => {
                    Console.WriteLine("Nothing to see here.");
                    return true;
                };
            }
        }

        public Delegate CreateDelegate(string method, string[] parameterNames, Type[] parameterTypes, Type returnType) {
            if (method == "AddPackageSource4") {
                return new Func<string, string, bool>((s, s1) => {
                    Console.WriteLine("works finehere.");
                    return true;
                });
            }
            return null;
        }

    }

    // So:
    // Take the Interface, generate a proxy class that has the <Member>Delegates and instances of the Delegates
    // and then generate the Constructor which takes an object instance.
    
    public interface IPretendProvider {

        bool AddPackageSource(string name, string location);

    }

    //internal delegate Delegate CreateDelegate(string memberName);

    public interface IDelegateCreator {
        Delegate CreateDelegate(string memberName, IEnumerable<string> pNames, IEnumerable<Type> pTypes, Type returnType);
    }

    
   
}