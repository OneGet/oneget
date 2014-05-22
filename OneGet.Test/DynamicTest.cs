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
    using System.Dynamic;
    using Core.AppDomains;
    using PowerShell.OneGet.CmdLets;
    using Xunit;

    public class DynamicTest {
        [Fact]
        public void VerifyConceptAboutDynamic() {
            var instance = new PPInstance();
            var proxy = new PackageProviderProxy(instance);
            proxy.AddPackageSource("test", "value");
            
            proxy.AddPackageSource2("NameValue", "LocationValue");
            proxy.AddPackageSource3("NameValue", "LocationValue");
            proxy.AddPackageSource4("NameValue", "LocationValue");
           
        }
    }


    public class PackageProviderProxy {
        public readonly Func<string, string, bool > AddPackageSource;
        public readonly AddPackageSourceDelegate AddPackageSource2;
        public readonly AddPackageSourceDelegate AddPackageSource3;
        public readonly AddPackageSourceDelegate AddPackageSource4;

        public delegate bool AddPackageSourceDelegate(string name, string location);

        public PackageProviderProxy(object instance) {
            var createDelegate = instance.CreateProxiedDelegate<CreateDelegate>("CreateDelegate" );

            AddPackageSource = instance.CreateProxiedDelegate<Func<string, string, bool>>("AddPackageSource", createDelegate);


            AddPackageSource2 = instance.CreateProxiedDelegate<AddPackageSourceDelegate>("AddPackageSource", createDelegate);

            AddPackageSource3 = instance.CreateProxiedDelegate<AddPackageSourceDelegate>("AddPackageSourceFunc", createDelegate);
            AddPackageSource4 = instance.CreateProxiedDelegate<AddPackageSourceDelegate>("AddPackageSource4", createDelegate);
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
    
    public interface IPackageProvider {

        bool AddPackageSource(string name, string location);

    }

    //internal delegate Delegate CreateDelegate(string memberName);

    public interface IDelegateCreator {
        Delegate CreateDelegate(string memberName, IEnumerable<string> pNames, IEnumerable<Type> pTypes, Type returnType);
    }
   
}