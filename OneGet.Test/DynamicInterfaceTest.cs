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
    using System.IO;
    using System.Reflection;
    using System.Security.Policy;
    using Core.Dynamic;
    using CSharp.RuntimeBinder;
    using Xunit;

    public class RequiredAttribute : Attribute {
    }

    public class BuildPSFirst {
        public void Build() {
            
        }
    }

    public interface IDynTest {
        [Required]
        void One();

        [Required]
        bool Two();

        string Three();

        string Four(int a);

        string Five(int a, string b);

        bool IsMethodImplemented(string name);
    }

    public abstract class AbstractDynTest {
        [Required]
        public abstract void One();

        [Required]
        public abstract bool Two();

        public abstract string Three();

        public abstract string Four(int a);

        public virtual string Five(int a, string b) {
            Console.WriteLine("DefaultImplementation");
            return "NO ANSWER";
        }

        public abstract bool IsMethodImplemented(string name);
    }

    

    public class DynInst {
        public void One() {
            Console.WriteLine("In One");
        }

        public bool Two() {
            Console.WriteLine("In Two");
            return true;
        }

        public string Three() {
            Console.WriteLine("In three");
            return "Three";
        }

        public string Four(int a) {
            Console.WriteLine("Four {0}", a);
            return "Four" + a;
        }

        public string Five(int a, string b) {
            Console.WriteLine("Five {0} {1}", a, b);
            return "Four" + a + b;
        }
    }

    public class DynamicInterfaceTest {
        [Fact]
        public void TestDynamicInterfaceAgainstClass() {
            var di = new DynamicInterface();

            var idyn = di.Create<IDynTest>(typeof (DynInst));

            idyn.One();
            Assert.True(idyn.Two());
            Assert.Equal("Three", idyn.Three());
            Assert.Equal("Four4", idyn.Four(4));
            Assert.Equal("Four5hi", idyn.Five(5, "hi"));

            Assert.True(idyn.IsMethodImplemented("One"));
            Assert.True(idyn.IsMethodImplemented("Two"));
            Assert.True(idyn.IsMethodImplemented("Three"));
            Assert.True(idyn.IsMethodImplemented("Four"));
            Assert.True(idyn.IsMethodImplemented("Five"));
        }

        [Fact]
        public void TestDynamicInterfaceAgainstAnonymousObject() {
            var di = new DynamicInterface();

            // proves that it reuses generated ProxyClasses.
            for (int i = 0; i < 10; i++) {
                var idyn2 = di.Create<IDynTest>(new {
                    One = new Action(() => {}),
                    Two = new Func<bool>(() => {return true;})
                });

                Assert.True(idyn2.IsMethodImplemented("One"));
                Assert.True(idyn2.IsMethodImplemented("Two"));
                Assert.False(idyn2.IsMethodImplemented("Three"));
                Assert.False(idyn2.IsMethodImplemented("Four"));
                Assert.False(idyn2.IsMethodImplemented("Five"));


                idyn2.One();

                Assert.True(idyn2.Two());
                Assert.Equal(null, idyn2.Four(4));
            }
        }



        [Fact]
        public void TestDynamicInterfaceAgainstAnonymousObjects() {
            var di = new DynamicInterface();

            var dynamicInstance = di.Create<IDynTest>(new {
                One = new Action(() => { }),
            }, new {
                Two = new Func<bool>(() => { return true; })
            }, new {
                Four = new Func<int, string>((i)=> "::"+i)
            });

            Assert.True(dynamicInstance.IsMethodImplemented("One"));
            Assert.True(dynamicInstance.IsMethodImplemented("Two"));
            Assert.False(dynamicInstance.IsMethodImplemented("Three"));
            Assert.True(dynamicInstance.IsMethodImplemented("Four"));
            Assert.False(dynamicInstance.IsMethodImplemented("Five"));

            Assert.True(dynamicInstance.Two());
            Assert.Equal("::4", dynamicInstance.Four(4));
        }

        private delegate bool Two(); 

        [Fact]
        public void TestAsFunction() {
            var d = new DynInst();
            var t = d.As<Two>();
            t();

            var p = new {
                Two = new Func<bool>(() => {
                    Console.WriteLine("In Func<bool> for Two!");
                    return true;
                })
            };

            var q = p.As<Two>();
            q();

            var z = new {
            };


            // this can't work, as the function doesn't exist.
            Assert.Throws<Exception>(() => {
                var tz = z.As<Two>();
                tz();
            });

            Func<bool> fTwo = new Func<bool>(() => {
                Console.WriteLine("In fTwo");
                return true;
            });

            fTwo.As<Two>()();

            Assert.Throws<Exception>(() => {
                Func<string> fThree = new Func<string>(() => {
                    Console.WriteLine("In fThree");
                    return "true";
                });

                fThree.As<Two>()();
            });
        }

        [Fact]
        public void TestClassImplementation() {
            var dynamicInstance = DynamicInterface.Instance.Create<AbstractDynTest>(new {
                One = new Action(() => { }),
            }, new {
                Two = new Func<bool>(() => { return true; })
            }, new {
                Four = new Func<int, string>((i) => "::" + i)
            });

            Assert.True(dynamicInstance.IsMethodImplemented("One"));
            Assert.True(dynamicInstance.IsMethodImplemented("Two"));
            Assert.False(dynamicInstance.IsMethodImplemented("Three"));
            Assert.True(dynamicInstance.IsMethodImplemented("Four"));
            Assert.False(dynamicInstance.IsMethodImplemented("Five"));

            dynamicInstance.One();
            dynamicInstance.Two();
            dynamicInstance.Three();
            dynamicInstance.Four(100);
            dynamicInstance.Five(100, "hi");
        }

        [Fact]
        public void TestDynamicInterfaceAgainstAnonymousObjectsInDifferentOrder() {
            var di = new DynamicInterface();

            var dynamicInstance = di.Create<IDynTest>( new {
                Two = new Func<bool>(() => { return true; })
            }, new {
                Four = new Func<int, string>((i) => "::" + i)
            },new {
                One = new Action(() => { }),
            });

            Assert.True(dynamicInstance.IsMethodImplemented("One"));
            Assert.True(dynamicInstance.IsMethodImplemented("Two"));
            Assert.False(dynamicInstance.IsMethodImplemented("Three"));
            Assert.True(dynamicInstance.IsMethodImplemented("Four"));
            Assert.False(dynamicInstance.IsMethodImplemented("Five"));

            Assert.True(dynamicInstance.Two());
            Assert.Equal("::4", dynamicInstance.Four(4));
        }

        [Fact]
        public void TestGetTypes() {
            var asm =  Assembly.GetExecutingAssembly();
            var types = asm.GetTypes();


            foreach (var t in types) {
                if (t.IsEnum) {
                    Console.WriteLine("ENUM: {0}", t.Name);
                    continue;
                }
                if (t.IsDelegate()) {
                    Console.WriteLine("Delegate: {0}", t.Name);
                    continue;
                }
            }
        }

        [Fact]
        public void TestChaining() {

            var di = new DynamicInterface();
            var instance = di.Create<IDynTest>(
                new {
                    One = new Action(() => {
                        Console.WriteLine("Instance1::One");
                    }),
                }, new {
                    Two = new Func<bool>(() => {
                        Console.WriteLine("Instance1::Two");
                        return true;
                    })
                }
            );


            instance.One();
            
            // override 1:one
            var instance2 = di.Create<IDynTest>(
                new {
                    One = new Action(() => {
                        Console.WriteLine("Instance3::One");
                    }), 
                    Two = new Func<bool>(() => {return instance.Two();}) }
                , instance);

            var instance3 = di.Create<IDynTest>(
                new {
                    Four = new Func<int, string>((i) => {
                        Console.WriteLine("Instance3::Four");
                        return "::" + i;
                    }),
                }, instance2);
            

            instance3.One();
            instance3.Two();
            instance3.Three();
            instance3.Four(100);
            instance3.Five(100,"hi");
        }


        [Fact]
        public void TestDynamicInterfaceRequired() {
            var di = new DynamicInterface();

            Assert.Throws<Exception>(() => {
                var idyn3 = di.Create<IDynTest>(new {
                    One = new Action(() => {})
                });
            });
        }

        [Fact]
        public void TestUsingStaticOnDyanmicType() {
            Assert.Throws<RuntimeBinderException>(() => {
                dynamic x = new TUSODT();
                Console.WriteLine(x.Hello());
            });
        }

        [Fact]
        public void CreateTypeWithBadArguments() {
            var di = new DynamicInterface();

            Assert.Throws<Exception>(() => {
                di.Create<IDynTest>(typeof (object), typeof (File));
            });

        }

        public class TUSODT {
            public static string Hello() {
                return "Hello";
            }
        }

        
    }
}