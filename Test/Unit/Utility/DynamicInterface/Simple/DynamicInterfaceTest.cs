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

namespace Microsoft.OneGet.Test.Utility.DynamicInterface.Simple {
    using System;
    using System.IO;
    using System.Reflection;
    using CSharp.RuntimeBinder;
    using OneGet.Utility.Plugin;
    using Xunit;
    using Xunit.Abstractions;
    using Console = Support.Console;

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

    public class DynamicInterfaceTest : Tests {
          public DynamicInterfaceTest(ITestOutputHelper outputHelper) : base (outputHelper) {
        }
        public delegate object ReturnsAnObject();

        public delegate string ReturnsAString();

        [Fact]
        public void TestDynamicInterfaceAgainstClass() {
            using (CaptureConsole) {
                var idyn = typeof (DynInst).Create<IDynTest>();

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
        }

        [Fact]
        public void TestDynamicInterfaceAgainstAnonymousObject() {
            using (CaptureConsole) {
                // proves that it reuses generated ProxyClasses.
                for (int i = 0; i < 10; i++) {
                    var idyn2 = DynamicInterface.DynamicCast<IDynTest>(new {
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
        }

        [Fact]
        public void TestDynamicInterfaceAgainstAnonymousObjects() {
            using (CaptureConsole) {
                var dynamicInstance = DynamicInterface.DynamicCast<IDynTest>(new {
                    One = new Action(() => {}),
                }, new {
                    Two = new Func<bool>(() => {return true;})
                }, new {
                    Four = new Func<int, string>((i) => "::" + i)
                });

                Assert.True(dynamicInstance.IsMethodImplemented("One"));
                Assert.True(dynamicInstance.IsMethodImplemented("Two"));
                Assert.False(dynamicInstance.IsMethodImplemented("Three"));
                Assert.True(dynamicInstance.IsMethodImplemented("Four"));
                Assert.False(dynamicInstance.IsMethodImplemented("Five"));

                Assert.True(dynamicInstance.Two());
                Assert.Equal("::4", dynamicInstance.Four(4));
            }
        }

        [Fact]
        public void TestAsFunction() {
            using (CaptureConsole) {
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

                // this creates a dummy function now.

                var tz = z.As<Two>();
                tz();

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
        }

        [Fact]
        public void TestClassImplementation() {
            using (CaptureConsole) {
                var dynamicInstance = DynamicInterface.DynamicCast<AbstractDynTest>(new {
                    One = new Action(() => {}),
                }, new {
                    Two = new Func<bool>(() => {return true;})
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

        }

        [Fact]
        public void TestDynamicInterfaceAgainstAnonymousObjectsInDifferentOrder() {
            using (CaptureConsole) {
                var dynamicInstance = DynamicInterface.DynamicCast<IDynTest>(new {
                    Two = new Func<bool>(() => {return true;})
                }, new {
                    Four = new Func<int, string>((i) => "::" + i)
                }, new {
                    One = new Action(() => {}),
                });

                Assert.True(dynamicInstance.IsMethodImplemented("One"));
                Assert.True(dynamicInstance.IsMethodImplemented("Two"));
                Assert.False(dynamicInstance.IsMethodImplemented("Three"));
                Assert.True(dynamicInstance.IsMethodImplemented("Four"));
                Assert.False(dynamicInstance.IsMethodImplemented("Five"));

                Assert.True(dynamicInstance.Two());
                Assert.Equal("::4", dynamicInstance.Four(4));
            }
        }

        [Fact]
        public void TestGetTypes() {
            using (CaptureConsole) {
                var asm = Assembly.GetExecutingAssembly();
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
        }

        [Fact]
        public void TestChaining() {
            using (CaptureConsole) {
                var instance = DynamicInterface.DynamicCast<IDynTest>(
                    new {
                        One = new Action(() => {Console.WriteLine("Instance1::One");}),
                    }, new {
                        Two = new Func<bool>(() => {
                            Console.WriteLine("Instance1::Two");
                            return true;
                        })
                    }
                    );

                instance.One();

                // override 1:one
                var instance2 = DynamicInterface.DynamicCast<IDynTest>(
                    new {
                        One = new Action(() => {Console.WriteLine("Instance3::One");}),
                        Two = new Func<bool>(() => {return instance.Two();})
                    }
                    , instance);

                var instance3 = DynamicInterface.DynamicCast<IDynTest>(
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
                instance3.Five(100, "hi");
            }
        }

        [Fact]
        public void TestDynamicInterfaceRequired() {
            using (CaptureConsole) {
                Assert.Throws<Exception>(() => {
                    var idyn3 = DynamicInterface.DynamicCast<IDynTest>(new {
                        One = new Action(() => {})
                    });
                });
            }
        }

        [Fact]
        public void TestUsingStaticOnDyanmicType() {
            using (CaptureConsole) {
                Assert.Throws<RuntimeBinderException>(() => {
                    dynamic x = new TUSODT();
                    Console.WriteLine(x.Hello());
                });
            }
        }

        [Fact]
        public void CreateTypeWithBadArguments() {
            using (CaptureConsole) {
                Assert.Throws<Exception>(() => {DynamicInterface.Create<IDynTest>(typeof (object), typeof (File));});
            }
        }

        [Fact]
        public void IsAMemoryStreamAssignableToAFileStream() {
            using (CaptureConsole) {
                Assert.False(typeof (MemoryStream).IsAssignableFrom(typeof (FileStream)));
                Assert.False(typeof (FileStream).IsAssignableFrom(typeof (MemoryStream)));

                Assert.False(typeof (Stream) == (typeof (FileStream)));
                Assert.False(typeof (FileStream) == (typeof (Stream)));

                Assert.True(typeof (Stream).IsAssignableFrom(typeof (FileStream)));
                Assert.False(typeof (FileStream).IsAssignableFrom(typeof (Stream)));
            }
        }

        [Fact]
        public void CheckForAcceptableTypes() {
            using (CaptureConsole) {
                var x = new ActualImplementation().As<ClientInterface>();
                var y = x.ActuallyReturnsString();
                var z = x.ActuallyReturnsFileStream();

                Console.WriteLine("Y is {0}", y.GetType().Name);
                Console.WriteLine("Z is {0}", z.GetType().Name);

                // this function doesn't match anything in the implemention
                // so a stub method gets created (which returns null)
                // MemoryStream a = x.ActuallyRetunsMemoryStream();
                // Assert.Null(a);

                // the clientinterface is more restricted than the implementation
                // but that's ok.
                MemoryStream ms = new MemoryStream();
                Assert.True(x.TakesAStream(ms));

                // the clientinterface is less restrictive than the implementation
                // and that's not ok.
                Assert.False(x.TakesAFileStream(ms));

                var shouldWork = new {
                    TakesAStream = new Func<Stream, bool>(stream => {return stream != null;})
                }.As<ClientInterface>();

                Assert.True(shouldWork.TakesAStream(ms));

                var shouldNotWork = new {
                    TakesAFileStream = new Func<MemoryStream, bool>(stream => {
                        Console.WriteLine("never called");
                        return stream != null;
                    })
                }.As<ClientInterface>();

                Assert.False(shouldWork.TakesAFileStream(ms));

                var shouldWorkToo = new {
                    ActuallyReturnsString = new Func<object>(() => "hello")
                }.As<ClientInterface>();

                Assert.NotNull(shouldWorkToo.ActuallyReturnsString());

                var shouldNotWorkToo = new {
                    ActuallyRetunsMemoryStream = new Func<Stream>(() => new MemoryStream())
                }.As<ClientInterface>();

                Assert.Null(shouldNotWorkToo.ActuallyRetunsMemoryStream());

                Func<object> fReturnsAString = new Func<object>(() => "hello");

                var fShouldWork = fReturnsAString.As<ReturnsAnObject>();

                Assert.NotNull(fShouldWork());

                Assert.Throws<Exception>(() => {
                    // this shouldn't work because the return type object
                    // can't be expressed as a string.
                    var fShouldNotWork = fReturnsAString.As<ReturnsAString>();
                });
            }
        }

        [Fact]
        public void TestEnum() {
            using (CaptureConsole) {
                Assert.False(typeof (Some).IsAssignableFrom(typeof (int)));
                Assert.False(typeof (int).IsAssignableFrom(typeof (Some)));
            }
        }

        private delegate bool Two();

        public class ActualImplementation {
            public string ActuallyReturnsString() {
                return "Hello";
            }

            public FileStream ActuallyReturnsFileStream() {
                return new FileStream(Assembly.GetExecutingAssembly().Location, FileMode.Open, FileAccess.Read);
            }

            public Stream ActuallyRetunsMemoryStream() {
                return new MemoryStream();
            }

            public bool TakesAStream(Stream s) {
                return s != null;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "#pw26506")]
            public bool TakesAFileStream(FileStream ms) {
                Console.WriteLine("HUH?");
                Console.WriteLine("Type of stream is {0}", ms.GetType().Name);
                Console.WriteLine("Name of stream is {0}", ms.Name);
                return ms != null;
            }
        }

        public interface ClientInterface {
            object ActuallyReturnsString();
            Stream ActuallyReturnsFileStream();
            MemoryStream ActuallyRetunsMemoryStream();
            bool TakesAStream(MemoryStream ms);
            bool TakesAFileStream(Stream ms);
        }

        public class TUSODT {
            public static string Hello() {
                return "Hello";
            }
        }

        private enum Some {
            One = 1,
            Two = 2,
        }
    }
}