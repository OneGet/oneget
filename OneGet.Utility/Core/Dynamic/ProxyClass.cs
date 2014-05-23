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

namespace Microsoft.OneGet.Core.Dynamic {
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using Extensions;

    internal class ProxyClass {
        private static int _counter = 1;
        private readonly List<Action<object>> _afterInstantiation = new List<Action<object>>();
        private readonly TypeBuilder _dynamicType;
        private readonly HashSet<string> _implementedMethods = new HashSet<string>();
        private readonly object[] _instances;
        private AssemblyBuilder _dynamicAssembly;
        private string _proxyName;

        private readonly Dictionary<FieldBuilder, object> _backing = new Dictionary<FieldBuilder, object>();

        private object _instance;
        private Type _type;

        internal ProxyClass(Type interfaceType, params object[] instances) {
            _instances = instances;

            _dynamicType = DefineDynamicType(interfaceType, instances.Select(each => each.GetType().Name).JoinWith("_"));

            // implement the methods that we can.
            var unimplementedMethods = instances.Aggregate((IEnumerable<MethodInfo>)interfaceType.GetMethods(), (current, actualInstance) => ImplementMethodsForInstance(actualInstance, current));

            foreach (var method in unimplementedMethods) {
                // did not find a matching method or signature, or the instance told us that it doesn't actually support it 
                // that's ok, if we get here, it must not be a required method.
                // we'll implement a placeholder method for it.
                _dynamicType.GenerateDefaultMethod(method);
            }

            // generate the constructor for the class.
            DefineConstructorWithBackingField();

            _dynamicType.OverrideInitializeLifetimeService();
        }

        internal Type Type {
            get {
                return _type ?? (_type = _dynamicType.CreateType());
            }
        }

        internal object Instance {
            get {
                if (_instance == null) {
                    var proxyConstructor = Type.GetConstructors()[0];

                    var proxyInstance = proxyConstructor.Invoke(_backing.Values.ToArray());
                    _instance = proxyInstance;

                    foreach (var action in _afterInstantiation) {
                        action(_instance);
                    }

                    var imf = Type.GetField("__implementedMethods", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (imf != null) {
                        imf.SetValue(_instance, _implementedMethods);
                    }

                    // only needed for testing.
                    // _dynamicAssembly.Save("{0}.dll".format(_proxyName));
                }
                return _instance;
            }
        }

        private IEnumerable<MethodInfo> ImplementMethodsForInstance(object actualInstance, IEnumerable<MethodInfo> methodsToImplement) {
            var instanceSupportsMethod = DynamicInterfaceExtensions.GenerateInstanceSupportsMethod(actualInstance);

            var instanceType = actualInstance.GetType();
            var instanceMethods = instanceType.GetPublicMethods();
            var instanceFields = instanceType.GetPublicDelegateFields();
            var instanceProperties = instanceType.GetPublicDelegateProperties();

            FieldBuilder backingField = null;
            

            foreach (var method in methodsToImplement) {
                if (method.Name == "IsImplemented") {
                    _dynamicType.GenerateIsImplemented(method);

                    _implementedMethods.Add(method.Name);
                    continue;
                }

                if (instanceSupportsMethod(method.Name)) {
                    var instanceMethod = instanceMethods.FindMethod(method);
                    if (instanceMethod != null) {
                        if (backingField == null) {
                            var name = "_instance_{0}".format(++_counter);
                            backingField = _dynamicType.DefineField(name, instanceType, FieldAttributes.Private);
                            _backing.Add(backingField, actualInstance);
                        }

                        _dynamicType.GenerateMethodForDirectCall(method, backingField, instanceMethod);

                        _implementedMethods.Add(method.Name);
                        continue;
                    }

                    var instanceDelegate = instanceFields.FindDelegate(actualInstance, method) ?? instanceProperties.FindDelegate(actualInstance, method);
                    if (instanceDelegate != null) {
                        var fieldName = _dynamicType.GenerateMethodForDelegateCall(method);

                        // make sure this object loads the value of delegate into the field after instantiation time.
                        _afterInstantiation.Add((instance) => {
                            var f = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                            if (f == null) {
                                throw new Exception("delegate storage field can't possibly be null.");
                            }

                            f.SetValue(instance, instanceDelegate);
                        });
                        _implementedMethods.Add(method.Name);
                        continue;
                    }
                }
                yield return method;
            }
        }

        private TypeBuilder DefineDynamicType(Type interfaceType, string actualInstanceName) {
            _proxyName = "{0}_proxy_{2}".format(interfaceType.NiceName().MakeSafeFileName(), actualInstanceName.MakeSafeFileName(), _counter++);

            _dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(_proxyName), AssemblyBuilderAccess.Run);

            // Define a dynamic module in this assembly.
            // , "{0}.dll".format(_proxyName)
            var dynamicModule = _dynamicAssembly.DefineDynamicModule(_proxyName);

            // Define a runtime class with specified name and attributes.
            var dynamicType = dynamicModule.DefineType(_proxyName, TypeAttributes.Public, typeof(MarshalByRefObject));
            dynamicType.AddInterfaceImplementation(interfaceType);
            return dynamicType;
        }

        internal void DefineConstructorWithBackingField() {
            // add constructor that takes the specific type of object that we're going to bind

            var types = _backing.Values.Select(each => each.GetType()).ToArray();

            var constructor = _dynamicType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, types);

            var il = constructor.GetILGenerator();
            int index = 1;
            foreach (var backingField in _backing.Keys) {
                // store actualInstance in backingField
                il.LoadArgument(0);
                il.LoadArgument(index++);
                il.StoreField(backingField);
            }
            // return
            il.Return();

        }
    }
}