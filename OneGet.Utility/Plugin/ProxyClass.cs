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

namespace Microsoft.OneGet.Plugin {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using Collections;
    using Extensions;

    internal class ProxyClass {
        private static int _typeCounter  = 1;
        
        private readonly TypeBuilder _dynamicType;
        private readonly HashSet<string> _implementedMethods = new HashSet<string>();
        private readonly List<FieldBuilder> _storageFields = new List<FieldBuilder>();
        private AssemblyBuilder _dynamicAssembly;
        private string _proxyName;
        private string _filename;
        private string _directory;
        private string _fullpath;
        private Type _type;

        internal ProxyClass(Type interfaceType, OrderedDictionary<Type, List<MethodInfo, MethodInfo>> methods, List<Delegate, MethodInfo> delegates, List<MethodInfo> stubs) {
            var counter = 0;
            _dynamicType = DefineDynamicType(interfaceType);

            foreach (var instanceType in methods.Keys) {
                // generate storage for object
                var field = _dynamicType.DefineField("_instance_{0}".format(++counter), instanceType, FieldAttributes.Private);
                _storageFields.Add(field);

                // create methods

                foreach (var method in methods[instanceType]) {
                    _dynamicType.GenerateMethodForDirectCall(method.Key, field, method.Value);
                    _implementedMethods.Add(method.Key.Name);
                }
            }

            foreach (var d in delegates) {
                var field = _dynamicType.DefineField("_delegate_{0}".format(++counter), d.Key.GetType(), FieldAttributes.Private);
                _storageFields.Add(field);
                _implementedMethods.Add(d.Value.Name);

                _dynamicType.GenerateMethodForDelegateCall(d.Value, field);
            }

            foreach (var method in stubs) {
                // did not find a matching method or signature, or the instance told us that it doesn't actually support it 
                // that's ok, if we get here, it must not be a required method.
                // we'll implement a placeholder method for it.
                _dynamicType.GenerateStubMethod(method);
            }

            _dynamicType.GenerateIsMethodImplemented();

            // generate the constructor for the class.
            DefineConstructor();

            //if (typeof (MarshalByRefObject).IsAssignableFrom(_dynamicType)) {
                _dynamicType.OverrideInitializeLifetimeService();
            //}
        }

        internal Type Type {
            get {
                lock (PluginDomain.DynamicAssemblyPaths) {
                    try {
                        if (_type == null) {
                            _type = _dynamicType.CreateType();
                            _dynamicAssembly.Save(_filename);
                            var registerDynamicAssembly = AppDomain.CurrentDomain.GetData("RegisterDynamicAssembly") as Action<string, string>;
                            if (registerDynamicAssembly != null) {
                                registerDynamicAssembly(_dynamicAssembly.FullName, _fullpath);
                            } else {
                                PluginDomain.DynamicAssemblyPaths.Add(_dynamicAssembly.FullName, _fullpath);
                            }
                        }
                        return _type;
                    } catch (Exception e) {
                        e.Dump();
                        throw;
                    }
                }
            }
        }

        internal object CreateInstance(List<object> instances, List<Delegate, MethodInfo> delegates) {
            var proxyConstructor = Type.GetConstructors()[0];
            var instance = proxyConstructor.Invoke(instances.Concat(delegates.Select(each => each.Key)).ToArray());
            // set the implemented methods collection 
            var imf = Type.GetField("__implementedMethods", BindingFlags.NonPublic | BindingFlags.Instance);
            if (imf != null) {
                imf.SetValue(instance, _implementedMethods);
            }

            return instance;
        }

        private TypeBuilder DefineDynamicType(Type interfaceType) {
            _proxyName = "{0}_proxy_{1}_in_{2}".format(interfaceType.NiceName().MakeSafeFileName(), _typeCounter++, AppDomain.CurrentDomain.FriendlyName.MakeSafeFileName().Replace("[","_").Replace("]","_"));

            _fullpath = (_proxyName+".dll").GenerateTemporaryFilename();
            _directory = Path.GetDirectoryName(_fullpath);
            _filename = Path.GetFileName(_fullpath);

            _dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(_proxyName), AssemblyBuilderAccess.RunAndSave, _directory);

            // Define a dynamic module in this assembly.
            // , "{0}.dll".format(_proxyName)
            var dynamicModule = _dynamicAssembly.DefineDynamicModule(_proxyName,_filename);

            // Define a runtime class with specified name and attributes.
            if (interfaceType.IsInterface) {
                var dynamicType = dynamicModule.DefineType(_proxyName, TypeAttributes.Public, typeof (MarshalByRefObject));
                dynamicType.AddInterfaceImplementation(interfaceType);
                return dynamicType;
            } else {
                var dynamicType = dynamicModule.DefineType(_proxyName, TypeAttributes.Public, interfaceType);
                return dynamicType;
            }
        }

        internal void DefineConstructor() {
            // add constructor that takes the specific type of object that we're going to bind

            var types = _storageFields.Select(each => each.FieldType).ToArray();

            var constructor = _dynamicType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, types);

            var il = constructor.GetILGenerator();
            var index = 1;
            foreach (var backingField in _storageFields) {
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